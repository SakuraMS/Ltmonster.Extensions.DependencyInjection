using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

namespace Ltmonster.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IEnumerable<Type> GetAllInterfaces(this Type type)
        => type.GetInterfaces().Union(type.GetCustomAttributes().SelectMany(g => g.GetType().GetInterfaces()));

    public static IEnumerable<Type> GetAllInterfaceAndAttribute(this Type type)
        => type.GetInterfaces().Union(type.GetCustomAttributes().Select(g => g.GetType()));

    public static IServiceCollection TryAddService(this IServiceCollection services, ServiceType serviceType, Type? igType, Type implementationType)
    {
        switch (serviceType)
        {
            case ServiceType.Singleton:
                if (igType is null)
                {
                    services.TryAddSingleton(implementationType);
                }
                else
                {
                    services.TryAddSingleton(igType, implementationType);
                }
                break;
            case ServiceType.Scoped:
                if (igType is null)
                {
                    services.TryAddScoped(implementationType);
                }
                else
                {
                    services.TryAddScoped(igType, implementationType);
                }
                break;
            case ServiceType.Transient:
                if (igType is null)
                {
                    services.TryAddTransient(implementationType);
                }
                else
                {
                    services.TryAddTransient(igType, implementationType);
                }
                break;
        }
        return services;
    }

    public static IServiceCollection AutoRegister(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var register = (Type igType, Type implementationType) =>
        {
            if (igType == typeof(ISingleton) || igType == typeof(SingletonAttribute))
            {
                services.TryAddService(ServiceType.Singleton, null, implementationType);
            }
            else if (igType == typeof(IScoped) || igType == typeof(ScopedAttribute))
            {
                services.TryAddService(ServiceType.Scoped, null, implementationType);
            }
            else if (igType == typeof(ITransient) || igType == typeof(TransientAttribute))
            {
                services.TryAddService(ServiceType.Transient, null, implementationType);
            }
            else if (igType.GetInterfaces().Any(t => t == typeof(ISingletonGeneric)))
            {
                services.TryAddService(ServiceType.Singleton, igType.GenericTypeArguments[0], implementationType);
            }
            else if (igType.GetInterfaces().Any(t => t == typeof(IScopedGeneric)))
            {
                services.TryAddService(ServiceType.Scoped, igType.GenericTypeArguments[0], implementationType);
            }
            else if (igType.GetInterfaces().Any(t => t == typeof(ITransientGeneric)))
            {
                services.TryAddService(ServiceType.Transient, igType.GenericTypeArguments[0], implementationType);
            }
        };
        var assemblys = configuration?.GetSection("Ltmonster:ServiceAutoRegister:Assemblies").Get<string[]>();
        if (assemblys?.Any() is true)
        {
            foreach (var item in assemblys)
            {
                var registerType = Assembly.Load(item).DefinedTypes.Where(t => t.IsClass && t.GetAllInterfaces().Any(ti => ti == typeof(IService)));
                foreach (var type in registerType)
                {
                    var iis = type.GetAllInterfaceAndAttribute();
                    foreach (var inter in type.GetAllInterfaceAndAttribute())
                    {
                        register(inter, type);
                    }
                }
            }
        }
        else
        {
            HashSet<Assembly> referencedAssembliesSet = new();
            Assembly.GetEntryAssembly()?.GetAllReferencedAssemblies(referencedAssembliesSet);
            foreach (var rassembly in referencedAssembliesSet)
            {
                var registerType = rassembly.DefinedTypes.Where(t => t.IsClass && t.GetAllInterfaces().Any(ti => ti == typeof(IService)));
                foreach (var type in registerType)
                {
                    var iis = type.GetAllInterfaceAndAttribute();
                    foreach (var inter in type.GetAllInterfaceAndAttribute())
                    {
                        register(inter, type);
                    }
                }
            }
        }
        return services;
    }

    private static void GetAllReferencedAssemblies(this Assembly assembly, HashSet<Assembly> referencedAssemblies)
    {
        foreach (var typeInfo in assembly.GetReferencedAssemblies())
        {
            if (typeInfo.Name?.StartsWith("System.") is true
                || typeInfo.Name?.StartsWith("Microsoft.") is true)
            {
                continue;
            }
            var amb = Assembly.Load(typeInfo);
            referencedAssemblies.Add(amb);
            GetAllReferencedAssemblies(amb, referencedAssemblies);
        }
    }
}


public interface IService { }

#region Singleton

public interface ISingletonGeneric : IService { }

public interface ISingleton : IService { }

public interface ISingleton<T> : ISingletonGeneric { }

[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : Attribute, IService { }

[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute<Service> : Attribute, ISingletonGeneric { }

#endregion


#region Scoped

public interface IScopedGeneric : IService { }

public interface IScoped : IService { }

public interface IScoped<T> : ISingletonGeneric { }

[AttributeUsage(AttributeTargets.Class)]
public class ScopedAttribute : Attribute, IService { }

[AttributeUsage(AttributeTargets.Class)]
public class ScopedAttribute<Service> : Attribute, IScopedGeneric { }

#endregion


#region Transient

public interface ITransientGeneric : IService { }

public interface ITransient : IService { }

public interface ITransient<T> : ITransientGeneric { }

[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : Attribute, IService { }

[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute<Service> : Attribute, ITransientGeneric { }

#endregion

public enum ServiceType
{
    Singleton = 0x01,
    Scoped = 0x02,
    Transient = 0x03
}