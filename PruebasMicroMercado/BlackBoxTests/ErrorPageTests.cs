using Xunit;
using OpenQA.Selenium;
using System.Threading;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class ErrorPageTests
    {
        private readonly WebDriverFixture _fixture;

        public ErrorPageTests(WebDriverFixture fixture) => _fixture = fixture;

        [Fact]
        public void ErrorPage_ShowsRequestIdAndMessage()
        {
            var driver = _fixture.Driver;
            driver.Navigate().GoToUrl("https://localhost:7040/Error");
            Thread.Sleep(500);

            Assert.Contains("Error", driver.PageSource);
            Assert.Contains("Request ID", driver.PageSource);
        }
    }
}
