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

    public IEnumerable<PropositionParameter> TemplateParameters { get; }

    SpecBase<TModel> IPropositionExport<TModel>.CreateInstance(string proposition) =>
        CreateInstance(proposition);

    SpecBase IPropositionExport.CreateInstance(string proposition) =>
        CreateInstance(proposition);
    
    public SpecBase<TModel, TMetadata> CreateInstance(string proposition)
    {
        var parametersValue = _proposition.DetermineParameterValues(proposition);

        var arguments = _tartgetConstructor.GetParameters().Select(parameter =>
            parametersValue.TryGetValue(parameter.Name ?? "", out var value)
                ? value
                : _serviceProvider.GetRequiredService(parameter.ParameterType));

        return (SpecBase<TModel, TMetadata>)_tartgetConstructor.Invoke(arguments.ToArray());
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