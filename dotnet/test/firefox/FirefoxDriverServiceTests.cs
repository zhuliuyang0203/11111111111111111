using NUnit.Framework;
using OpenQA.Selenium.Firefox;
using System;
using System.IO;

namespace OpenQA.Selenium.Firefox
{
    [TestFixture]
    public class FirefoxDriverServiceTests
    {
        private string tempFileName;

        [SetUp]
        public void Setup()
        {
            tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }

        [Test]
        public void CanSetLogPath()
        {
            var service = FirefoxDriverService.CreateDefaultService();
            service.LogPath = tempFileName;

            Assert.That(service.LogPath, Is.EqualTo(tempFileName), "LogPath should be set correctly");
            
            string commandLineArgs = service.CommandLineArguments;
            Assert.That(commandLineArgs, Contains.Substring($"--log \"{tempFileName}\""), 
                "Command line arguments should contain the log path");
        }

        [Test]
        public void LogFileIsCreatedWhenDriverStarts()
        {
            var service = FirefoxDriverService.CreateDefaultService();
            service.LogPath = tempFileName;

            service.Start();

            try
            {
                Assert.That(File.Exists(tempFileName), Is.True, "Log file should be created when service starts");
                
                string logContent = File.ReadAllText(tempFileName);
                Assert.That(logContent, Is.Not.Empty, "Log file should contain content");
            }
            finally
            {
                service.Dispose();
            }
        }
    }
}
