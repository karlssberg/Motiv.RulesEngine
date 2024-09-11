`namespace Api;

public class PaymentService(IExternalService externalService) : IPaymentService
{
    public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
    {
        var (isValid, errors) = ValidateRequest(request);
        var id = Guid.NewGuid().ToString();
        if (!isValid)
        {
            return new PaymentResponse
            {
                Status = PaymentStatus.Declined,
                Id = id,
                Amount = request.Amount,
                Currency = request.Currency,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                LastFourDigits = request.CardNumber[^4..]
            };
        }
        
        var success = await externalService.ProcessPayment(request);
        return new PaymentResponse
        {
            Status = success ? PaymentStatus.Authorized : PaymentStatus.Declined,
            Id = id,
            Amount = request.Amount,
            Currency = request.Currency,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            LastFourDigits = request.CardNumber[^4..]
        };
    }

    public Task<PaymentResponse?> GetPayment(string id)
    {
        return externalService.GetPayment(id);
    }
    
    private (bool isValid, IEnumerable<string> errors) ValidateRequest(PaymentRequest request)
    {
        var errors = new List<string>();
        if (request.CardNumber.Length is < 14 or > 19  || !request.CardNumber.All(char.IsDigit))
        {
            errors.Add("Card number must be 16 digits");
        }
        if (request.ExpiryMonth is < 1 or > 12)
        {
            errors.Add("Expiry month must be between 1 and 12");
        }

        var now = DateTime.Now;
        if (request.ExpiryYear >= now.Year && request.ExpiryMonth >= now.Month)
        {
            errors.Add("Expiry year must be in the future");
        }
        if (request.Currency.Length != 3)
        {
            errors.Add("Currency must be 3 characters");
        }
        if (request.Amount <= 0)
        {
            errors.Add("Amount must be greater than 0");
        }
        if (request.Cvv.Length is < 3 or > 4 || !request.Cvv.All(char.IsDigit))
        {
            errors.Add("CVV must be 3 digits and alphanumeric");
        }
        
        return (errors.Count == 0, errors);
    }
}

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPayment(PaymentRequest request);
    Task<PaymentResponse?> GetPayment(string id);
}

public interface IExternalService
{
    Task<bool> ProcessPayment(object request);
    Task<PaymentResponse?> GetPayment(string id);
}


public record PaymentRequest()
{
    public string CardNumber { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; }
    public int Amount { get; init; }
    public string Cvv { get; init; }
}

public record PaymentResponse()
{
    public string Id { get; init; }
    public PaymentStatus Status { get; init; }
    public string LastFourDigits { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; }
    public int Amount { get; init; }
}


public enum PaymentStatus
{
    Authorized,
    Declined
}