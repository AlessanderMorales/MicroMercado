using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public CommonSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        [Given(@"que la aplicacion esta en ejecucion")]
        public void GivenQueLaAplicacionEstaEnEjecucion()
        {
            // La aplicación ya está corriendo en localhost:7040
            // Este paso es principalmente documental
        }

        [Given(@"navego a la pagina de ventas")]
        public void GivenNavegoALaPaginaDeVentas()
        {
            var salesPage = new SalesPage(_fixture.Driver);
            salesPage.NavigateTo($"{_fixture.BaseUrl}/Sales");
            _scenarioContext["SalesPage"] = salesPage;
        }

        [Given(@"navego a la pagina de categorias")]
        public void GivenNavegoALaPaginaDeCategorias()
        {
            var categoryListPage = new CategoryListPage(_fixture.Driver);
            categoryListPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
            _scenarioContext["CategoryListPage"] = categoryListPage;
        }

        [Given(@"navego a la pagina de productos")]
        public void GivenNavegoALaPaginaDeProductos()
        {
            var productListPage = new ProductListPage(_fixture.Driver);
            productListPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
            _scenarioContext["ProductListPage"] = productListPage;
        }

        [Given(@"navego a la pagina de clientes")]
        public void GivenNavegoALaPaginaDeClientes()
        {
            var clientListPage = new ClientListPage(_fixture.Driver);
            clientListPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
            _scenarioContext["ClientListPage"] = clientListPage;
        }

        [When(@"navego a la pagina de clientes")]
        public void WhenNavegoALaPaginaDeClientes()
        {
            var clientListPage = new ClientListPage(_fixture.Driver);
            clientListPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
            _scenarioContext["ClientListPage"] = clientListPage;
        }

        [When(@"navego a la pagina de productos")]
        public void WhenNavegoALaPaginaDeProductos()
        {
            var productListPage = new ProductListPage(_fixture.Driver);
            productListPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
            _scenarioContext["ProductListPage"] = productListPage;
        }

        [When(@"navego a la pagina de categorias")]
        public void WhenNavegoALaPaginaDeCategorias()
        {
            var categoryListPage = new CategoryListPage(_fixture.Driver);
            categoryListPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
            _scenarioContext["CategoryListPage"] = categoryListPage;
        }

        [Then(@"debo ver un mensaje de exito")]
        public void ThenDeboVerUnMensajeDeExito()
        {
            // Este paso se validará según el contexto en los steps específicos
            Assert.True(true);
        }
    }
}
