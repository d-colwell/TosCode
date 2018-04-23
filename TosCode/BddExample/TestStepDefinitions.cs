using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BddExample
{
    public class TestStepDefinitions
    {
        IWebDriver driver;
        public TestStepDefinitions()
        {
            string chromeDriverPath = @"C:\Temp\ChromeDriver";
            ChromeOptions options = new ChromeOptions();
            string localPath = Environment.ExpandEnvironmentVariables("%APPDATA%");
            options.AddArguments($@"user-data-dir={localPath}\Google\Chrome\User Data\Selenium");
            driver = new ChromeDriver(chromeDriverPath, options);
        }

        public void OpenSampleApp(string url)
        {
            driver.Url = url;
            driver.Navigate();
        }
    }
}
