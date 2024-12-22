using FluentValidation;

namespace Durable.Utilities
{
    public class ServiceProviderValidatorFactory : IServiceProviderValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderValidatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IValidator<T> GetValidator<T>()
        {
            return (IValidator<T>)GetValidator(typeof(T));
        }

        public IValidator GetValidator(Type type)
        {
            Type serviceType = typeof(IValidator<>).MakeGenericType(type);

            object? service = _serviceProvider.GetService(serviceType);

            return service != null ? (IValidator)service : null;
        }
    }
}
