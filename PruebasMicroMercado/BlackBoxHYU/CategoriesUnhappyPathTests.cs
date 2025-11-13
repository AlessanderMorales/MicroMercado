using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PruebasMicroMercado.BlackBoxTests;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// Pruebas UNHAPPY PATH para Categorías
    /// Basado en TestCases Happy & Unhappy.txt
    [Collection("SeleniumTests")]
    public class CategoriesUnhappyPathTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly HappyPathHelpers _helpers;

        public CategoriesUnhappyPathTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _helpers = new HappyPathHelpers(_fixture.Driver);
        }

        #region TC-TBL-CAT-005: Validación de Unicidad de Nombre

        [Fact(DisplayName = "HYU-CAT-UH01: Crear categoría con nombre duplicado - Debe fallar")]
        public void CreateCategory_WithDuplicateName_ShouldFail()
        {
            // Arrange: Crear una categoría inicial
            _helpers.GoTo("https://localhost:7155/Category");
            var uniqueName = "Categoría Test " + Guid.NewGuid().ToString().Substring(0, 8);

            _helpers.ClickButtonByText("Agregar Categoría");
            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));

            _helpers.SetInputValue("NewCategory_Name", uniqueName);
            _helpers.SetInputValue("NewCategory_Description", "Descripción inicial");
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Category");

            // Act: Intentar crear categoría con el mismo nombre
            _helpers.ClickButtonByText("Agregar Categoría");
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));

            _helpers.SetInputValue("NewCategory_Name", uniqueName); // Nombre duplicado
            _helpers.SetInputValue("NewCategory_Description", "Otra descripción");
            _helpers.ClickButtonByText("Guardar");

            // Assert: Verificar que se muestra error de duplicado
            wait.Until(d =>
            {
                try
                {
                    var errorMessage = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-summary]"));
                    return errorMessage.Displayed &&
                           (errorMessage.Text.Contains("existe") ||
                            errorMessage.Text.Contains("duplicad") ||
                            errorMessage.Text.Contains("ya existe"));
                }
                catch
                {
                    return false;
                }
            });

            var finalError = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
            Assert.Contains("existe", finalError.Text.ToLower());
        }

        #endregion

        #region TC-TBL-CAT-006: Validación de Campos Obligatorios

        [Fact(DisplayName = "HYU-CAT-UH02: Crear categoría sin nombre - Debe fallar")]
        public void CreateCategory_WithoutName_ShouldFail()
        {
            // Arrange
            _helpers.GoTo("https://localhost:7155/Category");
            _helpers.ClickButtonByText("Agregar Categoría");

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));

            // Act: Intentar guardar sin nombre
            _helpers.SetInputValue("NewCategory_Description", "Descripción sin nombre");
            _helpers.ClickButtonByText("Guardar");

            // Assert: Verificar mensaje de validación
            var validationMessage = wait.Until(d =>
            {
                try
                {
                    var span = d.FindElement(By.CssSelector("[data-valmsg-for='NewCategory.Name'], .field-validation-error"));
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

        [Fact(DisplayName = "HYU-CAT-UH03: Crear categoría solo con espacios - Debe fallar")]
        public void CreateCategory_WithOnlySpaces_ShouldFail()
        {
            // Arrange
            _helpers.GoTo("https://localhost:7155/Category");
            _helpers.ClickButtonByText("Agregar Categoría");

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));

            // Act: Intentar guardar con solo espacios
            _helpers.SetInputValue("NewCategory_Name", "     ");
            _helpers.SetInputValue("NewCategory_Description", "Descripción");
            _helpers.ClickButtonByText("Guardar");

            // Assert: Verificar que se muestra error de validación
            var hasError = wait.Until(d =>
            {
                try
                {
                    var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-for='NewCategory.Name']"));
                    return error.Displayed;
                }
                catch
                {
                    return false;
                }
            });

            Assert.True(hasError, "Debe mostrar error al intentar crear categoría con solo espacios");
        }

        #endregion

        #region TC-TBL-CAT-007: Actualización con Nombre Duplicado

        [Fact(DisplayName = "HYU-CAT-UH04: Actualizar categoría con nombre existente - Debe fallar")]
        public void UpdateCategory_WithExistingName_ShouldFail()
        {
            // Arrange: Crear dos categorías diferentes
            _helpers.GoTo("https://localhost:7155/Category");

            var name1 = "Categoría A " + Guid.NewGuid().ToString().Substring(0, 8);
            var name2 = "Categoría B " + Guid.NewGuid().ToString().Substring(0, 8);

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));

            // Crear categoría A
            _helpers.ClickButtonByText("Agregar Categoría");
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));
            _helpers.SetInputValue("NewCategory_Name", name1);
            _helpers.SetInputValue("NewCategory_Description", "Descripción A");
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Category");

            // Crear categoría B
            _helpers.ClickButtonByText("Agregar Categoría");
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));
            _helpers.SetInputValue("NewCategory_Name", name2);
            _helpers.SetInputValue("NewCategory_Description", "Descripción B");
            _helpers.ClickButtonByText("Guardar");
            _helpers.WaitForUrlContains("/Category");

            // Act: Intentar actualizar categoría B con el nombre de categoría A
            var editButtons = wait.Until(d => d.FindElements(By.LinkText("Editar")));
            if (editButtons.Count > 0)
            {
                editButtons[editButtons.Count - 1].Click(); // Editar última categoría (B)
                wait.Until(d => d.FindElement(By.Id("EditCategory_Name")));

                _helpers.SetInputValue("EditCategory_Name", name1); // Nombre duplicado de A
                _helpers.ClickButtonByText("Guardar");

                // Assert: Verificar error de duplicado
                wait.Until(d =>
                {
                    try
                    {
                        var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                        return error.Displayed && error.Text.ToLower().Contains("existe");
                    }
                    catch
                    {
                        return false;
                    }
                });

                var finalError = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger"));
                Assert.Contains("existe", finalError.Text.ToLower());
            }
        }

        #endregion

        #region Validación de Longitud de Campos

        [Fact(DisplayName = "HYU-CAT-UH05: Crear categoría con nombre muy largo - Debe fallar")]
        public void CreateCategory_WithTooLongName_ShouldFail()
        {
            // Arrange
            _helpers.GoTo("https://localhost:7155/Category");
            _helpers.ClickButtonByText("Agregar Categoría");

            var wait = new WebDriverWait(_fixture.Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("NewCategory_Name")));

            // Act: Intentar guardar con nombre muy largo (más de 20 caracteres según el modelo)
            var longName = new string('A', 50); // 50 caracteres
            _helpers.SetInputValue("NewCategory_Name", longName);
            _helpers.SetInputValue("NewCategory_Description", "Descripción");
            _helpers.ClickButtonByText("Guardar");

            // Assert: Verificar error de longitud
            var hasError = wait.Until(d =>
            {
                try
                {
                    var error = d.FindElement(By.CssSelector(".alert-danger, .text-danger, [data-valmsg-for='NewCategory.Name']"));
                    return error.Displayed;
                }
                catch
                {
                    return false;
                }
            });

            Assert.True(hasError, "Debe mostrar error al exceder la longitud máxima del nombre");
        }

        #endregion
    }
}
