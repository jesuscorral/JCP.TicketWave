namespace JCP.TicketWave.PaymentService.Features.Payments
{
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Cancelled
    }
}