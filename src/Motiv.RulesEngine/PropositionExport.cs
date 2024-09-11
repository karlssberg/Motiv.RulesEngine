using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public class PropositionExport<TSpec, TModel, TMetadata>
    : IPropositionExport<TModel, TMetadata> where TSpec : SpecBase<TModel, TMetadata>
{
 
    private readonly ConstructorInfo _tartgetConstructor;
    private readonly IServiceProvider _serviceProvider;
    private readonly Proposition _proposition;
    
    public PropositionExport(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _proposition = new Proposition(GetPropositionName());
        _tartgetConstructor = GetTargetConstructor(_proposition.ParameterNames);
        var parameterInfos = _tartgetConstructor.GetParameters();
        var parameterLookup = parameterInfos
            .Where(p => p.Name is not null)
            .ToDictionary(p => p.Name!);

        TemplateParameters = _proposition.ParameterNames.Select(name =>
            new PropositionParameter(name, parameterLookup[name].ParameterType.ToMotivPrimitive()));
    }
    
    public string Id => _proposition.Id;

    public Type SpecType { get; } = typeof(TSpec);
    
    public string Template => _proposition.Template;

    public (bool isValid, IEnumerable<string> errors) Validate(string candidate)
    {
        var parametersValue = _proposition.CreateSerializedParameterValueLookup(candidate);

        var errors = TemplateParameters
            .Aggregate<PropositionParameter, IEnumerable<string>>(
                [],
                (errorAccumulator, parameter) =>
                {
                    if (!parametersValue.TryGetValue(parameter.Name, out var value))
                        return errorAccumulator.Append($"Missing parameter '{parameter.Name}'");

                    if (!parameter.TryParse(value, out _))
                        return errorAccumulator.Append(
                            $"""
                             Invalid parameter in the proposition '{candidate}'.
                                 Expected the parameter '{parameter.Name}' to be of type '{parameter.Kind}'.
                                 But determined the value '{value}' to be of type '{value.GetType().Name}'.
                             """);
                    return errorAccumulator;
                })
            .ToArray();
            
        return (errors.Length == 0, errors);
    }

    public IEnumerable<PropositionParameter> TemplateParameters { get; }

    SpecBase<TModel> IPropositionExport<TModel>.CreateInstance(string proposition) =>
        CreateInstance(proposition);

    SpecBase IPropositionExport.CreateInstance(string proposition) =>
        CreateInstance(proposition);
    
    public SpecBase<TModel, TMetadata> CreateInstance(string proposition)
    {
        var parametersValue = _proposition.CreateSerializedParameterValueLookup(proposition);

        var arguments = _tartgetConstructor.GetParameters().Select(parameter =>
            parametersValue.TryGetValue(parameter.Name ?? "", out var value)
                ? value
                : _serviceProvider.GetRequiredService(parameter.ParameterType));

        return (SpecBase<TModel, TMetadata>)_tartgetConstructor.Invoke(arguments.ToArray());
    }

    private IDictionary<string, object> CreateParameterValueLookup(string candidate)
    {
        var parameterValuesLookup = _proposition.CreateSerializedParameterValueLookup(candidate);
        return TemplateParameters
            .Select(parameter => (
                parameter.Name,
                Value: Convert.ChangeType(parameterValuesLookup[parameter.Name], GetTypeCode(parameter.Kind))))
            .ToDictionary(pair => pair.Name, pair => pair.Value);
    }

    private static TypeCode GetTypeCode(MotivPrimitive kind)
    {
        return kind switch
        {
            MotivPrimitive.String => TypeCode.String,
            MotivPrimitive.Decimal => TypeCode.Decimal,
            MotivPrimitive.DateTime => TypeCode.DateTime,
            MotivPrimitive.Integer => TypeCode.Int32,
            _ => TypeCode.Object
        };
    }
    
    private ConstructorInfo GetTargetConstructor(IEnumerable<string> parameterNames)
    {
        var candidateConstructorGroup = SelectConstructor(parameterNames)?.ToList();

        if (candidateConstructorGroup is null or { Count: 0 })
            throw CreateNoConstructorFoundException();

        if (candidateConstructorGroup!.Count > 1)
            throw CreateAmbiguousConstructorException();

        var constructor = candidateConstructorGroup.Single();
        return constructor;
    }

    
    private IEnumerable<ConstructorInfo>? SelectConstructor(IEnumerable<string> parameterNames)
    {
        return SpecType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Where(constructor =>
                parameterNames.All(parameterName => HasParameterName(constructor, parameterName)))
            .GroupBy(c => c.GetParameters().Length)
            .OrderByDescending(g => g.Key)
            .FirstOrDefault();

        bool HasParameterName(ConstructorInfo c, string parameterName)
        {
            return c.GetParameters().Any(p => p.Name == parameterName);
        }
    }

    private static string GetPropositionName() =>
        typeof(TSpec).GetCustomAttribute<ExportAttribute>()?.Identifier
        ?? throw CreateMissingIdentifierException();

    private static Exception CreateMissingIdentifierException() =>
        new InvalidOperationException(
            $"""
             Failed to determine the export name of the spec '{typeof(TSpec).FullName}'.
             """);

    private Exception CreateAmbiguousConstructorException() =>
        new InvalidOperationException(
            $"Ambiguous choice of constructors for the spec '{SpecType.FullName}'.");

    private Exception CreateNoConstructorFoundException() =>
        new InvalidOperationException(
            $"""
             No suitable constructors found for the spec '{SpecType.FullName}'that contain all the expected parameters.
                 Expected: {string.Join(", ", _proposition.ParameterNames)}
             """);
}