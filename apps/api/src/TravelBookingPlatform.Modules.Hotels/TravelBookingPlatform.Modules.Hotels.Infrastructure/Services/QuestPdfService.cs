using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;

public class QuestPdfService : IPdfService
{
    public async Task<byte[]> GenerateBookingConfirmationPdfAsync(BookingDetailsDto bookingDetails)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = new BookingConfirmationDocument(bookingDetails);
        return await Task.FromResult(document.GeneratePdf());
    }
}

public class BookingConfirmationDocument : IDocument
{
    private readonly BookingDetailsDto _model;

    public BookingConfirmationDocument(BookingDetailsDto model)
    {
        _model = model;
    }

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Header().Text($"Booking Confirmation #{_model.ConfirmationNumber}").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Hotel: {_model.HotelName}, {_model.CityName}");
                    column.Item().Text($"Guest: {_model.GuestName}");
                    column.Item().Text($"Check-in: {_model.CheckInDate:D}");
                    column.Item().Text($"Check-out: {_model.CheckOutDate:D}");
                    column.Item().Text($"Total Price: {_model.TotalPrice:C}");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
    }
}