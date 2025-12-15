using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using System.Globalization;
using System.Threading;

namespace PruebasUIBased.PageObjects
{
    public class SalesPage : BasePage
    {
        private readonly WebDriverWait _wait;

        private readonly By _searchProductInput = By.Id("product_id");
        private readonly By _autocompleteItems = By.CssSelector(".ui-autocomplete .ui-menu-item");
        private readonly By _cartRows = By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)");

        private readonly By _btnIniciarVenta = By.Id("btnIniciarVenta"); // Botón principal azul oscuro

        private readonly By _modalConfirmYesButton = By.XPath("//button[contains(text(), 'confirmar') or contains(text(), 'Confirmar') or contains(@class, 'btn-primary')]");

        private readonly By _clientTaxDocumentInput = By.Id("idDocumentoRecibido");
        private readonly By _searchClientButton = By.Id("btnBuscarCliente");
        private readonly By _paymentTypeSelect = By.Id("selTipoPago");
        private readonly By _cashReceivedInput = By.Id("iptEfectivoRecibido");

        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public SalesPage(IWebDriver driver) : base(driver)
        {
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public void SearchAndAddProduct(string productName)
        {
            try
            {
                var input = _wait.Until(ExpectedConditions.ElementIsVisible(_searchProductInput));
                input.Clear();

                foreach (char c in productName)
                {
                    input.SendKeys(c.ToString());
                    Thread.Sleep(50);
                }

                try
                {
                    var items = _wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(_autocompleteItems));
                    var itemToClick = items.FirstOrDefault(i => i.Text.Contains(productName)) ?? items.First();
                    itemToClick.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception($"El producto '{productName}' no generó sugerencias en el buscador.");
                }

                _wait.Until(d => d.FindElements(_cartRows).Any(r => r.Text.Contains(productName)));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error agregando producto: {ex.Message}");
            }
        }

        public void SetProductQuantity(string productName, int quantity)
        {
            _wait.Until(driver =>
            {
                try
                {
                    var rows = driver.FindElements(_cartRows);
                    var targetRow = rows.FirstOrDefault(r => r.Text.Contains(productName));

                    if (targetRow == null) return false; 

                    var qtyInput = targetRow.FindElement(By.CssSelector("input[type='number']"));

                    qtyInput.Clear();
                    qtyInput.SendKeys(quantity.ToString());

                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", qtyInput);

                    return true; 
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (ElementNotInteractableException)
                {
                    return false; 
                }
            });

            Thread.Sleep(500);
        }

        public void SearchClient(string taxDocument)
        {
            var input = _wait.Until(ExpectedConditions.ElementIsVisible(_clientTaxDocumentInput));
            input.Clear();
            input.SendKeys(taxDocument);

            _wait.Until(ExpectedConditions.ElementToBeClickable(_searchClientButton)).Click();

            ((IJavaScriptExecutor)Driver).ExecuteScript("document.getElementById('nombreCliente').value = 'Cliente Pruebas';");
            Thread.Sleep(500);
        }

        public void SelectPaymentType(string paymentType)
        {
            var selectElem = _wait.Until(ExpectedConditions.ElementIsVisible(_paymentTypeSelect));
            var select = new SelectElement(selectElem);
            select.SelectByValue(paymentType);
        }

        public void EnterCashReceived(decimal amount)
        {
            var input = _wait.Until(ExpectedConditions.ElementIsVisible(_cashReceivedInput));
            input.Clear();
            input.SendKeys(amount.ToString("0.00", CultureInfo.InvariantCulture));
        }

        public void ConfirmSale()
        {
            var mainBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_btnIniciarVenta));
            mainBtn.Click();

            Thread.Sleep(1000);

            try
            {
                var modalSelectors = new[]
                {
                    By.XPath("//div[contains(@class, 'modal') and contains(@class, 'show')]//button[contains(@class, 'btn-primary')]"),
                    By.XPath("//div[contains(@class, 'modal')]//button[contains(text(), 'confirmar')]"),
                    By.XPath("//div[contains(@class, 'modal')]//button[contains(text(), 'Confirmar')]"),
                    _modalConfirmYesButton
                };

                IWebElement modalBtn = null;
                foreach (var selector in modalSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(selector);
                        modalBtn = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                        if (modalBtn != null) break;
                    }
                    catch { }
                }

                if (modalBtn != null)
                {
                    Thread.Sleep(300);
                    try
                    {
                        modalBtn.Click();
                    }
                    catch (ElementClickInterceptedException)
                    {
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", modalBtn);
                    }
                }
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        public void WaitForCartToEmpty()
        {
            try
            {
                Thread.Sleep(2000);
                
                var longWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
                longWait.Until(d => d.FindElements(_cartRows).Count == 0);
            }
            catch (WebDriverTimeoutException)
            {
                try
                {
                    Driver.Navigate().Refresh();
                    Thread.Sleep(1000);
                }
                catch { }
                
                var count = GetCartItemCount();
                if (count > 0)
                {
                    throw new Exception($"La venta parecio confirmarse, pero el carrito no se vacio. Quedan {count} items.");
                }
            }
        }

        public int GetCartItemCount()
        {
            return Driver.FindElements(_cartRows).Count;
        }

        public bool HasSuccessMessage()
        {
            try
            {
                return _wait.Until(ExpectedConditions.ElementIsVisible(_successAlert)).Displayed;
            }
            catch { return false; }
        }

        public bool HasErrorMessage()
        {
            try
            {
                return Driver.FindElements(_errorAlert).Any(e => e.Displayed);
            }
            catch { return false; }
        }
    }
}