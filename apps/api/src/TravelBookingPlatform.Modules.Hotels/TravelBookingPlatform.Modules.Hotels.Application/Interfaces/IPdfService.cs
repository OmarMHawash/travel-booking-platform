using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateBookingConfirmationPdfAsync(BookingDetailsDto bookingDetails);
}