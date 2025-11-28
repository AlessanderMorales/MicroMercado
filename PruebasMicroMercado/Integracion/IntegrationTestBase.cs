using Microsoft.Extensions.DependencyInjection;
using System;

namespace PruebasMicroMercado.Integracion
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly CustomWebApplicationFactory<Program> Factory;
        protected readonly IServiceScope Scope;
        protected readonly IServiceProvider Services;

        protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Scope = factory.Services.CreateScope();
            Services = Scope.ServiceProvider;
            
            // Asegurar que los datos de prueba están inicializados
            factory.SeedDatabase();
        }

        public virtual void Dispose()
        {
            Scope?.Dispose();
        }
    }
}
