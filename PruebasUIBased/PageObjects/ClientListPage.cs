using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    public class ClientListPage : BasePage
    {
        private readonly WebDriverWait _wait;

        // --- SELECTORES BASADOS EN TU HTML PROPORCIONADO ---

        // HTML: <a asp-page="/NewClient" ...>
        private readonly By _addNewClientButton = By.CssSelector("a[href*='NewClient']");

        // HTML: <table id="categoryTable" ...> (Cuidado: usa ID de categoría por error de copy-paste en la app)
        private readonly By _clientRows = By.CssSelector("#categoryTable tbody tr");

        // HTML: DataTables genera input type='search'
        private readonly By _searchBox = By.CssSelector("input[type='search']");

        // --- MODAL DE ELIMINACIÓN ---

        // HTML: <div class="modal fade" id="deleteConfirmationModal" ...>
        private readonly By _deleteModal = By.Id("deleteConfirmationModal");

        // HTML: <form method="post" id="deleteClientForm" ...>
        // ESTA ERA LA CLAVE DEL ERROR: Aquí se llama deleteClientForm, no deleteCategoryForm
        private readonly By _confirmDeleteButton = By.CssSelector("#deleteClientForm button[type='submit']");

        // Alertas de éxito/error
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public ClientListPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void NavigateTo(string url) => Driver.Navigate().GoToUrl(url);
        public string GetCurrentUrl() => Driver.Url;

        public void ClickAddNewClient()
        {
            var btn = _wait.Until(ExpectedConditions.ElementToBeClickable(_addNewClientButton));
            btn.Click();
        }

        public void ClickEditClient(string documento)
        {
            var row = FindRowWithWait(documento);
            if (row != null)
            {
                var editButton = row.FindElement(By.CssSelector("a[href*='EditClient'], a.btn-warning"));

                ClickWithJs(editButton);
            }
            else
            {
                throw new Exception($"No se encontró el botón de editar para '{documento}'");
            }
        }

        public void ClickDeleteClient(string documento)
        {
            // 1. Filtrar la tabla para aislar el registro
            FilterTable(documento);

            // 2. Esperar a que DataTables renderice la fila
            var row = FindRowWithWait(documento);

            if (row != null)
            {
                // 3. Encontrar el botón en la fila
                // HTML: onclick="confirmDeleteCategory(...)"
                var deleteButton = row.FindElement(By.CssSelector("button[onclick*='confirmDeleteCategory']"));

                // Clic con JS para evitar problemas de scroll/overlays
                ClickWithJs(deleteButton);

                // 4. Manejo del Modal
                try
                {
                    // Esperar a que el modal sea visible (clase 'show' de bootstrap)
                    _wait.Until(ExpectedConditions.ElementIsVisible(_deleteModal));

                    // Esperar a que el botón Confirmar (dentro de deleteClientForm) sea cliqueable
                    var confirmBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_confirmDeleteButton));

                    // Pequeña pausa para asegurar que la animación terminó
                    System.Threading.Thread.Sleep(300);

                    confirmBtn.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception("El modal '#deleteConfirmationModal' no apareció o el botón en '#deleteClientForm' no fue accesible.");
                }

                // Esperar recarga de página
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                throw new Exception($"No se pudo eliminar: El cliente '{documento}' no aparece en la tabla #categoryTable.");
            }
        }

        public bool ClientExists(string texto)
        {
            FilterTable(texto);
            return FindRowWithWait(texto) != null;
        }

        public int GetClientCount()
        {
            try
            {
                // Limpiar filtro JS directo para rapidez
                var js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("var input = document.querySelector('input[type=\"search\"]'); if(input){ input.value = ''; input.dispatchEvent(new Event('input')); }");
                System.Threading.Thread.Sleep(500);
            }
            catch { }
            return Driver.FindElements(_clientRows).Count;
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

        // --- HELPERS ---

        private void FilterTable(string text)
        {
            try
            {
                var searchBox = _wait.Until(ExpectedConditions.ElementIsVisible(_searchBox));
                if (searchBox.GetAttribute("value") != text)
                {
                    searchBox.Clear();
                    searchBox.SendKeys(text);
                    // No usamos Enter, DataTables filtra al escribir (keyup)
                }
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Warning: Search box not found");
            }
        }

        // Método vital para esperar a que DataTables actualice el DOM
        private IWebElement FindRowWithWait(string text)
        {
            try
            {
                return _wait.Until(d =>
                {
                    var rows = d.FindElements(_clientRows);
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