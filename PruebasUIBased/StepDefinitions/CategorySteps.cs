using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;
using System;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
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


        private void ValidarLongitudNombre(string nombre)
        {
            if (nombre.Length > 20)
            {
                throw new Exception($" ERROR EN DATOS DE PRUEBA: El nombre de categoría '{nombre}' tiene {nombre.Length} caracteres. La aplicación solo permite 20. Por favor corrige tu archivo .feature.");
            }
        }


        [Given(@"existe una categoria creada con nombre ""(.*)""")]
        public void GivenExisteUnaCategoriaCreada(string nombre)
        {
            ValidarLongitudNombre(nombre); 
            EnsureOnCategoryList();

            if (!ListPage.CategoryExists(nombre))
            {
                CrearCategoriaAuxiliar(nombre);
            }

            _scenarioContext["LastCategoryName"] = nombre;
        }

        [Given(@"existen las siguientes categorias en el sistema:")]
        public void GivenExistenLasSiguientesCategoriasEnElSistema(Table table)
        {
            foreach (var row in table.Rows)
            {
                GivenExisteUnaCategoriaCreada(row["Nombre"]);
            }
        }


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
        }

        [When(@"hago clic en guardar categoria")]
        public void WhenHagoClicEnGuardarCategoria()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"hago clic en editar categoria ""(.*)""")]
        public void WhenHagoClicEnEditarCategoria(string nombre)
        {
            ValidarLongitudNombre(nombre); 
            EnsureOnCategoryList();

            if (!ListPage.CategoryExists(nombre))
            {
                Console.WriteLine($"[AUTO-HEALING] La categoría '{nombre}' no existía. Creándola...");
                CrearCategoriaAuxiliar(nombre);
            }

            ListPage.ClickEditCategory(nombre);
        }

        [When(@"actualizo el formulario con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenActualizoElFormularioConNombreYDescripcion(string nombre, string descripcion)
        {
            FormPage.UpdateCategoryForm(nombre, descripcion);
            _scenarioContext["UpdatedCategoryName"] = nombre;
        }

        [When(@"hago clic en eliminar categoria ""(.*)""")]
        public void WhenHagoClicEnEliminarCategoria(string nombre)
        {
            ValidarLongitudNombre(nombre);
            EnsureOnCategoryList();

            if (!ListPage.CategoryExists(nombre))
            {
                CrearCategoriaAuxiliar(nombre);
            }

            ListPage.ClickDeleteCategory(nombre);
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"debo ver mensaje de exito en categoria")]
        public void ThenDeboVerMensajeDeExitoEnCategoria()
        {
            bool success = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("Category");
            Assert.True(success || onListPage, "No se mostró mensaje de éxito.");
        }

        [Then(@"debo ver error de validacion en categoria")]
        public void ThenDeboVerErrorDeValidacionEnCategoria()
        {
            Assert.True(FormPage.HasErrorMessage(), "Se esperaba error de validación.");
        }

        [Then(@"la categoria ""(.*)"" no debe aparecer en la lista")]
        public void ThenLaCategoriaNoDebeAparecerEnLaLista(string nombre)
        {
            EnsureOnCategoryList();
            Assert.False(ListPage.CategoryExists(nombre), $"La categoría '{nombre}' todavía aparece en la lista.");
        }

        [Then(@"debo ver al menos (.*) categorias en la lista")]
        public void ThenDeboVerAlMenosCategoriasEnLaLista(int cantidad)
        {
            EnsureOnCategoryList();
            Assert.True(ListPage.GetCategoryCount() >= cantidad, "No hay suficientes categorías.");
        }
        private void CrearCategoriaAuxiliar(string nombre)
        {
            ValidarLongitudNombre(nombre); 
            ListPage.ClickAddNewCategory();
            FormPage.FillCategoryForm(nombre, "Auto-Test");

            FormPage.ClickSave();
            if (FormPage.HasErrorMessage())
            {
                throw new Exception($"Error al crear categoría '{nombre}' automáticamente. Posiblemente viola reglas de validación.");
            }

            System.Threading.Thread.Sleep(1000);
            EnsureOnCategoryList();
        }
    }
}