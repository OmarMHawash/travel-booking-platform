using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Application.Queries;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;


namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public record GetBookingConfirmationQuery(Guid BookingId, Guid UserId) : IQuery<BookingDetailsDto>;

public class GetBookingConfirmationQueryHandler : IRequestHandler<GetBookingConfirmationQuery, BookingDetailsDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public GetBookingConfirmationQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<BookingDetailsDto> Handle(GetBookingConfirmationQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(request.BookingId);

        if (booking is null || booking.UserId != request.UserId)
        {
            throw new NotFoundException("Booking", request.BookingId);
        }

        return _mapper.Map<BookingDetailsDto>(booking);
    }
}