using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetAllDealsQueryHandler : IRequestHandler<GetAllDealsQuery, List<DealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetAllDealsQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<List<DealDto>> Handle(GetAllDealsQuery request, CancellationToken cancellationToken)
    {
        var deals = await _dealRepository.GetActiveDealsAsync();
        return _mapper.Map<List<DealDto>>(deals);
    }
}