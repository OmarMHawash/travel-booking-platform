using MediatR;
using System.Diagnostics;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;

namespace TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;

public class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, SearchResultDto>
{
    private readonly IHotelRepository _hotelRepository;

    public SearchHotelsQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<SearchResultDto> Handle(SearchHotelsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var searchCriteria = SearchCriteria.Create(
            searchText: request.SearchRequest.SearchText,
            checkInDate: request.SearchRequest.CheckInDate,
            checkOutDate: request.SearchRequest.CheckOutDate,
            numberOfRooms: request.SearchRequest.NumberOfRooms,
            adults: request.SearchRequest.Adults,
            children: request.SearchRequest.Children,
            minRating: request.SearchRequest.MinRating,
            maxRating: request.SearchRequest.MaxRating,
            cityId: request.SearchRequest.CityId,
            pageNumber: request.SearchRequest.PageNumber,
            pageSize: request.SearchRequest.PageSize,
            sortBy: request.SearchRequest.SortBy);

        var (hotels, totalCount) = await _hotelRepository.SearchHotelsWithPaginationAsync(searchCriteria);

        var hotelDtos = new List<HotelSearchResultDto>();
        foreach (var hotel in hotels)
        {
            var availableRooms = 0;
            var isAvailable = true;
            decimal? minPrice = null;

            // Calculate available rooms and pricing if date range is provided
            if (searchCriteria.HasDateRange)
            {
                availableRooms = hotel.Rooms.Count(room =>
                    room.IsAvailableForPeriod(searchCriteria.CheckInDate!.Value, searchCriteria.CheckOutDate!.Value) &&
                    room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children));

                isAvailable = availableRooms >= searchCriteria.NumberOfRooms;

                if (availableRooms > 0)
                {
                    minPrice = hotel.Rooms
                        .Where(room => room.IsAvailableForPeriod(searchCriteria.CheckInDate!.Value, searchCriteria.CheckOutDate!.Value) &&
                                     room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children))
                        .Min(room => room.RoomType.PricePerNight);
                }
            }
            else
            {
                availableRooms = hotel.Rooms.Count(room =>
                    room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children));

                isAvailable = availableRooms >= searchCriteria.NumberOfRooms;

                if (availableRooms > 0)
                {
                    minPrice = hotel.Rooms
                        .Where(room => room.RoomType.CanAccommodate(searchCriteria.Adults, searchCriteria.Children))
                        .Min(room => room.RoomType.PricePerNight);
                }
            }

            hotelDtos.Add(new HotelSearchResultDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Rating = hotel.Rating,
                City = hotel.City.Name,
                Country = hotel.City.Country,
                PricePerNight = minPrice,
                ImageUrl = hotel.ImageURL,
                AvailableRooms = availableRooms,
                IsAvailable = isAvailable
            });
        }

        stopwatch.Stop();

        return new SearchResultDto
        {
            Hotels = hotelDtos,
            TotalCount = totalCount,
            PageNumber = searchCriteria.PageNumber,
            PageSize = searchCriteria.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / searchCriteria.PageSize),
            QueryTime = stopwatch.Elapsed
        };
    }
}