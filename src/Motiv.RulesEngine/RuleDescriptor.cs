namespace Motiv.RulesEngine;

internal class RuleDescriptor<TModel, TMetadata> : IRuleDescriptor
{
    private readonly RuleBase<TModel, TMetadata> _rule;
    private readonly IRuleStore _store;

    internal RuleDescriptor(RuleBase<TModel, TMetadata> rule, IRuleStore store)
    {
        _rule = rule;
        _store = store;
    }

    public string Name => _rule.Name;

    
    public string GetSource(IServiceProvider serviceProvider)
    {
        var source = _store.LoadRule(_rule.Name);
        if (source is not null)
            return source;
        
        var spec = _rule.DefaultFactory(serviceProvider);
        return spec.GenerateSource();
    }

    SpecBase IRuleDescriptor.GetSpec(IServiceProvider serviceProvider) => GetSpec(serviceProvider);
    public Type ModelType { get; } = typeof(TModel);
    public Type MetadataType { get; } = typeof(TMetadata);
    public void SaveSource(string source)
    {
        _store.SaveRule(Name, source);
    }

    public SpecBase<TModel, TMetadata> GetSpec(IServiceProvider serviceProvider)
    {
        var source = _store.LoadRule(_rule.Name);
        return source is not null 
            ? serviceProvider.ComposeSpec<TModel, TMetadata>(source)
            : _rule.DefaultFactory(serviceProvider);
    }

    public override string ToString() => Name;
}