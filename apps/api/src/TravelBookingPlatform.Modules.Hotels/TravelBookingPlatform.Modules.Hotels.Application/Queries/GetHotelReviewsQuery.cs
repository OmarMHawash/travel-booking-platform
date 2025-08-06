using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries;

public class PaginatedReviewsDto
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class GetHotelReviewsQuery : IRequest<PaginatedReviewsDto>
{
    public Guid HotelId { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public GetHotelReviewsQuery(Guid hotelId, int pageNumber, int pageSize)
    {
        HotelId = hotelId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}