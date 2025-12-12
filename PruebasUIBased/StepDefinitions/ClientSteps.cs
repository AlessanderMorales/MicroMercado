using PruebasUIBased.Infrastructure;
using PruebasUIBased.PageObjects;
using Reqnroll;
using Xunit;

namespace PruebasUIBased.StepDefinitions
{
    [Binding]
    public class ClientSteps
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public ClientSteps(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        private ClientListPage GetClientListPage()
        {
            if (!_scenarioContext.ContainsKey("ClientListPage"))
            {
                var page = new ClientListPage(_fixture.Driver);
                _scenarioContext["ClientListPage"] = page;
            }
            return (ClientListPage)_scenarioContext["ClientListPage"];
        }

        private ClientFormPage GetClientFormPage()
        {
            if (!_scenarioContext.ContainsKey("ClientFormPage"))
            {
                var page = new ClientFormPage(_fixture.Driver);
                _scenarioContext["ClientFormPage"] = page;
            }
            return (ClientFormPage)_scenarioContext["ClientFormPage"];
        }

        [Given(@"existe un cliente creado con documento ""(.*)""")]
        public void GivenExisteUnClienteCreadoConDocumento(string documento)
        {
            var listPage = GetClientListPage();
            
            // Verificar si el cliente ya existe buscando por documento
            // Si no existe, crearlo
            var clientExists = false;
            try
            {
                var rows = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector("#categoryTable tbody tr")); // NOTA: La página usa categoryTable por error
                foreach (var row in rows)
                {
                    if (row.Text.Contains(documento))
                    {
                        clientExists = true;
                        break;
                    }
                }
            }
            catch { }

            if (!clientExists)
            {
                listPage.ClickAddNewClient();
                System.Threading.Thread.Sleep(500);

                var formPage = GetClientFormPage();
                formPage.FillClientForm($"Cliente {documento}", $"cliente{documento}@example.com", documento, "Dirección de prueba");
                formPage.ClickSave();
                System.Threading.Thread.Sleep(1000);

                listPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                System.Threading.Thread.Sleep(500);
            }

            _scenarioContext["LastClientDocument"] = documento;
        }

