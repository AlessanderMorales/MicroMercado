using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class ProductSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public ProductSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private ProductListPage GetProductListPage()
        {
            if (!_scenarioContext.ContainsKey("ProductListPage"))
            {
                var page = new ProductListPage(_fixture.Driver);
                _scenarioContext["ProductListPage"] = page;
            }
            return (ProductListPage)_scenarioContext["ProductListPage"];
        }

        private ProductFormPage GetProductFormPage()
        {
            if (!_scenarioContext.ContainsKey("ProductFormPage"))
            {
                var page = new ProductFormPage(_fixture.Driver);
                _scenarioContext["ProductFormPage"] = page;
            }
            return (ProductFormPage)_scenarioContext["ProductFormPage"];
        }

        [Given(@"existe una categoria ""(.*)"" para productos")]
        public void GivenExisteUnaCategoria(string categoryName)
        {
            // Este paso es principalmente documental
            // Asumimos que la categoría ya existe en la base de datos
            _scenarioContext["CategoryForProducts"] = "1"; // ID de la categoría
        }

        [Given(@"existe un producto creado con nombre ""(.*)""")]
        public void GivenExisteUnProductoCreado(string nombre)
        {
            var listPage = GetProductListPage();
            
            if (!listPage.ProductExists(nombre))
            {
                listPage.ClickAddNewProduct();
                System.Threading.Thread.Sleep(500);

                var formPage = GetProductFormPage();
                formPage.FillProductForm(nombre, "Descripción de prueba", "MarcaPrueba", 10.50m, 100, "1");
                formPage.ClickSave();
                System.Threading.Thread.Sleep(1000);

                listPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
                System.Threading.Thread.Sleep(500);
            }

            _scenarioContext["LastProductName"] = nombre;
        }

        [Given(@"existen los siguientes productos en el sistema:")]
        public void GivenExistenLosSiguientesProductosEnElSistema(Table table)
        {
            var listPage = GetProductListPage();

            foreach (var row in table.Rows)
            {
                var nombre = row["Nombre"];
                
                if (!listPage.ProductExists(nombre))
                {
                    listPage.ClickAddNewProduct();
                    System.Threading.Thread.Sleep(500);

                    var formPage = GetProductFormPage();
                    formPage.FillProductForm(nombre, "Descripción de prueba", "MarcaPrueba", 10.00m, 50, "1");
                    formPage.ClickSave();
                    System.Threading.Thread.Sleep(1000);

                    listPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        [When(@"hago clic en agregar nuevo producto")]
        public void WhenHagoClicEnAgregarNuevoProducto()
        {
            var listPage = GetProductListPage();
            listPage.ClickAddNewProduct();
            System.Threading.Thread.Sleep(500);
        }

        [When(@"lleno el formulario de producto con los siguientes datos:")]
        public void WhenLlenoElFormularioDeProductoConLosSiguientesDatos(Table table)
        {
            var formPage = GetProductFormPage();
            
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
                    case "Nombre":
                        nombre = valor;
                        break;
                    case "Descripcion":
                        descripcion = valor;
                        break;
                    case "Marca":
                        marca = valor;
                        break;
                    case "Precio":
                        precio = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "Stock":
                        stock = int.Parse(valor);
                        break;
                    case "Categoria":
                        categoria = valor;
                        break;
                }
            }

            formPage.FillProductForm(nombre, descripcion, marca, precio, stock, categoria);
            _scenarioContext["ProductName"] = nombre;
        }

        [When(@"hago clic en guardar producto")]
        public void WhenHagoClicEnGuardarProducto()
        {
            var formPage = GetProductFormPage();
            formPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"hago clic en editar producto ""(.*)""")]
        public void WhenHagoClicEnEditarProducto(string nombre)
        {
            var listPage = GetProductListPage();
            listPage.ClickEditProduct(nombre);
            System.Threading.Thread.Sleep(2000);
        }

        [When(@"actualizo el formulario de producto con:")]
        public void WhenActualizoElFormularioDeProductoCon(Table table)
        {
            var formPage = GetProductFormPage();
            
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
                    case "Nombre":
                        nombre = valor;
                        break;
                    case "Descripcion":
                        descripcion = valor;
                        break;
                    case "Precio":
                        precio = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "Stock":
                        stock = int.Parse(valor);
                        break;
                }
            }

            formPage.UpdateProductForm(nombre, descripcion, "MarcaPrueba", precio, stock, "1");
            _scenarioContext["UpdatedProductName"] = nombre;
        }

        [When(@"hago clic en eliminar producto ""(.*)""")]
        public void WhenHagoClicEnEliminarProducto(string nombre)
        {
            var listPage = GetProductListPage();
            listPage.ClickDeleteProduct(nombre);
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"debo ver mensaje de exito en producto")]
        public void ThenDeboVerMensajeDeExitoEnProducto()
        {
            var listPage = GetProductListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/ProductPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
                System.Threading.Thread.Sleep(500);
            }

            if (_scenarioContext.ContainsKey("ProductName"))
            {
                var productName = (string)_scenarioContext["ProductName"];
                if (!string.IsNullOrEmpty(productName))
                {
                    Assert.True(listPage.ProductExists(productName), 
                        $"El producto '{productName}' debería existir en la lista");
                }
            }
        }

        [Then(@"debo ver error de validacion en producto")]
        public void ThenDeboVerErrorDeValidacionEnProducto()
        {
            var formPage = GetProductFormPage();
            Assert.True(formPage.HasErrorMessage(), 
                "Debería haber un error de validación");
        }

        [Then(@"el producto ""(.*)"" no debe aparecer en la lista")]
        public void ThenElProductoNoDebeAparecerEnLaLista(string nombre)
        {
            var listPage = GetProductListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/ProductPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/ProductPage");
                System.Threading.Thread.Sleep(500);
            }

            Assert.False(listPage.ProductExists(nombre), 
                $"El producto '{nombre}' no debería aparecer en la lista");
        }

        [Then(@"debo ver al menos (.*) productos en la lista")]
        public void ThenDeboVerAlMenosProductosEnLaLista(int cantidad)
        {
            var listPage = GetProductListPage();
            var count = listPage.GetProductCount();
            Assert.True(count >= cantidad, 
                $"Debería haber al menos {cantidad} productos, pero hay {count}");
        }
    }
}
