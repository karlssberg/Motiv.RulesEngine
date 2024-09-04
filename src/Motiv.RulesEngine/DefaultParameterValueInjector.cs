using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public class DefaultParameterValueAsInjectable(ParameterExpression serviceProviderParameter) : ExpressionVisitor
{
    protected override Expression VisitNew(NewExpression node)
    {
        if (!typeof(SpecBase).IsAssignableFrom(node.Type)) return base.VisitNew(node);

        var parameters = node.Constructor!.GetParameters();
        if (parameters.Length == 0) return base.VisitNew(node);
        
        var customParameters = node.Arguments
            .Select((arg, index) => (Expression: arg, Parameter: parameters[index]))
            .Where(tuple => tuple.Expression is ConstantExpression)
            .Where(tuple => IsValidMotivPrimitiveType(tuple.Parameter.ParameterType))
            .ToDictionary(tuple => tuple.Parameter.Name!, tuple => ((ConstantExpression)tuple.Expression).Value);

        var (model, metadata) = node.Type.GetModelAndMetadataTypes();
        
        var decoratorConstructor = typeof(SpecWithCustomParameterValues<,>)
            .MakeGenericType(model, metadata)
            .GetConstructor([node.Type, typeof(IDictionary<string, object>)])!;

        var decoratedNode = Expression.New(
            node.Constructor,
            node.Arguments.Select(expression => expression switch
            {
                DefaultExpression => CreateServiceLocatorCall(expression.Type),
                ConstantExpression { Value: null } => CreateServiceLocatorCall(expression.Type),
                _ => Visit(expression)
            }));
        
        return customParameters.Count == 0 
            ? base.VisitNew(node) 
            : Expression.New(decoratorConstructor, decoratedNode, Expression.Constant(customParameters));
    }

    private Expression CreateServiceLocatorCall(Type serviceType)
    {
        var getServiceMethodInfo = typeof(ServiceProviderServiceExtensions).GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), [typeof(IServiceProvider), typeof(Type)])!;
        var getServiceCall = Expression.Call(
            null,
            getServiceMethodInfo,
            serviceProviderParameter,
            Expression.Constant(serviceType)
        );
        
        return Expression.Convert(getServiceCall, serviceType);
    }

    public static Expression<Func<IServiceProvider, T>> Transform<T>(Expression<Func<T>> expression)
    {
        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var injector = new DefaultParameterValueAsInjectable(serviceProviderParam);
        var newBody = injector.Visit(expression.Body);
        return Expression.Lambda<Func<IServiceProvider, T>>(newBody, serviceProviderParam);
    }

    private bool IsValidMotivPrimitiveType(Type type) =>
        type == typeof(string) || type.GetInterface("INumber`1") is not null;
}