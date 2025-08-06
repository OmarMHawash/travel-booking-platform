using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;
public class SendGridEmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    public SendGridEmailService(IOptions<SendGridSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendBookingConfirmationEmailAsync(BookingDetailsDto bookingDetails, string recipientEmail, string recipientName)
    {
        var client = new SendGridClient(_settings.ApiKey);
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(recipientEmail, recipientName);
        var subject = $"Your Booking is Confirmed! Confirmation #{bookingDetails.ConfirmationNumber}";
        var plainTextContent = $"Dear {recipientName}, Your booking for {bookingDetails.HotelName} is confirmed.";
        var htmlContent = $"<strong>Dear {recipientName},</strong><p>Your booking for {bookingDetails.HotelName} from {bookingDetails.CheckInDate:D} to {bookingDetails.CheckOutDate:D} is confirmed.</p>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }
}