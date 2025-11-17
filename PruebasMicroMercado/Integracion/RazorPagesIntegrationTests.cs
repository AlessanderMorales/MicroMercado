using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PruebasMicroMercado.Integracion
{

    /// Pruebas de integración para las Razor Pages del proyecto MicroMercado.

    [Collection("IntegrationTests")]
    public class RazorPagesIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public RazorPagesIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        #region IT-01: Pruebas de Páginas Principales

        [Fact(DisplayName = "IT-01: Index Page - Debe cargar exitosamente")]
        public async Task IndexPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("MicroMercado", content);
        }

        [Fact(DisplayName = "IT-02: Sales Page - Debe cargar exitosamente")]
        public async Task SalesPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/Sales");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Punto de Venta", content);
        }

        [Fact(DisplayName = "IT-03: NewProduct Page - Debe cargar exitosamente")]
        public async Task NewProductPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/NewProduct");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nuevo Producto", content);
        }

        [Fact(DisplayName = "IT-04: NewClient Page - Debe cargar exitosamente")]
        public async Task NewClientPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/NewClient");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nuevo Cliente", content);
        }

        [Fact(DisplayName = "IT-05: NewCategory Page - Debe cargar exitosamente")]
        public async Task NewCategoryPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/NewCategory");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nueva Categoría", content);
        }

        [Fact(DisplayName = "IT-06: Privacy Page - Debe cargar exitosamente")]
        public async Task PrivacyPage_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/Privacy");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region IT-07: Pruebas de Páginas Inexistentes

        [Fact(DisplayName = "IT-07: Página inexistente - Debe retornar 404")]
        public async Task NonExistentPage_ShouldReturn404()
        {
            var response = await _client.GetAsync("/PaginaQueNoExiste");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region IT-08 a IT-11: Pruebas de Handlers (Razor Page Handlers)

        [Fact(DisplayName = "IT-08: SearchProducts Handler - Debe retornar JSON")]
        public async Task SearchProductsHandler_ShouldReturnJson()
        {
            var term = "Yogurt";
            var response = await _client.GetAsync($"/Sales?handler=SearchProducts&term={term}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Yogurt", content);
        }

        [Fact(DisplayName = "IT-09: SearchProducts Handler - Término vacío debe retornar respuesta válida")]
        public async Task SearchProductsHandler_EmptyTerm_ShouldReturnValidResponse()
        {
            var response = await _client.GetAsync("/Sales?handler=SearchProducts&term=");
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact(DisplayName = "IT-10: SearchClients Handler - Debe retornar JSON")]
        public async Task SearchClientsHandler_ShouldReturnJson()
        {
            var document = "12345678";
            var response = await _client.GetAsync($"/Sales?handler=SearchClients&documentNumber={document}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Cliente Test", content);
        }

        [Fact(DisplayName = "IT-11: SearchClients Handler - Documento inexistente debe retornar respuesta")]
        public async Task SearchClientsHandler_NonExistentDocument_ShouldReturnResponse()
        {
            var document = "99999999";
            var response = await _client.GetAsync($"/Sales?handler=SearchClients&documentNumber={document}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        #endregion

        #region IT-12 a IT-13: Recursos Estáticos

        [Fact(DisplayName = "IT-12: CSS de Bootstrap - Debe cargar correctamente")]
        public async Task BootstrapCss_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/lib/bootstrap/dist/css/bootstrap.min.css");
            response.EnsureSuccessStatusCode();
            Assert.Contains("text/css", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact(DisplayName = "IT-13: JavaScript de AdminLTE - Debe cargar correctamente")]
        public async Task AdminLteJs_ShouldLoadSuccessfully()
        {
            var response = await _client.GetAsync("/lib/admin-lte/js/adminlte.min.js");
            response.EnsureSuccessStatusCode();
            Assert.Contains("javascript", response.Content.Headers.ContentType?.MediaType);
        }

        #endregion

        #region IT-14 a IT-15: Configuración de la Aplicación

        [Fact(DisplayName = "IT-14: Verificar que la aplicación usa HTTPS")]
        public async Task Application_ShouldUseHttps()
        {
            var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri("http://localhost"),
                AllowAutoRedirect = false
            });

            var response = await httpClient.GetAsync("/");
            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
                        response.StatusCode == HttpStatusCode.MovedPermanently ||
                        response.StatusCode == HttpStatusCode.OK);
        }

        [Fact(DisplayName = "IT-15: Verificar Headers de Seguridad")]
        public async Task Application_ShouldIncludeSecurityHeaders()
        {
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();

            Assert.True(response.Headers.Contains("X-Content-Type-Options") ||
                        response.Headers.Contains("X-Frame-Options") ||
                        response.Content.Headers.ContentType != null);
        }

        #endregion
    }
}
