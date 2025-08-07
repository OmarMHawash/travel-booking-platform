using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Domain.Exceptions;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelGalleryQueryHandler : IRequestHandler<GetHotelGalleryQuery, List<HotelImageDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetHotelGalleryQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<List<HotelImageDto>> Handle(GetHotelGalleryQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetHotelWithImagesAsync(request.HotelId);

        if (hotel is null)
        {
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        }

        var images = hotel.Images.OrderBy(i => i.SortOrder).ToList();
        return _mapper.Map<List<HotelImageDto>>(images);
    }
}