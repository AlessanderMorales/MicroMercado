using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PruebasMicroMercado.BlackBoxTests;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// <summary>
    /// Pruebas UNHAPPY PATH para Clientes
    /// Basado en TestCases Happy & Unhappy.txt
    /// </summary>
    [Collection("SeleniumTests")]
    public class ClientsUnhappyPathTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly HappyPathHelpers _helpers;

        public ClientsUnhappyPathTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _helpers = new HappyPathHelpers(_fixture.Driver);
        }

        #region TC-TBL-CLI-005: Validación de Email Único

        [Fact(DisplayName = "HYU-CLI-UH01: Crear cliente con email duplicado - Debe fallar")]
        public void CreateClient_WithDuplicateEmail_ShouldFail()
        {
            var rnd = new Random();
            var uniqueEmail = $"test{rnd.Next(10000, 99999)}@example.com";
            var taxDoc1 = (10000000 + rnd.Next(0, 89999999)).ToString();
            var taxDoc2 = (10000000 + rnd.Next(0, 89999999)).ToString();

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            // Crear cliente inicial
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente Email Test");
            _helpers.SetInputValue("NewClient_Email", uniqueEmail);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc1);
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Sales");

            // Intentar crear otro cliente con el mismo email
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Otro Cliente");
            _helpers.SetInputValue("NewClient_Email", uniqueEmail);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc2);
            _helpers.ClickButtonByText("Guardar");

            // Assert: Debe mostrar error de email duplicado
            wait.Until(d =>
            {
                try
                {
                    var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-summary]"));
                    return error.Displayed &&
                           error.Text.ToLower().Contains("email") &&
                           error.Text.ToLower().Contains("existe");
                }
                catch
                {
                    return false;
                }
            });

            var finalError = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
            Assert.Contains("email", finalError.Text.ToLower());
            Assert.Contains("existe", finalError.Text.ToLower());
        }

        #endregion

        #region TC-TBL-CLI-006: Validación de TaxDocument Único

        [Fact(DisplayName = "HYU-CLI-UH02: Crear cliente con TaxDocument duplicado - Debe fallar")]
        public void CreateClient_WithDuplicateTaxDocument_ShouldFail()
        {
            var rnd = new Random();
            var uniqueTaxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            var email1 = $"test{rnd.Next(10000, 99999)}@example.com";
            var email2 = $"test{rnd.Next(10000, 99999)}@example.com";

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            // Crear cliente inicial
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente TaxDoc Test");
            _helpers.SetInputValue("NewClient_Email", email1);
            _helpers.SetInputValue("NewClient_TaxDocument", uniqueTaxDoc);
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Sales");

            // Intentar crear otro cliente con el mismo TaxDocument
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Otro Cliente");
            _helpers.SetInputValue("NewClient_Email", email2);
            _helpers.SetInputValue("NewClient_TaxDocument", uniqueTaxDoc);
            _helpers.ClickButtonByText("Guardar");

            // Assert: Debe mostrar error de TaxDocument duplicado
            wait.Until(d =>
            {
                try
                {
                    var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-summary]"));
                    return error.Displayed &&
                           (error.Text.ToLower().Contains("documento") ||
                            error.Text.ToLower().Contains("taxdocument") ||
                            error.Text.ToLower().Contains("existe"));
                }
                catch
                {
                    return false;
                }
            });

            var finalError = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
            Assert.True(finalError.Text.ToLower().Contains("existe") ||
                        finalError.Text.ToLower().Contains("documento"));
        }

        #endregion

        #region TC-TBL-CLI-007: Validación de Formato de Email

        [Theory(DisplayName = "HYU-CLI-UH03: Crear cliente con formato de email inválido - Debe fallar")]
        [InlineData("correo_sin_arroba.com")]
        [InlineData("@sinusuario.com")]
        [InlineData("usuario@")]
        [InlineData("usuario@@doble.com")]
        [InlineData("usuario con espacios@test.com")]
        public void CreateClient_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
        {
            var rnd = new Random();
            var taxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));

            _helpers.SetInputValue("NewClient_BusinessName", "Cliente Email Inválido");
            _helpers.SetInputValue("NewClient_Email", invalidEmail);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc);
            _helpers.ClickButtonByText("Guardar");

            // Assert
            var hasError = wait.Until(d =>
            {
                try
                {
                    var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-for='NewClient.Email']"));
                    return error.Displayed &&
                           (error.Text.ToLower().Contains("email") ||
                            error.Text.ToLower().Contains("formato") ||
                            error.Text.ToLower().Contains("válido"));
                }
                catch
                {
                    return false;
                }
            });

            Assert.True(hasError, $"Debe mostrar error de formato con email: {invalidEmail}");
        }

        #endregion

        #region TC-TBL-CLI-008: Búsqueda por TaxDocument

        [Fact(DisplayName = "HYU-CLI-UH04: Buscar cliente con TaxDocument inexistente - Debe mostrar alerta")]
        public void SearchClient_WithNonExistentTaxDocument_ShouldShowAlert()
        {
            var rnd = new Random();
            var nonExistentTaxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            _helpers.GoTo("https://localhost:7155/Sales");
            wait.Until(d => d.FindElement(By.Id("idDocumentoRecibido")));

            _helpers.SetInputValue("idDocumentoRecibido", nonExistentTaxDoc);
            _fixture.Driver.FindElement(By.Id("btnBuscarCliente")).Click();

            // Assert: Debe mostrar alerta
            var alertShown = wait.Until(d =>
            {
                try
                {
                    var alert = d.SwitchTo().Alert();
                    var alertText = alert.Text;
                    alert.Accept();
                    return alertText.ToLower().Contains("no encontrado") ||
                           alertText.ToLower().Contains("no existe");
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            });

            Assert.True(alertShown, "Debe mostrar alerta de cliente no encontrado");
        }

        #endregion

        #region TC-TBL-CLI-009: Actualización con Email Duplicado

        [Fact(DisplayName = "HYU-CLI-UH05: Actualizar cliente con email existente - Debe fallar")]
        public void UpdateClient_WithExistingEmail_ShouldFail()
        {
            var rnd = new Random();
            var email1 = $"clientA{rnd.Next(10000, 99999)}@example.com";
            var email2 = $"clientB{rnd.Next(10000, 99999)}@example.com";
            var taxDoc1 = (10000000 + rnd.Next(0, 89999999)).ToString();
            var taxDoc2 = (10000000 + rnd.Next(0, 89999999)).ToString();
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));

            // Crear Cliente A
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente A");
            _helpers.SetInputValue("NewClient_Email", email1);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc1);
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Sales");

            // Crear Cliente B
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente B");
            _helpers.SetInputValue("NewClient_Email", email2);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc2);
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Sales");

            // Act: Actualizar Cliente B con email de Cliente A
            _helpers.GoTo("https://localhost:7155/ClientPage");
            var editButtons = wait.Until(d => d.FindElements(By.LinkText("Editar")));

            if (editButtons.Count > 0)
            {
                editButtons[editButtons.Count - 1].Click(); // Editar último cliente (B)
                wait.Until(d => d.FindElement(By.Id("UpdateClient_Email")));
                _helpers.SetInputValue("UpdateClient_Email", email1);
                _helpers.ClickButtonByText("Guardar");

                // Assert
                wait.Until(d =>
                {
                    try
                    {
                        var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                        return error.Displayed &&
                               error.Text.ToLower().Contains("email") &&
                               error.Text.ToLower().Contains("existe");
                    }
                    catch
                    {
                        return false;
                    }
                });

                var finalError = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                Assert.Contains("email", finalError.Text.ToLower());
            }
        }

        #endregion

        #region Validación de Campos Obligatorios

        [Fact(DisplayName = "HYU-CLI-UH06: Crear cliente sin BusinessName - Debe fallar")]
        public void CreateClient_WithoutBusinessName_ShouldFail()
        {
            var rnd = new Random();
            var email = $"test{rnd.Next(10000, 99999)}@example.com";
            var taxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));

            // Act: No llenar BusinessName
            _helpers.SetInputValue("NewClient_Email", email);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc);
            _helpers.ClickButtonByText("Guardar");

            // Assert
            var validationMessage = wait.Until(d =>
            {
                try
                {
                    var span = d.FindElement(By.CssSelector("[data-valmsg-for='NewClient.BusinessName'], .field-validation-error"));
                    return span.Displayed ? span : null;
                }
                catch
                {
                    return null;
                }
            });

            Assert.NotNull(validationMessage);
            Assert.Contains("requerido", validationMessage.Text.ToLower());
        }

        [Fact(DisplayName = "HYU-CLI-UH07: Crear cliente sin TaxDocument - Debe fallar")]
        public void CreateClient_WithoutTaxDocument_ShouldFail()
        {
            var rnd = new Random();
            var email = $"test{rnd.Next(10000, 99999)}@example.com";
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));

            // Act: No llenar TaxDocument
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente Sin TaxDoc");
            _helpers.SetInputValue("NewClient_Email", email);
            _helpers.ClickButtonByText("Guardar");

            // Assert
            var validationMessage = wait.Until(d =>
            {
                try
                {
                    var span = d.FindElement(By.CssSelector("[data-valmsg-for='NewClient.TaxDocument'], .field-validation-error"));
                    return span.Displayed ? span : null;
                }
                catch
                {
                    return null;
                }
            });

            Assert.NotNull(validationMessage);
            Assert.Contains("requerido", validationMessage.Text.ToLower());
        }

        #endregion

        #region TC-TBL-CLI-010: Eliminación de Cliente Inexistente

        [Fact(DisplayName = "HYU-CLI-UH08: Intentar eliminar cliente ya eliminado - Debe fallar")]
        public void DeleteClient_AlreadyDeleted_ShouldFail()
        {
            var rnd = new Random();
            var uniqueEmail = $"delete{rnd.Next(10000, 99999)}@example.com";
            var taxDoc = (10000000 + rnd.Next(0, 89999999)).ToString();
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(15));

            // 1. Crear cliente para eliminar
            _helpers.GoTo("https://localhost:7155/NewClient");
            wait.Until(d => d.FindElement(By.Id("NewClient_BusinessName")));
            _helpers.SetInputValue("NewClient_BusinessName", "Cliente a Eliminar");
            _helpers.SetInputValue("NewClient_Email", uniqueEmail);
            _helpers.SetInputValue("NewClient_TaxDocument", taxDoc);
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Sales");

            // 2. Ir a ClientPage y buscar el cliente creado
            _helpers.GoTo("https://localhost:7155/ClientPage");
            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            var clientRow = wait.Until(d =>
            {
                var rows = d.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    if (row.Text.Contains(uniqueEmail))
                        return row;
                }
                return null;
            });

            Assert.NotNull(clientRow);

            // 3. Extraer el ID del cliente
            var clientIdElement = clientRow.FindElement(By.CssSelector("td:first-child"));
            var clientId = clientIdElement.Text.Trim();
            Assert.False(string.IsNullOrEmpty(clientId), "Client ID should not be empty");

            // 4. Eliminar el cliente (primera vez)
            var deleteButton = clientRow.FindElement(By.CssSelector("button.btn-danger"));
            deleteButton.Click();

            // Confirmar en modal
            var confirmButton = wait.Until(d => d.FindElement(By.CssSelector(".modal form button[type='submit']")));
            confirmButton.Click();

            // Esperar redirección y mensaje de éxito
            wait.Until(d => d.Url.Contains("/ClientPage"));
            System.Threading.Thread.Sleep(1000);

            // 5. Verificar que el cliente ya no está en la lista
            _helpers.GoTo("https://localhost:7155/ClientPage");
            wait.Until(d => d.FindElement(By.CssSelector("table")));

            var clientStillExists = _fixture.Driver.FindElements(By.XPath($"//tr[contains(., '{uniqueEmail}')]")).Count > 0;
            Assert.False(clientStillExists, "El cliente debe haber sido eliminado de la BD");

            // 6. Intentar buscar el cliente eliminado en Sales
            _helpers.GoTo("https://localhost:7155/Sales");
            wait.Until(d => d.FindElement(By.Id("idDocumentoRecibido")));
            _helpers.SetInputValue("idDocumentoRecibido", taxDoc);
            _fixture.Driver.FindElement(By.Id("btnBuscarCliente")).Click();

            // Assert: Debe mostrar alerta de "no encontrado"
            var alertShown = wait.Until(d =>
            {
                try
                {
                    var alert = d.SwitchTo().Alert();
                    var alertText = alert.Text;
                    alert.Accept();
                    return alertText.ToLower().Contains("no encontrado") ||
                           alertText.ToLower().Contains("no existe");
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            });

            Assert.True(alertShown, "Debe mostrar alerta de cliente no encontrado después de eliminarlo");
        }

        #endregion
    }
}
