from unittest.mock import Mock

import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.relative_locator import locate_with


class TestExpectedConditionsRelativeBy:
    """Test that expected conditions accept RelativeBy in type annotations"""

    def test_presence_of_element_located_accepts_relative_by(self):
        """Test presence_of_element_located accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "div").above({By.ID: "footer"})
        condition = EC.presence_of_element_located(relative_by)
        assert condition is not None

    def test_visibility_of_element_located_accepts_relative_by(self):
        """Test visibility_of_element_located accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "button").near({By.CLASS_NAME: "submit"})
        condition = EC.visibility_of_element_located(relative_by)
        assert condition is not None

    def test_presence_of_all_elements_located_accepts_relative_by(self):
        """Test presence_of_all_elements_located accepts RelativeBy"""
        relative_by = locate_with(By.CSS_SELECTOR, ".item").below({By.ID: "header"})
        condition = EC.presence_of_all_elements_located(relative_by)
        assert condition is not None

    def test_visibility_of_any_elements_located_accepts_relative_by(self):
        """Test visibility_of_any_elements_located accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "span").to_left_of({By.ID: "sidebar"})
        condition = EC.visibility_of_any_elements_located(relative_by)
        assert condition is not None

    def test_text_to_be_present_in_element_accepts_relative_by(self):
        """Test text_to_be_present_in_element accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "p").above({By.CLASS_NAME: "footer"})
        condition = EC.text_to_be_present_in_element(relative_by, "Hello")
        assert condition is not None

    def test_element_to_be_clickable_accepts_relative_by(self):
        """Test element_to_be_clickable accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "button").near({By.ID: "form"})
        condition = EC.element_to_be_clickable(relative_by)
        assert condition is not None

    def test_invisibility_of_element_located_accepts_relative_by(self):
        """Test invisibility_of_element_located accepts RelativeBy"""
        relative_by = locate_with(By.CSS_SELECTOR, ".loading").above({By.ID: "content"})
        condition = EC.invisibility_of_element_located(relative_by)
        assert condition is not None

    def test_element_located_to_be_selected_accepts_relative_by(self):
        """Test element_located_to_be_selected accepts RelativeBy"""
        relative_by = locate_with(By.TAG_NAME, "input").near({By.ID: "terms-label"})
        condition = EC.element_located_to_be_selected(relative_by)
        assert condition is not None
