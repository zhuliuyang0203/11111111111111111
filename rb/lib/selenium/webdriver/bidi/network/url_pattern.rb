# frozen_string_literal: true

require 'uri'

module Selenium
  module WebDriver
    class BiDi
      module UrlPattern
        module_function

        def format_pattern(url_patterns, pattern_type)
          case pattern_type
          when :string
            to_url_string_pattern(url_patterns)
          when :url
            to_url_pattern(url_patterns)
          else
            raise ArgumentError, "Unknown pattern type: #{pattern_type}"
          end
        end

        def to_url_pattern(*url_patterns)
          url_patterns.flatten.map do |url_pattern|
            uri = URI.parse(url_pattern)

            {
              type: 'pattern',
              protocol: uri.scheme || '',
              hostname: uri.host || '',
              port: uri.port.to_s || '',
              pathname: uri.path || '',
              search: uri.query || ''
            }
          end
        end

        def to_url_string_pattern(*url_patterns)
          url_patterns.flatten.map do |url_pattern|
            {
              type: 'string',
              pattern: url_pattern
            }
          end
        end
      end
    end
  end
end
