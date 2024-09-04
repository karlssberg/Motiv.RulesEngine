using System.Linq.Expressions;
using System.Reflection;

namespace Motiv.RulesEngine;

public abstract class RuleBase<TModel, TMetadata>(Expression<Func<SpecBase<TModel>>> defaultTemplate) : IRule
{
    private readonly ReaderWriterLockSlim _lock = new ();
    
    private SpecBase<TModel>? _specOverride;
    
    public Type ModelType { get; } = typeof(TModel);
    
    public Type MetadataType { get; } = typeof(TMetadata);
    
    private readonly Expression<Func<IServiceProvider, SpecBase<TModel>>> _defaultFactoryExpression =
        DefaultParameterValueAsInjectable.Transform(defaultTemplate) ;
    
    private Func<IServiceProvider, SpecBase<TModel>>? _defaultFactory;
    
    public Func<IServiceProvider, SpecBase<TModel>> DefaultFactory
    {
        get
        {
            _defaultFactory ??= _defaultFactoryExpression.Compile();
            return _defaultFactory;
        }
    }

    SpecBase IRule.GetSpec(IServiceProvider serviceProvider) => GetSpec(serviceProvider);
    public SpecBase<TModel> GetSpec(IServiceProvider serviceProvider)
    {
        var specOverride = GetSpecOverride();
        
        return specOverride ?? DefaultFactory(serviceProvider);
    }

    public void OverrideSpec(SpecBase spec)
    {
        _lock.EnterWriteLock();
        try
        {
            _specOverride = spec switch
            {
                SpecBase<TModel> specification => specification,
                _ => throw new InvalidOperationException(
                    $"Expecting a Spec<{ModelType.FullName}>, but received a Spec<{spec.GetModelType().FullName}>")
            };
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    private SpecBase<TModel>? GetSpecOverride()
    {
        if (!_lock.IsWriteLockHeld)
            return _specOverride;
            
        _lock.EnterReadLock();
        try
        {
            return _specOverride;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    

//    private static IReadOnlyList<SpecMeta> GetSpecsFromExpression(Expression defaultExpression)
//    {
//        var visitor = new SpecExpressionParameterSniffer();
//        visitor.Visit(defaultExpression);
//        return visitor.Propositions;
//    }
//
//    private class SpecExpressionParameterSniffer : ExpressionVisitor
//    {
//        private readonly List<SpecMeta> _propositions = new();
//
//        public IReadOnlyList<SpecMeta> Propositions => _propositions;
//
//        protected override Expression VisitNew(NewExpression node)
//        {
//            if (!typeof(SpecBase<TModel>).IsAssignableFrom(node.Type))
//                return base.VisitNew(node);
//            
//            var propInfo = new SpecMeta(
//                node.Type,
//                node.Constructor?.GetParameters() ?? [],
//                node.Arguments.Select(EvaluateExpression));
//
//            _propositions.Add(propInfo);
//
//            return base.VisitNew(node);
//        }
//
//        private object EvaluateExpression(Expression expr)
//        {
//            return expr switch
//            {
//                ConstantExpression constExpr => constExpr.Value ??
//                                                throw new InvalidOperationException("Constant expression value is null"),
//                _ => throw new InvalidOperationException("Only constant expressions are supported with de")
//            };
//        }
//    }

    private record SpecMeta(Type? spec, ParameterInfo[] parameters, IEnumerable<object> nodeArguments);
}

public abstract class RuleBase<TModel>(Expression<Func<SpecBase<TModel>>> defaultTemplate) : RuleBase<TModel, string>(defaultTemplate)
{
}