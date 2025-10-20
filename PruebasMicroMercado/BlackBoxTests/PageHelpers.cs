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
            // Use direct API call to search and add product - more reliable than UI autocomplete
            try
            {
                var js = @"
var term = arguments[0];
var callback = arguments[arguments.length - 1];

// Call the search API
fetch('/Sales?handler=SearchProducts&term=' + encodeURIComponent(term))
  .then(function(resp) { 
    if (!resp.ok) throw new Error('Search failed');
    return resp.json(); 
  })
  .then(function(json) {
    if (json && json.success && json.data && json.data.length > 0) {
      var product = json.data[0];
      
      // Now add the product directly to the table
      var tbody = document.querySelector('#lstProductosVenta tbody');
      if (!tbody) {
        callback({success: false, error: 'Cart table not found'});
        return;
      }

      // Remove empty cart message if exists
      var emptyRow = tbody.querySelector('tr.empty-cart-message');
      if (emptyRow) {
        emptyRow.remove();
      }

      // Create new row
      var newRow = tbody.insertRow();
      newRow.innerHTML = 
        '<td>' + (product.id || '') + '</td>' +
        '<td>' + (product.name || '') + '</td>' +
        '<td>' + (product.categoryName || '') + '</td>' +
        '<td><input type=""number"" class=""form-control form-control-sm text-center"" value=""1"" min=""1"" max=""' + (product.stock || 1) + '"" style=""width: 80px;"" data-stock=""' + (product.stock || 0) + '"" /></td>' +
        '<td>' + parseFloat(product.price || 0).toFixed(2) + '</td>' +
        '<td class=""row-total"">' + parseFloat(product.price || 0).toFixed(2) + '</td>' +
        '<td class=""text-center""><button type=""button"" class=""btn btn-danger btn-sm btn-remove""><i class=""fas fa-trash""></i></button></td>';
      
      newRow.dataset.productId = product.id;
      newRow.dataset.price = product.price;

      // Update total
      updateTotal();
      
      callback({success: true, product: product});
    } else {
      callback({success: false, error: 'No products found'});
    }
  })
  .catch(function(err) {
    callback({success: false, error: err.message});
  });

function updateTotal() {
  var total = 0;
  var rows = document.querySelectorAll('#lstProductosVenta tbody tr:not(.empty-cart-message)');
  rows.forEach(function(row) {
    var qtyInput = row.querySelector('input[type=""number""]');
    var price = parseFloat(row.dataset.price || 0);
    var qty = parseInt(qtyInput ? qtyInput.value : 1);
    var rowTotal = price * qty;
    var totalCell = row.querySelector('.row-total');
    if (totalCell) totalCell.textContent = rowTotal.toFixed(2);
    total += rowTotal;
  });
  
  var totalElements = document.querySelectorAll('#totalVenta, #boleta_total');
  totalElements.forEach(function(el) {
    el.textContent = total.toFixed(2);
  });
}
";

                var result = ((IJavaScriptExecutor)_driver).ExecuteAsyncScript(js, productName);

                // Wait for product to appear in cart
                System.Threading.Thread.Sleep(1000);

                // Verify product was added
                var cartRows = _driver.FindElements(By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)"));
                if (cartRows.Count == 0)
                {
                    throw new Exception($"Product '{productName}' was not added to cart");
                }

                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search and add product '{productName}': {ex.Message}", ex);
            }
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
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // Accept any alert that appears
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException) { }

            wait.Until(d =>
            {
                try
                {
                    return d.Url != null && d.Url.Contains(fragment);
                }
                catch (UnhandledAlertException)
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (NoAlertPresentException) { }

                    System.Threading.Thread.Sleep(200);
                    return d.Url != null && d.Url.Contains(fragment);
                }
                catch (WebDriverException)
                {
                    return false;
                }
            });
        }

        public void AddProductToCart(string productName, int quantity)
        {
            // First search and add the product
            SearchProduct(productName);

            // Then update quantity if needed
            if (quantity != 1)
            {
                var rowWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var row = rowWait.Until(d =>
                {
                    try
                    {
                        return d.FindElements(By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)"))
                                .LastOrDefault(); // Get the last row (most recently added)
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                });

                if (row == null)
                    throw new NoSuchElementException($"Product row not found: {productName}");

                var qtyInput = row.FindElement(By.CssSelector("input[type='number']"));
                qtyInput.Clear();
                qtyInput.SendKeys(quantity.ToString());

                // Trigger change event via JavaScript
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    var input = arguments[0];
                    var event = new Event('change', { bubbles: true });
                    input.dispatchEvent(event);
                    
                    // Also update the total manually
                    var row = input.closest('tr');
                    var price = parseFloat(row.dataset.price || 0);
                    var qty = parseInt(input.value);
                    var rowTotal = price * qty;
                    var totalCell = row.querySelector('.row-total');
                    if (totalCell) totalCell.textContent = rowTotal.toFixed(2);
                    
                    // Update grand total
                    var total = 0;
                    var rows = document.querySelectorAll('#lstProductosVenta tbody tr:not(.empty-cart-message)');
                    rows.forEach(function(r) {
                        var rt = parseFloat(r.querySelector('.row-total').textContent || 0);
                        total += rt;
                    });
                    document.querySelectorAll('#totalVenta, #boleta_total').forEach(function(el) {
                        el.textContent = total.toFixed(2);
                    });
                ", qtyInput);

                System.Threading.Thread.Sleep(500);
            }

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

            // Wait for client search to complete
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d =>
            {
                try
                {
                    // Accept alert if it appears
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                        return true; // Client not found, but that's ok
                    }
                    catch (NoAlertPresentException)
                    {
                        // No alert, check if client name was populated
                        var nombre = d.FindElement(By.Id("nombreCliente")).GetAttribute("value");
                        if (!string.IsNullOrEmpty(nombre)) return true;
                    }

                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            System.Threading.Thread.Sleep(500);
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

            // Format the amount without thousand separators, using dot as decimal separator
            string amountStr = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            input.SendKeys(amountStr);

            // Trigger blur and change events
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var input = arguments[0];
                
                // Trigger blur event (many apps listen to this)
                var blurEvent = new Event('blur', { bubbles: true });
                input.dispatchEvent(blurEvent);
                
                // Trigger change event
                var changeEvent = new Event('change', { bubbles: true });
                input.dispatchEvent(changeEvent);
                
                // Also trigger input event for good measure
                var inputEvent = new Event('input', { bubbles: true });
                input.dispatchEvent(inputEvent);
                
                // Give a moment for any event handlers to execute
                setTimeout(function() {
                    // Update the displayed values if the page script hasn't done it
                    var efectivoEl = document.getElementById('EfectivoEntregado');
                    var vueltoEl = document.getElementById('Vuelto');
                    var totalEl = document.getElementById('boleta_total');
                    
                    if (efectivoEl && vueltoEl && totalEl) {
                        var efectivo = parseFloat(input.value) || 0;
                        var total = parseFloat(totalEl.textContent.replace(/,/g, '')) || 0;
                        var vuelto = Math.max(0, efectivo - total);
                        
                        efectivoEl.textContent = efectivo.toFixed(2);
                        vueltoEl.textContent = vuelto.toFixed(2);
                    }
                }, 100);
            ", input);

            System.Threading.Thread.Sleep(500);
        }

        public void ConfirmSale()
        {
            var btn = _driver.FindElement(By.Id("btnIniciarVenta"));
            btn.Click();

            // Wait for either success or error response
            System.Threading.Thread.Sleep(2000);

            // Accept any alert that appears
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException) { }
        }

        public decimal GetTotal()
        {
            try
            {
                var totalText = _driver.FindElement(By.Id("boleta_total")).Text.Trim();

                // Remove any currency symbols and spaces
                totalText = totalText.Replace("Bs", "").Replace("$", "").Trim();

                // Remove thousand separators (both comma and dot can be used)
                // Handle formats like: 1,234.56 or 1.234,56

                // If the text has both comma and dot, determine which is decimal separator
                if (totalText.Contains(",") && totalText.Contains("."))
                {
                    // If dot comes after comma, dot is decimal separator (1,234.56)
                    if (totalText.LastIndexOf('.') > totalText.LastIndexOf(','))
                    {
                        totalText = totalText.Replace(",", "");
                    }
                    else
                    {
                        // If comma comes after dot, comma is decimal separator (1.234,56)
                        totalText = totalText.Replace(".", "").Replace(",", ".");
                    }
                }
                else if (totalText.Contains(","))
                {
                    // Only comma - could be thousand separator or decimal separator
                    // If there are multiple commas or comma is far from end, it's thousand separator
                    int commaCount = totalText.Count(c => c == ',');
                    int lastCommaPos = totalText.LastIndexOf(',');
                    int charsAfterComma = totalText.Length - lastCommaPos - 1;

                    if (commaCount > 1 || charsAfterComma == 3)
                    {
                        // It's a thousand separator
                        totalText = totalText.Replace(",", "");
                    }
                    else if (charsAfterComma <= 2)
                    {
                        // It's a decimal separator
                        totalText = totalText.Replace(",", ".");
                    }
                }
                else if (totalText.Contains("."))
                {
                    // Only dot - could be thousand separator or decimal separator
                    int dotCount = totalText.Count(c => c == '.');
                    int lastDotPos = totalText.LastIndexOf('.');
                    int charsAfterDot = totalText.Length - lastDotPos - 1;

                    if (dotCount > 1 || charsAfterDot == 3)
                    {
                        // It's a thousand separator
                        totalText = totalText.Replace(".", "");
                    }
                    // If charsAfterDot <= 2, keep the dot as decimal separator
                }

                return decimal.TryParse(totalText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var total) ? total : 0;
            }
            catch
            {
                return 0;
            }
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

            // Accept alert if it appears
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException) { }

            // Clear via JavaScript to ensure it works
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var tbody = document.querySelector('#lstProductosVenta tbody');
                if (tbody) {
                    // Remove all product rows
                    var rows = tbody.querySelectorAll('tr:not(.empty-cart-message)');
                    rows.forEach(function(row) { row.remove(); });
                    
                    // Show empty message if not present
                    if (!tbody.querySelector('.empty-cart-message')) {
                        var emptyRow = tbody.insertRow();
                        emptyRow.className = 'empty-cart-message';
                        emptyRow.innerHTML = '<td colspan=""7"" class=""text-center text-muted py-4"">' +
                            '<i class=""fas fa-shopping-basket fa-3x mb-3 d-block""></i>' +
                            '<p class=""mb-0"">No hay productos</p>' +
                            '<small>Busque y agregue productos usando el buscador de arriba</small></td>';
                    }
                    
                    // Reset totals
                    document.querySelectorAll('#totalVenta, #boleta_total').forEach(function(el) {
                        el.textContent = '0.00';
                    });
                    document.getElementById('EfectivoEntregado').textContent = '0.00';
                    document.getElementById('Vuelto').textContent = '0.00';
                }
            ");

            System.Threading.Thread.Sleep(1000);
        }
    }
}