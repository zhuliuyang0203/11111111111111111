import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.relative_locator import RelativeBy, locate_with
from selenium.webdriver.remote.webdriver import WebDriver
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.remote.shadowroot import ShadowRoot
from unittest.mock import Mock, MagicMock


class TestRelativeByAnnotations:
    """Test that RelativeBy is properly accepted in type annotations"""
    
    def test_webdriver_find_element_accepts_relative_by(self):
        """Test WebDriver.find_element accepts RelativeBy"""
        driver = Mock(spec=WebDriver)
        relative_by = locate_with(By.TAG_NAME, "div").above({By.ID: "footer"})
        
        # This should not raise type checking errors
        driver.find_element(by=relative_by)
        driver.find_element(relative_by)
    
    def test_webdriver_find_elements_accepts_relative_by(self):
        """Test WebDriver.find_elements accepts RelativeBy"""
        driver = Mock(spec=WebDriver)
        relative_by = locate_with(By.TAG_NAME, "div").below({By.ID: "header"})
        
        # This should not raise type checking errors
        driver.find_elements(by=relative_by)
        driver.find_elements(relative_by)
    
    def test_webelement_find_element_accepts_relative_by(self):
        """Test WebElement.find_element accepts RelativeBy"""
        element = Mock(spec=WebElement)
        relative_by = locate_with(By.TAG_NAME, "span").near({By.CLASS_NAME: "button"})
        
        # This should not raise type checking errors
        element.find_element(by=relative_by)
        element.find_element(relative_by)
    
    def test_webelement_find_elements_accepts_relative_by(self):
        """Test WebElement.find_elements accepts RelativeBy"""
        element = Mock(spec=WebElement)
        relative_by = locate_with(By.TAG_NAME, "input").to_left_of({By.ID: "submit"})
        
        # This should not raise type checking errors
        element.find_elements(by=relative_by)
        element.find_elements(relative_by)
    
    def test_shadowroot_find_element_accepts_relative_by(self):
        """Test ShadowRoot.find_element accepts RelativeBy"""
        shadow_root = Mock(spec=ShadowRoot)
        relative_by = locate_with(By.TAG_NAME, "button").to_right_of({By.ID: "cancel"})
        
        # This should not raise type checking errors
        shadow_root.find_element(by=relative_by)
        shadow_root.find_element(relative_by)
    
    def test_shadowroot_find_elements_accepts_relative_by(self):
        """Test ShadowRoot.find_elements accepts RelativeBy"""
        shadow_root = Mock(spec=ShadowRoot)
        relative_by = locate_with(By.CSS_SELECTOR, ".item").above({By.ID: "footer"})
        
        # This should not raise type checking errors
        shadow_root.find_elements(by=relative_by)
        shadow_root.find_elements(relative_by)