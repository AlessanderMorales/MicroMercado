using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    public class ProductListPage : BasePage
    {
        private readonly WebDriverWait _wait;

        private readonly By _addNewButton = By.CssSelector("a[href*='NewProduct'], a[href*='Create']");
        private readonly By _rows = By.CssSelector("#productTable tbody tr");
        private readonly By _searchBox = By.CssSelector("input[type='search']"); // DataTables
        private readonly By _successAlert = By.CssSelector(".alert-success");

        public ProductListPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void ClickAddNewProduct()
        {
            try
            {
                var btn = _wait.Until(ExpectedConditions.ElementToBeClickable(_addNewButton));
                btn.Click();
            }
            catch
            {
                Driver.FindElement(By.PartialLinkText("Agregar")).Click();
            }
        }

        public void ClickEditProduct(string productName)
        {
            FilterTable(productName);
            var row = FindRowWithWait(productName);

            if (row != null)
            {
                var editButton = row.FindElement(By.CssSelector("a[href*='EditProduct'], a.btn-warning"));
                ClickWithJs(editButton);
            }
            else
            {
                throw new Exception($"No se encontró el botón de editar para '{productName}'.");
            }
        }

        public void ClickDeleteProduct(string productName)
        {
            FilterTable(productName);
            var row = FindRowWithWait(productName);

            if (row != null)
            {
                var deleteButton = row.FindElement(By.CssSelector(".btn-danger"));

                var modalId = deleteButton.GetAttribute("data-bs-target");

                ClickWithJs(deleteButton);

                try
                {
                    var specificModal = By.CssSelector($"{modalId}.show");
                    _wait.Until(ExpectedConditions.ElementIsVisible(specificModal));

                    var confirmBtnSelector = By.CssSelector($"{modalId} form button[type='submit']");
                    var confirmBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(confirmBtnSelector));

                    System.Threading.Thread.Sleep(300);
                    confirmBtn.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception($"El modal de eliminación {modalId} no apareció.");
                }

                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                throw new Exception($"No se encontró el producto '{productName}' para eliminar.");
            }
        }

        public bool ProductExists(string name)
        {
            FilterTable(name);
            return FindRowWithWait(name) != null;
        }

        public int GetProductCount()
        {
            try
            {
                var js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("var s=document.querySelector('input[type=\"search\"]'); if(s){s.value=''; s.dispatchEvent(new Event('input'));}");
                System.Threading.Thread.Sleep(500);
            }
            catch { }

            return Driver.FindElements(_rows).Count;
        }

        public bool HasSuccessMessage()
        {
            try { return _wait.Until(ExpectedConditions.ElementIsVisible(_successAlert)).Displayed; }
            catch { return false; }
        }

        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        public void NavigateTo(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        // --- Helpers ---

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
            catch { }
        }

        private IWebElement FindRowWithWait(string text)
        {
            try
            {
                return _wait.Until(d =>
                {
                    var rows = d.FindElements(_rows);
                    return rows.FirstOrDefault(r => r.Displayed && r.Text.Contains(text));
                });
            }
            catch { return null; }
        }

        private void ClickWithJs(IWebElement element)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
            System.Threading.Thread.Sleep(200);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element);
        }
    }
}