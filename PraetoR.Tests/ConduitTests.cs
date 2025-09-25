using PraetoR;
using PraetoR.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace PraetoR.Tests
{
    public class PraetoRTests
    {
        [Fact]
        public async Task Send_Should_FindHandlerAndReturnResult()
        {
            // Arrange (Organizar)
            var query = new GetNumberQuery();
            var expectedResult = 42;
            var handler = new GetNumberQueryHandler();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOperationHandler<GetNumberQuery, int>)))
                .Returns(handler);

            var PraetoR = new PraetoR(serviceProviderMock.Object);

            // Act (Agir)
            var result = await PraetoR.Send(query, CancellationToken.None);

            // Assert (Verificar)
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Send_Should_ThrowInvalidOperationException_WhenHandlerIsNotFound()
        {
            // Arrange
            var query = new GetNumberQuery();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Returns(null); // Simulando que o handler não foi encontrado

            var PraetoR = new PraetoR(serviceProviderMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => PraetoR.Send(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Publish_Should_FindAndExecuteAllRegisteredHandlers()
        {
            // Arrange
            var testEvent = new MyTestEvent();

            // Criamos mocks para os handlers para poder verificar se foram chamados
            var handler1Mock = new Mock<IDictumHandler<MyTestEvent>>();
            var handler2Mock = new Mock<IDictumHandler<MyTestEvent>>();

            // Para testes de "GetServices", é mais fácil usar uma ServiceCollection real
            var services = new ServiceCollection();
            services.AddTransient<IDictumHandler<MyTestEvent>>(_ => handler1Mock.Object);
            services.AddTransient<IDictumHandler<MyTestEvent>>(_ => handler2Mock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var PraetoR = new PraetoR(serviceProvider);

            // Act
            await PraetoR.Publish(testEvent, CancellationToken.None);

            // Assert
            // Verificamos se o método Handle foi chamado em AMBOS os handlers
            handler1Mock.Verify(h => h.Handle(testEvent, It.IsAny<CancellationToken>()), Times.Once);
            handler2Mock.Verify(h => h.Handle(testEvent, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Publish_Should_NotThrow_WhenNoHandlersAreFound()
        {
            // Arrange
            var testEvent = new MyTestEvent();

            // Criamos um provedor de serviços vazio, sem handlers registrados
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var PraetoR = new PraetoR(serviceProvider);

            // Act
            var exception = await Record.ExceptionAsync(() => PraetoR.Publish(testEvent, CancellationToken.None));

            // Assert
            // Verificamos que nenhuma exceção foi lançada
            Assert.Null(exception);
        }
    }
}
