using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    /// Pruebas de integración automatizadas para el módulo de Categorías.
    /// Basadas en: Manual_Black_Box_Test_Cases.txt - Sección 1: GESTIÓN DE CATEGORÍAS
    [Collection("SeleniumTests")]
    public class CategoryPageTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public CategoryPageTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        #region Happy Path Tests

        [Fact(DisplayName = "Categories CRUD - Complete Happy Path")]
        public void Categories_CRUD_CompleteHappyPath_ShouldSucceed()
        {
            // Crear primera categoría "Electrónica"
            _page.GoTo("https://localhost:7155/CategoryPage");
            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);

            _page.SetInputValue("CategoryInput_Name", "Electrónica");
            _page.SetInputValue("CategoryInput_Description", "Dispositivos electrónicos");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(2000);

            // Verificar que "Electrónica" aparece en la lista
            var categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr:not(:has(.empty-cart-message))"));
            Assert.True(categoryRows.Any(row => row.Text.Contains("Electrónica")),
                "La categoría 'Electrónica' debería aparecer en la lista");

            // Editar la categoría
            var electronicaRow = categoryRows.FirstOrDefault(row => row.Text.Contains("Electrónica"));
            if (electronicaRow != null)
            {
                var editButton = electronicaRow.FindElement(By.CssSelector("a[href*='EditCategory']"));
                editButton.Click();
                System.Threading.Thread.Sleep(1000);

                var descriptionInput = _fixture.Driver.FindElement(By.Id("CategoryInput_Description"));
                descriptionInput.Clear();
                descriptionInput.SendKeys("Electrónica y tecnología");

                _page.ClickButtonByText("Guardar");
                System.Threading.Thread.Sleep(2000);

                categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
                Assert.True(categoryRows.Any(row => row.Text.Contains("Electrónica y tecnología")),
                    "La descripción actualizada debería aparecer");
            }

            // Crear segunda categoría "Alimentos"
            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);

            _page.SetInputValue("CategoryInput_Name", "Alimentos");
            _page.SetInputValue("CategoryInput_Description", "Productos comestibles");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(2000);

            // Verificar que hay al menos 2 categorías activas
            categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr:not(.text-center)"));
            var activeCategories = categoryRows.Count(row => row.Text.Contains("Activo") || !row.Text.Contains("Inactivo"));
            Assert.True(activeCategories >= 2, "Deberían haber al menos 2 categorías activas");

            // Eliminar "Electrónica" (borrado lógico)
            categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
            electronicaRow = categoryRows.FirstOrDefault(row => row.Text.Contains("Electrónica"));
            if (electronicaRow != null)
            {
                var deleteButton = electronicaRow.FindElement(By.CssSelector("button.btn-danger"));
                deleteButton.Click();
                System.Threading.Thread.Sleep(1000);

                try
                {
                    var confirmButton = _fixture.Driver.FindElement(By.CssSelector("#deleteConfirmationModal button[type='submit']"));
                    confirmButton.Click();
                }
                catch
                {
                    try
                    {
                        var alert = _fixture.Driver.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch { }
                }

                System.Threading.Thread.Sleep(2000);
            }

            // Verificar que solo "Alimentos" está visible
            categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
            var alimentosExists = categoryRows.Any(row => row.Text.Contains("Alimentos") && row.Text.Contains("Activo"));
            Assert.True(alimentosExists, "La categoría 'Alimentos' debería estar visible y activa");
        }

        #endregion

        #region Unhappy Path Tests

        [Fact(DisplayName = "Create Category With Empty Name - Should Show Validation Error")]
        public void CreateCategory_WithEmptyName_ShouldShowValidationError()
        {
            _page.GoTo("https://localhost:7155/CategoryPage");
            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);

            _page.SetInputValue("CategoryInput_Description", "Test");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(1000);

            var validationMessage = _page.GetValidationMessage("CategoryInput_Name");
            Assert.False(string.IsNullOrEmpty(validationMessage),
                "Debería aparecer un mensaje de validación para el campo 'Nombre'");
        }

        [Fact(DisplayName = "Create Category With Duplicate Name - Should Show Error")]
        public void CreateCategory_WithDuplicateName_ShouldShowError()
        {
            _page.GoTo("https://localhost:7155/CategoryPage");
            string uniqueName = $"Test Duplicado {DateTime.Now.Ticks}";

            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);
            _page.SetInputValue("CategoryInput_Name", uniqueName);
            _page.SetInputValue("CategoryInput_Description", "Primera categoría");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(2000);

            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);
            _page.SetInputValue("CategoryInput_Name", uniqueName);
            _page.SetInputValue("CategoryInput_Description", "Segunda categoría");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(1500);

            bool hasErrorMessage = false;
            try
            {
                var errorElement = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger, [class*='error']"));
                hasErrorMessage = errorElement.Text.ToLower().Contains("existe") ||
                                  errorElement.Text.ToLower().Contains("duplicado");
            }
            catch { }

            hasErrorMessage = hasErrorMessage || _fixture.Driver.Url.Contains("NewCategory");
            Assert.True(hasErrorMessage, "Debería mostrar un error indicando que la categoría ya existe");
        }

        [Fact(DisplayName = "Update Category With Empty Name - Should Show Validation Error")]
        public void UpdateCategory_WithEmptyName_ShouldShowValidationError()
        {
            _page.GoTo("https://localhost:7155/CategoryPage");

            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);
            string testName = $"Categoría Para Editar {DateTime.Now.Ticks}";

            _page.SetInputValue("CategoryInput_Name", testName);
            _page.SetInputValue("CategoryInput_Description", "Descripción original");
            _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(2000);

            var categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
            var targetRow = categoryRows.FirstOrDefault(row => row.Text.Contains(testName.Substring(0, 20)));

            if (targetRow != null)
            {
                var editButton = targetRow.FindElement(By.CssSelector("a[href*='EditCategory']"));
                editButton.Click();
                System.Threading.Thread.Sleep(1000);

                var nameInput = _fixture.Driver.FindElement(By.Id("CategoryInput_Name"));
                nameInput.Clear();

                _page.ClickButtonByText("Guardar");
                System.Threading.Thread.Sleep(1000);

                var validationMessage = _page.GetValidationMessage("CategoryInput_Name");
                Assert.False(string.IsNullOrEmpty(validationMessage),
                    "Debería aparecer mensaje de validación al dejar el nombre vacío");
            }
        }

        #endregion
    }
}
