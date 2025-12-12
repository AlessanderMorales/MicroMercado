using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Xunit;

namespace PruebasUIBased.Infrastructure
{
    /// <summary>
    /// Configuración de colección de pruebas para evitar ejecución paralela
    /// </summary>
    [CollectionDefinition("UITests", DisableParallelization = true)]
    public class UITestsCollection : ICollectionFixture<WebDriverFixture>
    {
    }

    /// <summary>
    /// Fixture para gestionar el ciclo de vida del WebDriver
    /// </summary>
    public class WebDriverFixture : IDisposable
    {
        public IWebDriver Driver { get; private set; }
        public string BaseUrl { get; private set; }

        public WebDriverFixture()
        {
            BaseUrl = "https://localhost:7040";

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");

            // Descomentar para modo headless
            // options.AddArgument("--headless");

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            catch
            {
                // Ignorar errores al cerrar el driver
            }
        }
    }
}
