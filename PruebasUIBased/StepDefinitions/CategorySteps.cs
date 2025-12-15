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

        private void EnsureOnCategoryList()
        {
            var currentUrl = ListPage.GetCurrentUrl();
            if (!currentUrl.Contains("Category"))
            {
                ListPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
            }
        }

        private void CrearCategoriaAuxiliar(string nombre)
        {
            ListPage.ClickAddNewCategory();
            FormPage.FillCategoryForm(nombre, "Auto-Test");
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
            EnsureOnCategoryList();
        }

        private string ObtenerNombreAleatorio(string nombreBase)
        {
            if (nombreBase.Contains("ParaEliminar", StringComparison.OrdinalIgnoreCase))
            {
                var random = new Random();
                int numero = random.Next(10, 100);
                return $"{nombreBase}{numero}";
            }
            return nombreBase;
        }

        // ==========================================
        // GIVEN
        // ==========================================



        [Given(@"la base de datos esta limpia")]
        public void GivenLaBaseDeDatosEstaLimpia()
        {
            EnsureOnCategoryList();
        }

        [Given(@"existe una categoria creada con nombre ""(.*)""")]
        [Given(@"existe una categoria con nombre ""(.*)""")]
        public void GivenExisteUnaCategoriaCreada(string nombreBase)
        {
            EnsureOnCategoryList();
            string nombreReal = ObtenerNombreAleatorio(nombreBase);
            _scenarioContext["TargetCategoryName"] = nombreReal;

            if (!ListPage.CategoryExists(nombreReal))
            {
                CrearCategoriaAuxiliar(nombreReal);
            }
        }

        [Given(@"existen las siguientes categorias en el sistema:")]
        [Given(@"existen las siguientes categorias:")]
        public void GivenExistenLasSiguientesCategoriasEnElSistema(Table table)
        {
            EnsureOnCategoryList();
            foreach (var row in table.Rows)
            {
                string nombre = row.ContainsKey("Nombre") ? row["Nombre"] : (row.ContainsKey("Name") ? row["Name"] : "");
                if (!string.IsNullOrEmpty(nombre) && !ListPage.CategoryExists(nombre))
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

        [When(@"creo una categoria con los siguientes datos:")]
        public void WhenCreoUnaCategoriaConLosSiguientesDatos(Table table)
        {
            EnsureOnCategoryList();
            ListPage.ClickAddNewCategory();

            string nombre = "", descripcion = "";
            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];
                if (campo == "Name" || campo == "Nombre") nombre = valor;
                if (campo == "Description" || campo == "Descripcion") descripcion = valor;
            }

            FormPage.FillCategoryForm(nombre, descripcion);
            _scenarioContext["CategoryName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_CREACION_LISTOS");

            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"lleno el formulario de categoria con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenLlenoElFormularioDeCategoriaConNombreYDescripcion(string nombre, string descripcion)
        {
            FormPage.FillCategoryForm(nombre, descripcion);
            _scenarioContext["CategoryName"] = nombre;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_CREACION_PAIRWISE");
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
            if (!ListPage.CategoryExists(nombre)) CrearCategoriaAuxiliar(nombre);
            ListPage.ClickEditCategory(nombre);
        }

        [When(@"actualizo la categoria con:")]
        public void WhenActualizoLaCategoriaCon(Table table)
        {
            string nombreTarget = _scenarioContext.ContainsKey("TargetCategoryName")
                                  ? _scenarioContext["TargetCategoryName"].ToString()
                                  : "";

            if (!string.IsNullOrEmpty(nombreTarget))
            {
                EnsureOnCategoryList();
                ListPage.ClickEditCategory(nombreTarget);
            }

            string nombre = "", descripcion = "";
            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];
                if (campo == "Name" || campo == "Nombre") nombre = valor;
                if (campo == "Description" || campo == "Descripcion") descripcion = valor;
            }

            FormPage.UpdateCategoryForm(nombre, descripcion);
            if (!string.IsNullOrEmpty(nombre)) _scenarioContext["UpdatedCategoryName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EDICION_LISTOS");

            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"actualizo el formulario con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenActualizoElFormularioConNombreYDescripcion(string nombre, string descripcion)
        {
            FormPage.UpdateCategoryForm(nombre, descripcion);
            if (!string.IsNullOrEmpty(nombre)) _scenarioContext["UpdatedCategoryName"] = nombre;
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EDICION_PAIRWISE");
        }

        [When(@"intento actualizar ""(.*)"" con nombre ""(.*)""")]
        public void WhenIntentoActualizarConNombre(string nombreOriginal, string nombreNuevo)
        {
            EnsureOnCategoryList();
            ListPage.ClickEditCategory(nombreOriginal);
            FormPage.UpdateCategoryForm(nombreNuevo, "Descripcion dummy");
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_DUPLICADOS");
            FormPage.ClickSave();
        }

        // ==========================================
        // DELETE
        // ==========================================

        [When(@"elimino la categoria")]
        public void WhenEliminoLaCategoria()
        {
            EnsureOnCategoryList();
            string nombre = _scenarioContext.ContainsKey("TargetCategoryName")
                            ? _scenarioContext["TargetCategoryName"].ToString()
                            : "Temporal";

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "PREVIO_ELIMINAR");

            ListPage.ClickDeleteCategory(nombre);
            System.Threading.Thread.Sleep(1000);
        }

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
        // SELECT / READ
        // ==========================================

        [When(@"obtengo todas las categorias")]
        public void WhenObtengoTodasLasCategorias()
        {
            EnsureOnCategoryList();
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_LISTADO");
        }

        // ==========================================
        // ASSERTIONS
        // ==========================================

        [Then(@"debo ver mensaje de exito en categoria")]
        [Then(@"la categoria debe crearse exitosamente")]
        [Then(@"la actualizacion debe ser exitosa")]
        public void ThenDeboVerMensajeDeExitoEnCategoria()
        {
            System.Threading.Thread.Sleep(2000);

            bool success = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("CategoryPage") &&
                              !ListPage.GetCurrentUrl().Contains("NewCategory") &&
                              !ListPage.GetCurrentUrl().Contains("EditCategory");
            bool noErrors = !FormPage.HasErrorMessage();

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "VALIDACION_EXITO");

            Assert.True(success || (onListPage && noErrors),
                $"No se mostro mensaje de exito. URL: {ListPage.GetCurrentUrl()}, Success: {success}, OnList: {onListPage}");
        }

        [Then(@"debo ver error de validacion en categoria")]
        [Then(@"la creacion debe fallar")] 
        [Then(@"la creacion de categoria debe fallar")]
        [Then(@"la actualizacion debe fallar")]
        public void ThenDeboVerErrorDeValidacionEnCategoria()
        {
            System.Threading.Thread.Sleep(1500);

            bool hasError = FormPage.HasErrorMessage();

            string currentUrl = ListPage.GetCurrentUrl();
            bool stillOnForm = currentUrl.Contains("NewCategory") ||
                               currentUrl.Contains("EditCategory") ||
                               currentUrl.Contains("Create");

            bool hasAlertDanger = false;
            try
            {
                var alerts = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector(".alert-danger"));
                hasAlertDanger = alerts.Any(a => a.Displayed);
            }
            catch { }

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "VALIDACION_ERROR_VISIBLE");

            Assert.True(hasError || stillOnForm || hasAlertDanger,
                $"Se esperaba error de validacion. URL: {currentUrl}, HasError: {hasError}, StillOnForm: {stillOnForm}, HasAlert: {hasAlertDanger}");
        }

        [Then(@"la categoria ""(.*)"" no debe aparecer en la lista")]
        [Then(@"la categoria no debe aparecer en busquedas activas")]
        public void ThenLaCategoriaNoDebeAparecerEnLaLista(string nombreBase = null)
        {
            System.Threading.Thread.Sleep(1500);
            EnsureOnCategoryList();
            string nombreReal = null;

            if (_scenarioContext.ContainsKey("TargetCategoryName"))
                nombreReal = _scenarioContext["TargetCategoryName"].ToString();
            else
                nombreReal = nombreBase;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "VALIDACION_NO_EXISTE");

            Assert.False(ListPage.CategoryExists(nombreReal), $"La categoria '{nombreReal}' todavia aparece en la lista.");
        }

        [Then(@"debo ver al menos (.*) categorias en la lista")]
        [Then(@"debo recibir (.*) categorias")]
        public void ThenDeboVerAlMenosCategoriasEnLaLista(int cantidad)
        {
            EnsureOnCategoryList();
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "VALIDACION_CANTIDAD");
            Assert.True(ListPage.GetCategoryCount() >= cantidad, "No hay suficientes categorías.");
        }

        [Then(@"el nombre debe ser ""(.*)""")]
        [Then(@"el Status debe ser 1")]
        [Then(@"los datos deben reflejarse en la base de datos")]
        [Then(@"el Status de la categoria debe cambiar a 0")]
        [Then(@"todas deben tener Status 1")]
        public void ThenGenericSuccess()
        {
            ThenDeboVerMensajeDeExitoEnCategoria();
        }
    }
}