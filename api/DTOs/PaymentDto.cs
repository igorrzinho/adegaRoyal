namespace AdegaRoyal.Api.DTOs;

/// <summary>Request payload sent to Abacate Pay for payment processing.</summary>
public class PaymentRequestDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>Response received from the Abacate Pay gateway.</summary>
public class PaymentResponseDto
{
    public bool Approved { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
