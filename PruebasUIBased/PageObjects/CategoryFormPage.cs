using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Necesario para ExpectedConditions
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    public class CategoryFormPage : BasePage
    {
        private readonly WebDriverWait _wait;

        private readonly By _newName = By.Id("NewCategory_Name");
        private readonly By _newDesc = By.Id("NewCategory_Description");

        private readonly By _updateName = By.Id("UpdateCategory_Name");
        private readonly By _updateDesc = By.Id("UpdateCategory_Description");
        private readonly By _updateStatus = By.Id("UpdateCategory_Status");

        private readonly By _saveButton = By.CssSelector("button[type='submit']");
        private readonly By _validationSummary = By.CssSelector(".text-danger ul li, .validation-summary-errors");
        private readonly By _fieldErrors = By.CssSelector(".field-validation-error, .text-danger");

        public CategoryFormPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void FillCategoryForm(string name, string description)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(_newName));
            }
            catch (WebDriverTimeoutException)
            {
                try
                {
                    var fallback = Driver.FindElement(By.Id("CreateCategory_Name"));
                    fallback.Clear();
                    fallback.SendKeys(name);
                    Driver.FindElement(By.Id("CreateCategory_Description")).SendKeys(description);
                    return;
                }
                catch { throw new Exception("No cargó el formulario de Nueva Categoría (Buscando #NewCategory_Name)."); }
            }

            EnterText(_newName, name);
            EnterText(_newDesc, description);
        }

        public void UpdateCategoryForm(string name, string description)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(_updateName));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception($"No se cargó el formulario de Editar Categoría (Buscando #UpdateCategory_Name). URL actual: {Driver.Url}");
            }

            if (!string.IsNullOrEmpty(name)) EnterText(_updateName, name);
            if (!string.IsNullOrEmpty(description)) EnterText(_updateDesc, description);
        }

        public void ClickSave()
        {

            var btn = Driver.FindElement(By.XPath("//button[@type='submit' or contains(text(), 'Guardar') or contains(text(), 'Save')]"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", btn);
            System.Threading.Thread.Sleep(1000);
        }

        public bool HasErrorMessage()
        {
            System.Threading.Thread.Sleep(500);
            
            // Verificar mensaje de error en alerta
            try
            {
                var alertDanger = Driver.FindElements(By.CssSelector(".alert-danger"));
                if (alertDanger.Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)))
                    return true;
            }
            catch { }
            
            // Verificar validation summary
            try
            {
                var summaryElements = Driver.FindElements(_validationSummary);
                if (summaryElements.Any(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text)))
                    return true;
            }
            catch { }

            // Verificar errores de campo
            var errors = Driver.FindElements(_fieldErrors);
            if (errors.Any(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text) && e.Text != "*"))
                return true;
            
            // Verificar si seguimos en el formulario de creacion (no hubo redireccion)
            string currentUrl = Driver.Url;
            if (currentUrl.Contains("NewCategory") || currentUrl.Contains("Create"))
            {
                // Estamos todavia en el formulario, puede ser un error
                var nameField = Driver.FindElements(_newName);
                if (nameField.Any(f => f.Displayed))
                {
                    // El formulario sigue visible, probablemente hubo error
                    return true;
                }
            }
            
            return false;
        }

        private void EnterText(By locator, string text)
        {
            var element = Driver.FindElement(locator);
            element.Clear();
            element.SendKeys(text);
        }
    }
}