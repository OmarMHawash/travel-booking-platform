using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationEmailAsync(BookingDetailsDto bookingDetails, string recipientEmail, string recipientName);
}