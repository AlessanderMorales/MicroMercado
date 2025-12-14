using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace PruebasUIBased.PageObjects
{
    public class ClientFormPage : BasePage
    {
        private readonly WebDriverWait _wait;

        // ==========================================
        // SELECTORES PARA CREACIÓN (Model: NewClient)
        // ==========================================
        private readonly By _newName = By.Id("NewClient_BusinessName");
        private readonly By _newEmail = By.Id("NewClient_Email");
        private readonly By _newAddress = By.Id("NewClient_Address");
        private readonly By _newDoc = By.Id("NewClient_TaxDocument");

        // ==========================================
        // SELECTORES PARA EDICIÓN (Model: EditClient)
        // ==========================================
        private readonly By _editName = By.Id("EditClient_BusinessName");
        private readonly By _editEmail = By.Id("EditClient_Email");
        private readonly By _editAddress = By.Id("EditClient_Address");
        private readonly By _editDoc = By.Id("EditClient_TaxDocument");

        private readonly By _saveButton = By.CssSelector("button[type='submit']");

        private readonly By _validationSummary = By.CssSelector(".text-danger ul li, .validation-summary-errors");

        public ClientFormPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Llena el formulario de CREACIÓN usando IDs de NewClient
        /// </summary>
        public void FillClientForm(string nombre, string email, string documento, string direccion)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(_newName));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("Timeout: No se cargó el formulario de Nuevo Cliente (Buscando #NewClient_BusinessName).");
            }

            EnterText(_newName, nombre);
            EnterText(_newEmail, email);
            EnterText(_newDoc, documento);

            if (!string.IsNullOrEmpty(direccion))
            {
                EnterText(_newAddress, direccion);
            }
        }

        /// <summary>
        /// Llena el formulario de EDICIÓN usando IDs de EditClient
        /// </summary>
        public void UpdateClientForm(string nombre, string email, string documento, string direccion)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(_editName));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("Timeout: No se cargó el formulario de Editar Cliente (Buscando #EditClient_BusinessName). Verifique si el enlace en la lista apunta a /EditClient.");
            }

            // Usamos los selectores _edit...
            if (!string.IsNullOrEmpty(nombre)) EnterText(_editName, nombre);
            if (!string.IsNullOrEmpty(email)) EnterText(_editEmail, email);
            if (!string.IsNullOrEmpty(direccion)) EnterText(_editAddress, direccion);

            if (!string.IsNullOrEmpty(documento))
            {
                try
                {
                    EnterText(_editDoc, documento);
                }
                catch {  }
            }
        }


        public void ClickSave()
        {
            var btn = Driver.FindElement(By.XPath("//button[@type='submit' or contains(text(), 'Guardar') or contains(text(), 'Save')]"));
            ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", btn);
            System.Threading.Thread.Sleep(1000);
        }

        public bool HasErrorMessage()
        {
            try
            {
                return _wait.Until(ExpectedConditions.ElementIsVisible(_validationSummary)).Displayed;
            }
            catch
            {
                var fieldErrors = Driver.FindElements(By.CssSelector(".field-validation-error, .text-danger"));
                foreach (var err in fieldErrors)
                {
                    if (err.Displayed && !string.IsNullOrWhiteSpace(err.Text) && err.Text != "*") return true;
                }
                return false;
            }
        }

        private void EnterText(By locator, string text)
        {
            var element = Driver.FindElement(locator);
            element.Clear();
            element.SendKeys(text);
        }
    }
}