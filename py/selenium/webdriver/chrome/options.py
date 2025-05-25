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

from typing import Optional

from selenium.webdriver.chromium.options import ChromiumOptions
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities


class Options(ChromiumOptions):
    def __init__(self) -> None:
        super().__init__()
        self._enable_webextensions = False

    @property
    def default_capabilities(self) -> dict:
        return DesiredCapabilities.CHROME.copy()

    @property
    def enable_webextensions(self) -> bool:
        """Returns whether webextension support is enabled for Chrome.

        :Returns: True if webextension support is enabled, False otherwise.
        """
        return self._enable_webextensions

    @enable_webextensions.setter
    def enable_webextensions(self, value: bool) -> None:
        """Enables or disables webextension support for Chrome.

        When enabled, this automatically adds the required Chrome flags:
        - --enable-unsafe-extension-debugging
        - --remote-debugging-pipe

        :Args:
         - value: True to enable webextension support, False to disable.
        """
        self._enable_webextensions = value
        if value:
            # Add required flags for Chrome webextension support
            required_flags = ["--enable-unsafe-extension-debugging", "--remote-debugging-pipe"]
            for flag in required_flags:
                if flag not in self._arguments:
                    self.add_argument(flag)
        else:
            # Remove webextension flags if disabling
            flags_to_remove = ["--enable-unsafe-extension-debugging", "--remote-debugging-pipe"]
            for flag in flags_to_remove:
                if flag in self._arguments:
                    self._arguments.remove(flag)

    def enable_mobile(
        self,
        android_package: Optional[str] = "com.android.chrome",
        android_activity: Optional[str] = None,
        device_serial: Optional[str] = None,
    ) -> None:
        super().enable_mobile(android_package, android_activity, device_serial)
