using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class SalesSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public SalesSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private SalesPage GetSalesPage()
        {
            if (!_scenarioContext.ContainsKey("SalesPage"))
            {
                var salesPage = new SalesPage(_fixture.Driver);
                _scenarioContext["SalesPage"] = salesPage;
            }
            return (SalesPage)_scenarioContext["SalesPage"];
        }

        [Given(@"que existe un cliente con documento ""(.*)""")]
        public void GivenQueExisteUnClienteConDocumento(string documento)
        {
            // Este paso es principalmente documental
            // Asumimos que el cliente ya existe en la base de datos de pruebas
            _scenarioContext["ClientDocument"] = documento;
        }

        [When(@"agrego el producto ""(.*)"" con cantidad (.*) al carrito")]
        public void WhenAgregoElProductoConCantidadAlCarrito(string productName, int quantity)
        {
            var salesPage = GetSalesPage();
            salesPage.SearchAndAddProduct(productName);
            
            if (quantity > 1)
            {
                salesPage.SetProductQuantity(productName, quantity);
            }

            System.Threading.Thread.Sleep(500);
        }

        [When(@"busco el cliente con documento ""(.*)""")]
        public void WhenBuscoElClienteConDocumento(string documento)
        {
            var salesPage = GetSalesPage();
            salesPage.SearchClient(documento);
        }

        [When(@"selecciono tipo de pago ""(.*)""")]
        public void WhenSeleccionoTipoDePago(string paymentType)
        {
            var salesPage = GetSalesPage();
            salesPage.SelectPaymentType(paymentType);
        }

        [When(@"ingreso efectivo recibido ""(.*)""")]
        public void WhenIngresoEfectivoRecibido(string amount)
        {
            var salesPage = GetSalesPage();
            salesPage.EnterCashReceived(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        }

        [When(@"confirmo la venta")]
        public void WhenConfirmoLaVenta()
        {
            var salesPage = GetSalesPage();
            salesPage.ConfirmSale();
        }

        [Then(@"la venta debe procesarse exitosamente")]
        public void ThenLaVentaDebeProcesarseExitosamente()
        {
            var salesPage = GetSalesPage();
            
            // Esperar a que la página se redirija o muestre confirmación
            System.Threading.Thread.Sleep(2000);
            
            // Verificar que no haya mensajes de error
            Assert.False(salesPage.HasErrorMessage(), "No debe haber mensajes de error");
        }

        [Then(@"el carrito debe estar vacio")]
        public void ThenElCarritoDebeEstarVacio()
        {
            var salesPage = GetSalesPage();
            
            // Esperar más tiempo después de la venta para que la página se recargue
            System.Threading.Thread.Sleep(3000);
            
            // Verificar si necesitamos navegar de vuelta a la página de ventas
            var currentUrl = salesPage.GetCurrentUrl();
            if (!currentUrl.Contains("/Sales"))
            {
                salesPage.NavigateTo($"{_fixture.BaseUrl}/Sales");
                System.Threading.Thread.Sleep(2000);
            }

            var itemCount = salesPage.GetCartItemCount();
            Assert.True(itemCount == 0, $"El carrito debería estar vacío pero tiene {itemCount} items");
        }
    }
}
