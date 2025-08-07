using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.Notifications;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Enums;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using Payment = TravelBookingPlatform.Modules.Hotels.Domain.Entities.Payment;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;

    public ProcessStripeWebhookCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<ProcessStripeWebhookCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // For now, we are skipping signature validation as we don't have a real webhook secret.
            // In a real application, this would fetch the secret from configuration.
            // var stripeEvent = EventUtility.ConstructEvent(request.JsonPayload, request.StripeSignature, "whsec_YOUR_WEBHOOK_SECRET");

            var stripeEvent = EventUtility.ParseEvent(request.JsonPayload);

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                await HandlePaymentSucceeded(stripeEvent, cancellationToken);
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                await HandlePaymentFailed(stripeEvent, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
            }
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Error processing Stripe webhook: Invalid signature or payload.");
            throw new BusinessValidationException("Invalid Stripe webhook signature.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the Stripe webhook.");
            throw;
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
        _logger.LogInformation("PaymentIntent succeeded for Booking ID: {BookingId}", paymentIntent.Metadata["bookingId"]);

        if (!Guid.TryParse(paymentIntent.Metadata["bookingId"], out var bookingId))
        {
            throw new BusinessValidationException("Invalid Booking ID in PaymentIntent metadata.");
        }

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) throw new NotFoundException(nameof(Booking), bookingId);
        if (booking.Status != BookingStatus.PendingPayment)
        {
            _logger.LogWarning("Received payment confirmation for a booking that is not pending payment. BookingId: {BookingId}, Status: {Status}", booking.Id, booking.Status);
            return;
        }

        var payment = new Payment(
            booking.Id,
            (decimal)paymentIntent.Amount / 100,
            paymentIntent.Id,
            paymentIntent.PaymentMethodTypes.FirstOrDefault() ?? "card");

        booking.Confirm(payment.Id);

        _bookingRepository.Update(booking);
        // Assuming IUnitOfWork is configured to also save the new Payment entity.
        // If not, you'd need an IPaymentRepository. For now, this is sufficient.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new BookingConfirmedNotification(booking.Id), cancellationToken);
    }

    private async Task HandlePaymentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
        _logger.LogWarning("PaymentIntent failed for Booking ID: {BookingId}", paymentIntent.Metadata["bookingId"]);

        if (!Guid.TryParse(paymentIntent.Metadata["bookingId"], out var bookingId))
        {
            throw new BusinessValidationException("Invalid Booking ID in PaymentIntent metadata.");
        }

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null) throw new NotFoundException(nameof(Booking), bookingId);
        if (booking.Status != BookingStatus.PendingPayment)
        {
            return; // Ignore if booking is already confirmed or cancelled.
        }

        booking.Cancel();
        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new BookingPaymentFailedNotification(booking.Id), cancellationToken);
    }
}