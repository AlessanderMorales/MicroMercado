using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    public class CategoryListPage : BasePage
    {
        private readonly WebDriverWait _wait;

        private readonly By _addNewCategoryButton = By.CssSelector("a[href*='NewCategory']");

        private readonly By _categoryRows = By.CssSelector("#categoryTable tbody tr");
        private readonly By _searchBox = By.CssSelector("input[type='search']"); 

        private readonly By _deleteModal = By.Id("deleteConfirmationModal");
        private readonly By _confirmDeleteButton = By.CssSelector("#deleteCategoryForm button[type='submit']");

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
            FilterTable(categoryName);

            var row = FindRowWithWait(categoryName);

            if (row != null)
            {
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
            System.Threading.Thread.Sleep(500);
            var row = FindRowWithWait(categoryName);

            if (row != null)
            {
                IWebElement deleteButton = null;
                var deleteSelectors = new[]
                {
                    By.CssSelector(".btn-danger"),
                    By.CssSelector("button[onclick*='confirmDelete']"),
                    By.CssSelector("button[data-bs-toggle='modal']"),
                    By.XPath(".//button[contains(@class, 'btn-danger')]")
                };

                foreach (var selector in deleteSelectors)
                {
                    try
                    {
                        deleteButton = row.FindElement(selector);
                        if (deleteButton != null && deleteButton.Displayed) break;
                    }
                    catch { }
                }

                if (deleteButton == null)
                {
                    throw new Exception($"No se encontro el boton de eliminar para '{categoryName}'.");
                }

                ClickWithJs(deleteButton);
                System.Threading.Thread.Sleep(500);

                try
                {
                    _wait.Until(ExpectedConditions.ElementIsVisible(_deleteModal));

                    var confirmBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_confirmDeleteButton));

                    System.Threading.Thread.Sleep(300); 
                    confirmBtn.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception("El modal de eliminacion no aparecio o el boton no respondio.");
                }

                System.Threading.Thread.Sleep(1000); 
            }
            else
            {
                throw new Exception($"No se pudo eliminar: La categoria '{categoryName}' no aparece en la tabla.");
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
                var js = (IJavaScriptExecutor)Driver;
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

        private void FilterTable(string text)
        {
            try
            {
                var searchBox = _wait.Until(ExpectedConditions.ElementIsVisible(_searchBox));

                if (searchBox.GetAttribute("value") != text)
                {
                    searchBox.Clear();
                    searchBox.SendKeys(text);
                }
            }
            catch (WebDriverTimeoutException)
            {
                try { Driver.FindElement(By.CssSelector(".dataTables_filter input")).SendKeys(text); } catch { }
            }
        }

        private IWebElement FindRowWithWait(string text)
        {
            try
            {
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