// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

use std::{ffi::{c_char, c_int, CStr, CString}, ptr::null_mut};

// this is callback function to be called each time when rust wants to send log data
type LogCallback = extern "C" fn(level: c_int, message: *const std::os::raw::c_char);

#[repr(C)]
pub struct WebDriverPathResult {
    success: bool,
    driver_path: *mut c_char,
    error: *mut c_char,
}

// this is just an example how to expose function for external usage
#[no_mangle]
pub extern "C" fn get_dummy_webdriver_path(driver_name: *const c_char, log: LogCallback) -> WebDriverPathResult {
    let result = std::panic::catch_unwind(|| {
        
        for i in 1..6 {
            let message = CString::new("Hello, I am logging message").unwrap();
            //let message = CString::new(String::from("A").repeat(10_000_000)).unwrap();
            log(i, message.as_ptr());
        }

        //panic!("Intentional panic for testing");

        let driver = unsafe { CStr::from_ptr(driver_name).to_str().unwrap() };

        return CString::new("This is dummy driver path for ".to_owned() + driver).unwrap().into_raw();
        //return CString::new(String::from("A").repeat(10_000_000)).unwrap().into_raw();
    });
    
    match result {
        Ok(driver_path) => WebDriverPathResult {
            success: true,
            driver_path,
            error: null_mut(),
        },
        Err(panic) => WebDriverPathResult {
            success: false,
            driver_path: null_mut(),
            error: CString::new(extract_panic_message(panic)).unwrap().into_raw(),
        }
    }
}

#[no_mangle]
pub extern "C" fn free_webdriver_path_result(result: *mut WebDriverPathResult) {
    if result.is_null() {
        return;
    }
    unsafe {
        let ffi_result = &mut *result;
        if !ffi_result.driver_path.is_null() {
            // Reconstruct CString to drop it and free memory
            let _ = CString::from_raw(ffi_result.driver_path);
        }
        if !ffi_result.error.is_null() {
            // Reconstruct CString to drop it and free memory
            let _ = CString::from_raw(ffi_result.error);
        }
    }
}

/// Extract panic message from `Box<dyn Any>`
fn extract_panic_message(panic: Box<dyn std::any::Any + Send>) -> String {
    // Try to downcast to common panic types
    if let Some(s) = panic.downcast_ref::<String>() {
        s.clone()
    } else if let Some(s) = panic.downcast_ref::<&str>() {
        s.to_string()
    } else {
        "Unknown panic (non-string payload)".to_string()
    }
}