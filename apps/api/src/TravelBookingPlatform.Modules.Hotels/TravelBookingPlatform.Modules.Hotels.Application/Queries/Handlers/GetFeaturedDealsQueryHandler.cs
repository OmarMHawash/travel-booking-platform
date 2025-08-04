using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetFeaturedDealsQueryHandler : IRequestHandler<GetFeaturedDealsQuery, List<FeaturedDealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetFeaturedDealsQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<List<FeaturedDealDto>> Handle(GetFeaturedDealsQuery request, CancellationToken cancellationToken)
    {
        var deals = await _dealRepository.GetFeaturedDealsAsync(request.Count);
        return _mapper.Map<List<FeaturedDealDto>>(deals);
    }
}