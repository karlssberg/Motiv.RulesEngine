namespace Motiv.RulesEngine;

public interface IRuleDescriptor
{
    string Name { get; }
    
    
    string GetSource(IServiceProvider serviceProvider);
    
    SpecBase GetSpec(IServiceProvider serviceProvider);
    
    Type ModelType { get; }
    
    Type MetadataType { get; }
    void SaveSource(string source);
}