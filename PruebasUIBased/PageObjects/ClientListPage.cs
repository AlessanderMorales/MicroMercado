using OpenQA.Selenium;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de listado de Clientes
    /// </summary>
    public class ClientListPage : BasePage
    {
        // Locators - Nota: La página usa "categoryTable" debido a copy-paste, pero es la tabla de clientes
        private readonly By _addNewClientButton = By.CssSelector("a[href*='NewClient']");
        private readonly By _clientTable = By.Id("categoryTable"); // NOTA: La página usa categoryTable por error
        private readonly By _clientRows = By.CssSelector("#categoryTable tbody tr");
        private readonly By _editButtons = By.CssSelector("a[href*='EditCategory']"); // NOTA: También usa EditCategory
        private readonly By _deleteButtons = By.CssSelector("button[onclick*='confirmDeleteCategory']"); // NOTA: confirmDeleteCategory
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public ClientListPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Hace clic en el botón para agregar nuevo cliente
        /// </summary>
        public void ClickAddNewClient()
        {
            ClickElement(_addNewClientButton);
        }

        /// <summary>
        /// Hace clic en editar cliente por nombre
        /// </summary>
        public void ClickEditClient(string clientName)
        {
            System.Threading.Thread.Sleep(1000);
            
            try
            {
                var searchBox = Driver.FindElement(By.CssSelector(".dataTables_filter input"));
                searchBox.Clear();
                searchBox.SendKeys(clientName);
                System.Threading.Thread.Sleep(500);
            }
            catch { }
            
            var rows = Driver.FindElements(_clientRows);
            bool found = false;
            
            foreach (var row in rows)
            {
                if (row.Text.Contains(clientName) && row.Displayed)
                {
                    try
                    {
                        var editButton = row.FindElement(By.CssSelector("a[href*='EditCategory']"));  // Nota: usa EditCategory por error en la página
                        
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
                throw new Exception($"No se encontró el botón de editar para el cliente '{clientName}'");
            }
        }

        /// <summary>
        /// Hace clic en eliminar cliente por nombre
        /// </summary>
        public void ClickDeleteClient(string clientName)
        {
            var rows = Driver.FindElements(_clientRows);
            foreach (var row in rows)
            {
                if (row.Text.Contains(clientName))
                {
                    var deleteButton = row.FindElement(By.CssSelector("button[onclick*='confirmDeleteCategory']")); // Usa confirmDeleteCategory
                    deleteButton.Click();
                    System.Threading.Thread.Sleep(500);

                    // Confirmar en el modal
                    var confirmButton = Driver.FindElement(By.CssSelector("#deleteClientForm button[type='submit']"));
                    confirmButton.Click();
                    System.Threading.Thread.Sleep(500);
                    break;
                }
            }
        }

        /// <summary>
        /// Verifica si existe un cliente con el nombre especificado
        /// </summary>
        public bool ClientExists(string clientName)
        {
            try
            {
                // Esperar a que DataTables esté inicializado
                System.Threading.Thread.Sleep(1000);
                
                // Usar la función de búsqueda de DataTables para encontrar el cliente
                var searchBox = Driver.FindElement(By.CssSelector(".dataTables_filter input"));
                searchBox.Clear();
                searchBox.SendKeys(clientName);
                
                // Esperar a que se filtre
                System.Threading.Thread.Sleep(500);
                
                // Verificar si hay filas visibles (sin contar la fila de "No se encontraron resultados")
                var rows = Driver.FindElements(By.CssSelector("#categoryTable tbody tr:not(.dataTables_empty)"));
                
                bool found = false;
                foreach (var row in rows)
                {
                    if (row.Text.Contains(clientName))
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
        /// Obtiene el número de clientes en la tabla
        /// </summary>
        public int GetClientCount()
        {
            try
            {
                return Driver.FindElements(_clientRows).Count;
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
