using Xunit;
using OpenQA.Selenium;
using System.Threading;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class PrivacyPageTests
    {
        private readonly WebDriverFixture _fixture;

        public PrivacyPageTests(WebDriverFixture fixture) => _fixture = fixture;

        [Fact]
        public void PrivacyPage_Loads()
        {
            var driver = _fixture.Driver;
            driver.Navigate().GoToUrl("https://localhost:7040/Privacy");
            Thread.Sleep(500);

            Assert.Contains("Sobre Nosotros", driver.PageSource);
        }
    }
}
