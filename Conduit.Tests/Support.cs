using Conduit;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Tests.Support
{
    // --- Suporte para testes de Message com Retorno ---
    public class GetNumberQuery : IMessage<int> { }

    public class GetNumberQueryHandler : IMessageHandler<GetNumberQuery, int>
    {
        public Task<int> Handle(GetNumberQuery message, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }

    // --- Suporte para testes de Event ---
    public class MyTestEvent : IEvent { }
}