using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de crear/editar Producto
    /// </summary>
    public class ProductFormPage : BasePage
    {
        // Locators - Create
        private readonly By _createNameInput = By.Id("NewProduct_Name");
        private readonly By _createDescriptionInput = By.Id("NewProduct_Description");
        private readonly By _createBrandInput = By.Id("NewProduct_Brand");
        private readonly By _createPriceInput = By.Id("NewProduct_Price");
        private readonly By _createStockInput = By.Id("NewProduct_Stock");
        private readonly By _createCategorySelect = By.Id("NewProduct_CategoryId");

        // Locators - Update (EditProduct)
        private readonly By _updateNameInput = By.Id("EditProduct_Name");
        private readonly By _updateDescriptionInput = By.Id("EditProduct_Description");
        private readonly By _updateBrandInput = By.Id("EditProduct_Brand");
        private readonly By _updatePriceInput = By.Id("EditProduct_Price");
        private readonly By _updateStockInput = By.Id("EditProduct_Stock");
        private readonly By _updateCategorySelect = By.Id("EditProduct_CategoryId");

        // Common
        private readonly By _saveButton = By.CssSelector("button[type='submit']");
        private readonly By _cancelButton = By.CssSelector("a[href*='Product']");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public ProductFormPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Llena el formulario de producto (para crear)
        /// </summary>
        public void FillProductForm(string name, string description, string brand, decimal price, int stock, string categoryId)
        {
            TypeText(_createNameInput, name);
            TypeText(_createDescriptionInput, description);
            TypeText(_createBrandInput, brand);
            TypeText(_createPriceInput, price.ToString(System.Globalization.CultureInfo.InvariantCulture));
            TypeText(_createStockInput, stock.ToString());
            SelectDropdownByValue(_createCategorySelect, categoryId);
        }

        /// <summary>
        /// Llena el formulario de producto (para actualizar)
        /// </summary>
        public void UpdateProductForm(string name, string description, string brand, decimal price, int stock, string categoryId)
        {
            System.Threading.Thread.Sleep(2000);
            
            var js = (IJavaScriptExecutor)Driver;
            
            try
            {
                var nameInput = Driver.FindElement(_updateNameInput);
                var descInput = Driver.FindElement(_updateDescriptionInput);
                var brandInput = Driver.FindElement(_updateBrandInput);
                var priceInput = Driver.FindElement(_updatePriceInput);
                var stockInput = Driver.FindElement(_updateStockInput);
                
                js.ExecuteScript("arguments[0].value = arguments[1];", nameInput, name);
                js.ExecuteScript("arguments[0].value = arguments[1];", descInput, description);
                js.ExecuteScript("arguments[0].value = arguments[1];", brandInput, brand);
                js.ExecuteScript("arguments[0].value = arguments[1];", priceInput, price.ToString(System.Globalization.CultureInfo.InvariantCulture));
                js.ExecuteScript("arguments[0].value = arguments[1];", stockInput, stock.ToString());
                
                System.Threading.Thread.Sleep(300);
                SelectDropdownByValue(_updateCategorySelect, categoryId);
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception($"No se encontraron los campos de actualización de producto. Error: {ex.Message}");
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
