# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
import errno
import logging
import os
import subprocess
from abc import ABC, abstractmethod
from io import IOBase
from platform import system
from subprocess import PIPE
from time import sleep
from typing import IO, Any, List, Mapping, Optional, Union, cast
from urllib import request
from urllib.error import URLError

from selenium.common.exceptions import WebDriverException
from selenium.types import SubprocessStdAlias
from selenium.webdriver.common import utils

logger = logging.getLogger(__name__)


class Service(ABC):
    def __init__(
        self,
        executable_path: Optional[str] = None,
        port: int = 0,
        log_output: Optional[SubprocessStdAlias] = None,
        env: Optional[Mapping[Any, Any]] = None,
        driver_path_env_key: Optional[str] = None,
        **kwargs,
    ) -> None:
        if isinstance(log_output, str):
            self.log_output = cast(IOBase, open(log_output, "a+", encoding="utf-8"))
        elif log_output == subprocess.STDOUT:
            self.log_output = cast(Optional[Union[int, IOBase]], None)
        elif log_output is None or log_output == subprocess.DEVNULL:
            self.log_output = cast(Optional[Union[int, IOBase]], subprocess.DEVNULL)
        else:
            self.log_output = log_output

        self.port = port or utils.free_port()
        self.popen_kw = kwargs.pop("popen_kw", {})
        self.creation_flags = self.popen_kw.pop("creation_flags", 0)
        self.env = env or os.environ
        self.DRIVER_PATH_ENV_KEY = driver_path_env_key
        self._path = self.env_path() or executable_path

    @property
    def service_url(self) -> str:
        return f"http://{utils.join_host_port('localhost', self.port)}"

    @abstractmethod
    def command_line_args(self) -> List[str]:
        raise NotImplementedError("This method needs to be implemented in a sub class")

    @property
    def path(self) -> str:
        return self._path or ""

    @path.setter
    def path(self, value: str) -> None:
        self._path = str(value)

    def start(self) -> None:
        if self._path is None:
            raise WebDriverException("Service path cannot be None.")
        self._start_process(self._path)

        count = 0
        while True:
            self.assert_process_still_running()
            if self.is_connectable():
                break
            sleep(min(0.01 + 0.05 * count, 0.5))
            count += 1
            if count == 70:
                raise WebDriverException(f"Can not connect to the Service {self._path}")

    def assert_process_still_running(self) -> None:
        return_code = self.process.poll()
        if return_code:
            raise WebDriverException(f"Service {self._path} unexpectedly exited. Status code was: {return_code}")

    def is_connectable(self) -> bool:
        return utils.is_connectable(self.port)

    def send_remote_shutdown_command(self) -> None:
        try:
            request.urlopen(f"{self.service_url}/shutdown")
        except URLError:
            return

        for _ in range(30):
            if not self.is_connectable():
                break
            sleep(1)

    def stop(self) -> None:
        if self.log_output not in {PIPE, subprocess.DEVNULL}:
            if isinstance(self.log_output, IOBase):
                self.log_output.close()
            elif isinstance(self.log_output, int):
                os.close(self.log_output)

        if self.process is not None and self.process.poll() is None:
            try:
                self.send_remote_shutdown_command()
            except TypeError:
                pass
            finally:
                self._terminate_process()

    def _terminate_process(self) -> None:
        try:
            stdin, stdout, stderr = (
                self.process.stdin,
                self.process.stdout,
                self.process.stderr,
            )
            for stream in stdin, stdout, stderr:
                try:
                    stream.close()  # type: ignore
                except AttributeError:
                    pass
            self.process.terminate()
            try:
                self.process.wait(60)
            except subprocess.TimeoutExpired:
                logger.error(
                    "Service process refused to terminate gracefully with SIGTERM, escalating to SIGKILL.",
                    exc_info=True,
                )
                self.process.kill()
        except OSError:
            logger.error("Error terminating service process.", exc_info=True)

    def __del__(self) -> None:
        try:
            self.stop()
        except Exception:
            pass

    def _start_process(self, path: str) -> None:
        """Creates a subprocess by executing the command provided.

        :param path: full command to execute
        """
        # Pre-check: validate path exists and is executable
        if not os.path.isfile(path):
            raise WebDriverException(f"The executable at path '{path}' does not exist.")
        if not os.access(path, os.X_OK):
            raise WebDriverException(f"The executable at path '{path}' is not executable.")

        cmd = [path]
        cmd.extend(self.command_line_args())
        close_file_descriptors = self.popen_kw.pop("close_fds", system() != "Windows")

        try:
            start_info = None
            if system() == "Windows":
                start_info = subprocess.STARTUPINFO()  # type: ignore[attr-defined]
                start_info.dwFlags = subprocess.CREATE_NEW_CONSOLE | subprocess.STARTF_USESHOWWINDOW  # type: ignore[attr-defined]
                start_info.wShowWindow = subprocess.SW_HIDE  # type: ignore[attr-defined]

            self.process = subprocess.Popen(
                cmd,
                env=self.env,
                close_fds=close_file_descriptors,
                stdout=cast(Optional[Union[int, IO[Any]]], self.log_output),
                stderr=cast(Optional[Union[int, IO[Any]]], self.log_output),
                stdin=PIPE,
                creationflags=self.creation_flags,
                startupinfo=start_info,
                **self.popen_kw,
            )
            logger.debug(
                "Started executable: `%s` in a child process with pid: %s using %s to output %s",
                self._path,
                self.process.pid,
                self.creation_flags,
                self.log_output,
            )
        except TypeError:
            raise
        except OSError as err:
            if err.errno == errno.EACCES:
                raise WebDriverException(
                    f"'{os.path.basename(self._path)}' executable may have wrong permissions."
                ) from err
            raise

    def env_path(self) -> Optional[str]:
        if self.DRIVER_PATH_ENV_KEY:
            return os.getenv(self.DRIVER_PATH_ENV_KEY, None)
        return None
