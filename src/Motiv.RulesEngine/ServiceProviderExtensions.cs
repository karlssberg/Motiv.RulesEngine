using Antlr4.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public static class ServiceProviderExtensions
{
    public static SpecBase<TModel, TMetadata> ComposeSpec<TModel, TMetadata>(this IServiceProvider provider, string source)
    {
        var input = CharStreams.fromString(source);
        var lexer = new PropositionalLogicLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PropositionalLogicParser(tokens);
        var ruleDeserializer = new RuleDeserializer<TModel, TMetadata>(provider);
        var root = parser.formula();
        
        return ruleDeserializer.Visit(root);
    }

    public static SpecBase<TModel, TMetadata> GetSpecByName<TModel, TMetadata>(this IServiceProvider provider, string proposition)
    {
        var normalizedProposition = Proposition.Normalize(proposition);
        var exportedSpec = provider.GetRequiredKeyedService<IPropositionExport<TModel, TMetadata>>(normalizedProposition);
        return exportedSpec.CreateInstance(proposition);
    }
    
    public static (bool IsValid, IEnumerable<MotivParseError> Errors) Validate(this IServiceProvider provider, string source)
    {
        var input = CharStreams.fromString(source);
        var lexer = new PropositionalLogicLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PropositionalLogicParser(tokens);
        var syntaxErrorListener = new MotivSyntaxErrorListener();
        var semanticErrorListener = new MotivSemanticErrorListener(provider);
        parser.AddParseListener(semanticErrorListener);
        parser.AddErrorListener(syntaxErrorListener);
        parser.formula();
        
        var errors = syntaxErrorListener.Errors.Concat(semanticErrorListener.Errors).ToArray();
        
        return (errors.Length == 0, errors);
    }    
}

public class MotivSyntaxErrorListener : BaseErrorListener
{
    private readonly List<MotivParseError> _errors = [];
    
    public IEnumerable<MotivParseError> Errors => _errors;

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        _errors.Add(new MotivParseError(line, charPositionInLine, msg, offendingSymbol.Text));
    }
}

public record MotivParseError(int Line, int Column, string Message, string OffendingSymbol);

public class MotivSemanticErrorListener(IServiceProvider provider) : PropositionalLogicBaseListener
{
    private readonly List<MotivParseError> _errors = [];
    
    public IEnumerable<MotivParseError> Errors => _errors;
    
    public override void ExitProposition(PropositionalLogicParser.PropositionContext context)
    {
        var propositionExpression = context.GetText();
        var (isValid, errors) = ValidateProposition(propositionExpression);
        if (!isValid)
        {
            _errors.Add(new MotivParseError(
                context.Start.Line,
                context.Start.Column,
                string.Join(Environment.NewLine, errors), 
                context.Start.Text));
        }
    }
    
    private (bool isValid, IEnumerable<string> errors) ValidateProposition(string proposition)
    {
        var normalizedProposition = Proposition.Normalize(proposition);
        var export = provider.GetKeyedService<IPropositionExport>(normalizedProposition);
        if (export is null)
        {
            return (false, [$"No proposition found for '{proposition}'"]);
        }
        return export.Validate(proposition);
    }
}