# frozen_string_literal: true

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

module Selenium
  module WebDriver
    class BiDi
      class InterceptedRequest < InterceptedItem
        attr_accessor :body, :cookies, :headers, :method, :url

        def initialize(**args)
          super(args[:network], args[:request])
          @body = args[:body]
          @cookies = args[:cookies]
          @headers = args[:headers] || []
          @method = args[:method]
          @url = args[:url]
        end

        def continue
          network.continue_request(id:, body:, cookies:, headers:, method:, url:)
        end

        def fail
          network.fail_request(id)
        end

        def add_header(name, value)
          headers.push(
            'name' => name,
            'value' => {
              'type' => 'string',
              'value' => value
            }
          )
        end
      end
    end # BiDi
  end # WebDriver
end # Selenium

