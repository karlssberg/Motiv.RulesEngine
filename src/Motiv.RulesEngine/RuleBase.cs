using System.Linq.Expressions;
using System.Reflection;

namespace Motiv.RulesEngine;

public abstract class RuleBase
{
    protected RuleBase()
    {
        Name = GetName();
    }
    
    public string Name { get; }
    

    private string GetName() => GetType().GetCustomAttribute<ExportAttribute>()?.Identifier ?? GetType().Name;
}

public abstract class RuleBase<TModel, TMetadata> : RuleBase
{
    public Expression<Func<IServiceProvider, SpecBase<TModel, TMetadata>>> DefaultTemplate { get; }

    public RuleBase(Expression<Func<SpecBase<TModel, TMetadata>>> defaultTemplate)
    {
        DefaultTemplate = DefaultParameterValueAsInjectable.Transform(defaultTemplate);
        DefaultFactory = DefaultTemplate.Compile();
    }
    
    public RuleBase(Expression<Func<IServiceProvider, SpecBase<TModel, TMetadata>>> defaultTemplate)
    {
        DefaultTemplate = defaultTemplate;
        DefaultFactory = defaultTemplate.Compile();
    }

    public Func<IServiceProvider, SpecBase<TModel, TMetadata>> DefaultFactory { get; init; }
}

public abstract class RuleBase<TModel> : RuleBase<TModel, string>
{
    public RuleBase(Expression<Func<SpecBase<TModel>>> defaultTemplate) : base(ConvertToExplanationSpec(defaultTemplate))
    {
    }

    private static Expression<Func<SpecBase<TModel, string>>> ConvertToExplanationSpec(Expression<Func<SpecBase<TModel>>> expression)
    {
        var methodInfo = typeof(SpecBase<TModel>).GetMethod(nameof(SpecBase<TModel>.ToExplanationSpec))
            ?? throw new InvalidOperationException("Could not find ToExplanationSpec method on SpecBase<TModel>.");
        
        return Expression.Lambda<Func<SpecBase<TModel, string>>>(Expression.Call(expression.Body, methodInfo));
    }
}
