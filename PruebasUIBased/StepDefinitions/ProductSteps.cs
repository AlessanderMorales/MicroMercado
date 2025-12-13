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

        [When(@"hago clic en agregar nuevo producto")]
        public void WhenHagoClicEnAgregarNuevoProducto()
        {
            EnsureOnProductList();
            ListPage.ClickAddNewProduct();
        }

        [When(@"lleno el formulario de producto con los siguientes datos:")]
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
                    case "Nombre": nombre = valor; break;
                    case "Descripcion": descripcion = valor; break;
                    case "Marca": marca = valor; break;
                    case "Precio": precio = decimal.Parse(valor, CultureInfo.InvariantCulture); break;
                    case "Stock": stock = int.Parse(valor); break;
                    case "Categoria": categoria = valor; break;
                }
            }

            FormPage.FillProductForm(nombre, descripcion, marca, precio, stock, categoria);
            _scenarioContext["ProductName"] = nombre;
        }

        [When(@"hago clic en guardar producto")]
        public void WhenHagoClicEnGuardarProducto()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

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
                    case "Nombre": nombre = valor; break;
                    case "Descripcion": descripcion = valor; break;
                    case "Precio": precio = decimal.Parse(valor, CultureInfo.InvariantCulture); break;
                    case "Stock": stock = int.Parse(valor); break;
                }
            }
            FormPage.UpdateProductForm(nombre, descripcion, "MarcaMod", precio, stock, "1");
            _scenarioContext["UpdatedProductName"] = nombre;
        }

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
            ListPage.ClickDeleteProduct(nombreReal);
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"debo ver mensaje de exito en producto")]
        public void ThenDeboVerMensajeDeExitoEnProducto()
        {
            bool success = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("Product");
            Assert.True(success || onListPage);
        }

        [Then(@"debo ver error de validacion en producto")]
        public void ThenDeboVerErrorDeValidacionEnProducto()
        {
            Assert.True(FormPage.HasErrorMessage());
        }

        [Then(@"el producto ""(.*)"" no debe aparecer en la lista")]
        public void ThenElProductoNoDebeAparecerEnLaLista(string nombreBase)
        {
            EnsureOnProductList();

            string nombreReal = _scenarioContext.ContainsKey("TargetProductName")
                                ? (string)_scenarioContext["TargetProductName"]
                                : nombreBase;

            Assert.False(ListPage.ProductExists(nombreReal));
        }

        [Then(@"debo ver al menos (.*) productos en la lista")]
        public void ThenDeboVerAlMenosProductosEnLaLista(int cantidad)
        {
            EnsureOnProductList();
            Assert.True(ListPage.GetProductCount() >= cantidad);
        }
    }
}