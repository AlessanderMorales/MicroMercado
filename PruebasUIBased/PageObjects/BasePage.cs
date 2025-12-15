using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace PruebasUIBased.PageObjects
{
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

        public void NavigateTo(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        protected IWebElement WaitForElement(By locator)
        {
            return Wait.Until(d =>
            {
                var element = d.FindElement(locator);
                return element.Displayed ? element : null;
            });
        }

        protected IWebElement WaitForClickableElement(By locator)
        {
            return Wait.Until(d =>
            {
                var element = d.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            });
        }

        protected void ClickElement(By locator)
        {
            WaitForClickableElement(locator).Click();
        }

        protected void TypeText(By locator, string text)
        {
            var element = WaitForElement(locator);
            element.Clear();
            element.SendKeys(text);
        }

        protected string GetText(By locator)
        {
            return WaitForElement(locator).Text;
        }

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

        protected void AcceptAlert()
        {
            try
            {
                var alert = Wait.Until(d => d.SwitchTo().Alert());
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
            }
        }

        protected void SelectDropdownByValue(By locator, string value)
        {
            var element = WaitForElement(locator);
            var select = new SelectElement(element);
            select.SelectByValue(value);
        }

        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        public bool UrlContains(string fragment)
        {
            return Wait.Until(d => d.Url.Contains(fragment));
        }
    }
}
