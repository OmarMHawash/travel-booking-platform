using MediatR;
using Microsoft.Extensions.Logging;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Application.Notifications;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.Enums; // Required for BookingStatus

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;

/// <summary>
/// A mock implementation of the payment gateway service for local development.
/// This service bypasses any real payment provider interaction and immediately
/// confirms the booking, triggering subsequent background processes.
/// </summary>
public class FakePaymentGatewayService : IPaymentGatewayService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<FakePaymentGatewayService> _logger;

    public FakePaymentGatewayService(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<FakePaymentGatewayService> logger)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Simulates the creation of a payment intent. Instead of calling an external
    /// service, it immediately finds the associated booking, confirms it, and publishes the
    /// BookingConfirmedNotification to trigger the rest of the confirmation workflow.
    /// </summary>
    /// <param name="amount">The total amount for the booking.</param>
    /// <param name="currency">The currency of the transaction.</param>
    /// <param name="bookingId">The ID of the booking to be confirmed.</param>
    /// <returns>A fake client secret for the frontend to consume.</returns>
    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, Guid bookingId)
    {
        _logger.LogInformation(
            "[DEV-ONLY] FakePaymentGatewayService is processing booking {BookingId}. Bypassing Stripe and confirming immediately.",
            bookingId);

        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            _logger.LogError("Booking with ID {BookingId} not found during fake payment processing.", bookingId);
            throw new NotFoundException(nameof(Booking), bookingId);
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            _logger.LogWarning("FakePaymentGateway received a booking that was not in 'PendingPayment' status. BookingId: {BookingId}, Status: {Status}. Flow may have already been processed.", booking.Id, booking.Status);
            return $"pi_fake_dev_already_processed_{Guid.NewGuid().ToString().Replace("-", "")}";
        }

        // 1. Simulate the successful payment by confirming the booking directly.
        // This is the step the real Stripe webhook would trigger.
        var fakePaymentId = Guid.NewGuid();
        booking.Confirm(fakePaymentId);

        // The repository update is often handled by the DbContext's change tracker,
        // but being explicit can clarify intent.
        _bookingRepository.Update(booking);

        // 2. Save the confirmed booking state to the database.
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} status updated to 'Confirmed' in the database via fake service.", bookingId);

        // 3. CRITICAL: Publish the notification to trigger downstream services (PDF generation, email sending).
        // This mimics the action of the real webhook handler.
        await _mediator.Publish(new BookingConfirmedNotification(booking.Id));
        _logger.LogInformation("Published BookingConfirmedNotification for booking {BookingId} to trigger background jobs.", bookingId);

        // 4. Return a fake client secret. The frontend might need a non-empty string.
        var fakeClientSecret = $"pi_fake_dev_{Guid.NewGuid().ToString().Replace("-", "")}";
        return fakeClientSecret;
    }
}