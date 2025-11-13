using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// Helper exclusivo para pruebas Happy Path de CRUDs
    /// Optimizado con WebDriverWait en lugar de Thread.Sleep
    public class HappyPathHelpers
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public HappyPathHelpers(IWebDriver driver, int timeoutInSeconds = 20)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        #region Navigation

        public void GoTo(string url)
        {
            _driver.Navigate().GoToUrl(url);
            System.Threading.Thread.Sleep(1500); // Espera más tiempo para que DataTables cargue
        }

        public void WaitForUrlContains(string fragment)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
    
            // Primero aceptar cualquier alerta que pueda estar presente
            try
            {
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
                System.Threading.Thread.Sleep(500);
            }
            catch (NoAlertPresentException) { }

            // Esperar a que la URL contenga el fragmento
            wait.Until(d =>
            {
                try
                {
                    // Verificar alertas durante la espera
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                        System.Threading.Thread.Sleep(200);
                    }
                    catch (NoAlertPresentException) { }

                    return d.Url != null && d.Url.Contains(fragment);
                }
                catch (UnhandledAlertException)
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                        System.Threading.Thread.Sleep(200);
                    }
                    catch (NoAlertPresentException) { }
                    return false;
                }
                catch (WebDriverException)
                {
                    return false;
                }
            });
            
            // Espera adicional para que DataTables termine de cargar
            System.Threading.Thread.Sleep(2000); // Aumentado de 1000 a 2000
        }

        #endregion

        #region Form Helpers

        public void SetInputValue(string id, string value)
        {
            var input = _wait.Until(d => d.FindElement(By.Id(id)));
            input.Clear();
            input.SendKeys(value);
        }

        public void ClickButton(string id)
        {
            var button = _wait.Until(d => d.FindElement(By.Id(id)));
            _wait.Until(d => button.Displayed && button.Enabled);
            button.Click();
        }

        public void ClickButtonByText(string visibleText)
        {
            // Busca cualquier botón o link que contenga el texto
            var xpath = $"//button[contains(normalize-space(.), '{visibleText}')] | " +
                         $"//a[contains(normalize-space(.), '{visibleText}')] | " +
                         $"//button[@type='submit'] | " +
                         $"//input[@type='submit' and contains(@value, '{visibleText}')]";
   
            var button = _wait.Until(d => d.FindElement(By.XPath(xpath)));
            _wait.Until(d => button.Displayed && button.Enabled);
            button.Click();
            
            // Pequeña espera después del click
            System.Threading.Thread.Sleep(500);
        }

        public string GetInputValue(string id)
        {
            var input = _wait.Until(d => d.FindElement(By.Id(id)));
            return input.GetAttribute("value") ?? string.Empty;
        }

        public void SelectDropdownByValue(string id, string value)
        {
            var select = new SelectElement(_wait.Until(d => d.FindElement(By.Id(id))));
            select.SelectByValue(value);
        }

        #endregion

        #region Table Helpers

        public bool IsRowPresent(string tableId, string searchText)
        {
            try
            {
                // Esperar a que la tabla esté visible
                var table = _wait.Until(d => d.FindElement(By.Id(tableId)));
                
                // Esperar a que DataTables esté completamente inicializado
                // DataTables agrega clases específicas cuando está listo
                var dtWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                dtWait.Until(d => 
                {
                    try
                    {
                        var dtWrapper = d.FindElements(By.CssSelector($"#{tableId}_wrapper"));
                        return dtWrapper.Count > 0;
                    }
                    catch
                    {
                        return false;
                    }
                });
               
                // Espera adicional para asegurar que los datos se renderizaron
                System.Threading.Thread.Sleep(1500);
               
                // Buscar en las filas visibles de la tabla
                var rows = table.FindElements(By.TagName("tr"));
                var found = rows.Any(row => 
                {
                    try
                    {
                        var text = row.Text;
                        return !string.IsNullOrEmpty(text) && text.Contains(searchText);
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                return found;
            }
            catch (WebDriverTimeoutException)
            {
                // Si DataTables no se inicializa, intentar buscar directamente
                try
                {
                    var table = _driver.FindElement(By.Id(tableId));
                    var rows = table.FindElements(By.TagName("tr"));
                    return rows.Any(row => row.Text.Contains(searchText));
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public void ClickEditButtonForRow(string rowIdentifier)
        {
            // Esperar a que DataTables cargue
            System.Threading.Thread.Sleep(1500);
 
            var xpath = $"//tr[contains(., '{rowIdentifier}')]//a[contains(@class, 'btn-warning') or contains(@title, 'Editar') or contains(text(), 'Editar')]";
            var editButton = _wait.Until(d => d.FindElement(By.XPath(xpath)));
            editButton.Click();
        }

        public void ClickDeleteButtonForRow(string rowIdentifier)
        {
            // Esperar a que DataTables cargue
            System.Threading.Thread.Sleep(1500);
      
            var xpath = $"//tr[contains(., '{rowIdentifier}')]//button[contains(@class, 'btn-danger') or contains(@title, 'Eliminar')]";
            var deleteButton = _wait.Until(d => d.FindElement(By.XPath(xpath)));
            deleteButton.Click();
      
            // Esperar a que el modal aparezca
            System.Threading.Thread.Sleep(500);
        }

        public void ConfirmDeleteModal()
        {
            // Esperar a que el modal esté visible
            _wait.Until(d => d.FindElement(By.CssSelector(".modal.show")));
            
            // Buscar el botón de submit dentro del formulario del modal
            // El modal tiene un form con un button type="submit" con texto "Eliminar"
            var confirmButton = _wait.Until(d => 
                d.FindElement(By.XPath("//div[contains(@class, 'modal')]//form//button[@type='submit' and contains(., 'Eliminar')]")));
            
            confirmButton.Click();
       
            // Esperar a que el modal se cierre
            System.Threading.Thread.Sleep(1000);
        }

        #endregion

        #region Success/Error Messages

        public bool IsSuccessMessageDisplayed()
        {
            try
            {
                var alert = _wait.Until(d => d.FindElement(By.CssSelector(".alert-success")));
                return alert.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public string GetSuccessMessageText()
        {
            var alert = _wait.Until(d => d.FindElement(By.CssSelector(".alert-success")));
            return alert.Text.Trim();
        }

        #endregion
    }
}
