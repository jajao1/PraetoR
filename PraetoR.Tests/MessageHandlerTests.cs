using PraetoR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PraetoR.Tests.Support
{
    // --- Entidade de Domínio ---
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    // --- Abstração do Repositório (a dependência que vamos mockar) ---
    public interface IUserRepository
    {
        Task<Guid> Add(User user);
    }

    // --- O Comando e seu Handler (o que vamos testar) ---
    public class CreateUserCommand : IOperation<Guid>
    {
        public string Name { get; }
        public CreateUserCommand(string name) => Name = name;
    }

    public class CreateUserCommandHandler : IOperationHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(CreateUserCommand message, CancellationToken cancellationToken)
        {
            var user = new User { Name = message.Name };
            var newId = await _userRepository.Add(user);
            return newId;
        }
    }
}