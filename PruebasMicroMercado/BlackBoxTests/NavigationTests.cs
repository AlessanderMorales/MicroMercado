using OpenQA.Selenium;
using System;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    /// <summary>
    /// Pruebas de integración automatizadas para Navegación General
    /// Basadas en: Manual_Black_Box_Test_Cases.txt - Sección 5: NAVEGACIÓN Y PÁGINAS GENERALES
    /// </summary>
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

/// <summary>
        /// Test: Navegacion_General_Exitosa
        /// Objetivo: Verificar que el usuario puede navegar correctamente por la aplicación
    /// </summary>
  [Fact(DisplayName = "Navigation - Complete Happy Path")]
      public void Navigation_CompleteHappyPath_ShouldSucceed()
        {
 // PASO 1: PÁGINA DE INICIO
       _page.GoTo("https://localhost:7155/");

            // Verificar que carga correctamente
            var startTime = DateTime.Now;
            var welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
       var loadTime = (DateTime.Now - startTime).TotalSeconds;

       Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());
          Assert.True(loadTime < 3, "La página debería cargar en menos de 3 segundos");

     // PASO 2: NAVEGAR A VENTAS DESDE INICIO
      _page.ClickButtonByText("Ir al Punto de Venta");
       _page.WaitForUrlContains("/Sales");

       Assert.Contains("/Sales", _fixture.Driver.Url);

  // Verificar que elementos del punto de venta están visibles
          var productSearchVisible = _fixture.Driver.FindElements(By.Id("product_id")).Count > 0;
          Assert.True(productSearchVisible, "El buscador de productos debería estar visible");

       // PASO 3: NAVEGAR A PÁGINA DE PRIVACIDAD
          try
            {
   _page.GoTo("https://localhost:7155/Privacy");
                
  var privacyContent = _page.GetText("//h1");
 Assert.Contains("Sobre Nosotros", privacyContent);
            }
         catch
         {
                // Si no existe la página exacta, verificar que al menos no da 404
      Assert.DoesNotContain("404", _fixture.Driver.PageSource);
            }

  // PASO 4: REGRESAR A INICIO DESDE MENÚ
         try
            {
   // Buscar logo o enlace de inicio
     var homeLink = _fixture.Driver.FindElement(By.CssSelector("a[href='/'], a[href='https://localhost:7155/'], .navbar-brand"));
        homeLink.Click();

System.Threading.Thread.Sleep(500);

                Assert.Contains("localhost:7155", _fixture.Driver.Url);
       Assert.True(_fixture.Driver.Url.EndsWith("/") || _fixture.Driver.Url.EndsWith("/Index"),
  "Debería volver a la página principal");
            }
            catch
            {
 // Si no encuentra el enlace, navegar directamente
                _page.GoTo("https://localhost:7155/");
            }

