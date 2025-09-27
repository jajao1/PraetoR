using PraetoR;
using System.Threading;
using System.Threading.Tasks;

namespace PraetoR.Tests.Support
{
    // --- Suporte para testes de Message com Retorno ---
    public class GetNumberQuery : ICommand<int> { }

    public class GetNumberQueryHandler : ICommandHandler<GetNumberQuery, int>
    {
        public Task<int> Handle(GetNumberQuery message, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }

    // --- Suporte para testes de Event ---
    public class MyTestEvent : IDictum { }

    // Comando SEM retorno
    public class DeleteUserCommand : ICommand
    {
        public Guid UserId { get; }
        public DeleteUserCommand(Guid userId) { UserId = userId; }
    }
}