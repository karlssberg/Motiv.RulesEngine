using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public static class SpecProviderExtensions 
{
    public static void EnsureExists<TModel>(this IServiceProvider services, string statement)
    {
        var spec = services.GetSpec<TModel>(statement);
        
        if (spec is null)
        {
            throw new InvalidOperationException($"Spec '{statement}' does not exist for the model '{typeof(TModel).FullName}'.");
        }
    }
    public static SpecBase<TModel> GetSpec<TModel>(this IServiceProvider services, string statement) =>
        (SpecBase<TModel>) services.GetSpec(typeof(TModel), statement);
    
    public static SpecBase<TModel, TMetadata> GetSpec<TModel, TMetadata>(this IServiceProvider services, string statement) =>
        (SpecBase<TModel, TMetadata>) services.GetSpec(typeof(TModel), typeof(TMetadata), statement);

    public static SpecBase GetSpec(this IServiceProvider services, Type model, Type metadata, string statement)
    {
        var specType = typeof(SpecBase<,>).MakeGenericType(model, metadata);
        
        return (SpecBase) services.GetRequiredKeyedService(specType, statement);
    }
    
    public static SpecBase GetSpec(this IServiceProvider services, Type model, string statement)
    {
        var specType = typeof(SpecBase<>).MakeGenericType(model);
        
        return (SpecBase) services.GetRequiredKeyedService(specType, statement);
    }
    
    public static IEnumerable<SpecBase<TModel, TMetadata>> GetAllSpecs<TModel, TMetadata>(this IServiceProvider services) =>
        (IEnumerable<SpecBase<TModel, TMetadata>>) services.GetAllSpecs(typeof(TModel), typeof(TMetadata));

    public static IEnumerable<SpecBase> GetAllSpecs(this IServiceProvider services, Type model, Type metadata)
    {
        var specType = typeof(SpecBase<,>).MakeGenericType(model, metadata);
        
        return (IEnumerable<SpecBase>) services.GetServices(specType);
    }
}