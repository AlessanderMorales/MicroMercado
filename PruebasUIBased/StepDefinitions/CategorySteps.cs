using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class CategorySteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public CategorySteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private CategoryListPage GetCategoryListPage()
        {
            if (!_scenarioContext.ContainsKey("CategoryListPage"))
            {
                var page = new CategoryListPage(_fixture.Driver);
                _scenarioContext["CategoryListPage"] = page;
            }
            return (CategoryListPage)_scenarioContext["CategoryListPage"];
        }

        private CategoryFormPage GetCategoryFormPage()
        {
            if (!_scenarioContext.ContainsKey("CategoryFormPage"))
            {
                var page = new CategoryFormPage(_fixture.Driver);
                _scenarioContext["CategoryFormPage"] = page;
            }
            return (CategoryFormPage)_scenarioContext["CategoryFormPage"];
        }

        [Given(@"existe una categoria creada con nombre ""(.*)""")]
        public void GivenExisteUnaCategoriaCreada(string nombre)
        {
            var listPage = GetCategoryListPage();
            
            // Primero verificar si ya existe
            if (!listPage.CategoryExists(nombre))
            {
                // Si no existe, crearla
                listPage.ClickAddNewCategory();
                System.Threading.Thread.Sleep(500);

                var formPage = GetCategoryFormPage();
                formPage.FillCategoryForm(nombre, "Descripción de prueba");
                formPage.ClickSave();
                System.Threading.Thread.Sleep(1000);

                // Volver a la lista
                listPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
                System.Threading.Thread.Sleep(500);
            }

            _scenarioContext["LastCategoryName"] = nombre;
        }

        [Given(@"existen las siguientes categorias en el sistema:")]
        public void GivenExistenLasSiguientesCategoriasEnElSistema(Table table)
        {
            var listPage = GetCategoryListPage();

            foreach (var row in table.Rows)
            {
                var nombre = row["Nombre"];
                
                if (!listPage.CategoryExists(nombre))
                {
                    listPage.ClickAddNewCategory();
                    System.Threading.Thread.Sleep(500);

                    var formPage = GetCategoryFormPage();
                    formPage.FillCategoryForm(nombre, "Descripción de prueba");
                    formPage.ClickSave();
                    System.Threading.Thread.Sleep(1000);

                    listPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        [When(@"hago clic en agregar nueva categoria")]
        public void WhenHagoClicEnAgregarNuevaCategoria()
        {
            var listPage = GetCategoryListPage();
            listPage.ClickAddNewCategory();
            System.Threading.Thread.Sleep(500);
        }

        [When(@"lleno el formulario de categoria con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenLlenoElFormularioDeCategoriaConNombreYDescripcion(string nombre, string descripcion)
        {
            var formPage = GetCategoryFormPage();
            formPage.FillCategoryForm(nombre, descripcion);
            _scenarioContext["CategoryName"] = nombre;
        }

        [When(@"hago clic en guardar categoria")]
        public void WhenHagoClicEnGuardarCategoria()
        {
            var formPage = GetCategoryFormPage();
            formPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"hago clic en editar categoria ""(.*)""")]
        public void WhenHagoClicEnEditarCategoria(string nombre)
        {
            var listPage = GetCategoryListPage();
            
            // Verificar URL antes de hacer clic
            var urlBeforeClick = listPage.GetCurrentUrl();
            
            listPage.ClickEditCategory(nombre);
            System.Threading.Thread.Sleep(3000); // Dar más tiempo para que cargue la página de edición
            
            // Verificar URL después de hacer clic
            var urlAfterClick = listPage.GetCurrentUrl();
            
            if (!urlAfterClick.Contains("/EditCategory"))
            {
                throw new Exception($"No se navegó a la página de edición. URL antes: {urlBeforeClick}, URL después: {urlAfterClick}");
            }
        }

        [When(@"actualizo el formulario con nombre ""(.*)"" y descripcion ""(.*)""")]
        public void WhenActualizoElFormularioConNombreYDescripcion(string nombre, string descripcion)
        {
            var formPage = GetCategoryFormPage();
            formPage.UpdateCategoryForm(nombre, descripcion);
            _scenarioContext["UpdatedCategoryName"] = nombre;
        }

        [When(@"hago clic en eliminar categoria ""(.*)""")]
        public void WhenHagoClicEnEliminarCategoria(string nombre)
        {
            var listPage = GetCategoryListPage();
            listPage.ClickDeleteCategory(nombre);
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"debo ver mensaje de exito en categoria")]
        public void ThenDeboVerMensajeDeExitoEnCategoria()
        {
            // Navegar de vuelta a la lista para ver el mensaje
            var listPage = GetCategoryListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/CategoryPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
                System.Threading.Thread.Sleep(500);
            }

            // Verificar que la categoría fue creada/actualizada
            if (_scenarioContext.ContainsKey("CategoryName"))
            {
                var categoryName = (string)_scenarioContext["CategoryName"];
                if (!string.IsNullOrEmpty(categoryName))
                {
                    Assert.True(listPage.CategoryExists(categoryName), 
                        $"La categoría '{categoryName}' debería existir en la lista");
                }
            }
        }

        [Then(@"debo ver error de validacion en categoria")]
        public void ThenDeboVerErrorDeValidacionEnCategoria()
        {
            var formPage = GetCategoryFormPage();
            Assert.True(formPage.HasNameValidationError() || formPage.HasErrorMessage(), 
                "Debería haber un error de validación");
        }

        [Then(@"la categoria ""(.*)"" no debe aparecer en la lista")]
        public void ThenLaCategoriaNoDebeAparecerEnLaLista(string nombre)
        {
            var listPage = GetCategoryListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/CategoryPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/CategoryPage");
                System.Threading.Thread.Sleep(500);
            }

            Assert.False(listPage.CategoryExists(nombre), 
                $"La categoría '{nombre}' no debería aparecer en la lista");
        }

        [Then(@"debo ver al menos (.*) categorias en la lista")]
        public void ThenDeboVerAlMenosCategoriasEnLaLista(int cantidad)
        {
            var listPage = GetCategoryListPage();
            var count = listPage.GetCategoryCount();
            Assert.True(count >= cantidad, 
                $"Debería haber al menos {cantidad} categorías, pero hay {count}");
        }
    }
}
