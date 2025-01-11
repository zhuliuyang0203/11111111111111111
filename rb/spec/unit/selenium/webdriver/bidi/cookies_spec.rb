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

require File.expand_path('../spec_helper', __dir__)
require File.expand_path('../../../../../lib/selenium/webdriver/bidi/network/cookies', __dir__)

module Selenium
  module WebDriver
    class BiDi
      describe Cookies do
        let(:cookies) { described_class.new }

        it 'returns a serialized array of cookie hashes' do
          cookies['key4'] = 'value4'
          cookies['session_id'] = 'xyz123'

          serialized = cookies.serialize
          expect(serialized).to be_an(Array)
          expect(serialized.size).to eq(2)

          key4_item = serialized.find { |h| h[:name] == 'key4' }
          expect(key4_item).not_to be_nil
          expect(key4_item[:value][:type]).to eq('string')
          expect(key4_item[:value][:value]).to eq('value4')

          session_item = serialized.find { |h| h[:name] == 'session_id' }
          expect(session_item).not_to be_nil
          expect(session_item[:value][:value]).to eq('xyz123')
        end
      end
    end
  end
end
