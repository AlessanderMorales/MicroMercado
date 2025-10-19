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
            string taxDocument = "987654321";

            _page.SetInputValue("NewClient_BusinessName", businessName);
            _page.SetInputValue("NewClient_TaxDocument", taxDocument);

            // Submit form
            _page.ClickButtonByText("Guardar Cliente");

            // Wait for redirection to Sales page
            _page.WaitForUrlContains("/Sales");

            // Validate that we are on the Sales page
            string currentUrl = _fixture.Driver.Url;
            Assert.Contains("/Sales", currentUrl);
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
