using Microsoft.Extensions.Options;
using Stripe;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;

public class StripeService : IPaymentGatewayService
{
    private readonly StripeSettings _stripeSettings;

    public StripeService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, Guid bookingId)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Amount in cents
            Currency = currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "bookingId", bookingId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return paymentIntent.ClientSecret;
    }
}