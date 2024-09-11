using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace Motiv.RulesEngine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMotivRulesEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IRuleService, RuleService>();
        
        return serviceCollection;
    }

    public static IServiceCollection AddRule<TRule>(
        this IServiceCollection serviceCollection,
        string proposition) where TRule : RuleBase, new()
    {
        var (modelType, metadataType) = typeof(TRule).GetModelAndMetadataTypes();
        serviceCollection.AddTransientRuleInternal(typeof(TRule), modelType, metadataType, proposition);
        
        return serviceCollection;
    }

    public static IServiceCollection AddRule<TRule>(
        this IServiceCollection serviceCollection) where TRule : RuleBase, new()
    {
        var proposition = GetExportIdentifier<TRule>() ??
                          throw new InvalidOperationException(
                              $"""
                               The externally visible name/proposition must be defined by either: 
                                   - Declaring a name using [Export] attribute, or
                                   - Supplying it in an {nameof(AddRule)}() overload
                               """);
        
        return AddRule<TRule>(serviceCollection, proposition);
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

    public static IServiceCollection AddTransientProposition<TSpec>(
        this IServiceCollection serviceCollection) where TSpec : SpecBase
    {
        var name = GetExportIdentifier<TSpec>() ??
                   throw new InvalidOperationException(
                       $"""
                        The externally visible name/proposition must be defined by either: 
                            - Declaring a name using [Export] attribute, or
                            - Supplying it in an {nameof(AddRule)}() overload
                        """);
        
        return serviceCollection.AddTransientProposition<TSpec>(name);
    }
    
    
    public static IServiceCollection AddTransientProposition<TSpec>(
        this IServiceCollection serviceCollection,
        string name) where TSpec : SpecBase
    {
        var normalizedName = Proposition.Normalize(name);
        
        var (modelType, metadataType) = typeof(TSpec).GetModelAndMetadataTypes();

        var nonGenericSpecType = typeof(IPropositionExport);
        var singleGenericSpecType = typeof(IPropositionExport<>).MakeGenericType(modelType);
        var doubleGenericSpecType = typeof(IPropositionExport<,>).MakeGenericType(modelType, metadataType);
        
        

        Func<IServiceProvider, object> implementationFactory =
            provider =>
            {
                var propositionExport = typeof(PropositionExport<,,>)
                    .MakeGenericType(typeof(TSpec), modelType, metadataType);

                if (Activator.CreateInstance(propositionExport, provider) is not IPropositionExport export)
                    throw new InvalidOperationException("Could not create PropositionExport.");
                
                return export;
            };

        serviceCollection.AddTransient(singleGenericSpecType, implementationFactory);
        serviceCollection.AddTransient(doubleGenericSpecType, implementationFactory);
        serviceCollection.AddKeyedTransient(
            nonGenericSpecType, 
            normalizedName, 
            (provider, _) => implementationFactory(provider));
        serviceCollection.AddKeyedTransient(
            doubleGenericSpecType, 
            normalizedName, 
            (provider, _) => implementationFactory(provider));
        
        return serviceCollection;
    }

    private static string? GetExportIdentifier<T>()  =>
        typeof(T).GetCustomAttribute<ExportAttribute>()?.Identifier;

    internal static string? GetExportIdentifier(this SpecBase specBase) => 
        specBase.GetType().GetCustomAttribute<ExportAttribute>()?.Identifier;

}