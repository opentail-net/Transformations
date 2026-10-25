using Transformations.Mapping;

namespace Smoke.Mapping.Domain;

[MapTo<PaymentRecord>]
public partial class Payment
{
    public int Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
}

public sealed record PaymentRecord(int Id, string Method, decimal Amount, string Status);
