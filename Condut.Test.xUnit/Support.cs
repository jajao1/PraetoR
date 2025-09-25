using PraetoR;
using System.Threading;
using System.Threading.Tasks;

namespace PraetoR.Tests.Support
{
    // --- Suporte para testes de Message com Retorno ---
    public class GetNumberQuery : IOperation<int> { }

    public class GetNumberQueryHandler : IOperationHandler<GetNumberQuery, int>
    {
        public Task<int> Handle(GetNumberQuery message, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }
    }

    // --- Suporte para testes de Event ---
    public class MyTestEvent : IDictum { }
}