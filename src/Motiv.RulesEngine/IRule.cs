namespace Motiv.RulesEngine;

public interface IRule
{
    Type ModelType { get; }
    
    Type MetadataType { get; }
    
    public SpecBase GetSpec(IServiceProvider serviceProvider);
    
    public void OverrideSpec(SpecBase spec);
}