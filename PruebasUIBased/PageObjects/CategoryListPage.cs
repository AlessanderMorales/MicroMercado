using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Asegúrate de tener este paquete
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de listado de Categorías (Optimizado)
    /// </summary>
    public class CategoryListPage : BasePage
    {
        private readonly WebDriverWait _wait;

        // --- SELECTORES (Basados en tu HTML de CategoryPage) ---
        private readonly By _addNewCategoryButton = By.CssSelector("a[href*='NewCategory']");

        // Tabla y Buscador
        private readonly By _categoryRows = By.CssSelector("#categoryTable tbody tr");
        private readonly By _searchBox = By.CssSelector("input[type='search']"); // DataTables suele usar este input type

        // Modal de Eliminación (IDs confirmados en tu HTML)
        private readonly By _deleteModal = By.Id("deleteConfirmationModal");
        private readonly By _confirmDeleteButton = By.CssSelector("#deleteCategoryForm button[type='submit']");

        // Alertas
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public CategoryListPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void ClickAddNewCategory()
        {
            var btn = _wait.Until(ExpectedConditions.ElementToBeClickable(_addNewCategoryButton));
            btn.Click();
        }

        public void ClickEditCategory(string categoryName)
        {
            // 1. FILTRAR: Fundamental para encontrar elementos en páginas ocultas
            FilterTable(categoryName);

            // 2. ENCONTRAR FILA: Esperar a que DataTables refresque
            var row = FindRowWithWait(categoryName);

            if (row != null)
            {
                // 3. BUSCAR BOTÓN: Buscamos por href parcial O por clase 'btn-warning' (amarillo)
                var editButton = row.FindElement(By.CssSelector("a[href*='EditCategory'], a.btn-warning"));

                ClickWithJs(editButton);
            }
            else
            {
                throw new Exception($"No se encontró el botón de editar para la categoría '{categoryName}' (incluso después de filtrar).");
            }
        }

        public void ClickDeleteCategory(string categoryName)
        {
            FilterTable(categoryName);
            var row = FindRowWithWait(categoryName);

            if (row != null)
            {
                // 1. Clic en el botón de eliminar de la fila (Rojo / btn-danger)
                var deleteButton = row.FindElement(By.CssSelector(".btn-danger, button[onclick*='confirmDeleteCategory']"));
                ClickWithJs(deleteButton);

                // 2. Manejo del Modal de Confirmación
                try
                {
                    // Esperar a que el modal sea visible
                    _wait.Until(ExpectedConditions.ElementIsVisible(_deleteModal));

                    // Esperar a que el botón Confirmar sea interactuable
                    var confirmBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_confirmDeleteButton));

                    System.Threading.Thread.Sleep(300); // Estabilidad para animación Bootstrap
                    confirmBtn.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception("El modal de eliminación no apareció o el botón no respondió.");
                }

                System.Threading.Thread.Sleep(1000); // Esperar recarga
            }
            else
            {
                throw new Exception($"No se pudo eliminar: La categoría '{categoryName}' no aparece en la tabla.");
            }
        }

        public bool CategoryExists(string categoryName)
        {
            FilterTable(categoryName);
            return FindRowWithWait(categoryName) != null;
        }

        public int GetCategoryCount()
        {
            try
            {
                // Limpiar filtro para contar todo
                var js = (IJavaScriptExecutor)Driver;
                // Intentamos limpiar via JS para rapidez
                js.ExecuteScript("var s=document.querySelector('input[type=\"search\"]'); if(s){s.value=''; s.dispatchEvent(new Event('input'));}");
                System.Threading.Thread.Sleep(500);
            }
            catch { }

            return Driver.FindElements(_categoryRows).Count;
        }

        public bool HasSuccessMessage()
        {
            try { return _wait.Until(ExpectedConditions.ElementIsVisible(_successAlert)).Displayed; }
            catch { return false; }
        }

        public bool HasErrorMessage()
        {
            try { return _wait.Until(ExpectedConditions.ElementIsVisible(_errorAlert)).Displayed; }
            catch { return false; }
        }

        // --- MÉTODOS PRIVADOS (Helpers) ---

        private void FilterTable(string text)
        {
            try
            {
                // Intentar encontrar la caja de búsqueda (DataTables crea input type='search')
                var searchBox = _wait.Until(ExpectedConditions.ElementIsVisible(_searchBox));

                if (searchBox.GetAttribute("value") != text)
                {
                    searchBox.Clear();
                    searchBox.SendKeys(text);
                    // DataTables filtra al escribir, no necesita Enter usualmente, pero esperamos un poco
                    // System.Threading.Thread.Sleep(300); // WaitForRow se encarga de esperar
                }
            }
            catch (WebDriverTimeoutException)
            {
                // Fallback: intentar selector alternativo si el genérico falla
                try { Driver.FindElement(By.CssSelector(".dataTables_filter input")).SendKeys(text); } catch { }
            }
        }

        private IWebElement FindRowWithWait(string text)
        {
            try
            {
                // Reintentar buscar la fila hasta que aparezca (útil tras filtrar)
                return _wait.Until(d =>
                {
                    var rows = d.FindElements(_categoryRows);
                    return rows.FirstOrDefault(r => r.Displayed && r.Text.Contains(text));
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

        private void ClickWithJs(IWebElement element)
        {
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
            System.Threading.Thread.Sleep(200);
            js.ExecuteScript("arguments[0].click();", element);
        }
    }
}