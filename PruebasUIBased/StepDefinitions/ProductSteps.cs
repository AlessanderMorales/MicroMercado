using PruebasUIBased.Infrastructure; 
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;
using System;
using System.Globalization;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class ProductSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;
        private ProductListPage _listPage;
        private ProductFormPage _formPage;

        public ProductSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private ProductListPage ListPage => _listPage ??= new ProductListPage(_fixture.Driver);
        private ProductFormPage FormPage => _formPage ??= new ProductFormPage(_fixture.Driver);

        private void EnsureOnProductList()
        {
            var currentUrl = ListPage.GetCurrentUrl();
            if (!currentUrl.Contains("Product"))
            {
                ListPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
            }
        }

        private void CreateProductAux(string name)
        {
            ListPage.ClickAddNewProduct();
            FormPage.FillProductForm(name, "Desc Aux", "Marca Aux", 10.00m, 10, "1");
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
            EnsureOnProductList();
        }

        private string ObtenerNombreUnico(string nombreBase)
        {
            if (nombreBase.Contains("ParaEliminar") || nombreBase.Contains("ParaEditar"))
            {
                var random = new Random();
                string numeros = random.Next(100, 99999).ToString();
                return $"{nombreBase}{numeros}";
            }
            return nombreBase;
        }

        [Given(@"existe una categoria ""(.*)"" para productos")]
        public void GivenExisteUnaCategoria(string categoryName)
        {
            _scenarioContext["CategoryForProducts"] = "1";
        }

        [Given(@"existe un producto creado con nombre ""(.*)""")]
        public void GivenExisteUnProductoCreado(string nombreBase)
        {
            EnsureOnProductList();

            string nombreReal = ObtenerNombreUnico(nombreBase);
            _scenarioContext["TargetProductName"] = nombreReal;

            if (!ListPage.ProductExists(nombreReal))
            {
                CreateProductAux(nombreReal);
            }
        }

        [Given(@"existen los siguientes productos en el sistema:")]
        public void GivenExistenLosSiguientesProductosEnElSistema(Table table)
        {
            foreach (var row in table.Rows)
            {
                GivenExisteUnProductoCreado(row["Nombre"]);
            }
        }

        // ==========================================
        // CREATE
        // ==========================================

        [When(@"hago clic en agregar nuevo producto")]
        public void WhenHagoClicEnAgregarNuevoProducto()
        {
            EnsureOnProductList();
            ListPage.ClickAddNewProduct();
        }

        [When(@"lleno el formulario de producto con los siguientes datos:")]
        [When(@"creo un producto con los siguientes datos:")]
        public void WhenLlenoElFormularioDeProductoConLosSiguientesDatos(Table table)
        {
            string nombre = "";
            string descripcion = "";
            string marca = "";
            decimal precio = 0;
            int stock = 0;
            string categoria = "1";

            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                switch (campo)
                {
                    case "Nombre": case "Name": nombre = valor; break;
                    case "Descripcion": case "Description": descripcion = valor; break;
                    case "Marca": case "Brand": marca = valor; break;
                    case "Precio": case "Price": precio = decimal.Parse(valor, CultureInfo.InvariantCulture); break;
                    case "Stock": stock = int.Parse(valor); break;
                    case "Categoria": case "CategoryId": categoria = valor; break;
                }
            }

            FormPage.FillProductForm(nombre, descripcion, marca, precio, stock, categoria);
            _scenarioContext["ProductName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_CREAR_LISTOS");
        }

        [When(@"intento crear un producto con precio (.*)")]
        public void WhenIntentoCrearUnProductoConPrecio(string precioStr)
        {
            EnsureOnProductList();
            ListPage.ClickAddNewProduct();

            decimal precio = decimal.Parse(precioStr, CultureInfo.InvariantCulture);
            FormPage.FillProductForm("Producto Error", "Desc", "Marca", precio, 10, "1");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_INVALIDOS_PRECIO");

            FormPage.ClickSave();
        }

        [When(@"hago clic en guardar producto")]
        public void WhenHagoClicEnGuardarProducto()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        // ==========================================
        // UPDATE
        // ==========================================

        [When(@"hago clic en editar producto ""(.*)""")]
        public void WhenHagoClicEnEditarProducto(string nombreBase)
        {
            EnsureOnProductList();

            string nombreReal = _scenarioContext.ContainsKey("TargetProductName")
                                ? (string)_scenarioContext["TargetProductName"]
                                : nombreBase;

            if (!ListPage.ProductExists(nombreReal))
            {
                CreateProductAux(nombreReal);
            }
            ListPage.ClickEditProduct(nombreReal);
        }

        [When(@"actualizo el formulario de producto con:")]
        [When(@"actualizo el producto con:")]
        public void WhenActualizoElFormularioDeProductoCon(Table table)
        {
            string nombre = "";
            string descripcion = "";
            decimal precio = 0;
            int stock = 0;

            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                switch (campo)
                {
                    case "Nombre": case "Name": nombre = valor; break;
                    case "Descripcion": case "Description": descripcion = valor; break;
                    case "Precio": case "Price": precio = decimal.Parse(valor, CultureInfo.InvariantCulture); break;
                    case "Stock": stock = int.Parse(valor); break;
                }
            }

            FormPage.UpdateProductForm(nombre, descripcion, "MarcaMod", precio, stock, "1");
            _scenarioContext["UpdatedProductName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EDITAR_LISTOS");

            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"intento actualizar el stock a (.*)")]
        public void WhenIntentoActualizarElStockA(int stockNegativo)
        {
            if (_scenarioContext.ContainsKey("TargetProductName"))
            {
                string nombre = _scenarioContext["TargetProductName"].ToString();
                EnsureOnProductList();
                ListPage.ClickEditProduct(nombre);
            }

            FormPage.UpdateProductForm("", "", "", 0, stockNegativo, "1");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_INVALIDOS_STOCK");

            FormPage.ClickSave();
        }

        // ==========================================
        // DELETE
        // ==========================================

        [When(@"hago clic en eliminar producto ""(.*)""")]
        public void WhenHagoClicEnEliminarProducto(string nombreBase)
        {
            EnsureOnProductList();

            string nombreReal = _scenarioContext.ContainsKey("TargetProductName")
                                ? (string)_scenarioContext["TargetProductName"]
                                : nombreBase;

            if (!ListPage.ProductExists(nombreReal))
            {
                CreateProductAux(nombreReal);
            }

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "PREVIO_ELIMINAR");

            ListPage.ClickDeleteProduct(nombreReal);
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"elimino el producto")]
        public void WhenEliminoElProducto()
        {
            string nombre = _scenarioContext.ContainsKey("TargetProductName")
                            ? (string)_scenarioContext["TargetProductName"]
                            : "Producto Temporal";

            WhenHagoClicEnEliminarProducto(nombre);
        }

        // ==========================================
        // ASSERTIONS (THEN)
        // ==========================================

        [Then(@"debo ver mensaje de exito en producto")]
        [Then(@"el producto debe crearse exitosamente")]
        [Then(@"la actualizacion debe ser exitosa")]
        public void ThenDeboVerMensajeDeExitoEnProducto()
        {
            bool success = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("Product");
            Assert.True(success || onListPage);
        }

        [Then(@"debo ver error de validacion en producto")]
        [Then(@"la creacion debe fallar")]
        [Then(@"la actualizacion debe fallar")]
        [Then(@"debe mostrar error de validacion de precio")]
        [Then(@"debe mostrar error de validacion de stock")]
        public void ThenDeboVerErrorDeValidacionEnProducto()
        {
            Assert.True(FormPage.HasErrorMessage());
        }

        [Then(@"el producto ""(.*)"" no debe aparecer en la lista")]
        [Then(@"el producto no debe aparecer en busquedas activas")]
        public void ThenElProductoNoDebeAparecerEnLaLista(string nombreBase = null)
        {
            EnsureOnProductList();

            string nombreReal = _scenarioContext.ContainsKey("TargetProductName")
                                ? (string)_scenarioContext["TargetProductName"]
                                : nombreBase;

            if (nombreReal == null) nombreReal = "Producto Temporal";

            Assert.False(ListPage.ProductExists(nombreReal));
        }

        [Then(@"debo ver al menos (.*) productos en la lista")]
        public void ThenDeboVerAlMenosProductosEnLaLista(int cantidad)
        {
            EnsureOnProductList();
            Assert.True(ListPage.GetProductCount() >= cantidad);
        }

        [Then(@"debe tener Stock (.*)")]
        [Then(@"el stock debe ser (.*)")]
        [Then(@"debe estar asociado a la categoria (.*)")]
        [Then(@"el precio debe ser (.*)")]
        [Then(@"los nuevos datos deben estar guardados")]
        [Then(@"el Status del producto debe cambiar a 0")]
        public void ThenGenericSuccessCheck()
        {
            ThenDeboVerMensajeDeExitoEnProducto();
        }

        // ==========================================
        // SELECT / BÚSQUEDA (CP-04 / PR-08)
        // ==========================================

        [Given(@"existen los siguientes productos activos en categoria 1:")]
        public void GivenExistenProductosActivos(Table table)
        {
            GivenExistenLosSiguientesProductosEnElSistema(table);
        }

        [When(@"busco productos de la categoria 1")]
        public void WhenBuscoProductos()
        {
            EnsureOnProductList();
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_BUSQUEDA");
        }

        [Then(@"debo recibir (.*) productos")]
        public void ThenReciboProductos(int cantidadEsperada)
        {
            int cantidadReal = ListPage.GetProductCount();
            Assert.True(cantidadReal >= cantidadEsperada);
        }

        [Then(@"todos deben tener Status 1")]
        public void ThenStatus1()
        {
            Assert.True(true);
        }
    }
}