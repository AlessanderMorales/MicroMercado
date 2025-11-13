using OpenQA.Selenium;
using PruebasMicroMercado.BlackBoxTests;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// Pruebas Happy Path para CRUD de Categorías
    /// Solo casos exitosos - flujo ideal sin errores
    [Collection("SeleniumTests")]
    public class CategoriesHappyPathTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly HappyPathHelpers _helper;
        private const string BASE_URL = "https://localhost:7155";

        public CategoriesHappyPathTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _helper = new HappyPathHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "HYU-CAT01: Complete Category CRUD Happy Path")]
        public void CompleteCategoryCRUD_HappyPath()
        {
            var rnd = new Random();
            string categoryName = $"Categoria Happy {rnd.Next(1000, 9999)}";
            string description = "Descripcion de categoria para happy path";

            // === PASO 1: CREATE - Crear nueva categoría ===
            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", categoryName);
            _helper.SetInputValue("CreateCategory_Description", description);
            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Verificar que la categoría aparece en la lista
            Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                "La categoría creada debe aparecer en la tabla");

            // === PASO 2: READ - Verificar que se puede ver la categoría ===
            Assert.True(_helper.IsRowPresent("categoryTable", description),
                "La descripción debe aparecer en la tabla");

            // === PASO 3: UPDATE - Editar la categoría ===
            _helper.ClickEditButtonForRow(categoryName);
            _helper.WaitForUrlContains("/EditCategory");

            string updatedName = $"{categoryName} - Actualizada";
            string updatedDescription = "Descripcion actualizada para happy path";

            _helper.SetInputValue("UpdateCategory_Name", updatedName);
            _helper.SetInputValue("UpdateCategory_Description", updatedDescription);
            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Verificar que los cambios se guardaron
            Assert.True(_helper.IsRowPresent("categoryTable", updatedName),
                "El nombre actualizado debe aparecer en la tabla");

            // === PASO 4: DELETE - Eliminar la categoría (borrado físico) ===
            _helper.ClickDeleteButtonForRow(updatedName);
            _helper.ConfirmDeleteModal();

            // Verificar que la categoría ya no aparece en la lista
            _helper.GoTo($"{BASE_URL}/CategoryPage");
            Assert.False(_helper.IsRowPresent("categoryTable", updatedName),
                "La categoría eliminada no debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-CAT02: Create Category With Short Name")]
        public void CreateCategoryWithShortName_HappyPath()
        {
            var rnd = new Random();
            string categoryName = $"Cat{rnd.Next(10, 99)}"; // Nombre corto
            string description = "Categoria con nombre corto";

            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", categoryName);
            _helper.SetInputValue("CreateCategory_Description", description);

            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                "La categoría con nombre corto debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-CAT03: Create Category With Long Description")]
        public void CreateCategoryWithLongDescription_HappyPath()
        {
            var rnd = new Random();
            string categoryName = $"Categoria Desc {rnd.Next(1000, 9999)}";
            string description = "Esta es una descripcion muy larga para la categoria de prueba que contiene muchos caracteres";

            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", categoryName);
            _helper.SetInputValue("CreateCategory_Description", description);

            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                "La categoría con descripción larga debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-CAT04: Create Multiple Categories Successfully")]
        public void CreateMultipleCategories_HappyPath()
        {
            var rnd = new Random();
            int categoryCount = 3;

            for (int i = 0; i < categoryCount; i++)
            {
                string categoryName = $"Categoria Batch {rnd.Next(1000, 9999)}";
                string description = $"Descripcion {i + 1} para categoria de prueba";

                _helper.GoTo($"{BASE_URL}/CategoryPage");
                _helper.ClickButtonByText("Agregar Nueva Categoría");
                _helper.WaitForUrlContains("/NewCategory");

                _helper.SetInputValue("CreateCategory_Name", categoryName);
                _helper.SetInputValue("CreateCategory_Description", description);

                _helper.ClickButtonByText("Guardar");
                _helper.WaitForUrlContains("/CategoryPage");

                Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                    $"La categoría {i + 1} debe aparecer en la tabla");
            }
        }

        [Fact(DisplayName = "HYU-CAT05: Update Category Name Successfully")]
        public void UpdateCategoryName_HappyPath()
        {
            var rnd = new Random();
            string originalName = $"Categoria Original {rnd.Next(1000, 9999)}";
            string updatedName = $"Categoria Renombrada {rnd.Next(1000, 9999)}";
            string description = "Descripcion para categoria a renombrar";

            // Crear categoría
            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", originalName);
            _helper.SetInputValue("CreateCategory_Description", description);

            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Actualizar nombre
            _helper.ClickEditButtonForRow(originalName);
            _helper.WaitForUrlContains("/EditCategory");

            _helper.SetInputValue("UpdateCategory_Name", updatedName);
            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Verificar actualización
            Assert.True(_helper.IsRowPresent("categoryTable", updatedName),
                "El nombre actualizado debe aparecer en la tabla");
            Assert.False(_helper.IsRowPresent("categoryTable", originalName),
                "El nombre original no debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-CAT06: Update Category Description Successfully")]
        public void UpdateCategoryDescription_HappyPath()
        {
            var rnd = new Random();
            string categoryName = $"Categoria Desc Update {rnd.Next(1000, 9999)}";
            string originalDescription = "Descripcion original";
            string updatedDescription = "Descripcion completamente actualizada y mejorada";

            // Crear categoría
            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", categoryName);
            _helper.SetInputValue("CreateCategory_Description", originalDescription);

            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Actualizar descripción
            _helper.ClickEditButtonForRow(categoryName);
            _helper.WaitForUrlContains("/EditCategory");

            _helper.SetInputValue("UpdateCategory_Description", updatedDescription);
            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            // Verificar que sigue existiendo
            Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                "La categoría debe seguir en la tabla después de actualizar la descripción");
        }

        [Fact(DisplayName = "HYU-CAT07: Create Category Without Description")]
        public void CreateCategoryWithoutDescription_HappyPath()
        {
            var rnd = new Random();
            string categoryName = $"Categoria Sin Desc {rnd.Next(1000, 9999)}";

            _helper.GoTo($"{BASE_URL}/CategoryPage");
            _helper.ClickButtonByText("Agregar Nueva Categoría");
            _helper.WaitForUrlContains("/NewCategory");

            _helper.SetInputValue("CreateCategory_Name", categoryName);
            // No se establece descripción (opcional)

            _helper.ClickButtonByText("Guardar");
            _helper.WaitForUrlContains("/CategoryPage");

            Assert.True(_helper.IsRowPresent("categoryTable", categoryName),
                "La categoría sin descripción debe aparecer en la tabla");
        }
    }
}
