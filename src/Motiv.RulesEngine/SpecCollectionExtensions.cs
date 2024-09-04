using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace Motiv.RulesEngine;

public static class SpecCollectionExtensions
{
    public static IServiceCollection AddSingletonSpec<TModel, TMetadata>(
        this IServiceCollection serviceCollection,
        SpecBase<TModel, TMetadata> spec)
    {
        serviceCollection.AddKeyedSingleton(spec.Statement, spec);
        serviceCollection.AddKeyedSingleton<SpecBase<TModel>>(spec.Statement, spec);
        serviceCollection.AddKeyedSingleton<SpecBase>(spec.Statement, spec);

        return serviceCollection;
    }
    
    public static IServiceCollection AddSingletonRule<TRule>(
        this IServiceCollection serviceCollection) where TRule : IRule
    {
        var name = GetExportName<TRule>();
        
        serviceCollection.AddKeyedSingleton<IRuleDescriptor, RuleDescriptor<TRule>>(name);
        serviceCollection.AddKeyedSingleton(name, RuleResolver<TRule>);

        return serviceCollection;
    }
    
    public static IServiceCollection AddTransientRule<TRule>(
        this IServiceCollection serviceCollection) where TRule : class, IRule
    {
        var name = GetExportName<TRule>();
        
        serviceCollection.AddKeyedTransient<TRule>(name);
        serviceCollection.AddKeyedSingleton<IRuleDescriptor, RuleDescriptor<TRule>>(name);
        serviceCollection.AddKeyedTransient(name, RuleResolver<TRule>);

        return serviceCollection;
    }
    
    private static IRule RuleResolver<TRule>(IServiceProvider provider, object? serviceKey) where TRule : IRule
    {
        var descriptor = provider.GetRequiredKeyedService<IRuleDescriptor>(serviceKey);
        var rule = provider.GetRequiredKeyedService<TRule>(serviceKey);

        if (descriptor.PredicateOverride is not null)
        {
            rule.OverrideSpec(descriptor.PredicateOverride);
        }

        return rule;
    }
    
    public static IServiceCollection AddSpec<TSpec>(
        this IServiceCollection serviceCollection) where TSpec : SpecBase
    {
        var (modelType, metadataType) = typeof(TSpec).GetModelAndMetadataTypes();
        var singleGenericSpecType = typeof(ISpecExport<>).MakeGenericType(modelType);
        var doubleGenericSpecType = typeof(ISpecExport<,>).MakeGenericType(modelType, metadataType);
        
        var name = GetExportName<TSpec>();

        Func<IServiceProvider, object> implementationFactory =
            provider =>
            {
                var descriptorType = typeof(SpecExport<,,>)
                    .MakeGenericType(typeof(TSpec), modelType, metadataType);

                if (Activator.CreateInstance(descriptorType, provider) is not ISpecExport descriptor)
                    throw new InvalidOperationException("Could not create SpecDescriptor.");
                
                return descriptor;
            };

        serviceCollection.AddSingleton(singleGenericSpecType, implementationFactory);
        serviceCollection.AddSingleton(doubleGenericSpecType, implementationFactory);
        serviceCollection.AddKeyedSingleton(
            doubleGenericSpecType, 
            name, 
            (provider, _) => implementationFactory(provider));
        
        return serviceCollection;
    }

    private static string? GetExportName<T>()  =>
        typeof(T).GetCustomAttribute<ExportAttribute>()?.Identifier;

    internal static string? GetExportName(this SpecBase specBase) => 
        specBase.GetType().GetCustomAttribute<ExportAttribute>()?.Identifier;

    internal static ExportAttribute? GetExportAttribute(this Type type) =>
        type.GetCustomAttribute<ExportAttribute>();
}