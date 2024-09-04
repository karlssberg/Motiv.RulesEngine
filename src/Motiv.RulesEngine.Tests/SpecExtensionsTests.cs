using FluentAssertions.Extension.Json;
using Microsoft.Extensions.DependencyInjection;
using static Motiv.Operator;

namespace Motiv.RulesEngine.Tests;

public class SpecConvertTests
{
//    [Theory]
//    [AutoData]
//    public void Should_serialize_spec_to_json(string statement)
//    {
//        // Arrange
//        var spec = Spec.Build((int _) => true).Create(statement);
//        var container = new ServiceCollection().AddSingletonSpec(spec);
//        
//        // Act
//        var json = SpecConvert.SerializeAsJson(spec, container.BuildServiceProvider());
//        
//        // Assert
//        json.AsJson().Should()
//            .HaveProperty("_type", "proposition").And
//            .HaveProperty("statement", statement).And
//            .HaveProperty("operands").Which.Should().BeEmpty();
//    }
//    
//    [Theory]
//    [InlineAutoData("AND", "and")]
//    [InlineAutoData("AND ALSO", "andAlso")]
//    [InlineAutoData("OR", "or")]
//    [InlineAutoData("OR ELSE", "orElse")]
//    [InlineAutoData("XOR", "xOr")]
//    public void Should_serialize_specs_composed_using_boolean_binary_operations(string operation, string kind, string left, string right)
//    {
//        // Arrange
//        var leftSpec = Spec.Build((int _) => true).Create(left);
//        var rightSpec = Spec.Build((int _) => true).Create(right);
//        var container = new ServiceCollection()
//            .AddSingletonSpec(leftSpec)
//            .AddSingletonSpec(rightSpec);
//
//        var rootSpec = Compose(leftSpec, rightSpec, operation);
//        
//        // Act
//        var jsonText = SpecConvert.SerializeAsJson(rootSpec, container.BuildServiceProvider());
//        
//        
//        // Assert
//        var json = jsonText.AsJson();
//        json.Should().HaveProperty("_type").Which.Should().Be(kind);
//        
//        json.Should().HaveProperty("statement");
//        
//        json.Should().HaveProperty("operands").Which.Should().BeArray()
//                .And
//                .HaveLength(2);
//            
//        json.GetProperty("operands")[0].Should()
//            .HaveProperty("_type").Which.Should().Be("proposition");
//        
//        json.GetProperty("operands")[0].Should()
//            .HaveProperty("statement").Which.Should().Be(left);
//                
//        json.GetProperty("operands")[1].Should()
//            .HaveProperty("_type").Which.Should().Be("proposition");
//        
//        json.GetProperty("operands")[1].Should()
//            .HaveProperty("statement").Which.Should().Be(right);
//    }
//    
//    private static SpecBase<TModel, TMetadata> Compose<TModel, TMetadata>(SpecBase<TModel, TMetadata> left, SpecBase<TModel, TMetadata> right, string operation) =>
//        operation switch
//        {
//            "AND" => left.And(right),
//            "AND ALSO" => left.AndAlso(right),
//            "OR" => left.Or(right),
//            "OR ELSE" => left.OrElse(right),
//            "XOR" => left.XOr(right),
//            _ => throw new InvalidOperationException("Invalid operation")
//        };
//
//    [Theory]
//    [AutoData]
//    public void Should_serialize_specs_composed_using_the_not_operation(string statement)
//    {
//        // Arrange
//        var operand = Spec.Build((int _) => true).Create(statement);
//        var container = new ServiceCollection()
//            .AddSingletonSpec(operand);
//
//        var rootSpec = operand.Not();
//
//        // Act
//        var jsonText = SpecConvert.SerializeAsJson(rootSpec, container.BuildServiceProvider());
//
//
//        // Assert
//        var json = jsonText.AsJson();
//        json.Should().HaveProperty("_type").Which.Should().Be("not");
//
//        json.Should().HaveProperty("statement");
//
//        json.Should().HaveProperty("operands").Which.Should().BeArray()
//            .And
//            .HaveLength(1);
//
//        json.GetProperty("operands")[0].Should()
//            .HaveProperty("_type").Which.Should().Be("proposition");
//
//        json.GetProperty("operands")[0].Should()
//            .HaveProperty("statement").Which.Should().Be(statement);
//    }
//
//    [Theory]
//    [AutoData]
//    public void Should_deserialize_a_serialized_spec_composition(string left, string right)
//    {
//        // Arrange
//        var leftSpec = Spec.Build((int _) => true).Create(left);
//        var rightSpec = Spec.Build((int _) => true).Create(right);
//        var container = new ServiceCollection()
//            .AddSingletonSpec(leftSpec)
//            .AddSingletonSpec(rightSpec);
//
//        var serviceProvider = container.BuildServiceProvider();
//
//        var rootSpec = leftSpec & !(leftSpec | (rightSpec ^ leftSpec));
//
//        var jsonText = SpecConvert.SerializeAsJson(rootSpec, serviceProvider);
//        
//        // Act
//        var act = SpecConvert.Deserialize<int>(jsonText, serviceProvider);
//        
//        // Assert
//        act?.Expression.Should().Be(rootSpec.Expression);
//    }
}