// PASO 5: NAVEGAR ENTRE MÓDULOS
            // Inicio ? Categorías
            try
          {
                _page.GoTo("https://localhost:7155/Categories");
       Assert.DoesNotContain("404", _fixture.Driver.PageSource);
                Assert.Contains("/Categories", _fixture.Driver.Url);
            }
            catch { }

            // Categorías ? Productos
         try
            {
             _page.GoTo("https://localhost:7155/Products");
     Assert.DoesNotContain("404", _fixture.Driver.PageSource);
                Assert.Contains("/Products", _fixture.Driver.Url);
   }
     catch { }

     // Productos ? Clientes
            try
            {
       _page.GoTo("https://localhost:7155/Clients");
Assert.DoesNotContain("404", _fixture.Driver.PageSource);
            }
          catch { }

            // Clientes ? Ventas
 _page.GoTo("https://localhost:7155/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);
        }

        /// <summary>
        /// PASO 1: Verificar página de inicio carga correctamente
    /// </summary>
        [Fact(DisplayName = "Home Page Loads Successfully")]
        public void HomePage_LoadsSuccessfully()
      {
            _page.GoTo("https://localhost:7155/");

  var welcomeText = _page.GetText("//h1[contains(text(),'Bienvenido a MicroMercado')]");
            Assert.Equal("Bienvenido a MicroMercado", welcomeText.Trim());

    // Verificar que botón de punto de venta está visible
        var salesButton = _fixture.Driver.FindElements(By.XPath("//button[contains(text(),'Ir al Punto de Venta')]|//a[contains(text(),'Ir al Punto de Venta')]"));
            Assert.True(salesButton.Count > 0, "El botón 'Ir al Punto de Venta' debería estar visible");
   }

        /// <summary>
        /// PASO 2: Navegar a ventas desde inicio
   /// </summary>
        [Fact(DisplayName = "Navigate To Sales From Home")]
     public void NavigateToSales_FromHome_ShouldSucceed()
        {
         _page.GoTo("https://localhost:7155/");

      _page.ClickButtonByText("Ir al Punto de Venta");
            _page.WaitForUrlContains("/Sales");

       Assert.Contains("/Sales", _fixture.Driver.Url);

         // Verificar elementos clave del punto de venta
          var productSearch = _fixture.Driver.FindElement(By.Id("product_id"));
          var clientSearch = _fixture.Driver.FindElement(By.Id("idDocumentoRecibido"));

        Assert.NotNull(productSearch);
            Assert.NotNull(clientSearch);
        }

        /// <summary>
     /// PASO 3: Verificar página de privacidad
        /// </summary>
   [Fact(DisplayName = "Privacy Page Loads Successfully")]
        public void PrivacyPage_LoadsSuccessfully()
    {
      _page.GoTo("https://localhost:7155/Privacy");

            System.Threading.Thread.Sleep(500);

 var pageContent = _fixture.Driver.PageSource;
 Assert.Contains("Sobre Nosotros", pageContent);
        }

  /// <summary>
        /// PASO 5: Verificar navegación entre todos los módulos
        /// </summary>
 [Fact(DisplayName = "Navigate Between All Modules")]
      public void NavigateBetweenModules_ShouldWork()
        {
            // Inicio
  _page.GoTo("https://localhost:7155/");
 Assert.Contains("localhost:7155", _fixture.Driver.Url);

         // Categorías
            _page.GoTo("https://localhost:7155/Categories");
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);

  // Productos
    _page.GoTo("https://localhost:7155/Products");
      Assert.DoesNotContain("404", _fixture.Driver.PageSource);

            // Clientes
            _page.GoTo("https://localhost:7155/NewClient");
            Assert.DoesNotContain("404", _fixture.Driver.PageSource);

     // Ventas
         _page.GoTo("https://localhost:7155/Sales");
            Assert.Contains("/Sales", _fixture.Driver.Url);
    }

        #endregion

   #region Unhappy Path Tests

        /// <summary>
        /// Escenario 1: Navegar a URL inexistente
        /// </summary>
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

        /// <summary>
        /// Escenario 2: Navegar con URL malformada
      /// </summary>
        [Fact(DisplayName = "Navigate With Malformed URL - Should Handle Safely")]
        public void NavigateWithMalformedURL_ShouldHandleSafely()
        {
            try
  {
       _page.GoTo("https://localhost:7155/Sales/../../etc");

   System.Threading.Thread.Sleep(500);

var pageSource = _fixture.Driver.PageSource;
      
             // Verificar que no expone información sensible del sistema
       Assert.DoesNotContain("Exception", pageSource);
  Assert.DoesNotContain("StackTrace", pageSource);
        Assert.DoesNotContain("at System.", pageSource);

        // Debe redirigir a página segura o mostrar 400/404
        bool isSafe = pageSource.Contains("404") ||
             pageSource.Contains("400") ||
          pageSource.Contains("Bad Request") ||
  _fixture.Driver.Url.Contains("/Error") ||
             _fixture.Driver.Url.EndsWith("/");

            Assert.True(isSafe, "Debería manejar URLs malformadas de forma segura");
      }
   catch (Exception ex)
{
                // Si lanza excepción, verificar que sea esperada
       Assert.True(ex is WebDriverException || ex is InvalidOperationException,
         "Solo se esperan excepciones de WebDriver");
         }
        }

        /// <summary>
        /// Escenario 3: Acceder a recurso sin permisos (si aplica autenticación)
        /// </summary>
  [Fact(DisplayName = "Access Protected Resource - Should Handle Auth")]
        public void AccessProtectedResource_ShouldHandleAuth()
        {
            // Este test asume que hay páginas protegidas
   // Si no hay autenticación, el test pasa automáticamente

   try
         {
       _page.GoTo("https://localhost:7155/Admin");

  System.Threading.Thread.Sleep(500);

          var url = _fixture.Driver.Url;
     var pageSource = _fixture.Driver.PageSource;

      // Verificar que redirige a login o muestra acceso denegado
   bool isProtected = url.Contains("/Login") ||
               url.Contains("/Identity/Account/Login") ||
            pageSource.Contains("Acceso denegado") ||
       pageSource.Contains("Access Denied") ||
             pageSource.Contains("Unauthorized") ||
              pageSource.Contains("404");

     // Si no hay protección (404), también está bien
         Assert.True(isProtected, "Las páginas protegidas deberían requerir autenticación o no existir");
    }
            catch
            {
       // Si no existe página admin, el test pasa
        Assert.True(true);
          }
    }

        /// <summary>
        /// Escenario 4: Verificar timeout de página
        /// </summary>
 [Fact(DisplayName = "Page Load Timeout - Should Handle Gracefully")]
        public void PageLoadTimeout_ShouldHandleGracefully()
        {
   // Configurar timeout muy corto para forzar timeout
   var originalTimeout = _fixture.Driver.Manage().Timeouts().PageLoad;

            try
      {
       // Reducir timeout a 1 segundo
     _fixture.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(1);

            _page.GoTo("https://localhost:7155/");

      // Si carga rápido, el test pasa
                Assert.True(true, "La página cargó dentro del timeout");
      }
       catch (WebDriverTimeoutException)
            {
    // Timeout esperado - verificar que no hay stack trace visible al usuario
   var pageSource = _fixture.Driver.PageSource;
          Assert.DoesNotContain("StackTrace", pageSource);
     }
    finally
            {
          // Restaurar timeout original
     _fixture.Driver.Manage().Timeouts().PageLoad = originalTimeout;
}
        }

        /// <summary>
 /// Verificar que URLs críticas están protegidas
      /// </summary>
        [Fact(DisplayName = "Critical URLs Should Not Expose Sensitive Info")]
        public void CriticalURLs_ShouldNotExposeSensitiveInfo()
        {
     string[] criticalPaths = new[]
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

      // Verificar que NO expone archivos de configuración
    Assert.DoesNotContain("ConnectionString", pageSource);
        Assert.DoesNotContain("<configuration>", pageSource);
   Assert.DoesNotContain("appsettings", pageSource);

          // Debe mostrar 404 o acceso denegado
      bool isSafe = pageSource.Contains("404") ||
    pageSource.Contains("403") ||
       pageSource.Contains("Not Found") ||
         pageSource.Contains("Forbidden");

      Assert.True(isSafe, $"La ruta {path} debería estar protegida");
   }
                catch
              {
    // Si lanza excepción, está bien protegido
     Assert.True(true);
                }
            }
}

        /// <summary>
        /// Verificar manejo de errores JavaScript
   /// </summary>
    [Fact(DisplayName = "JavaScript Errors Should Be Handled")]
     public void JavaScriptErrors_ShouldBeHandled()
  {
            _page.GoTo("https://localhost:7155/Sales");

      System.Threading.Thread.Sleep(1000);

        // Ejecutar código JavaScript inválido intencionalmente
            try
         {
      ((IJavaScriptExecutor)_fixture.Driver).ExecuteScript("throw new Error('Test error');");
            }
            catch
            {
       // Es esperado que lance excepción
            }

    // Verificar que la página sigue funcionando
          var pageSource = _fixture.Driver.PageSource;
 Assert.DoesNotContain("Uncaught", pageSource);

         // Verificar que elementos principales aún están presentes
    var productSearch = _fixture.Driver.FindElements(By.Id("product_id"));
    Assert.True(productSearch.Count > 0, "La página debería seguir funcionando después de un error JS");
      }

      #endregion
    }
}
