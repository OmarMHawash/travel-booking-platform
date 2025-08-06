namespace TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, Guid bookingId);
}