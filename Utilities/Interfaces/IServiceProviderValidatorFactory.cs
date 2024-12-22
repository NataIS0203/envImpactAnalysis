using FluentValidation;

namespace Durable.Utilities
{
    public interface IServiceProviderValidatorFactory
    {
        IValidator<T> GetValidator<T>();

        IValidator GetValidator(Type type);
    }
}
