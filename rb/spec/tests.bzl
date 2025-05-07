load("@rules_ruby//ruby:defs.bzl", "rb_library", "rb_test")
load(
    "//common:browsers.bzl",
    "COMMON_TAGS",
    "chrome_data",
    "edge_data",
    "firefox_beta_data",
    "firefox_data",
)

BROWSERS = {
    "chrome": {
        "data": chrome_data,
        "deps": ["//rb/lib/selenium/webdriver:chrome"],
        "tags": [],
        "target_compatible_with": [],
        "bidi_supported": True,
        "devtools_supported": True,
        "env": {
            "WD_REMOTE_BROWSER": "chrome",
            "WD_SPEC_DRIVER": "chrome",
        } | select({
            "@selenium//common:use_pinned_linux_chrome": {
                "CHROME_BINARY": "$(location @linux_chrome//:chrome-linux64/chrome)",
                "CHROMEDRIVER_BINARY": "$(location @linux_chromedriver//:chromedriver)",
            },
            "@selenium//common:use_pinned_macos_chrome": {
                "CHROME_BINARY": "$(location @mac_chrome//:Chrome.app)/Contents/MacOS/Chrome",
                "CHROMEDRIVER_BINARY": "$(location @mac_chromedriver//:chromedriver)",
            },
            "//conditions:default": {},
        }) | select({
            "@selenium//common:use_headless_browser": {"HEADLESS": "true"},
            "//conditions:default": {},
        }),
    },
    "edge": {
        "data": edge_data,
        "deps": ["//rb/lib/selenium/webdriver:edge"],
        "tags": [],
        "target_compatible_with": [],
        "bidi_supported": True,
        "devtools_supported": True,
        "env": {
            "WD_REMOTE_BROWSER": "edge",
            "WD_SPEC_DRIVER": "edge",
        } | select({
            "@selenium//common:use_pinned_linux_edge": {
                "EDGE_BINARY": "$(location @linux_edge//:opt/microsoft/msedge/microsoft-edge)",
                "MSEDGEDRIVER_BINARY": "$(location @linux_edgedriver//:msedgedriver)",
            },
            "@selenium//common:use_pinned_macos_edge": {
                "EDGE_BINARY": "$(location @mac_edge//:Edge.app)/Contents/MacOS/Microsoft\\ Edge",
                "MSEDGEDRIVER_BINARY": "$(location @mac_edgedriver//:msedgedriver)",
            },
            "//conditions:default": {},
        }) | select({
            "@selenium//common:use_headless_browser": {"HEADLESS": "true"},
            "//conditions:default": {},
        }),
    },
    "firefox": {
        "data": firefox_data,
        "deps": ["//rb/lib/selenium/webdriver:firefox"],
        "tags": [],
        "target_compatible_with": [],
        "bidi_supported": True,
        "env": {
            "WD_REMOTE_BROWSER": "firefox",
            "WD_SPEC_DRIVER": "firefox",
        } | select({
            "@selenium//common:use_pinned_linux_firefox": {
                "FIREFOX_BINARY": "$(location @linux_firefox//:firefox/firefox)",
                "GECKODRIVER_BINARY": "$(location @linux_geckodriver//:geckodriver)",
            },
            "@selenium//common:use_pinned_macos_firefox": {
                "FIREFOX_BINARY": "$(location @mac_firefox//:Firefox.app)/Contents/MacOS/firefox",
                "GECKODRIVER_BINARY": "$(location @mac_geckodriver//:geckodriver)",
            },
            "//conditions:default": {},
        }) | select({
            "@selenium//common:use_headless_browser": {"HEADLESS": "true"},
            "//conditions:default": {},
        }),
    },
    "firefox-beta": {
        "data": firefox_beta_data,
        "deps": ["//rb/lib/selenium/webdriver:firefox"],
        "tags": [],
        "target_compatible_with": [],
        "bidi_supported": True,
        "env": {
            "WD_REMOTE_BROWSER": "firefox",
            "WD_SPEC_DRIVER": "firefox",
        } | select({
            "@selenium//common:use_pinned_linux_firefox": {
                "FIREFOX_BINARY": "$(location @linux_beta_firefox//:firefox/firefox)",
                "GECKODRIVER_BINARY": "$(location @linux_geckodriver//:geckodriver)",
            },
            "@selenium//common:use_pinned_macos_firefox": {
                "FIREFOX_BINARY": "$(location @mac_beta_firefox//:Firefox.app)/Contents/MacOS/firefox",
                "GECKODRIVER_BINARY": "$(location @mac_geckodriver//:geckodriver)",
            },
            "//conditions:default": {},
        }) | select({
            "@selenium//common:use_headless_browser": {"HEADLESS": "true"},
            "//conditions:default": {},
        }),
    },
    "ie": {
        "data": [],
        "deps": ["//rb/lib/selenium/webdriver:ie"],
        "tags": [
            "skip-rbe",  # RBE is Linux-only.
        ],
        "target_compatible_with": ["@platforms//os:windows"],
        "env": {
            "WD_REMOTE_BROWSER": "ie",
            "WD_SPEC_DRIVER": "ie",
        },
    },
    "safari": {
        "data": [],
        "deps": ["//rb/lib/selenium/webdriver:safari"],
        "tags": [
            "exclusive-if-local",  # Safari cannot run in parallel.
            "skip-rbe",  # RBE is Linux-only.
        ],
        "target_compatible_with": ["@platforms//os:macos"],
        "env": {
            "WD_REMOTE_BROWSER": "safari",
            "WD_SPEC_DRIVER": "safari",
        },
    },
    "safari-preview": {
        "data": [],
        "deps": ["//rb/lib/selenium/webdriver:safari"],
        "tags": [
            "exclusive-if-local",  # Safari cannot run in parallel.
            "skip-rbe",  # RBE is Linux-only.
        ],
        "target_compatible_with": ["@platforms//os:macos"],
        "env": {
            "WD_REMOTE_BROWSER": "safari-preview",
            "WD_SPEC_DRIVER": "safari-preview",
        },
    },
}

