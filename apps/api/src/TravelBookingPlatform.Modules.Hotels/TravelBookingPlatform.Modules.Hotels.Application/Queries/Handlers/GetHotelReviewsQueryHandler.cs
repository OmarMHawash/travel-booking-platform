using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetHotelReviewsQueryHandler : IRequestHandler<GetHotelReviewsQuery, PaginatedReviewsDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetHotelReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedReviewsDto> Handle(GetHotelReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByHotelIdAsync(request.HotelId, request.PageNumber, request.PageSize);
        var totalCount = await _reviewRepository.GetCountByHotelIdAsync(request.HotelId);

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

        return new PaginatedReviewsDto
        {
            Reviews = reviewDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}