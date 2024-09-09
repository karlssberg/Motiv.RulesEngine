namespace Motiv.RulesEngine;

public class RuleDeserializer<TModel, TMetadata>(IServiceProvider provider) : PropositionalLogicBaseVisitor<SpecBase<TModel, TMetadata>>()
{
    
    public override SpecBase<TModel, TMetadata> VisitConditionalOrExpression(PropositionalLogicParser.ConditionalOrExpressionContext context) =>
        context.conditionalAndExpression()
            .Select(Visit)
            .Aggregate((left, right) => left.OrElse(right));
    
    public override SpecBase<TModel, TMetadata> VisitConditionalAndExpression(PropositionalLogicParser.ConditionalAndExpressionContext context) =>
        context.orExpression()
            .Select(Visit)
            .Aggregate((left, right) => left.AndAlso(right));

    public override SpecBase<TModel, TMetadata> VisitOrExpression(PropositionalLogicParser.OrExpressionContext context) =>
        context.xorExpression()
            .Select(Visit)
            .Aggregate((left, right) => left.Or(right));

    public override SpecBase<TModel, TMetadata> VisitXorExpression(PropositionalLogicParser.XorExpressionContext context) =>
        context.andExpression()
            .Select(Visit)
            .Aggregate((left, right) => left.XOr(right));

    public override SpecBase<TModel, TMetadata> VisitAndExpression(PropositionalLogicParser.AndExpressionContext context) =>
        context.notExpression()
            .Select(Visit)
            .Aggregate((left, right) => left.And(right));
    
    public override SpecBase<TModel, TMetadata> VisitNotExpression(PropositionalLogicParser.NotExpressionContext context) =>
        base.VisitNotExpression(context).Not();
    
    public override SpecBase<TModel, TMetadata> VisitProposition(PropositionalLogicParser.PropositionContext context) =>
        provider.GetSpecByName<TModel, TMetadata>(context.GetText());
}