def rb_integration_test(name, srcs, deps = [], data = [], browsers = BROWSERS.keys(), tags = []):
    # Generate a library target that is used by //rb/spec:spec to expose all tests to //rb:lint.
    rb_library(
        name = name,
        srcs = srcs,
        visibility = ["//rb:__subpackages__"],
    )

    VARIANTS = [
        {"suffix": "", "remote": False, "bidi": False},
        {"suffix": "-remote", "remote": True, "bidi": False},
        {"suffix": "-bidi", "remote": False, "bidi": True},
        {"suffix": "-bidi-remote", "remote": True, "bidi": True},
    ]

    for browser in browsers:
        config = BROWSERS[browser]
        for variant in VARIANTS:
            bidi_not_supported = variant["bidi"] and not config.get("bidi_supported", False)
            devtools_not_supported = "needs-devtools" in tags and not config.get("devtools_supported", False)
            excluded_by_bidi = "bidi-only" in tags and not variant["bidi"]
            excluded_by_grid = "remote-only" in tags and not variant["remote"]
            excluded_by_local = "no-grid" in tags and variant["remote"]

            if bidi_not_supported or devtools_not_supported or excluded_by_bidi or excluded_by_grid or excluded_by_local:
                continue

            target_name = "{}-{}{}".format(name, browser, variant["suffix"])

            env = config["env"]
            if variant["remote"]:
                env = env | {
                    "WD_SPEC_DRIVER": "remote",
                    "WD_BAZEL_JAVA_LOCATION": "$(rootpath //rb/spec:java-location)",
                }
            if variant["bidi"]:
                env = env | {
                    "WEBDRIVER_BIDI": "true"
            }

            test_data = config["data"] + data + ["//common/src/web"]
            if variant["remote"]:
                test_data += [
                    "//java/src/org/openqa/selenium/grid:selenium_server_deploy.jar",
                    "//rb/spec:java-location",
                    "@bazel_tools//tools/jdk:current_java_runtime",
                ]

            test_deps = [
                "//rb/spec/integration/selenium/webdriver:spec_helper"
            ] + config["deps"] + deps
            if variant["bidi"]:
                test_deps.append("//rb/lib/selenium/webdriver:bidi")

            test_tags = COMMON_TAGS + config["tags"] + tags + [browser]
            if variant["bidi"]:
                test_tags.append("bidi")

            rb_test(
                name = target_name,
                size = "large",
                srcs = srcs,
                args = ["rb/spec/integration"],
                data = test_data,
                env = env,
                main = "@bundle//bin:rspec",
                tags = test_tags,
                deps = depset(test_deps),
                visibility = ["//rb:__subpackages__"],
                target_compatible_with = config["target_compatible_with"],
            )

def rb_unit_test(name, srcs, deps, data = []):
    rb_test(
        name = name,
        size = "small",
        srcs = srcs,
        args = ["rb/spec/"],
        main = "@bundle//bin:rspec",
        data = data,
        tags = ["no-sandbox"],  # TODO: Do we need this?
        deps = ["//rb/spec/unit/selenium/webdriver:spec_helper"] + deps,
        visibility = ["//rb:__subpackages__"],
    )
