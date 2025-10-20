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
            _page.GoTo("https://localhost:7040/");

            string welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
            Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());
        }

        [Fact(DisplayName = "Navigate to Sales from Index")]
        public void NavigateToSales_FromIndex()
        {
            _page.GoTo("https://localhost:7040/");

            _page.ClickButtonByText("Ir al Punto de Venta");

            _page.WaitForUrlContains("/Sales");

            string currentUrl = _fixture.Driver.Url;
            Assert.Contains("/Sales", currentUrl);
        }
    }
}
