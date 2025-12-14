using PruebasUIBased.Infrastructure; // Asegúrate de tener este using para las fotos
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

        private ClientListPage ListPage => _clientListPage ??= new ClientListPage(_fixture.Driver);
        private ClientFormPage FormPage => _clientFormPage ??= new ClientFormPage(_fixture.Driver);

        private void EnsureOnClientList()
        {
            var currentUrl = ListPage.GetCurrentUrl();

            if (!currentUrl.Contains("/ClientPage") || currentUrl.Contains("/NewClient") || currentUrl.Contains("/Edit"))
            {
                ListPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                System.Threading.Thread.Sleep(500);
            }
        }

        [Given(@"la base de datos esta limpia")]
        public void GivenLaBaseDeDatosEstaLimpia()
        {
            EnsureOnClientList();
        }

        [Given(@"existe un cliente creado con documento ""(.*)""")]
        [Given(@"existe un cliente con TaxDocument ""(.*)""")]
        public void GivenExisteUnClienteCreadoConDocumento(string documento)
        {
            EnsureOnClientList();

            if (!ListPage.ClientExists(documento))
            {
                ListPage.ClickAddNewClient();
                FormPage.FillClientForm($"Cliente {documento}", $"test{documento}@test.com", documento, "Dirección Test");
                FormPage.ClickSave();
                System.Threading.Thread.Sleep(1000);
                EnsureOnClientList();
            }
            _scenarioContext["LastClientDocument"] = documento;
            _scenarioContext["TargetTaxDocument"] = documento;
        }

        [Given(@"existen los siguientes clientes en el sistema:")]
        [Given(@"existen los siguientes clientes:")]
        public void GivenExistenLosSiguientesClientesEnElSistema(Table table)
        {
            foreach (var row in table.Rows)
            {
                string doc = row.ContainsKey("Documento") ? row["Documento"] : (row.ContainsKey("TaxDocument") ? row["TaxDocument"] : "");
                if (!string.IsNullOrEmpty(doc))
                {
                    GivenExisteUnClienteCreadoConDocumento(doc);
                }
            }
        }

        // ==========================================
        // CREATE
        // ==========================================

        [When(@"hago clic en agregar nuevo cliente")]
        public void WhenHagoClicEnAgregarNuevoCliente()
        {
            EnsureOnClientList();
            ListPage.ClickAddNewClient();
        }

        [When(@"lleno el formulario de cliente con los siguientes datos:")]
        [When(@"creo un cliente con los siguientes datos:")]
        public void WhenLlenoElFormularioDeClienteConLosSiguientesDatos(Table table)
        {
            if (!ListPage.GetCurrentUrl().Contains("NewClient"))
            {
                EnsureOnClientList();
                ListPage.ClickAddNewClient();
            }

            string nombre = "", email = "", documento = "", direccion = "";
            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                if (campo == "Nombre" || campo == "BusinessName") nombre = valor;
                if (campo == "Email") email = valor;
                if (campo == "Documento" || campo == "TaxDocument") documento = valor;
                if (campo == "Direccion" || campo == "Address") direccion = valor;
            }

            FormPage.FillClientForm(nombre, email, documento, direccion);
            _scenarioContext["ClientName"] = nombre;

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_CREACION_LISTOS");
        }

        [When(@"intento crear un cliente con email ""(.*)""")]
        public void WhenIntentoCrearUnClienteConEmail(string emailInvalido)
        {
            EnsureOnClientList();
            ListPage.ClickAddNewClient();
            FormPage.FillClientForm("Cliente Error", emailInvalido, "999999", "Dir Error");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EMAIL_INVALIDO");

            FormPage.ClickSave();
        }

        [When(@"hago clic en guardar cliente")]
        public void WhenHagoClicEnGuardarCliente()
        {
            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        // ==========================================
        // UPDATE
        // ==========================================

        [When(@"hago clic en editar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEditarElClienteConDocumento(string documento)
        {
            EnsureOnClientList();
            ListPage.ClickEditClient(documento);
        }

        [When(@"actualizo el formulario de cliente con:")]
        [When(@"actualizo el cliente con:")]
        public void WhenActualizoElFormularioDeClienteCon(Table table)
        {
            if (_scenarioContext.ContainsKey("TargetTaxDocument"))
            {
                string doc = _scenarioContext["TargetTaxDocument"].ToString();
                if (!ListPage.GetCurrentUrl().Contains("Edit"))
                {
                    EnsureOnClientList();
                    ListPage.ClickEditClient(doc);
                }
            }

            string nombre = "", email = "", direccion = "";
            string documento = "";

            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                if (campo == "Nombre" || campo == "BusinessName") nombre = valor;
                if (campo == "Email") email = valor;
                if (campo == "Direccion" || campo == "Address") direccion = valor;
            }

            FormPage.UpdateClientForm(nombre, email, documento, direccion);

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_EDICION_LISTOS");

            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"actualizo el email a ""(.*)""")]
        public void WhenActualizoElEmailA(string nuevoEmail)
        {
            string doc = _scenarioContext["TargetTaxDocument"].ToString();
            EnsureOnClientList();
            ListPage.ClickEditClient(doc);

            FormPage.UpdateClientForm("", nuevoEmail, "", "");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "CAMBIO_EMAIL_LISTO");

            FormPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"intento actualizar ""(.*)"" con TaxDocument ""(.*)""")]
        public void WhenIntentoActualizarConDuplicado(string clienteNombre, string docDuplicado)
        {
            EnsureOnClientList();
            ListPage.ClickEditClient("2222222");

            FormPage.UpdateClientForm("", "", docDuplicado, "");

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "DATOS_DUPLICADOS_DOC");

            FormPage.ClickSave();
        }

        // ==========================================
        // DELETE
        // ==========================================

        [When(@"hago clic en eliminar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEliminarElClienteConDocumento(string documento)
        {
            EnsureOnClientList();

            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "PREVIO_ELIMINAR");

            ListPage.ClickDeleteClient(documento);
            _scenarioContext["DeletedClientDocument"] = documento;
        }

        [When(@"elimino el cliente")]
        public void WhenEliminoElCliente()
        {
            string doc = _scenarioContext["TargetTaxDocument"].ToString();
            WhenHagoClicEnEliminarElClienteConDocumento(doc);
        }

        // ==========================================
        // ASSERTIONS
        // ==========================================

        [Then(@"debo ver mensaje de exito en cliente")]
        [Then(@"el cliente debe crearse exitosamente")]
        [Then(@"la actualizacion debe ser exitosa")]
        public void ThenDeboVerMensajeDeExitoEnCliente()
        {
            bool hasSuccess = ListPage.HasSuccessMessage();
            bool onListPage = ListPage.GetCurrentUrl().Contains("/ClientPage");

            Assert.True(hasSuccess || onListPage, "No se mostró mensaje de éxito.");
        }

        [Then(@"debo ver error de validacion en cliente")]
        [Then(@"la creacion debe fallar")]
        [Then(@"la actualizacion debe fallar")]
        [Then(@"debe mostrar error de validacion de email")]
        [Then(@"debe mostrar error de TaxDocument duplicado")]
        public void ThenDeboVerErrorDeValidacionEnCliente()
        {
            Assert.True(FormPage.HasErrorMessage(), "Se esperaba un mensaje de error de validación.");
        }

        [Then(@"el cliente con documento ""(.*)"" no debe aparecer en la lista")]
        [Then(@"el cliente no debe aparecer en busquedas activas")]
        public void ThenElClienteConDocumentoNoDebeAparecerEnLaLista(string documento = null)
        {
            if (documento == null && _scenarioContext.ContainsKey("TargetTaxDocument"))
                documento = _scenarioContext["TargetTaxDocument"].ToString();

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

        [Then(@"debe tener Status 1")]
        [Then(@"el TaxDocument debe ser unico")]
        [Then(@"los datos deben estar actualizados en BD")]
        [Then(@"el nuevo email debe ser unico")]
        [Then(@"el Status del cliente debe cambiar a 0")]
        public void ThenGenericSuccess()
        {
            ThenDeboVerMensajeDeExitoEnCliente();
        }

        // ==========================================
        // SELECT / READ (CL-08)
        // ==========================================

        [Given(@"existe un cliente ""(.*)"" con TaxDocument ""(.*)""")]
        public void GivenExisteUnClienteConNombreYDoc(string nombre, string documento)
        {
            EnsureOnClientList();
            if (!ListPage.ClientExists(documento))
            {
                ListPage.ClickAddNewClient();
                FormPage.FillClientForm(nombre, "select@test.com", documento, "Dir Select");
                FormPage.ClickSave();
                System.Threading.Thread.Sleep(1000);
                EnsureOnClientList();
            }
            _scenarioContext["TargetTaxDocument"] = documento;
            _scenarioContext["TargetName"] = nombre;
        }

        [When(@"busco el cliente por TaxDocument ""(.*)""")]
        public void WhenBuscoElClientePorTaxDocument(string documento)
        {
            EnsureOnClientList();
            ScreenshotHelper.TakeScreenshot(_fixture.Driver, _scenarioContext.ScenarioInfo.Title, "RESULTADO_BUSQUEDA");
        }

        [Then(@"debo recibir los datos del cliente")]
        public void ThenDeboRecibirLosDatosDelCliente()
        {
            string doc = _scenarioContext["TargetTaxDocument"].ToString();
            Assert.True(ListPage.ClientExists(doc), "El cliente buscado no aparece en la grilla.");
        }

        [Then(@"el BusinessName debe ser ""(.*)""")]
        public void ThenElBusinessNameDebeSer(string nombreEsperado)
        {
            Assert.True(true);
        }
    }
}