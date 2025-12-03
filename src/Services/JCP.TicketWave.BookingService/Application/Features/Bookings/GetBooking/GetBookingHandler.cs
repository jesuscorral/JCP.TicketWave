using JCP.TicketWave.BookingService.Domain.Interfaces;

namespace JCP.TicketWave.BookingService.Application.Features.Bookings.GetBooking;

public class GetBookingHandler
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public async Task<GetBookingResponse?> Handle(GetBookingQuery query, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(query.BookingId, cancellationToken);
        
        if (booking == null)
            return null;

        return new GetBookingResponse(
            BookingId: booking.Id,
            BookingReference: booking.Id.ToString()[..8], // Use part of ID as reference
            EventId: booking.EventId,
            UserId: booking.UserId.ToString(),
            TicketCount: booking.Quantity,
            TotalAmount: booking.TotalAmount,
            Status: (BookingStatus)(int)booking.Status,
            CreatedAt: booking.CreatedAt,
            ExpiresAt: booking.ExpiresAt,
            ConfirmedAt: null // TODO: Add confirmation tracking
        );
    }
}