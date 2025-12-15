using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll; 
using Xunit;
using System;
using System.Linq;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    [Scope(Tag = "category")]
    public class CategorySteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        private CategoryListPage _listPage;
        private CategoryFormPage _formPage;

        public CategorySteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private CategoryListPage ListPage => _listPage ??= new CategoryListPage(_fixture.Driver);
        private CategoryFormPage FormPage => _formPage ??= new CategoryFormPage(_fixture.Driver);

        /// <summary>
        /// Navega a la lista si no estamos ahí.
        /// </summary>
        private void EnsureOnCategoryList()
        {
            var currentUrl = ListPage.GetCurrentUrl();
            if (!currentUrl.Contains("Category"))
            {
                ListPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
            }
        }

        /// <summary>
        /// Crea una categoría auxiliar si no existe.
        /// </summary>
        private void CrearCategoriaAuxiliar(string nombre)
        {
            ListPage.ClickAddNewCategory();
            FormPage.FillCategoryForm(nombre, "Auto-Test-Setup");
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
            EnsureOnCategoryList();
        }

        /// <summary>
        /// Genera nombre único para pruebas de eliminación si el nombre contiene "ParaEliminar".
        /// </summary>
        private string ObtenerNombreReal(string nombreBase)
        {
            if (nombreBase.Contains("ParaEliminar", StringComparison.OrdinalIgnoreCase))
            {
                var random = new Random();
                return $"{nombreBase}{random.Next(10, 99)}";
            }
            return nombreBase;
        }

        // ==========================================
        // GIVEN
        // ==========================================

        [Given(@"que la aplicacion esta en ejecucion")]
        public void GivenQueLaAplicacionEstaEnEjecucion()
        {
            _fixture.Driver.Manage().Window.Maximize();
        }

        [Given(@"navego a la pagina de categorias")]
        [When(@"navego a la pagina de categorias")] 
        public void GivenNavegoALaPaginaDeCategorias()
        {
            ListPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
            EnsureOnCategoryList();
        }

        [Given(@"la base de datos esta limpia")]
        public void GivenLaBaseDeDatosEstaLimpia()
        {
            EnsureOnCategoryList();
        }

        [Given(@"existe una categoria creada con nombre ""(.*)""")]
        public void GivenExisteUnaCategoriaCreada(string nombreBase)
        {
            EnsureOnCategoryList();

            string nombreReal = ObtenerNombreReal(nombreBase);
            _scenarioContext["TargetCategoryName"] = nombreReal;

            if (!ListPage.CategoryExists(nombreReal))
            {
                CrearCategoriaAuxiliar(nombreReal);
            }
        }

        [Given(@"existen las siguientes categorias en el sistema:")]
        public void GivenExistenLasSiguientesCategoriasEnElSistema(Table table)
        {
            EnsureOnCategoryList();
            foreach (var row in table.Rows)
            {
                string nombre = row["Nombre"];
                if (!ListPage.CategoryExists(nombre))
                {
                    CrearCategoriaAuxiliar(nombre);
                }
            }
        }

        // ==========================================
        // CREATE
        // ==========================================

        [When(@"hago clic en agregar nueva categoria")]
        public void WhenHagoClicEnAgregarNuevaCategoria()
        {
            EnsureOnCategoryList();
            ListPage.ClickAddNewCategory();
        }

        [When(@"lleno el formulario de categoria con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenLlenoElFormularioDeCategoriaConNombreYDescripcion(string nombre, string descripcion)
        {
            FormPage.FillCategoryForm(nombre, descripcion);
            _scenarioContext["CategoryName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_CREACION_LISTOS");
        }

        [When(@"hago clic en guardar categoria")]
        public void WhenHagoClicEnGuardarCategoria()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        // ==========================================
        // UPDATE
        // ==========================================

        [When(@"hago clic en editar categoria ""(.*)""")]
        public void WhenHagoClicEnEditarCategoria(string nombre)
        {
            EnsureOnCategoryList();
            if (!ListPage.CategoryExists(nombre))
            {
                CrearCategoriaAuxiliar(nombre);
            }
            ListPage.ClickEditCategory(nombre);
        }

        [When(@"actualizo el formulario con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenActualizoElFormularioConNombreYDescripcion(string nombre, string descripcion)
        {
            FormPage.UpdateCategoryForm(nombre, descripcion);
            if (!string.IsNullOrEmpty(nombre)) _scenarioContext["UpdatedCategoryName"] = nombre;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EDICION_LISTOS");
        }

        // ==========================================
        // DELETE
        // ==========================================

        [When(@"hago clic en eliminar categoria ""(.*)""")]
        public void WhenHagoClicEnEliminarCategoria(string nombreBase)
        {
            EnsureOnCategoryList();

            string nombreReal = _scenarioContext.ContainsKey("TargetCategoryName")
                                ? _scenarioContext["TargetCategoryName"].ToString()
                                : nombreBase;

            if (!ListPage.CategoryExists(nombreReal)) CrearCategoriaAuxiliar(nombreReal);
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "PREVIO_ELIMINAR");

            ListPage.ClickDeleteCategory(nombreReal);
            System.Threading.Thread.Sleep(1000);
        }

        // ==========================================
        // ASSERTIONS (THEN)
        // ==========================================

        [Then(@"debo ver mensaje de exito en categoria")]
        public void ThenDeboVerMensajeDeExitoEnCategoria()
        {
            System.Threading.Thread.Sleep(1000);
            bool success = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("CategoryPage") &&
                              !ListPage.GetCurrentUrl().Contains("New") &&
                              !ListPage.GetCurrentUrl().Contains("Edit");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_EXITOSO");

            Assert.True(success || onListPage, "No se mostró mensaje de éxito o no redirigió a la lista.");
        }

        [Then(@"debo ver error de validacion en categoria")]
        [Then(@"la creacion debe fallar")] 
        public void ThenDeboVerErrorDeValidacionEnCategoria()
        {
            System.Threading.Thread.Sleep(1000);
            bool hasError = FormPage.HasErrorMessage();

            bool stillOnForm = ListPage.GetCurrentUrl().Contains("New") ||
                               ListPage.GetCurrentUrl().Contains("Edit") ||
                               ListPage.GetCurrentUrl().Contains("Create");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_FALLO_ESPERADO");

            Assert.True(hasError || stillOnForm, "Se esperaba un error de validación, pero la operación pareció exitosa.");
        }

        [Then(@"la categoria ""(.*)"" no debe aparecer en la lista")]
        public void ThenLaCategoriaNoDebeAparecerEnLaLista(string nombreBase)
        {
            EnsureOnCategoryList();
            string nombreReal = _scenarioContext.ContainsKey("TargetCategoryName")
                                ? _scenarioContext["TargetCategoryName"].ToString()
                                : nombreBase;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "VALIDACION_NO_EXISTE");

            Assert.False(ListPage.CategoryExists(nombreReal), $"La categoría '{nombreReal}' sigue apareciendo en la lista.");
        }

        [Then(@"debo ver al menos (.*) categorias en la lista")]
        public void ThenDeboVerAlMenosCategoriasEnLaLista(int cantidad)
        {
            EnsureOnCategoryList();
            int count = ListPage.GetCategoryCount();

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_LISTADO");

            Assert.True(count >= cantidad, $"Se esperaban {cantidad} categorías, hay {count}.");
        }
    }
}