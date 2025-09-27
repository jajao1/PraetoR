using FluentValidation;
using Moq;
using PraetoR.FluentValidation;
using PraetoR.Tests.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraetoR.Test.xUnit
{
    public class ValidationBehaviorNoResultTests
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldCallNextDelegate()
        {
            // Arrange
            var validCommand = new DeleteUserCommand(Guid.NewGuid());
            var validator = new DeleteUserCommandValidator();

            // Mock para o "próximo passo" no pipeline (o handler principal)
            var nextMock = new Mock<CommandHandlerDelegate>();
            nextMock.Setup(next => next()).Returns(Task.CompletedTask); // Comandos sem retorno retornam Task

            // O nosso "System Under Test" é o próprio behavior
            var behavior = new ValidationBehaviorNoResult<DeleteUserCommand>(new[] { validator });

            // Act
            await behavior.Handle(validCommand, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(), Times.Once); // Próximo passo foi chamado
        }

        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException_AndNotCallNext()
        {
            // Arrange
            var invalidCommand = new DeleteUserCommand(Guid.Empty); // Comando com dados inválidos
            var validator = new DeleteUserCommandValidator();

            var nextMock = new Mock<CommandHandlerDelegate>();

            var behavior = new ValidationBehaviorNoResult<DeleteUserCommand>(new[] { validator });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(invalidCommand, nextMock.Object, CancellationToken.None)
            );

            nextMock.Verify(next => next(), Times.Never); // Próximo passo NÃO foi chamado
        }

        [Fact]
        public async Task Handle_WithNoValidators_ShouldCallNextDelegate()
        {
            // Arrange
            var command = new DeleteUserCommand(Guid.NewGuid());

            var nextMock = new Mock<CommandHandlerDelegate>();
            nextMock.Setup(next => next()).Returns(Task.CompletedTask);

            // Comportamento sem nenhum validador registrado
            var behavior = new ValidationBehaviorNoResult<DeleteUserCommand>(Array.Empty<IValidator<DeleteUserCommand>>());

            // Act
            await behavior.Handle(command, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(), Times.Once); // Próximo passo foi chamado
        }
    }
}
