using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace PruebasUIBased.PageObjects
{
    /// <summary>
    /// Clase base para todos los Page Objects.
    /// Implementa funcionalidades comunes como espera de elementos, navegación, etc.
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected const int DefaultTimeoutSeconds = 45; // Aumentado a 45s para páginas muy lentas

        protected BasePage(IWebDriver driver)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds));
        }

        /// <summary>
        /// Navega a una URL específica
        /// </summary>
        public void NavigateTo(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Espera hasta que un elemento sea visible
        /// </summary>
        protected IWebElement WaitForElement(By locator)
        {
            return Wait.Until(d =>
            {
                var element = d.FindElement(locator);
                return element.Displayed ? element : null;
            });
        }

        /// <summary>
        /// Espera hasta que un elemento sea clickeable
        /// </summary>
        protected IWebElement WaitForClickableElement(By locator)
        {
            return Wait.Until(d =>
            {
                var element = d.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            });
        }

        /// <summary>
        /// Hace clic en un elemento
        /// </summary>
        protected void ClickElement(By locator)
        {
            WaitForClickableElement(locator).Click();
        }

        /// <summary>
        /// Escribe texto en un campo de entrada
        /// </summary>
        protected void TypeText(By locator, string text)
        {
            var element = WaitForElement(locator);
            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Obtiene el texto de un elemento
        /// </summary>
        protected string GetText(By locator)
        {
            return WaitForElement(locator).Text;
        }

        /// <summary>
        /// Verifica si un elemento está visible
        /// </summary>
        protected bool IsElementVisible(By locator)
        {
            try
            {
                return Driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Espera a que aparezca un alert y lo acepta
        /// </summary>
        protected void AcceptAlert()
        {
            try
            {
                var alert = Wait.Until(d => d.SwitchTo().Alert());
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                // No hay alert presente
            }
        }

        /// <summary>
        /// Selecciona un elemento de un dropdown por valor
        /// </summary>
        protected void SelectDropdownByValue(By locator, string value)
        {
            var element = WaitForElement(locator);
            var select = new SelectElement(element);
            select.SelectByValue(value);
        }

        /// <summary>
        /// Obtiene la URL actual
        /// </summary>
        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        /// <summary>
        /// Verifica si la URL contiene un fragmento específico
        /// </summary>
        public bool UrlContains(string fragment)
        {
            return Wait.Until(d => d.Url.Contains(fragment));
        }
    }
}
