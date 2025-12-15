using PruebasUIBased.Infrastructure;
using Reqnroll;
using System;

namespace PruebasUIBased.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly WebDriverFixture _fixture;
        private readonly ScenarioContext _scenarioContext;

        public Hooks(WebDriverFixture fixture, ScenarioContext scenarioContext)
        {
            _fixture = fixture;
            _scenarioContext = scenarioContext;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_scenarioContext.TestError != null)
            {
                string scenarioName = _scenarioContext.ScenarioInfo.Title;
                ScreenshotHelper.TakeScreenshot(_fixture.Driver, scenarioName, "FALLO");
            }
        }
    }
}