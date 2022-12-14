using System.Reflection;
using ServiceMethodInterceptorPoC.API.Decorators;

namespace ServiceMethodInterceptorPoC.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonWithStopWatch<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.DecorateWithStopWatch<TService>();
        return services;
    }

    public static IServiceCollection AddSingletonWithMemoryCache<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
        services.DecorateWithMemoryCache<TService>();
        return services;
    }
    
    public static IServiceCollection DecorateWithStopWatch<TInterface>(this IServiceCollection services)
        where TInterface : class
    {
        return services.Decorate<TInterface, SimpleStopwatchDecorator<TInterface>>();
    }

    public static IServiceCollection DecorateWithMemoryCache<TInterface>(this IServiceCollection services)
        where TInterface : class
    {
        return services.Decorate<TInterface, CachingDecorator<TInterface>>();
    }
    
    private static IServiceCollection Decorate<TInterface, TProxy>(this IServiceCollection services)
        where TInterface : class
        where TProxy : DispatchProxy
    {
        MethodInfo createMethod;
        try
        {
            createMethod = typeof(TProxy)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(info => !info.IsGenericMethod && info.ReturnType == typeof(TInterface));
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                $"Looks like there is no static method in {typeof(TProxy)} " +
                $"which creates instance of {typeof(TInterface)} (note that this method should not be generic)",
                e);
        }

        var argInfos = createMethod.GetParameters();

        // Save all descriptors that needs to be decorated into a list.
        var descriptorsToDecorate = services
            .Where(s => s.ServiceType == typeof(TInterface))
            .ToList();

        if (descriptorsToDecorate.Count == 0)
            throw new InvalidOperationException(
                $"Attempted to Decorate services of type {typeof(TInterface)}, " +
                "but no such services are present in ServiceCollection");

        foreach (var descriptor in descriptorsToDecorate)
        {
            var decorated = ServiceDescriptor.Describe(
                typeof(TInterface),
                sp =>
                {
                    var decoratorInstance = createMethod.Invoke(null,
                        argInfos.Select(
                                info => info.ParameterType ==
                                        (descriptor.ServiceType ?? descriptor.ImplementationType)
                                    ? sp.CreateInstance(descriptor)
                                    : sp.GetRequiredService(info.ParameterType))
                            .ToArray());

                    return (TInterface)decoratorInstance;
                },
                descriptor.Lifetime);

            services.Remove(descriptor);
            services.Add(decorated);
        }

        return services;
    }

    private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null) return descriptor.ImplementationInstance;

        return descriptor.ImplementationFactory != null
            ? descriptor.ImplementationFactory(services)
            : ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
    }
}