using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace PruebasMicroMercado.BlackBoxTests
{
    public class PageHelpers
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public PageHelpers(IWebDriver driver, int timeoutInSeconds = 10)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        public void GoTo(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public string GetText(string selector)
        {
            IWebElement el = selector.StartsWith("//") || selector.StartsWith("(")
                ? _wait.Until(d => d.FindElement(By.XPath(selector)))
                : _wait.Until(d => d.FindElement(By.CssSelector(selector)));
            return el.Text ?? string.Empty;
        }

        public void SearchProduct(string productName)
        {
            var input = _wait.Until(d => d.FindElement(By.Id("product_id")));
            input.Clear();
            input.SendKeys(productName);

            // Try to use the page search API and add product via the page's salesManager to avoid flaky UI autocomplete
            try
            {
                var js = @"
var term = arguments[0];
var callback = arguments[arguments.length - 1];
fetch('/Sales?handler=SearchProducts&term=' + encodeURIComponent(term))
  .then(function(resp){ return resp.json(); })
  .then(function(json){
    if(json && json.success && json.data && json.data.length>0 && window.salesManager && typeof window.salesManager.addProductToCart === 'function'){
      // Use the first product
      window.salesManager.addProductToCart(json.data[0]);
      callback(true);
    } else {
      callback(false);
    }
  })
  .catch(function(){ callback(false); });
";

                var added = ((IJavaScriptExecutor)_driver).ExecuteAsyncScript(js, productName);
                if (added is bool b && b) return;
            }
            catch
            {
                // ignore and fallback to autocomplete method below
            }

            // Fallback: Wait for autocomplete suggestions to appear and click the first matching item
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d =>
            {
                try
                {
                    var suggestions = d.FindElements(By.CssSelector("ul.ui-autocomplete li"));
                    if (suggestions != null && suggestions.Count > 0)
                    {
                        try { suggestions[0].Click(); } catch { try { ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", suggestions[0]); } catch { } }
                        return d.FindElements(By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)")).Any(r => r.Text.Contains(productName));
                    }

                    return false;
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    try { var alert = d.SwitchTo().Alert(); alert.Accept(); } catch (OpenQA.Selenium.NoAlertPresentException) { }
                    System.Threading.Thread.Sleep(200);
                    return false;
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public void SetInputValue(string id, string value)
        {
            var input = _wait.Until(d => d.FindElement(By.Id(id)));
            input.Clear();
            input.SendKeys(value);
        }

        public void ClickButtonByText(string visibleText)
        {
            var xpath = "//button[contains(normalize-space(.), '" + visibleText + "')]" +
                        "|//a[contains(normalize-space(.), '" + visibleText + "')]" +
                        "|//input[(translate(@type,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='submit' " +
                        "or translate(@type,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='button') " +
                        $"and contains(@value, '{visibleText}')]";

            var el = _wait.Until(d => d.FindElement(By.XPath(xpath)));
            el.Click();
        }

        public void WaitForUrlContains(string fragment)
        {
            // Use a longer wait for navigation operations and handle unexpected alerts
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // If an alert appears right away, accept it before waiting for navigation
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (OpenQA.Selenium.NoAlertPresentException) { }

            wait.Until(d =>
            {
                try
                {
                    return d.Url != null && d.Url.Contains(fragment);
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (OpenQA.Selenium.NoAlertPresentException) { }

                    // Give the browser a short moment after dismissing the alert
                    System.Threading.Thread.Sleep(200);
                    return d.Url != null && d.Url.Contains(fragment);
                }
                catch (OpenQA.Selenium.WebDriverException)
                {
                    // Some driver exceptions may be transient during navigation; retry
                    return false;
                }
            });
        }

        public void AddProductToCart(string productName, int quantity)
        {
            SearchProduct(productName);
            // Locate the row while handling alerts and stale elements
            var rowWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            var row = rowWait.Until(d =>
            {
                try
                {
                    return d.FindElements(By.CssSelector("#lstProductosVenta tbody tr"))
                            .FirstOrDefault(r => r.Text.Contains(productName));
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (OpenQA.Selenium.NoAlertPresentException) { }

                    System.Threading.Thread.Sleep(200);
                    return null;
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (row == null) throw new NoSuchElementException($"Product row not found: {productName}");

            var qtyInput = row.FindElement(By.CssSelector("input[type='number']"));
            qtyInput.Clear();
            qtyInput.SendKeys(quantity.ToString());
            qtyInput.SendKeys(Keys.Tab);

            // Wait until total updates
            _wait.Until(d => GetTotal() > 0);
        }

        public void SetClientTaxDocument(string taxDocument)
        {
            var input = _wait.Until(d => d.FindElement(By.Id("idDocumentoRecibido")));
            input.Clear();
            input.SendKeys(taxDocument);

            var searchBtn = _driver.FindElement(By.Id("btnBuscarCliente"));
            searchBtn.Click();

            // Wait until either the client name is populated or the payment select is available
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d =>
            {
                try
                {
                    var nombre = d.FindElement(By.Id("nombreCliente")).GetAttribute("value");
                    if (!string.IsNullOrEmpty(nombre)) return true;

                    var sel = d.FindElements(By.Id("selTipoPago")).FirstOrDefault();
                    if (sel != null && sel.Displayed) return true;

                    return false;
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    try { var alert = d.SwitchTo().Alert(); alert.Accept(); } catch (OpenQA.Selenium.NoAlertPresentException) { }
                    System.Threading.Thread.Sleep(200);
                    return false;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    return false;
                }
            });
        }

        public void SetPaymentType(int type)
        {
            var select = new SelectElement(_driver.FindElement(By.Id("selTipoPago")));
            select.SelectByValue(type.ToString());
        }

        public void SetCashReceived(decimal amount)
        {
            var input = _driver.FindElement(By.Id("iptEfectivoRecibido"));
            input.Clear();
            input.SendKeys(amount.ToString("0.00"));
            input.SendKeys(Keys.Tab);
            _wait.Until(d => GetTotal() > 0);
        }

        public void ConfirmSale()
        {
            var btn = _driver.FindElement(By.Id("btnIniciarVenta"));
            btn.Click();
            _wait.Until(d => d.Url.Contains("/Sales"));
        }

        public decimal GetTotal()
        {
            var totalText = _driver.FindElement(By.Id("boleta_total")).Text;
            return decimal.TryParse(totalText, out var total) ? total : 0;
        }

        public string GetValidationMessage(string inputId)
        {
            try
            {
                var selector = "#" + inputId + " + span";
                var span = _driver.FindElements(By.CssSelector(selector)).FirstOrDefault();
                if (span != null) return span.Text?.Trim() ?? string.Empty;

                var dotted = inputId.Replace("_", ".");
                var byData = _driver.FindElements(By.CssSelector($"[data-valmsg-for='{dotted}']")).FirstOrDefault();
                if (byData != null) return byData.Text?.Trim() ?? string.Empty;

                var input = _driver.FindElement(By.Id(inputId));
                var siblingSpan = input.FindElements(By.XPath("following-sibling::span[1]")).FirstOrDefault();
                if (siblingSpan != null) return siblingSpan.Text?.Trim() ?? string.Empty;
            }
            catch { }

            return string.Empty;
        }

        public void ClearCart()
        {
            var btn = _driver.FindElement(By.Id("btnVaciarListado"));
            btn.Click();
            // Sometimes the UI shows an alert or takes longer to clear; accept alert proactively
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (OpenQA.Selenium.NoAlertPresentException) { }

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(d =>
            {
                try
                {
                    // Count only actual product rows (exclude the placeholder row with class 'empty-cart-message')
                    var rows = d.FindElements(By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)")).Count;
                    return rows == 0;
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (OpenQA.Selenium.NoAlertPresentException) { }

                    System.Threading.Thread.Sleep(200);
                    return false;
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    // Element list refreshed while checking; retry
                    System.Threading.Thread.Sleep(100);
                    return false;
                }
                catch (OpenQA.Selenium.WebDriverException)
                {
                    // Transient driver errors; retry
                    System.Threading.Thread.Sleep(100);
                    return false;
                }
            });
        }
    }
}
