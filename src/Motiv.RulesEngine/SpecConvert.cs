using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public static class SpecConvert
{
    private static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    }; 
    
    public static SpecBase<TModel, string>? Deserialize<TModel>(string json, IServiceProvider services)
    {
        var specDefinitionRoot = JsonSerializer.Deserialize<SpecResource>(json, SerializerOptions);
        return specDefinitionRoot?.ToSpec<TModel>(services).ToExplanationSpec();
    }

    public static SpecBase ToSpec(this SpecResource rootSpecResource, Type modelType, IServiceProvider services)
    {
        var result = typeof(SpecConvert)
                         .GetMethod(nameof(ToSpec), [typeof(SpecResource), typeof(IServiceProvider)])?
                         .MakeGenericMethod(modelType)
                         .Invoke(null, [rootSpecResource, services])
                     ?? throw new InvalidOperationException("Failed to convert spec definition to spec");
        
        return (SpecBase) result;
    }

    public static SpecBase<TModel> ToSpec<TModel>(this SpecResource rootSpecResource, IServiceProvider services)
    {
        return Visit(rootSpecResource);
        
        SpecBase<TModel> Visit(SpecResource specDefinition)
        {
            var operands = specDefinition.Operands.Select(Visit).ToList();
            return (specDefinition.Kind, operands) switch
            {
                (SpecKind.Proposition, _) =>
                    services.GetSpec<TModel, string>(specDefinition.Name),
                (SpecKind.And, _) =>
                    operands.Select(op => op.ToExplanationSpec()).AndTogether(),
                (SpecKind.AndAlso, _) =>
                    operands.Select(op => op.ToExplanationSpec()).AndAlsoTogether(),
                (SpecKind.Or, _) =>
                    operands.Select(op => op.ToExplanationSpec()).OrTogether(),
                (SpecKind.OrElse, _) =>
                    operands.Select(op => op.ToExplanationSpec()).OrElseTogether(),
                (SpecKind.XOr, [var left, var right]) =>
                    left.XOr(right),
                (SpecKind.Not, [var operand]) =>
                    operand.ToExplanationSpec().Not(),
                _ => throw new InvalidOperationException("Invalid spec kind or operand count")
            };
        }
    }

    public static PropositionResource ToPropositionResource(this ISpecExport export)
    {
        var parameters = export.TemplateParameters.ToArray();
        
        return new PropositionResource(export.Template)
        {
            Parameters = parameters.Length == 0 
                ? null 
                : parameters.ToDictionary(
                    p => p.Name,
                    p => new MotivTypeResource(p.Kind)) 
        };
    }

    public static SpecResource ToSpecResource(this SpecBase rootSpec, IServiceProvider? services = null)
    {
        return Visit(rootSpec);
        
        SpecResource Visit(SpecBase spec)
        {
            return spec switch
            {
                IBinaryOperationSpec binarySpec when binarySpec.Operation == Operator.And =>
                    new SpecResource(SpecKind.And)
                    {
                        Operands = binarySpec.GetOperands().Select(Visit)
                    },
                IBinaryOperationSpec binarySpec when binarySpec.Operation == Operator.AndAlso =>
                    new SpecResource(SpecKind.AndAlso)
                    {
                        Operands = binarySpec.GetOperands().Select(Visit)
                    },
                IBinaryOperationSpec binarySpec when binarySpec.Operation == Operator.Or =>
                    new SpecResource(SpecKind.Or)
                    {
                        Operands = binarySpec.GetOperands().Select(Visit)
                    },
                IBinaryOperationSpec binarySpec when binarySpec.Operation == Operator.OrElse =>
                    new SpecResource(SpecKind.OrElse)
                    {
                        Operands = binarySpec.GetOperands().Select(Visit)
                    },
                IBinaryOperationSpec binarySpec when binarySpec.Operation == Operator.XOr =>
                    new SpecResource(SpecKind.XOr)
                    {
                        Operands = binarySpec.GetOperands().Select(Visit)
                    },
                IUnaryOperationSpec unarySpec when unarySpec.Operation == Operator.Not =>
                    new SpecResource(SpecKind.Not)
                    {
                        Operands = Visit(unarySpec.Operand).ToEnumerable()
                    },
                _ => CreatePropositionDefinition(spec)
            };
        }

        SpecResource CreatePropositionDefinition(SpecBase spec)
        {
            var (modelType, _) = spec.GetModelAndMetadataTypes();
            
            if (services is not null)
            {
                EnsureSpecIsRegistered(modelType, spec.Statement.ToExternalName());
            }
            
            return new SpecResource(SpecKind.Proposition)
            {
                Name = spec.GetExportName() ?? spec.Statement.ToExternalName()
            };
        }
        
        void EnsureSpecIsRegistered(Type modelType, string statement)
        {
            services.GetKeyedServices(modelType, statement);
        }
    }
    
    public static string ToExternalName(this string statement) => statement.Replace(" ", "-");

    

    private static string GetBinaryOperatorLiteral(SpecKind kind) =>
        kind switch
        {
            SpecKind.And => "&",
            SpecKind.AndAlso => "&&",
            SpecKind.Or => "|",
            SpecKind.OrElse => "||",
            SpecKind.XOr => "^",
            _ => throw new InvalidOperationException($"Invalid binary operator '{kind}'")
        };
}