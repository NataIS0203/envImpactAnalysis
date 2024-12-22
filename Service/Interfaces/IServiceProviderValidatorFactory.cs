using FluentValidation;

namespace Durable.Services
{
    public interface IServiceProviderValidatorFactory
    {
        IValidator<T> GetValidator<T>();

        IValidator GetValidator(Type type);
    }
}
