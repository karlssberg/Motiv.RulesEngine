namespace Motiv.RulesEngine;

public interface IPropositionExport
{
    SpecBase CreateInstance(string proposition);
    string Id { get; }
    
    IEnumerable<PropositionParameter> TemplateParameters { get; }
    string Template { get; }
    (bool isValid, IEnumerable<string> errors) Validate(string candidate);
}

public interface IPropositionExport<TModel> : IPropositionExport
{
    new SpecBase<TModel> CreateInstance(string proposition);
}

public interface IPropositionExport<TModel, TMetadata> : IPropositionExport<TModel>
{
    new SpecBase<TModel, TMetadata> CreateInstance(string proposition);
}