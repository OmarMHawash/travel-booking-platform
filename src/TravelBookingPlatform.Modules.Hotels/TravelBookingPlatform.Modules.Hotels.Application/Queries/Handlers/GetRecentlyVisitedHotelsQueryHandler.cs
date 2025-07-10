using AutoMapper;
using MediatR;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Application.Services;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class GetRecentlyVisitedHotelsQueryHandler : IRequestHandler<GetRecentlyVisitedHotelsQuery, List<RecentlyVisitedHotelDto>>
{
    private readonly IActivityTrackingService _activityTrackingService;
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public GetRecentlyVisitedHotelsQueryHandler(
        IActivityTrackingService activityTrackingService,
        IHotelRepository hotelRepository,
        IMapper mapper)
    {
        _activityTrackingService = activityTrackingService;
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<List<RecentlyVisitedHotelDto>> Handle(GetRecentlyVisitedHotelsQuery request, CancellationToken cancellationToken)
    {
        // Get recent hotel view activities for the user
        var recentActivities = await _activityTrackingService.GetRecentActivitiesAsync(
            request.UserId,
            ActivityType.HotelView,
            request.Limit * 2); // Get more activities to account for potential duplicates

        if (!recentActivities.Any())
        {
            return new List<RecentlyVisitedHotelDto>();
        }

        // Group activities by hotel to get visit counts and last visited dates
        var hotelActivities = recentActivities
            .Where(a => a.TargetId.HasValue && a.TargetType == "Hotel")
            .GroupBy(a => a.TargetId!.Value)
            .Select(g => new
            {
                HotelId = g.Key,
                LastVisitedDate = g.Max(a => a.ActivityDate),
                VisitCount = g.Count()
            })
            .OrderByDescending(x => x.LastVisitedDate)
            .Take(request.Limit)
            .ToList();

        if (!hotelActivities.Any())
        {
            return new List<RecentlyVisitedHotelDto>();
        }

        // Get hotel details for the visited hotels
        var hotelIds = hotelActivities.Select(ha => ha.HotelId).ToList();
        var hotels = await _hotelRepository.GetHotelsWithDetailsAsync(hotelIds);

        // Combine activity data with hotel data
        var result = new List<RecentlyVisitedHotelDto>();

        foreach (var hotelActivity in hotelActivities)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == hotelActivity.HotelId);
            if (hotel == null) continue; // Skip if hotel not found (might be deleted)

            var dto = new RecentlyVisitedHotelDto
            {
                HotelId = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Rating = hotel.Rating,
                ImageUrl = hotel.ImageURL,
                LastVisitedDate = hotelActivity.LastVisitedDate,
                VisitCount = hotelActivity.VisitCount,
                CityId = hotel.CityId,
                CityName = hotel.City?.Name ?? string.Empty,
                Country = hotel.City?.Country ?? string.Empty,
                MinPrice = hotel.Rooms?.Where(r => r.RoomType != null)
                                    .Min(r => r.RoomType!.PricePerNight),
                MaxPrice = hotel.Rooms?.Where(r => r.RoomType != null)
                                    .Max(r => r.RoomType!.PricePerNight)
            };

            result.Add(dto);
        }

        return result;
    }
}