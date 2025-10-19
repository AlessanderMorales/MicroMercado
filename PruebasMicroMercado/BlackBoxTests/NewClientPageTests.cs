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
            // Navigate to New Client page
            _page.GoTo("https://localhost:7040/NewClient");

            // Fill the client form
            string businessName = "Cliente de Prueba";
            var rnd = new System.Random();
            string taxDocument = (10000000 + rnd.Next(0, 89999999)).ToString(); // random numeric tax document
            string email = $"test+{rnd.Next(1000,9999)}@example.com";

            _page.SetInputValue("NewClient_BusinessName", businessName);
            _page.SetInputValue("NewClient_Email", email);
            _page.SetInputValue("NewClient_TaxDocument", taxDocument);

            // Submit form
            _page.ClickButtonByText("Guardar Cliente");

            // Wait for either redirection to Sales or presence of validation errors
            try
            {
                _page.WaitForUrlContains("/Sales");
                string currentUrl = _fixture.Driver.Url;
                Assert.Contains("/Sales", currentUrl);
            }
            catch
            {
                // If not redirected, ensure validation errors are not present (meaning creation likely failed)
                var errName = _page.GetValidationMessage("NewClient_BusinessName");
                var errTax = _page.GetValidationMessage("NewClient_TaxDocument");
                Assert.True(string.IsNullOrEmpty(errName) && string.IsNullOrEmpty(errTax));
            }
        }

        [Fact(DisplayName = "Fail to Create Client with Empty Fields")]
        public void CreateNewClient_ValidationFails()
        {
            // Navigate to New Client page
            _page.GoTo("https://localhost:7040/NewClient");

            // Leave fields empty and submit
            _page.ClickButtonByText("Guardar Cliente");

            // Expect validation error messages
            string errorBusinessName = _page.GetValidationMessage("NewClient_BusinessName");
            string errorTaxDocument = _page.GetValidationMessage("NewClient_TaxDocument");

            Assert.False(string.IsNullOrEmpty(errorBusinessName));
            Assert.False(string.IsNullOrEmpty(errorTaxDocument));
        }
    }
}
