using OpenQA.Selenium;
using Xunit;
using PruebasMicroMercado.BlackBoxTests;

namespace PruebasMicroMercado.BlackBoxHYU
{
    /// Pruebas Happy Path para CRUD de Clientes
    /// Solo casos exitosos - flujo ideal sin errores
    [Collection("SeleniumTests")]
    public class ClientsHappyPathTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly HappyPathHelpers _helper;
        private const string BASE_URL = "https://localhost:7155";

        public ClientsHappyPathTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _helper = new HappyPathHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "HYU-C01: Complete Client CRUD Happy Path")]
        public void CompleteClientCRUD_HappyPath()
        {
            var rnd = new System.Random();
            string businessName = $"Cliente Happy Test {rnd.Next(1000, 9999)}";
            string taxDocument = $"{10000000 + rnd.Next(0, 89999999)}";
            string email = $"happytest{rnd.Next(1000, 9999)}@example.com";
            string address = "Av. Test 123, La Paz";

            // === PASO 1: CREATE - Crear nuevo cliente ===
            _helper.GoTo($"{BASE_URL}/NewClient");
            _helper.SetInputValue("NewClient_BusinessName", businessName);
            _helper.SetInputValue("NewClient_TaxDocument", taxDocument);
            _helper.SetInputValue("NewClient_Email", email);
            _helper.SetInputValue("NewClient_Address", address);

            _helper.ClickButtonByText("Guardar Cliente");
            _helper.WaitForUrlContains("/Sales"); // ? La app redirige a Sales
            
            // Navegar a ClientPage para verificar que el cliente fue creado
            _helper.GoTo($"{BASE_URL}/ClientPage");

            // Verificar que el cliente aparece en la lista
            Assert.True(_helper.IsRowPresent("lstClients", businessName), "El cliente creado debe aparecer en la tabla");

            // === PASO 2: READ - Verificar que se puede ver la información del cliente ===
            Assert.True(_helper.IsRowPresent("lstClients", taxDocument), "El documento tributario debe aparecer en la tabla");
            Assert.True(_helper.IsRowPresent("lstClients", email), "El email debe aparecer en la tabla");

            // === PASO 3: UPDATE - Editar el cliente ===
            _helper.ClickEditButtonForRow(businessName);
            _helper.WaitForUrlContains("/EditClient");

            string updatedBusinessName = $"{businessName} - Actualizado";
            string updatedAddress = "Av. Actualizada 456, Santa Cruz";

            _helper.SetInputValue("UpdateClient_BusinessName", updatedBusinessName);
            _helper.SetInputValue("UpdateClient_Address", updatedAddress);

            _helper.ClickButtonByText("Guardar Cambios");
            _helper.WaitForUrlContains("/ClientPage"); // EditClient sí redirige a ClientPage

            // Verificar que los cambios se guardaron
            Assert.True(_helper.IsRowPresent("lstClients", updatedBusinessName), "El nombre actualizado debe aparecer en la tabla");

            // === PASO 4: DELETE - Eliminar el cliente (borrado lógico) ===
            _helper.ClickDeleteButtonForRow(updatedBusinessName);
            _helper.ConfirmDeleteModal();

            // Verificar que el cliente ya no aparece en la lista
            _helper.GoTo($"{BASE_URL}/ClientPage"); // Refrescar la página
            Assert.False(_helper.IsRowPresent("lstClients", updatedBusinessName), "El cliente eliminado no debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-C02: Create Multiple Clients Successfully")]
        public void CreateMultipleClients_HappyPath()
        {
            var rnd = new System.Random();
            int clientCount = 3;

            for (int i = 0; i < clientCount; i++)
            {
                string businessName = $"Cliente Batch {rnd.Next(1000, 9999)}";
                string taxDocument = $"{10000000 + rnd.Next(0, 89999999)}";
                string email = $"batch{rnd.Next(1000, 9999)}@example.com";

                _helper.GoTo($"{BASE_URL}/NewClient");
                _helper.SetInputValue("NewClient_BusinessName", businessName);
                _helper.SetInputValue("NewClient_TaxDocument", taxDocument);
                _helper.SetInputValue("NewClient_Email", email);

                _helper.ClickButtonByText("Guardar Cliente");
                _helper.WaitForUrlContains("/Sales"); // ? Redirige a Sales
 
      // Navegar a ClientPage para verificar
         _helper.GoTo($"{BASE_URL}/ClientPage");
    Assert.True(_helper.IsRowPresent("lstClients", businessName), $"El cliente {i + 1} debe aparecer en la tabla");
    }
        }

      [Fact(DisplayName = "HYU-C03: Update Client Email Successfully")]
        public void UpdateClientEmail_HappyPath()
        {
          var rnd = new System.Random();
            string businessName = $"Cliente Email Test {rnd.Next(1000, 9999)}";
       string taxDocument = $"{10000000 + rnd.Next(0, 89999999)}";
   string originalEmail = $"original{rnd.Next(1000, 9999)}@example.com";
    string updatedEmail = $"updated{rnd.Next(1000, 9999)}@example.com";

   // Crear cliente
    _helper.GoTo($"{BASE_URL}/NewClient");
            _helper.SetInputValue("NewClient_BusinessName", businessName);
_helper.SetInputValue("NewClient_TaxDocument", taxDocument);
            _helper.SetInputValue("NewClient_Email", originalEmail);
 _helper.ClickButtonByText("Guardar Cliente");
      _helper.WaitForUrlContains("/Sales"); // ? Redirige a Sales
 
   // Navegar a ClientPage
            _helper.GoTo($"{BASE_URL}/ClientPage");

          // Actualizar email
       _helper.ClickEditButtonForRow(businessName);
            _helper.WaitForUrlContains("/EditClient");
      _helper.SetInputValue("UpdateClient_Email", updatedEmail);
            _helper.ClickButtonByText("Guardar Cambios");
      _helper.WaitForUrlContains("/ClientPage");

    // Verificar actualización
         Assert.True(_helper.IsRowPresent("lstClients", updatedEmail), "El email actualizado debe aparecer en la tabla");
     }

        [Fact(DisplayName = "HYU-C04: Create Client With All Fields")]
        public void CreateClientWithAllFields_HappyPath()
        {
            var rnd = new System.Random();
            string businessName = $"Cliente Completo {rnd.Next(1000, 9999)}";
     string taxDocument = $"{10000000 + rnd.Next(0, 89999999)}";
  string email = $"completo{rnd.Next(1000, 9999)}@example.com";
     string address = "Calle Principal #123, Edificio Torre, Piso 5, Oficina 501, La Paz, Bolivia";

            _helper.GoTo($"{BASE_URL}/NewClient");
        _helper.SetInputValue("NewClient_BusinessName", businessName);
       _helper.SetInputValue("NewClient_TaxDocument", taxDocument);
            _helper.SetInputValue("NewClient_Email", email);
 _helper.SetInputValue("NewClient_Address", address);

    _helper.ClickButtonByText("Guardar Cliente");
            _helper.WaitForUrlContains("/Sales"); // ? Redirige a Sales
            
        // Navegar a ClientPage para verificar
            _helper.GoTo($"{BASE_URL}/ClientPage");
        Assert.True(_helper.IsRowPresent("lstClients", businessName), "El cliente con todos los campos debe aparecer en la tabla");
        }

        [Fact(DisplayName = "HYU-C05: Update Client Address Successfully")]
        public void UpdateClientAddress_HappyPath()
        {
   var rnd = new System.Random();
       string businessName = $"Cliente Address {rnd.Next(1000, 9999)}";
 string taxDocument = $"{10000000 + rnd.Next(0, 89999999)}";
            string email = $"address{rnd.Next(1000, 9999)}@example.com";
       string originalAddress = "Dirección Original 123";
  string updatedAddress = "Nueva Dirección Actualizada 456, Santa Cruz";

            // Crear cliente
   _helper.GoTo($"{BASE_URL}/NewClient");
      _helper.SetInputValue("NewClient_BusinessName", businessName);
            _helper.SetInputValue("NewClient_TaxDocument", taxDocument);
            _helper.SetInputValue("NewClient_Email", email);
            _helper.SetInputValue("NewClient_Address", originalAddress);
  _helper.ClickButtonByText("Guardar Cliente");
   _helper.WaitForUrlContains("/Sales"); // ? Redirige a Sales
            
       // Navegar a ClientPage
            _helper.GoTo($"{BASE_URL}/ClientPage");

          // Actualizar dirección
      _helper.ClickEditButtonForRow(businessName);
       _helper.WaitForUrlContains("/EditClient");
      _helper.SetInputValue("UpdateClient_Address", updatedAddress);
            _helper.ClickButtonByText("Guardar Cambios");
            _helper.WaitForUrlContains("/ClientPage");

         // Verificar actualización (happy path)
            Assert.True(_helper.IsRowPresent("lstClients", businessName), "El cliente debe seguir en la tabla después de actualizar la dirección");
}
    }
}
