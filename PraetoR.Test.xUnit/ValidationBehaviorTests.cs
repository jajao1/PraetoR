using FluentValidation;
using Moq;
using PraetoR.FluentValidation;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;

namespace PraetoR.Test.xUnit
{
    public class CreateUserCommand : ICommand<Guid>
    {
        public string UserName { get; }
        public string Email { get; }
        public CreateUserCommand(string userName, string email) { UserName = userName; Email = email; }
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    // --- A CLASSE DE TESTES ---
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_WithValidCommand_ShouldCallNextDelegate()
        {
            // Arrange
            var validCommand = new CreateUserCommand("John Doe", "john.doe@example.com");
            var validator = new CreateUserCommandValidator();

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();
            nextMock.Setup(next => next()).Returns(Task.FromResult(Guid.NewGuid()));

            var behavior = new ValidationBehavior<CreateUserCommand, Guid>(new[] { validator });

            // Act
            await behavior.Handle(validCommand, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(), Times.Once); // Próximo passo foi chamado
        }

        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException_AndNotCallNext()
        {
            // Arrange
            var invalidCommand = new CreateUserCommand("", "not-an-email");
            var validator = new CreateUserCommandValidator();

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();

            var behavior = new ValidationBehavior<CreateUserCommand, Guid>(new[] { validator });

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
            var command = new CreateUserCommand("Any", "any@example.com");

            var nextMock = new Mock<RequestHandlerDelegate<Guid>>();
            nextMock.Setup(next => next()).Returns(Task.FromResult(Guid.NewGuid()));

            // Comportamento sem nenhum validador registrado
            var behavior = new ValidationBehavior<CreateUserCommand, Guid>(Array.Empty<IValidator<CreateUserCommand>>());

            // Act
            await behavior.Handle(command, nextMock.Object, CancellationToken.None);

            // Assert
            nextMock.Verify(next => next(), Times.Once); // Próximo passo foi chamado
        }
    }
}
