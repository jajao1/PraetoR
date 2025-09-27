
using FluentValidation;

namespace PraetoR.FluentValidation
{
    public class ValidationBehavior<TResquest, TResponse> : IPipelineBehavior<TResquest, TResponse>
        where TResquest : ICommand<TResponse>
    {

        private readonly IEnumerable<IValidator<TResquest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TResquest>> validators)
        {
            _validators = validators;
        }


        public async Task<TResponse> Handle(TResquest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) 
            { 
                return await next();
            }

            var context = new ValidationContext<TResquest>(request);

            var _validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = _validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0) 
            {
                throw new ValidationException(failures);
            }

            return await next();

        }
    }

    public class ValidationBehaviorNoResult<TRequest> : IPipelineBehavior<TRequest>
       where TRequest : ICommand // Restrição correta para este behavior
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviorNoResult(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task Handle(TRequest request, CommandHandlerDelegate next, CancellationToken cancellationToken)
        {
            await Validate(request, cancellationToken); // Chama o método auxiliar de validação
            await next();
        }

        private async Task Validate(TRequest request, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return;

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0) throw new ValidationException(failures);
        }
    }
}
