namespace JCP.TicketWave.PaymentService.Domain.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}

public enum PaymentEventType
{
    Created = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5,
    Updated = 6
}

public enum PaymentMethodType
{
    Card = 0,
    PayPal = 1,
    ApplePay = 2,
    GooglePay = 3,
    BankTransfer = 4,
    Cryptocurrency = 5
}