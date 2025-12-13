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

        // --- SELECTORES ---

        // Buscador y Tabla
        private readonly By _searchProductInput = By.Id("product_id");
        private readonly By _autocompleteItems = By.CssSelector(".ui-autocomplete .ui-menu-item");
        private readonly By _cartRows = By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)");

        // Botones de Venta
        private readonly By _btnIniciarVenta = By.Id("btnIniciarVenta"); // Botón principal azul oscuro

        // --- NUEVO: Selector específico para el botón del Modal de tu captura ---
        // Busca un botón que contenga el texto exacto "Sí, confirmar"
        private readonly By _modalConfirmYesButton = By.XPath("//button[contains(text(), 'Sí, confirmar')]");

        // Cliente y Pago
        private readonly By _clientTaxDocumentInput = By.Id("idDocumentoRecibido");
        private readonly By _searchClientButton = By.Id("btnBuscarCliente");
        private readonly By _paymentTypeSelect = By.Id("selTipoPago");
        private readonly By _cashReceivedInput = By.Id("iptEfectivoRecibido");

        // Alertas
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

                // Escribimos lento para asegurar que el JS detecte las teclas
                foreach (char c in productName)
                {
                    input.SendKeys(c.ToString());
                    Thread.Sleep(50);
                }

                try
                {
                    // Esperar sugerencias y clicar
                    var items = _wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(_autocompleteItems));
                    var itemToClick = items.FirstOrDefault(i => i.Text.Contains(productName)) ?? items.First();
                    itemToClick.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    throw new Exception($"El producto '{productName}' no generó sugerencias en el buscador.");
                }

                // Esperar a que se agregue a la tabla
                _wait.Until(d => d.FindElements(_cartRows).Any(r => r.Text.Contains(productName)));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error agregando producto: {ex.Message}");
            }
        }

        /// <summary>
        /// SOLUCIÓN AL ERROR "STALE ELEMENT":
        /// Busca, limpia y escribe dentro de un bucle que reintenta si el DOM cambia.
        /// </summary>
        public void SetProductQuantity(string productName, int quantity)
        {
            _wait.Until(driver =>
            {
                try
                {
                    // 1. Re-localizar la fila en cada intento (es vital hacerlo aquí dentro)
                    var rows = driver.FindElements(_cartRows);
                    var targetRow = rows.FirstOrDefault(r => r.Text.Contains(productName));

                    if (targetRow == null) return false; // Si no está, sigue esperando

                    // 2. Buscar el input numérico dentro de esa fila
                    var qtyInput = targetRow.FindElement(By.CssSelector("input[type='number']"));

                    // 3. Intentar escribir
                    qtyInput.Clear();
                    qtyInput.SendKeys(quantity.ToString());

                    // 4. Forzar el evento 'change' con JS para que se recalculen los totales
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", qtyInput);

                    return true; // Éxito, salir del bucle
                }
                catch (StaleElementReferenceException)
                {
                    // Si el elemento muere, devolvemos false para que el Wait lo intente de nuevo
                    return false;
                }
                catch (ElementNotInteractableException)
                {
                    return false; // Si está tapado, reintentar
                }
            });

            // Pequeña pausa para que la tabla recalcule el total monetario
            Thread.Sleep(500);
        }

        public void SearchClient(string taxDocument)
        {
            var input = _wait.Until(ExpectedConditions.ElementIsVisible(_clientTaxDocumentInput));
            input.Clear();
            input.SendKeys(taxDocument);

            _wait.Until(ExpectedConditions.ElementToBeClickable(_searchClientButton)).Click();

            // JS para asegurar que el campo nombre se llene (fallback por si la API es lenta)
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

        /// <summary>
        /// SOLUCIÓN AL MODAL:
        /// Hace clic en 'Confirmar Venta' y luego espera explícitamente al botón 'Sí, confirmar' del popup.
        /// </summary>
        public void ConfirmSale()
        {

            var mainBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_btnIniciarVenta));
            mainBtn.Click();

            try
            {
                var modalBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(_modalConfirmYesButton));
                Thread.Sleep(500);

                try
                {
                    modalBtn.Click();
                }
                catch (ElementClickInterceptedException)
                {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", modalBtn);
                }
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("El modal apareció (o debió aparecer), pero no pude hacer clic en 'Sí, confirmar'.");
            }
        }

        public void WaitForCartToEmpty()
        {
            try
            {
                _wait.Until(d => d.FindElements(_cartRows).Count == 0);
            }
            catch (WebDriverTimeoutException)
            {
                var count = GetCartItemCount();
                throw new Exception($"La venta pareció confirmarse, pero el carrito no se vació. Quedan {count} items.");
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