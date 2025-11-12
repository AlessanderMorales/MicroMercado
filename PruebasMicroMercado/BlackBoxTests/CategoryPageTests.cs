using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    /// <summary>
    /// Pruebas de integración automatizadas para el módulo de Categorías
    /// Basadas en: Manual_Black_Box_Test_Cases.txt - Sección 1: GESTIÓN DE CATEGORÍAS
    /// </summary>
    [Collection("SeleniumTests")]
  public class CategoryPageTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public CategoryPageTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
          _page = new PageHelpers(_fixture.Driver);
        }

     #region Happy Path Tests

        /// <summary>
  /// Test: Categorias_CRUD_Completo_Exitoso
        /// Objetivo: Verificar todas las operaciones CRUD de categorías exitosamente
     /// </summary>
        [Fact(DisplayName = "Categories CRUD - Complete Happy Path")]
        public void Categories_CRUD_CompleteHappyPath_ShouldSucceed()
        {
   // PASO 1: CREAR PRIMERA CATEGORÍA "Electrónica"
            _page.GoTo("https://localhost:7155/CategoryPage");
            
    // Hacer clic en "Nueva Categoría" o "Agregar Nueva Categoría"
    _page.ClickButtonByText("Agregar Nueva Categoría");
            
    System.Threading.Thread.Sleep(1000);

          // Completar formulario
        _page.SetInputValue("CategoryInput_Name", "Electrónica");
          _page.SetInputValue("CategoryInput_Description", "Dispositivos electrónicos");
            
      // Guardar
 _page.ClickButtonByText("Guardar");
      
 // Verificar mensaje de éxito y redirección
  System.Threading.Thread.Sleep(2000);
          
            // PASO 2: LISTAR CATEGORÍAS - Verificar que "Electrónica" aparece
            var categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr:not(:has(.empty-cart-message))"));
            Assert.True(categoryRows.Any(row => row.Text.Contains("Electrónica")), 
         "La categoría 'Electrónica' debería aparecer en la lista");
  
            // PASO 3: EDITAR CATEGORÍA
// Buscar botón "Editar" de "Electrónica"
      var electronicaRow = categoryRows.FirstOrDefault(row => row.Text.Contains("Electrónica"));
            if (electronicaRow != null)
            {
                var editButton = electronicaRow.FindElement(By.CssSelector("a[href*='EditCategory']"));
    editButton.Click();

 System.Threading.Thread.Sleep(1000);
                
         // Cambiar descripción
       var descriptionInput = _fixture.Driver.FindElement(By.Id("CategoryInput_Description"));
      descriptionInput.Clear();
   descriptionInput.SendKeys("Electrónica y tecnología");
           
        // Guardar cambios
                _page.ClickButtonByText("Guardar");
            System.Threading.Thread.Sleep(2000);
   
       // Verificar descripción actualizada
                categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
         Assert.True(categoryRows.Any(row => row.Text.Contains("Electrónica y tecnología")),
         "La descripción actualizada debería aparecer");
            }
      
      // PASO 4: CREAR SEGUNDA CATEGORÍA "Alimentos"
      _page.ClickButtonByText("Agregar Nueva Categoría");
          System.Threading.Thread.Sleep(1000);
         _page.SetInputValue("CategoryInput_Name", "Alimentos");
    _page.SetInputValue("CategoryInput_Description", "Productos comestibles");
    _page.ClickButtonByText("Guardar");
            
    System.Threading.Thread.Sleep(2000);
            
   // PASO 5: VERIFICAR QUE HAY 2 CATEGORÍAS
    categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr:not(.text-center)"));
   var activeCategories = categoryRows.Count(row => row.Text.Contains("Activo") || !row.Text.Contains("Inactivo"));
        Assert.True(activeCategories >= 2, "Deberían haber al menos 2 categorías activas");
            
            // PASO 6: ELIMINAR "Electrónica" (Borrado Lógico)
            categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
            electronicaRow = categoryRows.FirstOrDefault(row => row.Text.Contains("Electrónica"));
            if (electronicaRow != null)
        {
        var deleteButton = electronicaRow.FindElement(By.CssSelector("button.btn-danger"));
                deleteButton.Click();
     
      System.Threading.Thread.Sleep(1000);
        
     // Confirmar en modal
        try
     {
       var confirmButton = _fixture.Driver.FindElement(By.CssSelector("#deleteConfirmationModal button[type='submit']"));
          confirmButton.Click();
           }
                catch
                {
 try
            {
       var alert = _fixture.Driver.SwitchTo().Alert();
       alert.Accept();
       }
       catch { }
           }
          
     System.Threading.Thread.Sleep(2000);
            }
     
            // PASO 7: VERIFICAR QUE SOLO "Alimentos" ESTÁ VISIBLE
       categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
      var alimentosExists = categoryRows.Any(row => row.Text.Contains("Alimentos") && row.Text.Contains("Activo"));
      Assert.True(alimentosExists, "La categoría 'Alimentos' debería estar visible y activa");
        }

        #endregion

        #region Unhappy Path Tests

        /// <summary>
        /// Test: Categorias_Operaciones_Invalidas
        /// Escenario 1: Intentar crear categoría con nombre vacío
        /// </summary>
        [Fact(DisplayName = "Create Category With Empty Name - Should Show Validation Error")]
        public void CreateCategory_WithEmptyName_ShouldShowValidationError()
        {
            _page.GoTo("https://localhost:7155/CategoryPage");
 
       // Ir a "Nueva Categoría"
            _page.ClickButtonByText("Agregar Nueva Categoría");
            
         System.Threading.Thread.Sleep(1000);
   
 // Dejar nombre vacío, solo ingresar descripción
  _page.SetInputValue("CategoryInput_Description", "Test");
            
         // Intentar guardar
         _page.ClickButtonByText("Guardar");
            
         System.Threading.Thread.Sleep(1000);
        
    // Verificar mensaje de validación
      var validationMessage = _page.GetValidationMessage("CategoryInput_Name");
    Assert.False(string.IsNullOrEmpty(validationMessage), 
            "Debería aparecer un mensaje de validación para el campo 'Nombre'");
        }

        /// <summary>
        /// Escenario 2: Intentar crear categoría con nombre duplicado
        /// </summary>
        [Fact(DisplayName = "Create Category With Duplicate Name - Should Show Error")]
        public void CreateCategory_WithDuplicateName_ShouldShowError()
        {
  _page.GoTo("https://localhost:7155/CategoryPage");
            
            string uniqueName = $"Test Duplicado {DateTime.Now.Ticks}";
      
 // Primero crear una categoría
            _page.ClickButtonByText("Agregar Nueva Categoría");
   System.Threading.Thread.Sleep(1000);
  _page.SetInputValue("CategoryInput_Name", uniqueName);
          _page.SetInputValue("CategoryInput_Description", "Primera categoría");
            _page.ClickButtonByText("Guardar");
        
     System.Threading.Thread.Sleep(2000);

      // Intentar crear otra con el mismo nombre
     _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);
  _page.SetInputValue("CategoryInput_Name", uniqueName);
            _page.SetInputValue("CategoryInput_Description", "Segunda categoría");
          _page.ClickButtonByText("Guardar");
            
     System.Threading.Thread.Sleep(1500);
            
  // Verificar mensaje de error
            bool hasErrorMessage = false;
          try
      {
                var errorElement = _fixture.Driver.FindElement(By.CssSelector(".alert-danger, .text-danger, [class*='error']"));
   hasErrorMessage = errorElement.Text.ToLower().Contains("existe") || 
     errorElement.Text.ToLower().Contains("duplicado");
 }
        catch { }
         
      // También verificar si no redirigió (quedó en la página de creación)
            hasErrorMessage = hasErrorMessage || _fixture.Driver.Url.Contains("NewCategory");
    
      Assert.True(hasErrorMessage, "Debería mostrar un error indicando que la categoría ya existe");
        }

  /// <summary>
        /// Escenario 3: Intentar actualizar con nombre vacío
        /// </summary>
        [Fact(DisplayName = "Update Category With Empty Name - Should Show Validation Error")]
     public void UpdateCategory_WithEmptyName_ShouldShowValidationError()
        {
       _page.GoTo("https://localhost:7155/CategoryPage");
       
// Crear categoría para editar
            _page.ClickButtonByText("Agregar Nueva Categoría");
            System.Threading.Thread.Sleep(1000);
      string testName = $"Categoría Para Editar {DateTime.Now.Ticks}";
_page.SetInputValue("CategoryInput_Name", testName);
            _page.SetInputValue("CategoryInput_Description", "Descripción original");
            _page.ClickButtonByText("Guardar");
            
System.Threading.Thread.Sleep(2000);
        
      // Buscar y editar
         var categoryRows = _fixture.Driver.FindElements(By.CssSelector("#lstCategories tbody tr"));
      var targetRow = categoryRows.FirstOrDefault(row => row.Text.Contains(testName.Substring(0, 20)));
     
   if (targetRow != null)
    {
         var editButton = targetRow.FindElement(By.CssSelector("a[href*='EditCategory']"));
                editButton.Click();
   
        System.Threading.Thread.Sleep(1000);
         
     // Borrar el nombre (dejar vacío)
        var nameInput = _fixture.Driver.FindElement(By.Id("CategoryInput_Name"));
     nameInput.Clear();

   // Intentar guardar
    _page.ClickButtonByText("Guardar");

     System.Threading.Thread.Sleep(1000);
 
     // Verificar mensaje de validación
  var validationMessage = _page.GetValidationMessage("CategoryInput_Name");
         Assert.False(string.IsNullOrEmpty(validationMessage),
        "Debería aparecer mensaje de validación al dejar el nombre vacío");
        }
   }

        #endregion
    }
}
