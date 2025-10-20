using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [CollectionDefinition("SeleniumTests")]
    public class SeleniumTestsCollection : ICollectionFixture<WebDriverFixture>
    {
        // This class is just a collection definition for xUnit
    }

    public class WebDriverFixture : IDisposable
    {
        public IWebDriver Driver { get; private set; }

        public WebDriverFixture()
        {
            var options = new ChromeOptions();

            // Start browser maximized
            options.AddArgument("--start-maximized");

            // Disable annoying notifications and infobars
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");

            // Remove headless to see the browser by default
            // options.AddArgument("--headless"); // Commented out for visible browser

            // Initialize Chrome driver
            Driver = new ChromeDriver(options);

            // Optional: implicit wait for elements (helps with slow page load)
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            try
            {
                Driver.Quit();
                Driver.Dispose();
            }
            catch
            {
                // Ignore exceptions on dispose
            }
        }
    }
}
