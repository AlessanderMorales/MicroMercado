using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de listado de Categorías
    /// </summary>
    public class CategoryListPage : BasePage
    {
        // Locators
        private readonly By _addNewCategoryButton = By.CssSelector("a[href*='NewCategory']");
        private readonly By _categoryTable = By.Id("categoryTable");
        private readonly By _categoryRows = By.CssSelector("#categoryTable tbody tr");
        private readonly By _editButtons = By.CssSelector("a[href*='EditCategory']");
        private readonly By _deleteButtons = By.CssSelector("button[onclick*='confirmDeleteCategory']");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public CategoryListPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Hace clic en el botón para agregar nueva categoría
        /// </summary>
        public void ClickAddNewCategory()
        {
            ClickElement(_addNewCategoryButton);
        }

        /// <summary>
        /// Hace clic en editar categoría por nombre
        /// </summary>
        public void ClickEditCategory(string categoryName)
        {
            System.Threading.Thread.Sleep(2000); // Esperar a que DataTables se inicialice
            
            // Deshabilitar paginación para ver todas las filas (opcional, comentado por ahora)
            var js = (IJavaScriptExecutor)Driver;
            
            // Intentar encontrar la categoría directamente buscando en todas las filas
            // Usar un selector más específico y esperar a que las filas estén presentes
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElements(By.CssSelector("#categoryTable tbody tr")).Count > 0);
            
            var allRows = Driver.FindElements(By.CssSelector("#categoryTable tbody tr"));
            
            foreach (var row in allRows)
            {
                try
                {
                    // Verificar si la fila contiene el nombre de la categoría
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count > 0)
                    {
                        var firstCell = cells[0]; // Generalmente el nombre está en la primera columna
                        if (firstCell.Text.Trim().Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                            firstCell.Text.Contains(categoryName))
                        {
                            // Buscar el enlace de editar en esta fila
                            var editLinks = row.FindElements(By.TagName("a"));
                            
                            foreach (var link in editLinks)
                            {
                                var href = link.GetAttribute("href");
                                if (href != null && href.Contains("EditCategory"))
                                {
                                    // Hacer scroll y clic con JavaScript
                                    js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'instant', block: 'center'});", link);
                                    System.Threading.Thread.Sleep(300);
                                    js.ExecuteScript("arguments[0].click();", link);
                                    System.Threading.Thread.Sleep(500);
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // Si el elemento se vuelve obsoleto, continuar con la siguiente fila
                    continue;
                }
            }
            
            throw new Exception($"No se encontró el botón de editar para la categoría '{categoryName}'. " +
                $"Filas encontradas: {allRows.Count}");
        }

        /// <summary>
        /// Hace clic en eliminar categoría por nombre
        /// </summary>
        public void ClickDeleteCategory(string categoryName)
        {
            var rows = Driver.FindElements(_categoryRows);
            foreach (var row in rows)
            {
                if (row.Text.Contains(categoryName))
                {
                    var deleteButton = row.FindElement(By.CssSelector("button[onclick*='confirmDeleteCategory']"));
                    deleteButton.Click();
                    System.Threading.Thread.Sleep(500);

                    // Confirmar en el modal
                    var confirmButton = Driver.FindElement(By.CssSelector("#deleteCategoryForm button[type='submit']"));
                    confirmButton.Click();
                    System.Threading.Thread.Sleep(500);
                    break;
                }
            }
        }

        /// <summary>
        /// Verifica si existe una categoría con el nombre especificado
        /// </summary>
        public bool CategoryExists(string categoryName)
        {
            try
            {
                // Esperar a que DataTables esté inicializado
                System.Threading.Thread.Sleep(1000);
                
                // Usar la función de búsqueda de DataTables
                var searchBox = Driver.FindElement(By.CssSelector(".dataTables_filter input"));
                searchBox.Clear();
                searchBox.SendKeys(categoryName);
                
                // Esperar a que se filtre
                System.Threading.Thread.Sleep(500);
                
                // Verificar si hay filas visibles
                var rows = Driver.FindElements(By.CssSelector("#categoryTable tbody tr:not(.dataTables_empty)"));
                
                bool found = false;
                foreach (var row in rows)
                {
                    if (row.Text.Contains(categoryName))
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
        /// Obtiene el número de categorías en la tabla
        /// </summary>
        public int GetCategoryCount()
        {
            try
            {
                return Driver.FindElements(_categoryRows).Count;
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
