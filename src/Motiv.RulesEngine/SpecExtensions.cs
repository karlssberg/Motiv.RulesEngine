namespace Motiv.RulesEngine;

public static class SpecExtensions
{
    public static Type GetModelType(this SpecBase spec)
    {
        var type = spec.GetType();
        do
        {
            if (IsSpecOf(typeof(SpecBase<>)))
                return type.GenericTypeArguments[0];
            
            type = type.BaseType;
        } while (type is not null);
        
        throw new InvalidOperationException("Spec<> type not found");

        bool IsSpecOf(Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }
    
    public static (Type Model, Type Metadata) GetModelAndMetadataTypes(this Type specType)
    {
        var type = specType;
        do
        {
            if (IsSpecOf(typeof(SpecBase<,>)))
                return (type.GenericTypeArguments[0], type.GenericTypeArguments[1]);
            
            type = type.BaseType;
        } while (type is not null);
        
        throw new InvalidOperationException("Spec<,> type not found");

        bool IsSpecOf(Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }
    
    public static (Type Model, Type Metadata) GetModelAndMetadataTypes(this SpecBase spec) => 
        spec.GetType().GetModelAndMetadataTypes();
    
}