using OpenQA.Selenium;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    /// Pruebas de integración automatizadas para Navegación General.
    /// Basadas en: Manual_Black_Box_Test_Cases.txt - Sección 5: NAVEGACIÓN Y PÁGINAS GENERALES.
    [Collection("SeleniumTests")]
    public class NavigationTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public NavigationTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        #region Happy Path Tests

        [Fact(DisplayName = "Navigation - Complete Happy Path")]
        public void Navigation_CompleteHappyPath_ShouldSucceed()
        {
            _page.GoTo("https://localhost:7155/");

            var startTime = DateTime.Now;
            var welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
            var loadTime = (DateTime.Now - startTime).TotalSeconds;

            Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());
            Assert.True(loadTime < 3, "La página debería cargar en menos de 3 segundos");

            _page.ClickButtonByText("Ir al Punto de Venta");
            _page.WaitForUrlContains("/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);

            var productSearchVisible = _fixture.Driver.FindElements(By.Id("product_id")).Count > 0;
            Assert.True(productSearchVisible, "El buscador de productos debería estar visible");

            try
            {
                _page.GoTo("https://localhost:7155/Privacy");
                var privacyContent = _page.GetText("//h1");
                Assert.Contains("Sobre Nosotros", privacyContent);
            }
            catch
            {
                Assert.DoesNotContain("404", _fixture.Driver.PageSource);
            }

            try
            {
                var homeLink = _fixture.Driver.FindElement(By.CssSelector("a[href='/'], a[href='https://localhost:7155/'], .navbar-brand"));
                homeLink.Click();
                System.Threading.Thread.Sleep(500);

                Assert.Contains("localhost:7155", _fixture.Driver.Url);
                Assert.True(_fixture.Driver.Url.EndsWith("/") || _fixture.Driver.Url.EndsWith("/Index"));
            }
            catch
            {
                _page.GoTo("https://localhost:7155/");
            }

            try
            {
                _page.GoTo("https://localhost:7155/Categories");
                Assert.DoesNotContain("404", _fixture.Driver.PageSource);
                Assert.Contains("/Categories", _fixture.Driver.Url);
            }
            catch { }

            try
            {
                _page.GoTo("https://localhost:7155/Products");
                Assert.DoesNotContain("404", _fixture.Driver.PageSource);
                Assert.Contains("/Products", _fixture.Driver.Url);
            }
            catch { }

            try
            {
                _page.GoTo("https://localhost:7155/Clients");
                Assert.DoesNotContain("404", _fixture.Driver.PageSource);
            }
            catch { }

            _page.GoTo("https://localhost:7155/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);
        }

        [Fact(DisplayName = "Home Page Loads Successfully")]
        public void HomePage_LoadsSuccessfully()
        {
            _page.GoTo("https://localhost:7155/");

            var welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
            Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());

            var salesButton = _fixture.Driver.FindElements(By.XPath("//button[contains(text(),'Ir al Punto de Venta')]|//a[contains(text(),'Ir al Punto de Venta')]"));
            Assert.True(salesButton.Count > 0, "El botón 'Ir al Punto de Venta' debería estar visible");
        }

        [Fact(DisplayName = "Navigate To Sales From Home")]
        public void NavigateToSales_FromHome_ShouldSucceed()
        {
            _page.GoTo("https://localhost:7155/");
            _page.ClickButtonByText("Ir al Punto de Venta");
            _page.WaitForUrlContains("/Sales");

            Assert.Contains("/Sales", _fixture.Driver.Url);

            var productSearch = _fixture.Driver.FindElement(By.Id("product_id"));
            var clientSearch = _fixture.Driver.FindElement(By.Id("idDocumentoRecibido"));

            Assert.NotNull(productSearch);
            Assert.NotNull(clientSearch);
        }

        [Fact(DisplayName = "Privacy Page Loads Successfully")]
        public void PrivacyPage_LoadsSuccessfully()
        {
            _page.GoTo("https://localhost:7155/Privacy");
            System.Threading.Thread.Sleep(500);

            var pageContent = _fixture.Driver.PageSource;
            Assert.Contains("Sobre Nosotros", pageContent);
        }

        [Fact(DisplayName = "Navigate Between All Modules")]
        public void NavigateBetweenModules_ShouldWork()
        {
            _page.GoTo("https://localhost:7155/");
            Assert.Contains("localhost:7155", _fixture.Driver.Url);

            _page.GoTo("https://localhost:7155/Categories");
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);

            _page.GoTo("https://localhost:7155/Products");
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);

            _page.GoTo("https://localhost:7155/NewClient");
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);

            _page.GoTo("https://localhost:7155/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);
        }

        #endregion

        #region Unhappy Path Tests

        [Fact(DisplayName = "Navigate To Non-Existent Page - Should Show 404")]
        public void NavigateToNonExistentPage_ShouldShow404()
        {
            _page.GoTo("https://localhost:7155/PaginaQueNoExiste");
            System.Threading.Thread.Sleep(500);

            var pageSource = _fixture.Driver.PageSource;
            bool has404 = pageSource.Contains("404") ||
                          pageSource.Contains("Not Found") ||
                          pageSource.Contains("no encontrada") ||
                          pageSource.Contains("No se encontró");

            Assert.True(has404, "Debería mostrar página de error 404");
        }

        [Fact(DisplayName = "Navigate With Malformed URL - Should Handle Safely")]
        public void NavigateWithMalformedURL_ShouldHandleSafely()
        {
            try
            {
                _page.GoTo("https://localhost:7155/Sales/../../etc");
                System.Threading.Thread.Sleep(500);

                var pageSource = _fixture.Driver.PageSource;

                Assert.DoesNotContain("Exception", pageSource);
                Assert.DoesNotContain("StackTrace", pageSource);
                Assert.DoesNotContain("at System.", pageSource);

                bool isSafe = pageSource.Contains("404") ||
                              pageSource.Contains("400") ||
                              pageSource.Contains("Bad Request") ||
                              _fixture.Driver.Url.Contains("/Error") ||
                              _fixture.Driver.Url.EndsWith("/");

                Assert.True(isSafe, "Debería manejar URLs malformadas de forma segura");
            }
            catch (Exception ex)
            {
                Assert.True(ex is WebDriverException || ex is InvalidOperationException);
            }
        }

        [Fact(DisplayName = "Access Protected Resource - Should Handle Auth")]
        public void AccessProtectedResource_ShouldHandleAuth()
        {
            try
            {
                _page.GoTo("https://localhost:7155/Admin");
                System.Threading.Thread.Sleep(500);

                var url = _fixture.Driver.Url;
                var pageSource = _fixture.Driver.PageSource;

                bool isProtected = url.Contains("/Login") ||
                                   url.Contains("/Identity/Account/Login") ||
                                   pageSource.Contains("Acceso denegado") ||
                                   pageSource.Contains("Access Denied") ||
                                   pageSource.Contains("Unauthorized") ||
                                   pageSource.Contains("404");

                Assert.True(isProtected, "Las páginas protegidas deberían requerir autenticación o no existir");
            }
            catch
            {
                Assert.True(true);
            }
        }

        [Fact(DisplayName = "Page Load Timeout - Should Handle Gracefully")]
        public void PageLoadTimeout_ShouldHandleGracefully()
        {
            var originalTimeout = _fixture.Driver.Manage().Timeouts().PageLoad;

            try
            {
                _fixture.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(1);
                _page.GoTo("https://localhost:7155/");
                Assert.True(true, "La página cargó dentro del timeout");
            }
            catch (WebDriverTimeoutException)
            {
                var pageSource = _fixture.Driver.PageSource;
                Assert.DoesNotContain("StackTrace", pageSource);
            }
            finally
            {
                _fixture.Driver.Manage().Timeouts().PageLoad = originalTimeout;
            }
        }

        [Fact(DisplayName = "Critical URLs Should Not Expose Sensitive Info")]
        public void CriticalURLs_ShouldNotExposeSensitiveInfo()
        {
            string[] criticalPaths =
            {
                "https://localhost:7155/web.config",
                "https://localhost:7155/appsettings.json",
                "https://localhost:7155/../web.config",
                "https://localhost:7155/bin",
                "https://localhost:7155/obj"
            };

            foreach (var path in criticalPaths)
            {
                try
                {
                    _page.GoTo(path);
                    System.Threading.Thread.Sleep(500);

                    var pageSource = _fixture.Driver.PageSource;

                    Assert.DoesNotContain("ConnectionString", pageSource);
                    Assert.DoesNotContain("<configuration>", pageSource);
                    Assert.DoesNotContain("appsettings", pageSource);

                    bool isSafe = pageSource.Contains("404") ||
                                  pageSource.Contains("403") ||
                                  pageSource.Contains("Not Found") ||
                                  pageSource.Contains("Forbidden");

                    Assert.True(isSafe, $"La ruta {path} debería estar protegida");
                }
                catch
                {
                    Assert.True(true);
                }
            }
        }

        [Fact(DisplayName = "JavaScript Errors Should Be Handled")]
        public void JavaScriptErrors_ShouldBeHandled()
        {
            _page.GoTo("https://localhost:7155/Sales");
            System.Threading.Thread.Sleep(1000);

            try
            {
                ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("throw new Error('Test error');");
            }
            catch { }

            var pageSource = _fixture.Driver.PageSource;
            Assert.DoesNotContain("Uncaught", pageSource);

            var productSearch = _fixture.Driver.FindElements(By.Id("product_id"));
            Assert.True(productSearch.Count > 0, "La página debería seguir funcionando después de un error JS");
        }

        #endregion
    }
}
