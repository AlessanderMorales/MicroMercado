using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de crear/editar Categoría
    /// </summary>
    public class CategoryFormPage : BasePage
    {
        // Locators
        private readonly By _nameInput = By.Id("CreateCategory_Name");
        private readonly By _descriptionInput = By.Id("CreateCategory_Description");
        private readonly By _updateNameInput = By.Id("UpdateCategory_Name");
        private readonly By _updateDescriptionInput = By.Id("UpdateCategory_Description");
        private readonly By _updateStatusSelect = By.Id("UpdateCategory_Status");
        private readonly By _saveButton = By.CssSelector("button[type='submit']");
        private readonly By _cancelButton = By.CssSelector("a[href*='Category']");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");
        private readonly By _nameValidationError = By.CssSelector("span[data-valmsg-for='CreateCategory.Name'], span[data-valmsg-for='UpdateCategory.Name']");

        public CategoryFormPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Llena el formulario de categoría (para crear)
        /// </summary>
        public void FillCategoryForm(string name, string description)
        {
            TypeText(_nameInput, name);
            TypeText(_descriptionInput, description);
        }

        /// <summary>
        /// Llena el formulario de categoría (para actualizar)
        /// </summary>
        public void UpdateCategoryForm(string name, string description)
        {
            // Esperar a que la página cargue completamente
            System.Threading.Thread.Sleep(3000); // Aumentar el tiempo de espera
            
            // Verificar que estamos en la página correcta
            var currentUrl = Driver.Url;
            if (!currentUrl.Contains("/EditCategory"))
            {
                throw new Exception($"No estamos en la página de edición de categoría. URL actual: {currentUrl}");
            }
            
            // Usar JavaScript Executor para mayor robustez
            var js = (IJavaScriptExecutor)Driver;
            
            try
            {
                // Intentar encontrar los campos de Update con un wait más robusto
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                
                var nameElement = wait.Until(d => {
                    try {
                        var el = d.FindElement(_updateNameInput);
                        return (el != null && el.Displayed) ? el : null;
                    } catch { return null; }
                });
                
                var descElement = wait.Until(d => {
                    try {
                        var el = d.FindElement(_updateDescriptionInput);
                        return (el != null && el.Displayed) ? el : null;
                    } catch { return null; }
                });
                
                if (nameElement == null || descElement == null)
                {
                    throw new NoSuchElementException("No se pudieron encontrar los elementos del formulario");
                }
                
                // Limpiar y llenar usando JavaScript
                js.ExecuteScript("arguments[0].value = arguments[1];", nameElement, name);
                js.ExecuteScript("arguments[0].value = arguments[1];", descElement, description);
                
                // Disparar eventos de cambio
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", nameElement);
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", descElement);
            }
            catch (Exception ex)
            {
                throw new Exception($"No se encontraron los campos de actualización de categoría. " +
                    $"URL actual: {currentUrl}. Error: {ex.Message}");
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

        /// <summary>
        /// Verifica si hay error de validación en el nombre
        /// </summary>
        public bool HasNameValidationError()
        {
            try
            {
                var element = Driver.FindElement(_nameValidationError);
                return !string.IsNullOrEmpty(element.Text);
            }
            catch
            {
                return false;
            }
        }
    }
}
