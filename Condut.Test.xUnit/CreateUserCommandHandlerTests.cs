using PraetoR;
using PraetoR.Tests.Support;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace PraetoR.Tests
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            // O setup é feito no construtor para ser reutilizado em todos os testes da classe.
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new CreateUserCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CallRepositoryAdd_AndReturnNewUserId()
        {
            // Arrange (Organizar)
            var command = new CreateUserCommand("John Doe");
            var expectedId = Guid.NewGuid();

            // Configuramos o mock: quando o método Add for chamado com QUALQUER objeto User,
            // ele deve retornar o nosso Guid esperado.
            _userRepositoryMock
                .Setup(repo => repo.Add(It.IsAny<User>()))
                .ReturnsAsync(expectedId);

            // Act (Agir)
            var resultId = await _handler.Handle(command, CancellationToken.None);

            // Assert (Verificar)
            // 1. Verificamos se o resultado retornado é o ID que esperamos.
            Assert.Equal(expectedId, resultId);

            // 2. Verificamos se o método Add do repositório foi chamado exatamente uma vez.
            // Isso garante que a ação de "salvar" foi tentada.
            _userRepositoryMock.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
        }
    }
}