        [Given(@"existen los siguientes clientes en el sistema:")]
        public void GivenExistenLosSiguientesClientesEnElSistema(Table table)
        {
            var listPage = GetClientListPage();

            foreach (var row in table.Rows)
            {
                var nombre = row["Nombre"];
                var documento = row["Documento"];
                
                var clientExists = false;
                try
                {
                    var rows = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector("#categoryTable tbody tr")); // NOTA: La página usa categoryTable por error
                    foreach (var r in rows)
                    {
                        if (r.Text.Contains(documento))
                        {
                            clientExists = true;
                            break;
                        }
                    }
                }
                catch { }

                if (!clientExists)
                {
                    listPage.ClickAddNewClient();
                    System.Threading.Thread.Sleep(500);

                    var formPage = GetClientFormPage();
                    formPage.FillClientForm(nombre, $"{documento}@example.com", documento, "Dirección de prueba");
                    formPage.ClickSave();
                    System.Threading.Thread.Sleep(1000);

                    listPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        [When(@"hago clic en agregar nuevo cliente")]
        public void WhenHagoClicEnAgregarNuevoCliente()
        {
            var listPage = GetClientListPage();
            listPage.ClickAddNewClient();
            System.Threading.Thread.Sleep(500);
        }

        [When(@"lleno el formulario de cliente con los siguientes datos:")]
        public void WhenLlenoElFormularioDeClienteConLosSiguientesDatos(Table table)
        {
            var formPage = GetClientFormPage();
            
            string nombre = "";
            string email = "";
            string documento = "";
            string direccion = "";

            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                switch (campo)
                {
                    case "Nombre":
                        nombre = valor;
                        break;
                    case "Email":
                        email = valor;
                        break;
                    case "Documento":
                        documento = valor;
                        break;
                    case "Direccion":
                        direccion = valor;
                        break;
                }
            }

            formPage.FillClientForm(nombre, email, documento, direccion);
            _scenarioContext["ClientDocument"] = documento;
            _scenarioContext["ClientName"] = nombre;
        }

        [When(@"hago clic en guardar cliente")]
        public void WhenHagoClicEnGuardarCliente()
        {
            var formPage = GetClientFormPage();
            formPage.ClickSave();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"hago clic en editar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEditarElClienteConDocumento(string documento)
        {
            var listPage = GetClientListPage();
            
            // Buscar el cliente por documento en la tabla
            var rows = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector("#categoryTable tbody tr")); // NOTA: La página usa categoryTable por error
            foreach (var row in rows)
            {
                if (row.Text.Contains(documento))
                {
                    var editButton = row.FindElement(OpenQA.Selenium.By.CssSelector("a[href*='EditCategory']")); // NOTA: La página usa EditCategory por error
                    editButton.Click();
                    System.Threading.Thread.Sleep(2000);
                    break;
                }
            }
        }

        [When(@"actualizo el formulario de cliente con:")]
        public void WhenActualizoElFormularioDeClienteCon(Table table)
        {
            var formPage = GetClientFormPage();
            
            string nombre = "";
            string email = "";
            string direccion = "";
            var documento = _scenarioContext.ContainsKey("LastClientDocument") 
                ? (string)_scenarioContext["LastClientDocument"] 
                : "";

            foreach (var row in table.Rows)
            {
                var campo = row["Campo"];
                var valor = row["Valor"];

                switch (campo)
                {
                    case "Nombre":
                        nombre = valor;
                        break;
                    case "Email":
                        email = valor;
                        break;
                    case "Direccion":
                        direccion = valor;
                        break;
                }
            }

            formPage.UpdateClientForm(nombre, email, documento, direccion);
            _scenarioContext["UpdatedClientName"] = nombre;
        }

        [When(@"hago clic en eliminar el cliente con documento ""(.*)""")]
        public void WhenHagoClicEnEliminarElClienteConDocumento(string documento)
        {
            var listPage = GetClientListPage();
            
            // Buscar el cliente por documento en la tabla
            var rows = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector("#categoryTable tbody tr")); // NOTA: La página usa categoryTable por error
            foreach (var row in rows)
            {
                if (row.Text.Contains(documento))
                {
                    var deleteButton = row.FindElement(OpenQA.Selenium.By.CssSelector("button[onclick*='confirmDeleteCategory']")); // NOTA: La página usa confirmDeleteCategory por error
                    deleteButton.Click();
                    System.Threading.Thread.Sleep(500);

                    var confirmButton = _fixture.Driver.FindElement(OpenQA.Selenium.By.CssSelector("#deleteClientForm button[type='submit']"));
                    confirmButton.Click();
                    System.Threading.Thread.Sleep(1000);
                    break;
                }
            }

            _scenarioContext["DeletedClientDocument"] = documento;
        }

        [Then(@"debo ver mensaje de exito en cliente")]
        public void ThenDeboVerMensajeDeExitoEnCliente()
        {
            var listPage = GetClientListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/ClientPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                System.Threading.Thread.Sleep(500);
            }

            if (_scenarioContext.ContainsKey("ClientName"))
            {
                var clientName = (string)_scenarioContext["ClientName"];
                if (!string.IsNullOrEmpty(clientName))
                {
                    Assert.True(listPage.ClientExists(clientName), 
                        $"El cliente '{clientName}' debería existir en la lista");
                }
            }
        }

        [Then(@"debo ver error de validacion en cliente")]
        public void ThenDeboVerErrorDeValidacionEnCliente()
        {
            var formPage = GetClientFormPage();
            Assert.True(formPage.HasErrorMessage(), 
                "Debería haber un error de validación");
        }

        [Then(@"el cliente con documento ""(.*)"" no debe aparecer en la lista")]
        public void ThenElClienteConDocumentoNoDebeAparecerEnLaLista(string documento)
        {
            var listPage = GetClientListPage();
            
            if (!listPage.GetCurrentUrl().Contains("/ClientPage"))
            {
                listPage.NavigateTo($"{_fixture.BaseUrl}/ClientPage");
                System.Threading.Thread.Sleep(500);
            }

            var rows = _fixture.Driver.FindElements(OpenQA.Selenium.By.CssSelector("#categoryTable tbody tr")); // NOTA: La página usa categoryTable por error
            foreach (var row in rows)
            {
                Assert.False(row.Text.Contains(documento), 
                    $"El cliente con documento '{documento}' no debería aparecer en la lista");
            }
        }

        [Then(@"debo ver al menos (.*) clientes en la lista")]
        public void ThenDeboVerAlMenosClientesEnLaLista(int cantidad)
        {
            var listPage = GetClientListPage();
            var count = listPage.GetClientCount();
            Assert.True(count >= cantidad, 
                $"Debería haber al menos {cantidad} clientes, pero hay {count}");
        }
    }
}
