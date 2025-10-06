using Xunit;
using Moq;
using MicroMercado.Services;

namespace PruebasMicroMercado
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var mock = new Mock<ExampleService>();
            mock.Setup(s => s.GetValue()).Returns(100);
            var result = mock.Object.GetValue();
            Assert.Equal(100, result);
        }
    }
}