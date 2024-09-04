namespace Motiv.RulesEngine;

public class SpecWithCustomParameterValues<TModel, TMetadata>(
    SpecBase<TModel, TMetadata> underlyingSpec,
    IDictionary<string, object> defaultParameterConstants
) : Spec<TModel, TMetadata>(underlyingSpec)
{
    public IDictionary<string, object> ParametersValues = defaultParameterConstants;
    public new SpecBase<TModel, TMetadata> Underlying { get;  } = underlyingSpec;
    
    protected override BooleanResultBase<TMetadata> IsSpecSatisfiedBy(TModel model) => Underlying.IsSatisfiedBy(model);
}