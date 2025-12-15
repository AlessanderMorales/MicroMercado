using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;
using System;
using System.Globalization;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    [Scope(Tag = "sales")]
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

        // ==========================================
        // BACKGROUND & GIVEN 
        // ==========================================

        [Given(@"que la aplicacion esta en ejecucion")]
        public void GivenQueLaAplicacionEstaEnEjecucion()
        {
        }

        [Given(@"navego a la pagina de ventas")]
        public void GivenNavegoALaPaginaDeVentas()
        {
            SalesPage.NavigateTo($"{_fixture.BaseUrl}/Sales"); 
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "INICIO_VENTAS");
        }

        [Given(@"que existe un cliente con documento ""(.*)""")]
        public void GivenQueExisteUnClienteConDocumento(string documento)
        {
            _scenarioContext["ClientDocument"] = documento;
        }

        // ==========================================
        // WHEN
        // ==========================================

        [When(@"agrego el producto ""(.*)"" con cantidad (.*) al carrito")]
        public void WhenAgregoElProductoConCantidadAlCarrito(string productName, int quantity)
        {
            SalesPage.SearchAndAddProduct(productName);
            if (quantity > 1)
            {
                SalesPage.SetProductQuantity(productName, quantity);
            }

            string prodNameShort = productName.Length > 10 ? productName.Substring(0, 10) : productName;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, $"PROD_AGREGADO_{prodNameShort}");
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
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, title, "DATOS_PAGO_LISTOS");
        }

        [When(@"confirmo la venta")]
        public void WhenConfirmoLaVenta()
        {
            string title = _scenarioContext.ScenarioInfo.Title;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, title, "PREVIO_CONFIRMAR");

            SalesPage.ConfirmSale();
            SalesPage.WaitForCartToEmpty();
        }

        // ==========================================
        // THEN
        // ==========================================

        [Then(@"la venta debe procesarse exitosamente")]
        public void ThenLaVentaDebeProcesarseExitosamente()
        {
            System.Threading.Thread.Sleep(2000);

            bool successMsg = SalesPage.HasSuccessMessage();
            bool cartEmpty = SalesPage.GetCartItemCount() == 0;
            bool hasError = SalesPage.HasErrorMessage();

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_VENTA");

            Assert.True(successMsg || cartEmpty, $"La venta fallo: cartEmpty={cartEmpty}, successMsg={successMsg}");
            Assert.False(hasError, "Se encontro un mensaje de error en la pantalla.");
        }

        [Then(@"el carrito debe estar vacio")]
        public void ThenElCarritoDebeEstarVacio()
        {
            Assert.Equal(0, SalesPage.GetCartItemCount());
        }

        [Then(@"debo ver un mensaje de exito")]
        public void ThenDeboVerUnMensajeDeExito()
        {
            ThenLaVentaDebeProcesarseExitosamente();
        }
    }
}