using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de crear/editar Cliente
    /// </summary>
    public class ClientFormPage : BasePage
    {
        // Locators - Create
        private readonly By _createBusinessNameInput = By.Id("NewClient_BusinessName");
        private readonly By _createEmailInput = By.Id("NewClient_Email");
        private readonly By _createTaxDocumentInput = By.Id("NewClient_TaxDocument");
        private readonly By _createAddressInput = By.Id("NewClient_Address");

        // Locators - Update (EditClient)
        private readonly By _updateBusinessNameInput = By.Id("EditClient_BusinessName");
        private readonly By _updateEmailInput = By.Id("EditClient_Email");
        private readonly By _updateTaxDocumentInput = By.Id("EditClient_TaxDocument");
        private readonly By _updateAddressInput = By.Id("EditClient_Address");

        // Common
        private readonly By _saveButton = By.CssSelector("button[type='submit']");
        private readonly By _cancelButton = By.CssSelector("a[href*='Client']");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public ClientFormPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Llena el formulario de cliente (para crear)
        /// </summary>
        public void FillClientForm(string businessName, string email, string taxDocument, string address)
        {
            TypeText(_createBusinessNameInput, businessName);
            TypeText(_createEmailInput, email);
            TypeText(_createTaxDocumentInput, taxDocument);
            TypeText(_createAddressInput, address);
        }

        /// <summary>
        /// Llena el formulario de cliente (para actualizar)
        /// </summary>
        public void UpdateClientForm(string businessName, string email, string taxDocument, string address)
        {
            System.Threading.Thread.Sleep(2000);
            
            var js = (IJavaScriptExecutor)Driver;
            
            try
            {
                var nameInput = Driver.FindElement(_updateBusinessNameInput);
                var emailInput = Driver.FindElement(_updateEmailInput);
                var taxDocInput = Driver.FindElement(_updateTaxDocumentInput);
                var addressInput = Driver.FindElement(_updateAddressInput);
                
                js.ExecuteScript("arguments[0].value = arguments[1];", nameInput, businessName);
                js.ExecuteScript("arguments[0].value = arguments[1];", emailInput, email);
                js.ExecuteScript("arguments[0].value = arguments[1];", taxDocInput, taxDocument);
                js.ExecuteScript("arguments[0].value = arguments[1];", addressInput, address);
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception($"No se encontraron los campos de actualización de cliente. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Hace clic en el botón de guardar
        /// </summary>
        public void ClickSave()
        {
            ClickElement(_saveButton);
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Hace clic en el botón de cancelar
        /// </summary>
        public void ClickCancel()
        {
            ClickElement(_cancelButton);
        }

        /// <summary>
        /// Verifica si hay un mensaje de éxito
        /// </summary>
        public bool HasSuccessMessage()
        {
            return IsElementVisible(_successAlert);
        }

        /// <summary>
        /// Verifica si hay un mensaje de error
        /// </summary>
        public bool HasErrorMessage()
        {
            System.Threading.Thread.Sleep(500); // Esperar a que se muestre el error
            
            // Buscar alertas generales
            if (IsElementVisible(_errorAlert))
                return true;
            
            // Buscar spans de validación específicos
            var validationSpans = Driver.FindElements(
                By.CssSelector("span.text-danger, [class*='validation'], [data-valmsg-for]"));
            
            return validationSpans.Any(s => 
                s.Displayed && !string.IsNullOrWhiteSpace(s.Text));
        }
    }
}
