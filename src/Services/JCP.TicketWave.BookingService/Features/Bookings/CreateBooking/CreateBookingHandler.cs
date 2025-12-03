using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Domain.Models;
using JCP.TicketWave.BookingService.Application.Features.Bookings.CreateBooking;

namespace JCP.TicketWave.BookingService.Features.Bookings.CreateBooking;

public class CreateBookingHandler
{
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public async Task<CreateBookingResponse> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        // Create booking using factory method
        var booking = Booking.Create(
            eventId: command.EventId,
            userId: Guid.Parse(command.UserId),
            customerEmail: "user@example.com", // TODO: Get from user context
            quantity: command.TicketCount,
            totalAmount: 0, // TODO: Calculate based on event price
            expiresAt: DateTime.UtcNow.AddMinutes(15)
        );

        await _bookingRepository.CreateAsync(booking, cancellationToken);
        
        return new CreateBookingResponse(
            BookingId: booking.Id,
            BookingReference: booking.Id.ToString()[..8], // Use part of ID as reference
            Status: (Application.Features.Bookings.CreateBooking.BookingStatus)(int)booking.Status,
            CreatedAt: booking.CreatedAt,
            ExpiresAt: booking.ExpiresAt ?? DateTime.UtcNow.AddMinutes(15));
    }
}
