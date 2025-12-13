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

        // --- SELECTORES PARA CREACIÓN (Asumiendo modelo NewCategory) ---
        // Si tu HTML usa asp-for="NewCategory.Name", el ID es este:
        private readonly By _newName = By.Id("NewCategory_Name");
        private readonly By _newDesc = By.Id("NewCategory_Description");

        // --- SELECTORES PARA EDICIÓN (Confirmado en tu HTML anterior UpdateCategory) ---
        private readonly By _updateName = By.Id("UpdateCategory_Name");
        private readonly By _updateDesc = By.Id("UpdateCategory_Description");
        private readonly By _updateStatus = By.Id("UpdateCategory_Status");

        // Botones y Alertas
        private readonly By _saveButton = By.CssSelector("button[type='submit']");
        private readonly By _validationSummary = By.CssSelector(".text-danger ul li, .validation-summary-errors");
        private readonly By _fieldErrors = By.CssSelector(".field-validation-error, .text-danger");

        public CategoryFormPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Llena el formulario de CREACIÓN
        /// </summary>
        public void FillCategoryForm(string name, string description)
        {
            try
            {
                // Esperamos el campo de CREACIÓN
                _wait.Until(ExpectedConditions.ElementIsVisible(_newName));
            }
            catch (WebDriverTimeoutException)
            {
                // Si falla, intentamos buscar con el ID antiguo por si acaso el modelo se llama diferente
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

        /// <summary>
        /// Llena el formulario de ACTUALIZACIÓN
        /// </summary>
        public void UpdateCategoryForm(string name, string description)
        {
            try
            {
                // Esperamos el campo de EDICIÓN (Sin Thread.Sleep)
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
            var btn = _wait.Until(ExpectedConditions.ElementToBeClickable(_saveButton));

            // Scroll para asegurar visibilidad
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btn);
            System.Threading.Thread.Sleep(300); // Pequeña pausa visual

            btn.Click();
        }

        public bool HasErrorMessage()
        {
            try
            {
                // Busca el resumen de validación
                if (_wait.Until(ExpectedConditions.ElementIsVisible(_validationSummary)).Displayed)
                    return true;
            }
            catch { }

            // Busca errores individuales en los campos
            var errors = Driver.FindElements(_fieldErrors);
            return errors.Any(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text) && e.Text != "*");
        }

        // --- Helpers ---

        private void EnterText(By locator, string text)
        {
            var element = Driver.FindElement(locator);
            element.Clear();
            element.SendKeys(text);
        }
    }
}