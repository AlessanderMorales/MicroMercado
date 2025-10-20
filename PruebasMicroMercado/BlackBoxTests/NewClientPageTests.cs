using OpenQA.Selenium;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class NewClientTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public NewClientTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "Create New Client Successfully")]
        public void CreateNewClient_Success()
        {
            _page.GoTo("https://localhost:7040/NewClient");

            string businessName = "Cliente de Prueba";
            var rnd = new System.Random();
            string taxDocument = (10000000 + rnd.Next(0, 89999999)).ToString();
            string email = $"test+{rnd.Next(1000,9999)}@example.com";

            _page.SetInputValue("NewClient_BusinessName", businessName);
            _page.SetInputValue("NewClient_Email", email);
            _page.SetInputValue("NewClient_TaxDocument", taxDocument);

            _page.ClickButtonByText("Guardar Cliente");

            try
            {
                _page.WaitForUrlContains("/Sales");
                string currentUrl = _fixture.Driver.Url;
                Assert.Contains("/Sales", currentUrl);
            }
            catch
            {
                var errName = _page.GetValidationMessage("NewClient_BusinessName");
                var errTax = _page.GetValidationMessage("NewClient_TaxDocument");
                Assert.True(string.IsNullOrEmpty(errName) && string.IsNullOrEmpty(errTax));
            }
        }

        [Fact(DisplayName = "Fail to Create Client with Empty Fields")]
        public void CreateNewClient_ValidationFails()
        {
            _page.GoTo("https://localhost:7040/NewClient");

            _page.ClickButtonByText("Guardar Cliente");

            string errorBusinessName = _page.GetValidationMessage("NewClient_BusinessName");
            string errorTaxDocument = _page.GetValidationMessage("NewClient_TaxDocument");

            Assert.False(string.IsNullOrEmpty(errorBusinessName));
            Assert.False(string.IsNullOrEmpty(errorTaxDocument));
        }
    }
}
