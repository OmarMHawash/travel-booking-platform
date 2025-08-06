using MediatR;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public class ProcessStripeWebhookCommand : IRequest
{
    public string JsonPayload { get; }
    public string StripeSignature { get; }

    public ProcessStripeWebhookCommand(string jsonPayload, string stripeSignature)
    {
        JsonPayload = jsonPayload;
        StripeSignature = stripeSignature;
    }
}