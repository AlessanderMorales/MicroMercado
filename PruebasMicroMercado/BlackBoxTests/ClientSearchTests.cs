using OpenQA.Selenium;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class ClientSearchTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public ClientSearchTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "Search Client - Exists")]
        public void SearchClient_Exists_ShouldPopulateClientName()
        {
            // Known existing client (seeded in app/test data)
            const string existingTax = "12345678";

            _page.GoTo("https://localhost:7040/Sales");

            // Use helper which inputs and clicks search; it waits until payment select is visible for success
            _page.SetClientTaxDocument(existingTax);

            // Read the nombreCliente input value to verify client was populated
            var nombreEl = _fixture.Driver.FindElement(By.Id("nombreCliente"));
            var nombre = nombreEl.GetAttribute("value") ?? string.Empty;

            Assert.False(string.IsNullOrEmpty(nombre));
        }

        [Fact(DisplayName = "Search Client - Not Exists")]
        public void SearchClient_NotExists_ShouldShowAlertAndNotPopulate()
        {
            // Random tax document unlikely to exist
            var rnd = new Random();
            var randomTax = (10000000 + rnd.Next(0, 89999999)).ToString();

            _page.GoTo("https://localhost:7040/Sales");

            // Set input and click search button directly (don't use SetClientTaxDocument because it waits for success)
            _page.SetInputValue("idDocumentoRecibido", randomTax);
            var searchBtn = _fixture.Driver.FindElement(By.Id("btnBuscarCliente"));
            searchBtn.Click();

            // Wait briefly to allow any UI feedback (toasts) or alerts
            System.Threading.Thread.Sleep(500);

            // Try to accept any alert if present
            try
            {
                var alert = _fixture.Driver.SwitchTo().Alert();
                var text = alert.Text ?? string.Empty;
                alert.Accept();
                Assert.Contains("Cliente no encontrado", text);
            }
            catch (OpenQA.Selenium.NoAlertPresentException)
            {
                // No native alert; fallback: verify the nombreCliente field remains empty
                var nombreEl = _fixture.Driver.FindElement(By.Id("nombreCliente"));
                var nombre = nombreEl.GetAttribute("value") ?? string.Empty;
                Assert.True(string.IsNullOrEmpty(nombre));
            }

            // Verify nombreCliente is empty in any case
            var finalNombreEl = _fixture.Driver.FindElement(By.Id("nombreCliente"));
            var finalNombre = finalNombreEl.GetAttribute("value") ?? string.Empty;
            Assert.True(string.IsNullOrEmpty(finalNombre));
        }
    }
}
