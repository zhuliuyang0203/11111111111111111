using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

namespace OpenQA.Selenium
{
    // DELETE IT IN FINAL MERGE
    [NonParallelizable]
    class _TempSharedDriverServiceTest
    {
        [Test]
        public void Implicitly()
        {
            using (var driver = new ChromeDriver())
            {

            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void Normal()
        {
            using (var service = ChromeDriverService.CreateDefaultService())
            {
                using var driver = new ChromeDriver(service);
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void Shared()
        {
            using (var service = ChromeDriverService.CreateDefaultService())
            {
                using (var driver1 = new ChromeDriver(service))
                {
                    driver1.Url = "https://google.com";
                }

                using (var driver2 = new ChromeDriver(service))
                {
                    driver2.Url = "https://google.com";
                }
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }
    }
}
