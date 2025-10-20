using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class ClientSearchTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;
        private readonly WebDriverWait _wait;

        public ClientSearchTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
            _wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));
        }

        [Fact(DisplayName = "Search Client - Exists")]
        public void SearchClient_Exists_ShouldPopulateClientName()
        {
            const string existingTax = "9404687";
            const string expectedClientName = "Alessander";

            _page.GoTo("https://localhost:7040/Sales");
            _page.SetClientTaxDocument(existingTax);
            _wait.Until(d =>
            {
                var nombreEl = d.FindElement(By.Id("nombreCliente"));
                var actualName = nombreEl.GetAttribute("value") ?? string.Empty;
                return actualName.Equals(expectedClientName, StringComparison.OrdinalIgnoreCase);
            });
            var nombreElFinal = _fixture.Driver.FindElement(By.Id("nombreCliente"));
            var nombre = nombreElFinal.GetAttribute("value") ?? string.Empty;
            Assert.Equal(expectedClientName, nombre, StringComparer.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Search Client - Not Exists")]
        public void SearchClient_NotExists_ShouldShowAlertAndNotPopulate()
        {
            var rnd = new Random();
            var randomTax = (10000000 + rnd.Next(0, 89999999)).ToString();

            _page.GoTo("https://localhost:7040/Sales");
            _page.SetInputValue("idDocumentoRecibido", randomTax);
            var searchBtn = _fixture.Driver.FindElement(By.Id("btnBuscarCliente"));
            searchBtn.Click();
            System.Threading.Thread.Sleep(500);
            try
            {
                var alert = _fixture.Driver.SwitchTo().Alert();
                var text = alert.Text ?? string.Empty;
                alert.Accept();
                Assert.Contains("Cliente no encontrado", text);
            }
            catch (OpenQA.Selenium.NoAlertPresentException)
            {
                var nombreEl = _fixture.Driver.FindElement(By.Id("nombreCliente"));
                var nombre = nombreEl.GetAttribute("value") ?? string.Empty;
                Assert.True(string.IsNullOrEmpty(nombre));
            }
            var finalNombreEl = _fixture.Driver.FindElement(By.Id("nombreCliente"));
            var finalNombre = finalNombreEl.GetAttribute("value") ?? string.Empty;
            Assert.True(string.IsNullOrEmpty(finalNombre));
        }
    }
}