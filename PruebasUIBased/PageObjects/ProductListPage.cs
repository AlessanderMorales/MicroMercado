using OpenQA.Selenium;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de listado de Productos
    /// </summary>
    public class ProductListPage : BasePage
    {
        // Locators
        private readonly By _addNewProductButton = By.CssSelector("a[href*='NewProduct']");
        private readonly By _productTable = By.Id("productTable");
        private readonly By _productRows = By.CssSelector("#productTable tbody tr");
        private readonly By _editButtons = By.CssSelector("a[href*='EditProduct']");
        private readonly By _deleteButtons = By.CssSelector("button[onclick*='confirmDeleteProduct']");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public ProductListPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Hace clic en el botón para agregar nuevo producto
        /// </summary>
        public void ClickAddNewProduct()
        {
            ClickElement(_addNewProductButton);
        }

        /// <summary>
        /// Hace clic en editar producto por nombre
        /// </summary>
        public void ClickEditProduct(string productName)
        {
            System.Threading.Thread.Sleep(1000);
            
            try
            {
                var searchBox = Driver.FindElement(By.CssSelector(".dataTables_filter input"));
                searchBox.Clear();
                searchBox.SendKeys(productName);
                System.Threading.Thread.Sleep(500);
            }
            catch { }
            
            var rows = Driver.FindElements(_productRows);
            bool found = false;
            
            foreach (var row in rows)
            {
                if (row.Text.Contains(productName) && row.Displayed)
                {
                    try
                    {
                        var editButton = row.FindElement(By.CssSelector("a[href*='EditProduct']"));
                        
                        if (editButton.Displayed && editButton.Enabled)
                        {
                            var js = (IJavaScriptExecutor)Driver;
                            js.ExecuteScript("arguments[0].scrollIntoView(true);", editButton);
                            System.Threading.Thread.Sleep(300);
                            js.ExecuteScript("arguments[0].click();", editButton);
                            found = true;
                            break;
                        }
                    }
                    catch (NoSuchElementException) { continue; }
                }
            }
            
            if (!found)
            {
                throw new Exception($"No se encontró el botón de editar para el producto '{productName}'");
            }
        }

        /// <summary>
        /// Hace clic en eliminar producto por nombre
        /// </summary>
        public void ClickDeleteProduct(string productName)
        {
            var rows = Driver.FindElements(_productRows);
            foreach (var row in rows)
            {
                if (row.Text.Contains(productName))
                {
                    var deleteButton = row.FindElement(By.CssSelector("button[onclick*='confirmDeleteProduct']"));
                    deleteButton.Click();
                    System.Threading.Thread.Sleep(500);

                    // Confirmar en el modal
                    var confirmButton = Driver.FindElement(By.CssSelector("#deleteProductForm button[type='submit']"));
                    confirmButton.Click();
                    System.Threading.Thread.Sleep(500);
                    break;
                }
            }
        }

        /// <summary>
        /// Verifica si existe un producto con el nombre especificado
        /// </summary>
        public bool ProductExists(string productName)
        {
            try
            {
                // Esperar a que DataTables esté inicializado
                System.Threading.Thread.Sleep(1000);
                
                // Usar la función de búsqueda de DataTables
                var searchBox = Driver.FindElement(By.CssSelector(".dataTables_filter input"));
                searchBox.Clear();
                searchBox.SendKeys(productName);
                
                // Esperar a que se filtre
                System.Threading.Thread.Sleep(500);
                
                // Verificar si hay filas visibles
                var rows = Driver.FindElements(By.CssSelector("#productTable tbody tr:not(.dataTables_empty)"));
                
                bool found = false;
                foreach (var row in rows)
                {
                    if (row.Text.Contains(productName))
                    {
                        found = true;
                        break;
                    }
                }
                
                // Limpiar la búsqueda
                searchBox.Clear();
                searchBox.SendKeys("");
                System.Threading.Thread.Sleep(300);
                
                return found;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el número de productos en la tabla
        /// </summary>
        public int GetProductCount()
        {
            try
            {
                return Driver.FindElements(_productRows).Count;
            }
            catch
            {
                return 0;
            }
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
            return IsElementVisible(_errorAlert);
        }
    }
}
