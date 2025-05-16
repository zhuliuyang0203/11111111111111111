def test_reusing_closed_stdout_fails():
    import sys
    from selenium.webdriver.chrome.service import Service
    from selenium.common.exceptions import WebDriverException

    service = Service(log_output=sys.stdout)
    service._log_output.close()
    with pytest.raises(ValueError):
        Service(log_output=sys.stdout)
