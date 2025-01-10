using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

namespace OpenQA.Selenium
{
    // DELETE IT IN FINAL MERGE
    class _TempSharedDriverServiceTest
    {
        [Test]
        public void Normal()
        {
            var service = ChromeDriverService.CreateDefaultService();

            using var driver = new ChromeDriver(service);

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }

        [Test]
        public void Shared()
        {
            var service = ChromeDriverService.CreateDefaultService();

            using (var driver1 = new ChromeDriver(service))
            {
                driver1.Url = "https://google.com";
            }

            using (var driver2 = new ChromeDriver(service))
            {
                driver2.Url = "https://google.com";
            }

            Assert.That(Process.GetProcessesByName("chromedriver"), Is.Empty);
        }
    }
}
