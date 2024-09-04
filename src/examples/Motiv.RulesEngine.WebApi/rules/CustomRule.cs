namespace Motiv.RulesEngine.WebApi.rules;

[Export("custom-rule")]
public class CustomRule() : RuleBase<Money>(() => 
    new IsPositive()  & new IsGreaterThanCurrencyAmount(10, "USD", default!));

public record Money(decimal Amount, string Currency, decimal NormalizedAmount);

[Export("is-even")]
public class IsEven() : Spec<Money>(
    Spec.Build((decimal n) => n % 2 == 0)
        .WhenTrue("is even")
        .WhenFalse("is odd")
        .Create()
        .ChangeModelTo<Money>(m => m.Amount));

[Export("is-positive")]
public class IsPositive() : Spec<Money>(
    Spec.Build((decimal n) => n % 2 == 0)
        .WhenTrue("is positive")
        .WhenFalse("is not positive")
        .Create()
        .ChangeModelTo<Money>(m => m.Amount));

[Export("is-negative")]
public class IsNegative() : Spec<Money>(
    Spec.Build((decimal n) => n % 2 == 0)
        .WhenTrue("is negative")
        .WhenFalse("is not negative")
        .Create()
        .ChangeModelTo<Money>(m => m.Amount));

[Export("is-zero")]
public class IsZero() : Spec<Money>(
    Spec.Build((decimal m) => m == 0)
        .WhenTrue($"is 0")
        .WhenFalse($"is not 0")
        .Create()
        .ChangeModelTo<Money>(m => m.Amount));

[Export("is-at-least-{amount}-{currency}")]
public class IsGreaterThanCurrencyAmount(decimal amount, string currency, MoneyNormalizer moneyNormalizer) : Spec<Money>(() =>
{
    var money = new Money(amount, currency, moneyNormalizer.ConvertToUsd(amount, currency));
    
    return Spec.Build((Money m) => m.NormalizedAmount > money.NormalizedAmount)
        .WhenTrue($"is {amount} {currency}")
        .WhenFalse($"is not {amount} {currency}")
        .Create();
});

public class MoneyNormalizer
{
    public decimal ConvertToUsd(decimal amount, string fromCurrency)
    {
        return fromCurrency switch
        {
            "USD" => amount,
            "EUR" => amount * 1.2m,
            "GBP" => amount * 1.4m,
            _ => throw new NotSupportedException()
        };
    }
}