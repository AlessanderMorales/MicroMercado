using OpenQA.Selenium;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class IndexPageTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public IndexPageTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "Index Page Loads Successfully")]
        public void IndexPage_Loads()
        {
            // Navigate to Index page
            _page.GoTo("https://localhost:7040/");

            // Verify welcome text exists
            string welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
            Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());
        }

        [Fact(DisplayName = "Navigate to Sales from Index")]
        public void NavigateToSales_FromIndex()
        {
            // Navigate to Index page
            _page.GoTo("https://localhost:7040/");

            // Click the "Ir al Punto de Venta" button
            _page.ClickButtonByText("Ir al Punto de Venta");

            // Wait for redirection to Sales page
            _page.WaitForUrlContains("/Sales");

            // Validate redirection
            string currentUrl = _fixture.Driver.Url;
            Assert.Contains("/Sales", currentUrl);
        }
    }
}
