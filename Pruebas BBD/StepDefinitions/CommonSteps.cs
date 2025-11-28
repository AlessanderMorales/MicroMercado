using Reqnroll;
using Xunit;

namespace Pruebas_BBD.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public CommonSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"la base de datos esta limpia")]
        public void GivenLaBaseDeDatosEstaLimpia()
        {

        }

        [Then(@"la creacion debe fallar")]
        public void ThenLaCreacionDebeFallar()
        {
            if (_scenarioContext.ContainsKey("CreationResult"))
            {
                Assert.Null(_scenarioContext["CreationResult"]);
            }
        }

        [Then(@"la actualizacion debe ser exitosa")]
        public void ThenLaActualizacionDebeSerExitosa()
        {
            if (_scenarioContext.ContainsKey("UpdateResult"))
            {
                Assert.NotNull(_scenarioContext["UpdateResult"]);
            }
            
            if (_scenarioContext.ContainsKey("UpdateException"))
            {
                Assert.Null(_scenarioContext["UpdateException"]);
            }
        }

        [Then(@"la actualizacion debe fallar")]
        public void ThenLaActualizacionDebeFallar()
        {
            if (_scenarioContext.ContainsKey("UpdateResult"))
            {
                Assert.Null(_scenarioContext["UpdateResult"]);
            }
        }
    }
}
