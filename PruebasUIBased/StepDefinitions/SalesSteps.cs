using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll; 
using Xunit;
using System;
using System.Globalization;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class SalesSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;
        private SalesPage _salesPage;

        public SalesSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private SalesPage SalesPage => _salesPage ??= new SalesPage(_fixture.Driver);

        [Given(@"que existe un cliente con documento ""(.*)""")]
        public void GivenQueExisteUnClienteConDocumento(string documento)
        {
            _scenarioContext["ClientDocument"] = documento;
        }

        [When(@"agrego el producto ""(.*)"" con cantidad (.*) al carrito")]
        public void WhenAgregoElProductoConCantidadAlCarrito(string productName, int quantity)
        {
            SalesPage.SearchAndAddProduct(productName);
            if (quantity > 1)
            {
                SalesPage.SetProductQuantity(productName, quantity);
            }
        }

        [When(@"busco el cliente con documento ""(.*)""")]
        public void WhenBuscoElClienteConDocumento(string documento)
        {
            SalesPage.SearchClient(documento);
        }

        [When(@"selecciono tipo de pago ""(.*)""")]
        public void WhenSeleccionoTipoDePago(string paymentType)
        {
            SalesPage.SelectPaymentType(paymentType);
        }

        [When(@"ingreso efectivo recibido ""(.*)""")]
        public void WhenIngresoEfectivoRecibido(string amount)
        {
            SalesPage.EnterCashReceived(decimal.Parse(amount, CultureInfo.InvariantCulture));
            string title = _scenarioContext.ScenarioInfo.Title;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, title, "DATOS_LISTOS");
        }

        [When(@"confirmo la venta")]
        public void WhenConfirmoLaVenta()
        {

            string title = _scenarioContext.ScenarioInfo.Title;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, title, "DATOS_LISTOS");
            SalesPage.ConfirmSale();
            SalesPage.WaitForCartToEmpty();
        }

        [Then(@"la venta debe procesarse exitosamente")]
        public void ThenLaVentaDebeProcesarseExitosamente()
        {
            System.Threading.Thread.Sleep(2000);
            
            bool successMsg = SalesPage.HasSuccessMessage();
            bool cartEmpty = SalesPage.GetCartItemCount() == 0;
            bool hasError = SalesPage.HasErrorMessage();

            Assert.True(successMsg || cartEmpty, $"La venta fallo: cartEmpty={cartEmpty}, successMsg={successMsg}");
            Assert.False(hasError, "Se encontro un mensaje de error en la pantalla.");
        }

        [Then(@"el carrito debe estar vacio")]
        public void ThenElCarritoDebeEstarVacio()
        {
            Assert.Equal(0, SalesPage.GetCartItemCount());
        }
    }
}