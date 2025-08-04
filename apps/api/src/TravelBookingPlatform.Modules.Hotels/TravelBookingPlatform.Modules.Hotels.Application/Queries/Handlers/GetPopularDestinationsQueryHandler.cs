using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetPopularDestinationsQueryHandler : IRequestHandler<GetPopularDestinationsQuery, List<CityDto>>
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;

    public GetPopularDestinationsQueryHandler(ICityRepository cityRepository, IMapper mapper)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
    }

    public async Task<List<CityDto>> Handle(GetPopularDestinationsQuery request, CancellationToken cancellationToken)
    {
        var popularCities = await _cityRepository.GetPopularDestinationsAsync(request.Count);
        return _mapper.Map<List<CityDto>>(popularCities);
    }
}