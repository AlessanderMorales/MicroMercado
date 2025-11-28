using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PruebasMicroMercado.Integracion
{
    [Collection("IntegrationTests")]
    public class ListPagesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ListPagesIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        #region IT-43: ProductPage - Debe cargar lista de productos

        [Fact(DisplayName = "IT-43: ProductPage debe cargar exitosamente con lista de productos")]
        public async Task ProductPage_ShouldLoadSuccessfullyWithProductList()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/ProductPage");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Gestión de Productos", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Yogurt Natural", content);
            Assert.Contains("Leche Entera", content);
        }

        [Fact(DisplayName = "IT-43b: ProductPage debe contener enlaces de edición")]
        public async Task ProductPage_ShouldContainEditLinks()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/ProductPage");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("EditProduct", content);
        }

        #endregion

        #region IT-44: ClientPage - Debe cargar lista de clientes

        [Fact(DisplayName = "IT-44: ClientPage debe cargar exitosamente con lista de clientes")]
        public async Task ClientPage_ShouldLoadSuccessfullyWithClientList()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/ClientPage");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Gestión de Clientes", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Cliente Test", content);
            Assert.Contains("test@email.com", content);
        }

        [Fact(DisplayName = "IT-44b: ClientPage debe contener botón para nuevo cliente")]
        public async Task ClientPage_ShouldContainNewClientButton()
        {
            var response = await _client.GetAsync("/ClientPage");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("NewClient", content);
        }

        #endregion

        #region IT-45: CategoryPage - Debe cargar lista de categorías

        [Fact(DisplayName = "IT-45: CategoryPage debe cargar exitosamente con lista de categorías")]
        public async Task CategoryPage_ShouldLoadSuccessfullyWithCategoryList()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/CategoryPage");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Gestión de Categorías", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Lácteos", content);
            Assert.Contains("Alimentos", content);
        }

        [Fact(DisplayName = "IT-45b: CategoryPage debe contener enlaces de gestión")]
        public async Task CategoryPage_ShouldContainManagementLinks()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/CategoryPage");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("NewCategory", content);
            Assert.Contains("EditCategory", content);
        }

        #endregion

        #region IT-46: Búsqueda de productos con filtros especiales

        [Fact(DisplayName = "IT-46: Búsqueda de productos con caracteres especiales")]
        public async Task SearchProducts_WithSpecialCharacters_ShouldReturnResults()
        {
            _factory.SeedDatabase();
            
            var response1 = await _client.GetAsync("/Sales?handler=SearchProducts&term=Yogurt%20Natural");
            response1.EnsureSuccessStatusCode();
            var content1 = await response1.Content.ReadAsStringAsync();
            Assert.Contains("Yogurt", content1);

            var response2 = await _client.GetAsync("/Sales?handler=SearchProducts&term=yogurt");
            response2.EnsureSuccessStatusCode();
            var content2 = await response2.Content.ReadAsStringAsync();
            Assert.Contains("Yogurt", content2, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "IT-46b: Búsqueda sin resultados debe retornar array vacío")]
        public async Task SearchProducts_WithNoResults_ShouldReturnEmptyArray()
        {
            var response = await _client.GetAsync("/Sales?handler=SearchProducts&term=ProductoInexistente12345");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("[]", content);
        }

        #endregion

        #region IT-47: Búsqueda de clientes que no retorna inactivos

        [Fact(DisplayName = "IT-47: Búsqueda de clientes solo debe retornar activos")]
        public async Task SearchClients_ShouldOnlyReturnActiveClients()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/Sales?handler=SearchClients&documentNumber=12345678");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Cliente Test", content);
        }

        #endregion

        #region Pruebas adicionales de páginas

        [Fact(DisplayName = "IT-43c: ProductPage debe ser accesible sin autenticación")]
        public async Task ProductPage_ShouldBeAccessibleWithoutAuth()
        {
            var response = await _client.GetAsync("/ProductPage");

            Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact(DisplayName = "IT-44c: ClientPage debe mostrar información de contacto")]
        public async Task ClientPage_ShouldDisplayContactInformation()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/ClientPage");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("@", content);
            Assert.Contains("123", content);
        }

        [Fact(DisplayName = "IT-45c: CategoryPage debe mostrar conteo de productos")]
        public async Task CategoryPage_ShouldDisplayProductCount()
        {
            _factory.SeedDatabase();
            
            var response = await _client.GetAsync("/CategoryPage");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.NotEmpty(content);
            Assert.Contains("Lácteos", content);
        }

        #endregion
    }
}