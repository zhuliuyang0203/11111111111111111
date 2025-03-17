# frozen_string_literal: true

require 'English'
require 'open3'
require 'rake'
require 'io/wait'
require_relative 'selenium_rake/checks'

module Bazel
  def self.execute(kind, args, target, verbose: Rake::FileUtilsExt.verbose_flag, log_file: nil, &block)
    if target.end_with?(':run')
      kind = 'run'
      target = target[0, target.length - 4]
    end

    cmd = %w[bazel] + [kind, target] + (args || [])
    cmd_out = ''
    cmd_exit_code = 0
    puts "Executing:\n#{cmd.join(' ')}" if verbose

    if SeleniumRake::Checks.windows?
      cmd_line = cmd.join(' ')
      begin
        cmd_out = `#{cmd_line} 2>&1`.encode('UTF-8', 'binary', invalid: :replace, undef: :replace, replace: '')
        puts cmd_out if verbose
        File.write(log_file, cmd_out) if log_file
        cmd_exit_code = $CHILD_STATUS.exitstatus
      rescue => e
        raise "Windows command execution failed: #{e.message}"
      end
    else
      begin
        Open3.popen2e(*cmd) do |stdin, stdouts, wait|
          stdin.close
          log = log_file ? File.open(log_file, 'a') : nil
          while (line = stdouts.gets)
            cmd_out += line
            $stdout.print line if verbose
            log&.write(line)
            log&.flush
          end
          log&.close
          cmd_exit_code = wait.value.exitstatus
        end
      rescue => e
        raise "Command execution failed: #{e.message}"
      end
    end

    raise "#{cmd.join(' ')} failed with exit code: #{cmd_exit_code}\nOutput: #{cmd_out}" if cmd_exit_code != 0

    block&.call(cmd_out)
    return unless cmd_out =~ %r{\s+(bazel-bin/\S+)}

    out_artifact = Regexp.last_match(1)
    puts "#{target} -> #{out_artifact}" if out_artifact
    out_artifact
  end
end
