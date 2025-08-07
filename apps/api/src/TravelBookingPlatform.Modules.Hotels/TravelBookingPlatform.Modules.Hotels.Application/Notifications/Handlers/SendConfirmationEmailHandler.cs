using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Notifications.Handlers;

public class SendConfirmationEmailHandler : INotificationHandler<BookingConfirmedNotification>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEmailService _emailService;
    // private readonly IUserService _userService; // Ideal implementation
    private readonly IMapper _mapper;
    private readonly ILogger<SendConfirmationEmailHandler> _logger;

    public SendConfirmationEmailHandler(IBookingRepository bookingRepository, IEmailService emailService, IMapper mapper, ILogger<SendConfirmationEmailHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(BookingConfirmedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending confirmation email for Booking ID: {BookingId}", notification.BookingId);
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(notification.BookingId)
                ?? throw new NotFoundException("Booking", notification.BookingId);

            var userEmail = "testuser@example.com";
            var userName = booking.GuestName;

            var bookingDetailsDto = _mapper.Map<BookingDetailsDto>(booking);
            bookingDetailsDto.UserEmail = userEmail;

            await _emailService.SendBookingConfirmationEmailAsync(bookingDetailsDto, userEmail, userName);
            _logger.LogInformation("Successfully sent confirmation email for Booking ID: {BookingId}", notification.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for Booking ID: {BookingId}", notification.BookingId);
        }
    }
}