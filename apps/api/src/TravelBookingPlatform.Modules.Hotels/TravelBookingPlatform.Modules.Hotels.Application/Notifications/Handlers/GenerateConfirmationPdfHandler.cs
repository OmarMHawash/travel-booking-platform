using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Notifications.Handlers;

public class GenerateConfirmationPdfHandler : INotificationHandler<BookingConfirmedNotification>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPdfService _pdfService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateConfirmationPdfHandler> _logger;

    public GenerateConfirmationPdfHandler(IBookingRepository bookingRepository, IPdfService pdfService, IFileStorageService fileStorageService, IUnitOfWork unitOfWork, IMapper mapper, ILogger<GenerateConfirmationPdfHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _pdfService = pdfService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(BookingConfirmedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating PDF confirmation for Booking ID: {BookingId}", notification.BookingId);
        var booking = await _bookingRepository.GetByIdAsync(notification.BookingId);

        if (booking is null)
        {
            _logger.LogError("Booking {BookingId} not found for PDF generation.", notification.BookingId);
            return;
        }

        try
        {
            booking.ResetPdfGenerationStatus();

            var bookingDetailsDto = _mapper.Map<BookingDetailsDto>(booking);
            var pdfBytes = await _pdfService.GenerateBookingConfirmationPdfAsync(bookingDetailsDto);

            var fileName = $"booking-confirmation-{booking.Id}.pdf";
            var fileUrl = await _fileStorageService.UploadFileAsync(fileName, pdfBytes, "application/pdf");

            booking.SetConfirmationPdfUrl(fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for Booking ID: {BookingId}", notification.BookingId);

            booking.MarkPdfGenerationAsFailed(ex.Message);
        }
        finally
        {
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}