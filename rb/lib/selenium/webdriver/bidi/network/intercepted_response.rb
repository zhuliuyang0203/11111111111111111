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
      class InterceptedResponse < InterceptedItem
        attr_accessor :cookies, :headers, :credentials, :reason

        def new
          super(args[:network], args[:request])
          @cookies = args[:cookies]
          @headers = args[:headers]
          @credentials = args[:credentials]
          @reason = args[:reason]
        end

        def continue
          network.continue_response(id:, cookies:, headers:, credentials:, reason:)
        end
      end
    end # BiDi
  end # WebDriver
end # Selenium

