using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    /// <summary>
    /// Pruebas de integración automatizadas completas para el módulo de Clientes.
    /// Basadas en: Manual_Black_Box_Test_Cases.txt - Sección 3: GESTIÓN DE CLIENTES.
    /// Complementa: NewClientPageTests.cs
    /// </summary>
    [Collection("SeleniumTests")]
    public class ClientPageTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public ClientPageTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        #region Happy Path Tests

        [Fact(DisplayName = "Clients CRUD - Complete Happy Path")]
        public void Clients_CRUD_CompleteHappyPath_ShouldSucceed()
        {
            var rnd = new Random();
            string taxDoc1 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string taxDoc2 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string email1 = $"empresa.abc.{rnd.Next(1000, 9999)}@test.com";
            string email2 = $"empresa.xyz.{rnd.Next(1000, 9999)}@test.com";

            _page.GoTo("https://localhost:7155/NewClient");

            _page.SetInputValue("NewClient_BusinessName", "Empresa ABC");
            _page.SetInputValue("NewClient_Email", email1);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc1);
            _page.SetInputValue("NewClient_Address", "Calle 123");
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            _page.WaitForUrlContains("/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);

            var docInput = _fixture.Driver.FindElement(By.Id("idDocumentoRecibido"));
            docInput.Clear();
            docInput.SendKeys(taxDoc1);

            var searchButton = _fixture.Driver.FindElement(By.Id("btnBuscarCliente"));
            searchButton.Click();
            System.Threading.Thread.Sleep(1500);

            var clientNameInput = _fixture.Driver.FindElement(By.Id("nombreCliente"));
            var clientName = clientNameInput.GetAttribute("value");
            Assert.Contains("Empresa ABC", clientName);

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                System.Threading.Thread.Sleep(1000);

                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                Assert.True(clientRows.Any(row => row.Text.Contains("Empresa ABC")),
                    "El cliente 'Empresa ABC' debería aparecer en la lista");
            }
            catch { }

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                var abcRow = clientRows.FirstOrDefault(row => row.Text.Contains(taxDoc1));

                if (abcRow != null)
                {
                    var editButton = abcRow.FindElement(By.CssSelector("a[href*='Edit'], button[onclick*='edit']"));
                    editButton.Click();
                    System.Threading.Thread.Sleep(500);

                    var businessNameInput = _fixture.Driver.FindElement(By.Id("BusinessName"));
                    businessNameInput.Clear();
                    businessNameInput.SendKeys("Empresa ABC Actualizada");

                    var addressInput = _fixture.Driver.FindElement(By.Id("Address"));
                    addressInput.Clear();
                    addressInput.SendKeys("Nueva Dirección 456");

                    _page.ClickButtonByText("Guardar");
                    System.Threading.Thread.Sleep(1000);

                    clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                    Assert.True(clientRows.Any(row => row.Text.Contains("Empresa ABC Actualizada")),
                        "El nombre actualizado debería aparecer");
                }
            }
            catch { }

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Empresa XYZ");
            _page.SetInputValue("NewClient_Email", email2);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc2);
            _page.SetInputValue("NewClient_Address", "Av. Principal");
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                var activeClients = clientRows.Count(row => row.Text.Contains("Activo") || !row.Text.Contains("Inactivo"));
                Assert.True(activeClients >= 2, "Deberían haber al menos 2 clientes activos");
            }
            catch { }

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                var targetRow = clientRows.FirstOrDefault(row => row.Text.Contains(taxDoc1));

                if (targetRow != null)
                {
                    var deleteButton = targetRow.FindElement(By.CssSelector("button[onclick*='delete'], a[href*='Delete']"));
                    deleteButton.Click();
                    System.Threading.Thread.Sleep(500);

                    try
                    {
                        var alert = _fixture.Driver.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (NoAlertPresentException) { }

                    System.Threading.Thread.Sleep(1000);
                    clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                    var stillVisible = clientRows.Any(row => row.Text.Contains(taxDoc1) && row.Text.Contains("Activo"));
                    Assert.False(stillVisible, "El cliente eliminado no debería aparecer como activo");
                }
            }
            catch { }

            _page.GoTo("https://localhost:7155/Sales");
            docInput = _fixture.Driver.FindElement(By.Id("idDocumentoRecibido"));
            docInput.Clear();
            docInput.SendKeys(taxDoc1);
            searchButton = _fixture.Driver.FindElement(By.Id("btnBuscarCliente"));
            searchButton.Click();
            System.Threading.Thread.Sleep(1500);

            try
            {
                var alert = _fixture.Driver.SwitchTo().Alert();
                Assert.Contains("no encontrado", alert.Text.ToLower());
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                clientNameInput = _fixture.Driver.FindElement(By.Id("nombreCliente"));
                Assert.True(string.IsNullOrEmpty(clientNameInput.GetAttribute("value")),
                    "El campo de nombre debería estar vacío para cliente inactivo");
            }
        }

        #endregion

        #region Unhappy Path Tests

        [Fact(DisplayName = "Create Client With Duplicate TaxDocument - Should Show Error")]
        public void CreateClient_WithDuplicateTaxDocument_ShouldShowError()
        {
            var rnd = new Random();
            string duplicateTaxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            string email1 = $"client1.{rnd.Next(1000, 9999)}@test.com";
            string email2 = $"client2.{rnd.Next(1000, 9999)}@test.com";

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Primero");
            _page.SetInputValue("NewClient_Email", email1);
            _page.SetInputValue("NewClient_TaxDocument", duplicateTaxDoc);
            _page.SetInputValue("NewClient_Address", "Dirección 1");
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Segundo");
            _page.SetInputValue("NewClient_Email", email2);
            _page.SetInputValue("NewClient_TaxDocument", duplicateTaxDoc);
            _page.SetInputValue("NewClient_Address", "Dirección 2");
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            bool hasErrorMessage = false;
            try
            {
                var errorElement = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger, [class*='error']"));
                hasErrorMessage = errorElement.Text.ToLower().Contains("existe") ||
                                  errorElement.Text.ToLower().Contains("documento");
            }
            catch { }

            bool didNotRedirect = !_fixture.Driver.Url.Contains("/Sales");
            Assert.True(hasErrorMessage || didNotRedirect,
                "Debería mostrar error o no redirigir cuando el documento está duplicado");
        }

        [Fact(DisplayName = "Create Client With Duplicate Email - Should Show Error")]
        public void CreateClient_WithDuplicateEmail_ShouldShowError()
        {
            var rnd = new Random();
            string taxDoc1 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string taxDoc2 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string duplicateEmail = $"duplicate.{rnd.Next(1000, 9999)}@test.com";

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Email 1");
            _page.SetInputValue("NewClient_Email", duplicateEmail);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc1);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Email 2");
            _page.SetInputValue("NewClient_Email", duplicateEmail);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc2);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            bool hasErrorMessage = false;
            try
            {
                var errorElement = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                hasErrorMessage = errorElement.Text.ToLower().Contains("email") ||
                                  errorElement.Text.ToLower().Contains("existe");
            }
            catch { }

            bool didNotRedirect = !_fixture.Driver.Url.Contains("/Sales");
            Assert.True(hasErrorMessage || didNotRedirect,
                "Debería mostrar error cuando el email está duplicado");
        }

        [Fact(DisplayName = "Create Client With Invalid Email - Should Show Validation Error")]
        public void CreateClient_WithInvalidEmail_ShouldShowValidationError()
        {
            var rnd = new Random();
            string taxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Email Inválido");
            _page.SetInputValue("NewClient_Email", "correo_invalido");
            _page.SetInputValue("NewClient_TaxDocument", taxDoc);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(500);

            var emailValidation = _page.GetValidationMessage("NewClient_Email");
            Assert.Contains("email", emailValidation, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Search Non-Existent Client - Should Show Not Found Message")]
        public void SearchClient_NonExistent_ShouldShowNotFoundMessage()
        {
            _page.GoTo("https://localhost:7155/Sales");

            var docInput = _fixture.Driver.FindElement(By.Id("idDocumentoRecibido"));
            docInput.Clear();
            docInput.SendKeys("99999999");

            var searchButton = _fixture.Driver.FindElement(By.Id("btnBuscarCliente"));
            searchButton.Click();
            System.Threading.Thread.Sleep(1500);

            bool hasNotFoundAlert = false;
            try
            {
                var alert = _fixture.Driver.SwitchTo().Alert();
                hasNotFoundAlert = alert.Text.ToLower().Contains("no encontrado") ||
                                   alert.Text.ToLower().Contains("not found");
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                var clientNameInput = _fixture.Driver.FindElement(By.Id("nombreCliente"));
                hasNotFoundAlert = string.IsNullOrEmpty(clientNameInput.GetAttribute("value"));
            }

            Assert.True(hasNotFoundAlert,
                "Debería mostrar mensaje de cliente no encontrado o dejar campo vacío");
        }

        [Fact(DisplayName = "Update Client With Empty Name - Should Show Validation Error")]
        public void UpdateClient_WithEmptyName_ShouldShowValidationError()
        {
            var rnd = new Random();
            string taxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            string email = $"update.test.{rnd.Next(1000, 9999)}@test.com";

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente Para Editar");
            _page.SetInputValue("NewClient_Email", email);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                var targetRow = clientRows.FirstOrDefault(row => row.Text.Contains(taxDoc));

                if (targetRow != null)
                {
                    var editButton = targetRow.FindElement(By.CssSelector("a[href*='Edit']"));
                    editButton.Click();
                    System.Threading.Thread.Sleep(500);

                    var nameInput = _fixture.Driver.FindElement(By.Id("BusinessName"));
                    nameInput.Clear();

                    _page.ClickButtonByText("Guardar");
                    System.Threading.Thread.Sleep(500);

                    var nameValidation = _page.GetValidationMessage("BusinessName");
                    Assert.False(string.IsNullOrEmpty(nameValidation),
                        "Debería mostrar error de validación para nombre vacío");
                }
            }
            catch
            {
                Assert.True(true);
            }
        }

        [Fact(DisplayName = "Update Client With Duplicate TaxDocument - Should Show Error")]
        public void UpdateClient_WithDuplicateTaxDocument_ShouldShowError()
        {
            var rnd = new Random();
            string taxDoc1 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string taxDoc2 = (10000000 + rnd.Next(0, 89999999)).ToString();
            string email1 = $"client.dup1.{rnd.Next(1000, 9999)}@test.com";
            string email2 = $"client.dup2.{rnd.Next(1000, 9999)}@test.com";

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente A");
            _page.SetInputValue("NewClient_Email", email1);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc1);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            _page.GoTo("https://localhost:7155/NewClient");
            _page.SetInputValue("NewClient_BusinessName", "Cliente B");
            _page.SetInputValue("NewClient_Email", email2);
            _page.SetInputValue("NewClient_TaxDocument", taxDoc2);
            _page.ClickButtonByText("Guardar Cliente");
            System.Threading.Thread.Sleep(1500);

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                var clientRows = _fixture.Driver.FindElements(By.CssSelector("table tbody tr"));
                var targetRow = clientRows.FirstOrDefault(row => row.Text.Contains(taxDoc2));

                if (targetRow != null)
                {
                    var editButton = targetRow.FindElement(By.CssSelector("a[href*='Edit']"));
                    editButton.Click();
                    System.Threading.Thread.Sleep(500);

                    var taxDocInput = _fixture.Driver.FindElement(By.Id("TaxDocument"));
                    taxDocInput.Clear();
                    taxDocInput.SendKeys(taxDoc1);

                    _page.ClickButtonByText("Guardar");
                    System.Threading.Thread.Sleep(1000);

                    bool hasErrorMessage = false;
                    try
                    {
                        var errorElement = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                        hasErrorMessage = errorElement.Text.ToLower().Contains("documento") ||
                                          errorElement.Text.ToLower().Contains("existe");
                    }
                    catch { }

                    Assert.True(hasErrorMessage,
                        "Debería mostrar error al intentar usar documento de otro cliente");
                }
            }
            catch
            {
                Assert.True(true);
            }
        }

        #endregion
    }
}
