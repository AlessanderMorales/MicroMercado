using OpenQA.Selenium;
using Xunit;
using PruebasMicroMercado.BlackBoxTests;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// Pruebas Happy Path para CRUD de Productos
    /// Solo casos exitosos - flujo ideal sin errores
    [Collection("SeleniumTests")]
    public class ProductsHappyPathTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly HappyPathHelpers _helper;
        private const string BASE_URL = "https://localhost:7155";

        public ProductsHappyPathTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _helper = new HappyPathHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "HYU-P01: Complete Product CRUD Happy Path")]
        public void CompleteProductCRUD_HappyPath()
        {
            var rnd = new System.Random();
            string productName = $"Producto Happy {rnd.Next(1000, 9999)}";
            string description = "Descripción de prueba happy path";
            string brand = "Marca Test";
            string price = "25.50";
            string stock = "100";

            // === PASO 1: CREATE - Crear nuevo producto ===
            _helper.GoTo($"{BASE_URL}/ProductPage");
            _helper.ClickButtonByText("Agregar Nuevo Producto");
            _helper.WaitForUrlContains("/NewProduct");

            _helper.SetInputValue("NewProduct_Name", productName);
            _helper.SetInputValue("NewProduct_Description", description);
            _helper.SetInputValue("NewProduct_Brand", brand);
            _helper.SetInputValue("NewProduct_Price", price);
            _helper.SetInputValue("NewProduct_Stock", stock);
            _helper.SelectDropdownByValue("NewProduct_CategoryId", "2"); // Alimentos Diversos

            _helper.ClickButtonByText("Guardar Producto");
            _helper.WaitForUrlContains("/ProductPage");

            // Verificar que el producto aparece en la lista
            Assert.True(_helper.IsRowPresent("lstProducts", productName), "El producto creado debe aparecer en la tabla");

            // === PASO 2: READ - Verificar que se puede ver el producto ===
            Assert.True(_helper.IsRowPresent("lstProducts", brand), "La marca debe aparecer en la tabla");
            Assert.True(_helper.IsRowPresent("lstProducts", price), "El precio debe aparecer en la tabla");

            // === PASO 3: UPDATE - Editar el producto ===
            _helper.ClickEditButtonForRow(productName);
            _helper.WaitForUrlContains("/EditProduct");

            string updatedName = $"{productName} - Actualizado";
            string updatedPrice = "30.00";
            string updatedStock = "150";

            _helper.SetInputValue("UpdateProduct_Name", updatedName);
            _helper.SetInputValue("UpdateProduct_Price", updatedPrice);
            _helper.SetInputValue("UpdateProduct_Stock", updatedStock);

            _helper.ClickButtonByText("Guardar Cambios");
            _helper.WaitForUrlContains("/ProductPage");

            // Verificar que los cambios se guardaron
            Assert.True(_helper.IsRowPresent("lstProducts", updatedName), "El nombre actualizado debe aparecer en la tabla");

            // === PASO 4: DELETE - Eliminar el producto (borrado lógico) ===
            _helper.ClickDeleteButtonForRow(updatedName);
            _helper.ConfirmDeleteModal();

            // Verificar que el producto ya no aparece en la lista
            _helper.GoTo($"{BASE_URL}/ProductPage");
            Assert.False(_helper.IsRowPresent("lstProducts", updatedName), "El producto eliminado no debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-P02: Create Product With Minimum Price")]
        public void CreateProductWithMinimumPrice_HappyPath()
        {
            var rnd = new System.Random();
            string productName = $"Producto Económico {rnd.Next(1000, 9999)}";
            string description = "Producto de bajo costo";
            string brand = "Económica";
            string price = "0.50"; // Precio mínimo
            string stock = "500";

            _helper.GoTo($"{BASE_URL}/ProductPage");
            _helper.ClickButtonByText("Agregar Nuevo Producto");
            _helper.WaitForUrlContains("/NewProduct");

            _helper.SetInputValue("NewProduct_Name", productName);
            _helper.SetInputValue("NewProduct_Description", description);
            _helper.SetInputValue("NewProduct_Brand", brand);
            _helper.SetInputValue("NewProduct_Price", price);
            _helper.SetInputValue("NewProduct_Stock", stock);
            _helper.SelectDropdownByValue("NewProduct_CategoryId", "2");

            _helper.ClickButtonByText("Guardar Producto");
            _helper.WaitForUrlContains("/ProductPage");

            Assert.True(_helper.IsRowPresent("lstProducts", productName), "El producto con precio mínimo debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-P03: Create Product With Maximum Stock")]
        public void CreateProductWithMaximumStock_HappyPath()
        {
            var rnd = new System.Random();
            string productName = $"Producto Alto Stock {rnd.Next(1000, 9999)}";
            string description = "Producto con inventario alto";
            string brand = "SuperStock";
            string price = "15.00";
            string stock = "9999"; // Stock alto

            _helper.GoTo($"{BASE_URL}/ProductPage");
            _helper.ClickButtonByText("Agregar Nuevo Producto");
            _helper.WaitForUrlContains("/NewProduct");

            _helper.SetInputValue("NewProduct_Name", productName);
            _helper.SetInputValue("NewProduct_Description", description);
            _helper.SetInputValue("NewProduct_Brand", brand);
            _helper.SetInputValue("NewProduct_Price", price);
            _helper.SetInputValue("NewProduct_Stock", stock);
            _helper.SelectDropdownByValue("NewProduct_CategoryId", "1"); // Limpieza

            _helper.ClickButtonByText("Guardar Producto");
            _helper.WaitForUrlContains("/ProductPage");

            Assert.True(_helper.IsRowPresent("lstProducts", productName), "El producto con stock máximo debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-P04: Update Product Category Successfully")]
        public void UpdateProductCategory_HappyPath()
        {
            var rnd = new System.Random();
            string productName = $"Producto Cambio Cat {rnd.Next(1000, 9999)}";
            string description = "Producto para cambiar categoría";
            string brand = "Flexible";
            string price = "20.00";
            string stock = "50";

            // Crear producto en categoría Alimentos (2)
            _helper.GoTo($"{BASE_URL}/ProductPage");
            _helper.ClickButtonByText("Agregar Nuevo Producto");
            _helper.WaitForUrlContains("/NewProduct");

            _helper.SetInputValue("NewProduct_Name", productName);
            _helper.SetInputValue("NewProduct_Description", description);
            _helper.SetInputValue("NewProduct_Brand", brand);
            _helper.SetInputValue("NewProduct_Price", price);
            _helper.SetInputValue("NewProduct_Stock", stock);
            _helper.SelectDropdownByValue("NewProduct_CategoryId", "2");

            _helper.ClickButtonByText("Guardar Producto");
            _helper.WaitForUrlContains("/ProductPage");

            // Cambiar a categoría Limpieza (1)
            _helper.ClickEditButtonForRow(productName);
            _helper.WaitForUrlContains("/EditProduct");
            _helper.SelectDropdownByValue("UpdateProduct_CategoryId", "1");
            _helper.ClickButtonByText("Guardar Cambios");
            _helper.WaitForUrlContains("/ProductPage");

            Assert.True(_helper.IsRowPresent("lstProducts", productName), "El producto debe seguir en la tabla después de cambiar categoría");
        }

        [Fact(DisplayName = "HYU-P05: Create Multiple Products In Different Categories")]
        public void CreateMultipleProductsInDifferentCategories_HappyPath()
        {
            var rnd = new System.Random();
            var categories = new[] { ("1", "Limpieza"), ("2", "Alimentos"), ("3", "Frutas") };

            foreach (var (categoryId, categoryName) in categories)
            {
                string productName = $"Producto {categoryName} {rnd.Next(1000, 9999)}";
                string description = $"Descripción para {categoryName}";
                string brand = $"Marca {categoryName}";
                string price = $"{rnd.Next(5, 50)}.00";
                string stock = $"{rnd.Next(10, 200)}";

                _helper.GoTo($"{BASE_URL}/ProductPage");
                _helper.ClickButtonByText("Agregar Nuevo Producto");
                _helper.WaitForUrlContains("/NewProduct");

                _helper.SetInputValue("NewProduct_Name", productName);
                _helper.SetInputValue("NewProduct_Description", description);
                _helper.SetInputValue("NewProduct_Brand", brand);
                _helper.SetInputValue("NewProduct_Price", price);
                _helper.SetInputValue("NewProduct_Stock", stock);
                _helper.SelectDropdownByValue("NewProduct_CategoryId", categoryId);

                _helper.ClickButtonByText("Guardar Producto");
                _helper.WaitForUrlContains("/ProductPage");

                Assert.True(_helper.IsRowPresent("lstProducts", productName), $"El producto en categoría {categoryName} debe aparecer en la tabla");
            }
        }

        [Fact(DisplayName = "HYU-P06: Update Product Stock Successfully")]
        public void UpdateProductStock_HappyPath()
        {
            var rnd = new System.Random();
            string productName = $"Producto Stock Update {rnd.Next(1000, 9999)}";
            string description = "Producto para actualizar stock";
            string brand = "StockBrand";
            string price = "12.50";
            string originalStock = "100";
            string updatedStock = "250";

            // Crear producto
            _helper.GoTo($"{BASE_URL}/ProductPage");
            _helper.ClickButtonByText("Agregar Nuevo Producto");
            _helper.WaitForUrlContains("/NewProduct");

            _helper.SetInputValue("NewProduct_Name", productName);
            _helper.SetInputValue("NewProduct_Description", description);
            _helper.SetInputValue("NewProduct_Brand", brand);
            _helper.SetInputValue("NewProduct_Price", price);
            _helper.SetInputValue("NewProduct_Stock", originalStock);
            _helper.SelectDropdownByValue("NewProduct_CategoryId", "2");

            _helper.ClickButtonByText("Guardar Producto");
            _helper.WaitForUrlContains("/ProductPage");

            // Actualizar stock
            _helper.ClickEditButtonForRow(productName);
            _helper.WaitForUrlContains("/EditProduct");
            _helper.SetInputValue("UpdateProduct_Stock", updatedStock);
            _helper.ClickButtonByText("Guardar Cambios");
            _helper.WaitForUrlContains("/ProductPage");

            Assert.True(_helper.IsRowPresent("lstProducts", productName), "El producto debe seguir en la tabla después de actualizar el stock");
        }
    }
}
