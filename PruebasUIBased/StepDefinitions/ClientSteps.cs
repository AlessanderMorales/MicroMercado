using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;
using System;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class ClientSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;
        private ClientListPage _clientListPage;
        private ClientFormPage _clientFormPage;

        public ClientSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        // Lazy Loading
        private ClientListPage ListPage => _clientListPage ??= new ClientListPage(_fixture.Driver);
        private ClientFormPage FormPage => _clientFormPage ??= new ClientFormPage(_fixture.Driver);

        /// <summary>
        /// Método CRÍTICO: Asegura que el navegador esté en la Lista de Clientes.
        /// Corrige el error de "NoSuchElement: NewClient" al intentar crear múltiples clientes seguidos.
        /// </summary>
        private void EnsureOnClientList()
        {
            var currentUrl = ListPage.GetCurrentUrl();

            // Verificamos si NO estamos en la página de lista (ClientPage)
            // O si estamos atrapados en la página de creación (NewClient) o Edición (Edit)
            if (!currentUrl.Contains("/ClientPage") || currentUrl.Contains("/NewClient") || currentUrl.Contains("/Edit"))
            {
                Console.WriteLine($"Navegando a la Lista de Clientes. URL actual: {currentUrl}");
                // URL corregida basada en tu HTML (@page de ClientPageModel)
                ListPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                System.Threading.Thread.Sleep(500); // Pequeña espera para carga de red
            }
        }

        [Given(@"existe un cliente creado con documento ""(.*)""")]
        public void GivenExisteUnClienteCreadoConDocumento(string documento)
        {
            // 1. Asegurar que estamos en la lista antes de buscar
            EnsureOnClientList();

            // 2. Verificar si ya existe
            if (!ListPage.ClientExists(documento))
            {
                // 3. Si no existe, clic en Nuevo (Aquí fallaba antes porque no estaba en la lista)
                ListPage.ClickAddNewClient();

                // 4. Llenar formulario
                FormPage.FillClientForm($"Cliente {documento}", $"test{documento}@test.com", documento, "Dirección Test");
                FormPage.ClickSave();

                // 5. Esperar procesamiento y volver a asegurar estar en la lista
                System.Threading.Thread.Sleep(1000);
                EnsureOnClientList();
            }
            _scenarioContext["LastClientDocument"] = documento;
        }

        [Given(@"existen los siguientes clientes en el sistema:")]
        public void GivenExistenLosSiguientesClientesEnElSistema(Table table)
        {
            foreach (var row in table.Rows)
            {
                GivenExisteUnClienteCreadoConDocumento(row["Documento"]);
            }
        }

        [When(@"hago clic en agregar nuevo cliente")]
        public void WhenHagoClicEnAgregarNuevoCliente()
        {
            EnsureOnClientList();
            ListPage.ClickAddNewClient();
        }

        [When(@"lleno el formulario de cliente con los siguientes datos:")]
        public void WhenLlenoElFormularioDeClienteConLosSiguientesDatos(Table table)
        {
            string nombre = "", email = "", documento = "", direccion = "";
            foreach (var row in table.Rows)
            {
                if (row["Campo"] == "Nombre") nombre = row["Valor"];
                if (row["Campo"] == "Email") email = row["Valor"];
                if (row["Campo"] == "Documento") documento = row["Valor"];
                if (row["Campo"] == "Direccion") direccion = row["Valor"];
            }
            FormPage.FillClientForm(nombre, email, documento, direccion);
            _scenarioContext["ClientName"] = nombre;
        }

        [When(@"hago clic en guardar cliente")]
        public void WhenHagoClicEnGuardarCliente()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"hago clic en editar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEditarElClienteConDocumento(string documento)
        {
            EnsureOnClientList();
            ListPage.ClickEditClient(documento);
        }

        [When(@"actualizo el formulario de cliente con:")]
        public void WhenActualizoElFormularioDeClienteCon(Table table)
        {
            string nombre = "", email = "", direccion = "";
            string documento = _scenarioContext.ContainsKey("LastClientDocument") ? (string)_scenarioContext["LastClientDocument"] : "";

            foreach (var row in table.Rows)
            {
                if (row["Campo"] == "Nombre") nombre = row["Valor"];
                if (row["Campo"] == "Email") email = row["Valor"];
                if (row["Campo"] == "Direccion") direccion = row["Valor"];
            }
            FormPage.UpdateClientForm(nombre, email, documento, direccion);
        }

        [When(@"hago clic en eliminar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEliminarElClienteConDocumento(string documento)
        {
            EnsureOnClientList();
            ListPage.ClickDeleteClient(documento);
            _scenarioContext["DeletedClientDocument"] = documento;
        }

        [Then(@"debo ver mensaje de exito en cliente")]
        public void ThenDeboVerMensajeDeExitoEnCliente()
        {
            bool hasSuccess = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("/ClientPage");

            Assert.True(hasSuccess || onListPage, "No se mostró mensaje de éxito.");
        }

        [Then(@"debo ver error de validacion en cliente")]
        public void ThenDeboVerErrorDeValidacionEnCliente()
        {
            Assert.True(FormPage.HasErrorMessage(), "Se esperaba un mensaje de error de validación.");
        }

        [Then(@"el cliente con documento ""(.*)"" no debe aparecer en la lista")]
        public void ThenElClienteConDocumentoNoDebeAparecerEnLaLista(string documento)
        {
            EnsureOnClientList();
            Assert.False(ListPage.ClientExists(documento), $"El cliente {documento} no debería aparecer en la lista.");
        }

        [Then(@"debo ver al menos (.*) clientes en la lista")]
        public void ThenDeboVerAlMenosClientesEnLaLista(int cantidad)
        {
            EnsureOnClientList();
            int count = ListPage.GetClientCount();
            Assert.True(count >= cantidad, $"Se esperaban al menos {cantidad} clientes, pero hay {count}.");
        }
    }
}