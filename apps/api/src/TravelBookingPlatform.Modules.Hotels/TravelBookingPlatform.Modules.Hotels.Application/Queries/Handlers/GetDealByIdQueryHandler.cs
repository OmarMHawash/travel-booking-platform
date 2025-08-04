using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetDealByIdQueryHandler : IRequestHandler<GetDealByIdQuery, DealDto?>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetDealByIdQueryHandler(IDealRepository dealRepository, IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<DealDto?> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetDealWithDetailsAsync(request.Id);
        return deal == null ? null : _mapper.Map<DealDto>(deal);
    }
}