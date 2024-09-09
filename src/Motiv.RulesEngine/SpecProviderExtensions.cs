using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public static class SpecProviderExtensions 
{
    public static SpecBase<TModel, TMetadata> GetSpec<TModel, TMetadata>(this IServiceProvider services, string statement) =>
        (SpecBase<TModel, TMetadata>) services.GetSpec(typeof(TModel), typeof(TMetadata), statement);

    public static SpecBase GetSpec(this IServiceProvider services, Type model, Type metadata, string statement)
    {
        var specType = typeof(SpecBase<,>).MakeGenericType(model, metadata);
        
        return (SpecBase) services.GetRequiredKeyedService(specType, statement);
    }
    
}