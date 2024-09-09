using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace Motiv.RulesEngine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransientRule<TRule>(
        this IServiceCollection serviceCollection,
        string proposition) where TRule : RuleBase, new()
    {
        var (modelType, metadataType) = typeof(TRule).GetModelAndMetadataTypes();
        serviceCollection.AddTransientRuleInternal(typeof(TRule), modelType, metadataType, proposition);
        
        return serviceCollection;
    }

    internal static IServiceCollection AddTransientRuleInternal(
        this IServiceCollection serviceCollection,
        Type ruleType,
        Type modelType,
        Type metadataType,
        string proposition) 
    {
        typeof(ServiceCollectionExtensions)
            .GetMethod(
                nameof(AddTransientRuleInternal),
                BindingFlags.NonPublic | BindingFlags.Static, 
                [typeof(IServiceCollection), typeof(string)])!
            .MakeGenericMethod(ruleType, modelType, metadataType)
            .Invoke(null, [serviceCollection, proposition]);
        
        return serviceCollection;
    }

    internal static IServiceCollection AddTransientRuleInternal<TRule, TModel, TMetadata>(
        this IServiceCollection serviceCollection,
        string proposition) where TRule : RuleBase<TModel, TMetadata>, new()
    {
        var normalizedProposition = Proposition.Normalize(proposition);
        
        serviceCollection.AddKeyedTransient<IRuleDescriptor>(
            normalizedProposition,
            (provider, _) =>
                new RuleDescriptor<TModel, TMetadata>(
                    new TRule(), 
                    provider.GetRequiredService<IRuleStore>()));
        
        return serviceCollection;
    }
//
//    private static Func<IServiceProvider, object> CreateRuleDescriptorFactory<TRule>(string key) where TRule : RuleBase, new()
//    {
//        var ruleType = typeof(TRule);
//        var (modelType, metadataType) = ruleType.GetModelAndMetadataTypes();
//        var ruleBaseType = typeof(RuleBase<,>).MakeGenericType(modelType, metadataType);
//
//        var ruleDescriptorType = typeof(RuleDescriptor<,>).MakeGenericType(modelType, metadataType);
//        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "provider");
//        
//        // Call GetRequiredKeyedService<RuleBase<TModel, TMetadata>>(key)
//        var newRuleExpression = Expression.New(ruleType.GetConstructor(Type.EmptyTypes)!);
//
//        // Find constructor for new RuleDescriptor<TModel, TMetadata>(ruleBase)
//        var ruleDescriptorConstructor = ruleDescriptorType.GetConstructor(
//            BindingFlags.NonPublic | BindingFlags.Instance,
//            null,
//            CallingConventions.Any,
//            [ruleBaseType],
//            null);
//        
//        // Create new RuleDescriptor<TModel, TMetadata>(ruleBase)
//        var newExpression = Expression.New(
//            ruleDescriptorConstructor!,
//            newRuleExpression);   
//        
//        return Expression.Lambda<Func<IServiceProvider, object>>(newExpression, serviceProviderParam).Compile();
//    }

//    private static MethodCallExpression CallRequiredKeyedServiceExpression(
//        Type requiredServiceType, 
//        Expression serviceProviderExpression,
//        Expression keyExpression)
//    {
//
//        // GetRequiredKeyedService method info
//        var getRequiredKeyedServiceMethod = typeof(ServiceProviderKeyedServiceExtensions)
//            .GetMethod(nameof(ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService), 
//                new[] { typeof(IServiceProvider), typeof(object) })!
//            .MakeGenericMethod(requiredServiceType);
//
//        // Method call expression
//        return Expression.Call(
//            getRequiredKeyedServiceMethod,
//            serviceProviderExpression,
//            keyExpression);
//    }
    
    
    public static IServiceCollection AddTransientRule<TRule>(
        this IServiceCollection serviceCollection) where TRule : RuleBase, new()
    {
        var proposition = GetExportName<TRule>() ??
                   throw new InvalidOperationException(
                       $"""
                        The externally visible name/proposition must be defined by either: 
                            - Declaring a name using [Export] attribute, or
                            - Supplying it in an {nameof(AddTransientRule)}() overload
                        """);
        
        return AddTransientRule<TRule>(serviceCollection, proposition);
    }
    public static IServiceCollection AddSpec<TSpec>(
        this IServiceCollection serviceCollection) where TSpec : SpecBase
    {
        var (modelType, metadataType) = typeof(TSpec).GetModelAndMetadataTypes();
        var singleGenericSpecType = typeof(IPropositionExport<>).MakeGenericType(modelType);
        var doubleGenericSpecType = typeof(IPropositionExport<,>).MakeGenericType(modelType, metadataType);
        
        var name = GetExportName<TSpec>();

        Func<IServiceProvider, object> implementationFactory =
            provider =>
            {
                var descriptorType = typeof(PropositionExport<,,>)
                    .MakeGenericType(typeof(TSpec), modelType, metadataType);

                if (Activator.CreateInstance(descriptorType, provider) is not IPropositionExport descriptor)
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