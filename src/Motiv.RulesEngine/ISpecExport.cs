namespace Motiv.RulesEngine;

public interface ISpecExport
{
    SpecBase Activate(IDictionary<string, object> parametersValues);
    string Id { get; }
    
    IEnumerable<PropositionParameter> TemplateParameters { get; }
    string Template { get; }
}

public interface ISpecExport<TModel> : ISpecExport
{
    new SpecBase<TModel> Activate(IDictionary<string, object> parametersValues);
}

public interface ISpecExport<TModel, TMetadata> : ISpecExport<TModel>
{
    new SpecBase<TModel, TMetadata> Activate(IDictionary<string, object> parametersValues);
}