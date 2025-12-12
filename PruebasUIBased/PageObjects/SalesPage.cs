using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Page Object para la página de Ventas (Sales)
    /// </summary>
    public class SalesPage : BasePage
    {
        // Locators
        private readonly By _searchProductInput = By.Id("product_id"); // Corregido
        private readonly By _clientTaxDocumentInput = By.Id("idDocumentoRecibido");
        private readonly By _searchClientButton = By.Id("btnBuscarCliente");
        private readonly By _clientNameInput = By.Id("nombreCliente");
        private readonly By _paymentTypeSelect = By.Id("selTipoPago");
        private readonly By _cashReceivedInput = By.Id("iptEfectivoRecibido");
        private readonly By _totalLabel = By.Id("boleta_total");
        private readonly By _changeLabel = By.Id("Vuelto");
        private readonly By _confirmSaleButton = By.Id("btnIniciarVenta");
        private readonly By _clearCartButton = By.Id("btnVaciarListado");
        private readonly By _cartTableBody = By.CssSelector("#lstProductosVenta tbody");
        private readonly By _cartRows = By.CssSelector("#lstProductosVenta tbody tr:not(.empty-cart-message)");
        private readonly By _successAlert = By.CssSelector(".alert-success");
        private readonly By _errorAlert = By.CssSelector(".alert-danger");

        public SalesPage(IWebDriver driver) : base(driver) { }

        /// <summary>
        /// Busca y agrega un producto al carrito
        /// </summary>
        public void SearchAndAddProduct(string productName)
        {
            // Escribir el nombre del producto en el buscador
            TypeText(_searchProductInput, productName);
            System.Threading.Thread.Sleep(500);

            // Ejecutar script para buscar y agregar el producto
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteAsyncScript(@"
                var term = arguments[0];
                var callback = arguments[arguments.length - 1];

                fetch('/Sales?handler=SearchProducts&term=' + encodeURIComponent(term))
                  .then(function(resp) { 
                    if (!resp.ok) throw new Error('Search failed');
                    return resp.json(); 
                  })
                  .then(function(json) {
                    if (json && json.success && json.data && json.data.length > 0) {
                      var product = json.data[0];
                      
                      var tbody = document.querySelector('#lstProductosVenta tbody');
                      if (!tbody) {
                        callback({success: false, error: 'Cart table not found'});
                        return;
                      }

                      var emptyRow = tbody.querySelector('tr.empty-cart-message');
                      if (emptyRow) {
                        emptyRow.remove();
                      }

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
            ", productName);

            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Establece la cantidad de un producto en el carrito
        /// </summary>
        public void SetProductQuantity(string productName, int quantity)
        {
            var rows = Driver.FindElements(_cartRows);
            foreach (var row in rows)
            {
                var productCell = row.FindElements(By.TagName("td"))[1];
                if (productCell.Text.Contains(productName))
                {
                    var qtyInput = row.FindElement(By.CssSelector("input[type='number']"));
                    qtyInput.Clear();
                    qtyInput.SendKeys(quantity.ToString());

                    // Disparar evento change
                    var js = (IJavaScriptExecutor)Driver;
                    js.ExecuteScript(@"
                        var input = arguments[0];
                        var event = new Event('change', { bubbles: true });
                        input.dispatchEvent(event);
                        
                        var row = input.closest('tr');
                        var price = parseFloat(row.dataset.price || 0);
                        var qty = parseInt(input.value);
                        var rowTotal = price * qty;
                        var totalCell = row.querySelector('.row-total');
                        if (totalCell) totalCell.textContent = rowTotal.toFixed(2);
                        
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
                    break;
                }
            }
        }

        /// <summary>
        /// Busca un cliente por documento de identidad
        /// </summary>
        public void SearchClient(string taxDocument)
        {
            TypeText(_clientTaxDocumentInput, taxDocument);
            ClickElement(_searchClientButton);
            System.Threading.Thread.Sleep(500);

            // Aceptar alert si aparece
            try
            {
                AcceptAlert();
            }
            catch { }

            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Selecciona el tipo de pago
        /// </summary>
        public void SelectPaymentType(string paymentType)
        {
            SelectDropdownByValue(_paymentTypeSelect, paymentType);
        }

        /// <summary>
        /// Ingresa el efectivo recibido
        /// </summary>
        public void EnterCashReceived(decimal amount)
        {
            var input = WaitForElement(_cashReceivedInput);
            input.Clear();
            input.SendKeys(amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript(@"
                var input = arguments[0];
                var blurEvent = new Event('blur', { bubbles: true });
                input.dispatchEvent(blurEvent);
                var changeEvent = new Event('change', { bubbles: true });
                input.dispatchEvent(changeEvent);
                
                setTimeout(function() {
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

        /// <summary>
        /// Confirma la venta
        /// </summary>
        public void ConfirmSale()
        {
            ClickElement(_confirmSaleButton);
            System.Threading.Thread.Sleep(2000);
            
            try
            {
                AcceptAlert();
            }
            catch { }
        }

        /// <summary>
        /// Limpia el carrito
        /// </summary>
        public void ClearCart()
        {
            ClickElement(_clearCartButton);
            
            try
            {
                AcceptAlert();
            }
            catch { }

            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Obtiene el total de la venta
        /// </summary>
        public decimal GetTotal()
        {
            try
            {
                var totalText = GetText(_totalLabel).Trim();
                totalText = totalText.Replace("Bs", "").Replace("$", "").Trim();

                if (decimal.TryParse(totalText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var total))
                {
                    return total;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el cambio
        /// </summary>
        public decimal GetChange()
        {
            try
            {
                var changeText = GetText(_changeLabel).Trim();
                changeText = changeText.Replace("Bs", "").Replace("$", "").Trim();

                if (decimal.TryParse(changeText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var change))
                {
                    return change;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el nombre del cliente
        /// </summary>
        public string? GetClientName()
        {
            return Driver.FindElement(_clientNameInput).GetAttribute("value");
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

        /// <summary>
        /// Obtiene el número de productos en el carrito
        /// </summary>
        public int GetCartItemCount()
        {
            try
            {
                return Driver.FindElements(_cartRows).Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
