using Microsoft.EntityFrameworkCore;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Hotels.Domain.ValueObjects;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Persistence.Repositories;

public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
{
    public HotelRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Hotel>> GetByCityIdAsync(Guid cityId)
    {
        return await _dbSet
            .Where(h => h.CityId == cityId)
            .Include(h => h.City)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchByNameAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(h => h.Name.ToLower().Contains(lowerSearchTerm))
            .Include(h => h.City)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchByCityNameAsync(string cityName)
    {
        var lowerCityName = cityName.ToLower();
        return await _dbSet
            .Where(h => h.City.Name.ToLower().Contains(lowerCityName))
            .Include(h => h.City)
            .OrderBy(h => h.City.Name)
            .ThenBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetByRatingRangeAsync(decimal minRating, decimal maxRating)
    {
        return await _dbSet
            .Where(h => h.Rating >= minRating && h.Rating <= maxRating)
            .Include(h => h.City)
            .OrderByDescending(h => h.Rating)
            .ToListAsync();
    }

    public async Task<Hotel?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(h => h.City)
            .FirstOrDefaultAsync(h => h.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(h => h.Id != excludeId.Value);
        }

        return await query.AnyAsync(h => h.Name == name);
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelsWithAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        int adults,
        int children)
    {
        return await _dbSet
            .Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= adults &&
                r.RoomType.MaxChildren >= children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate && b.CheckOutDate > checkInDate)))
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> SearchWithFiltersAsync(
        string? searchTerm = null,
        Guid? cityId = null,
        decimal? minRating = null,
        decimal? maxRating = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        int? adults = null,
        int? children = null)
    {
        var query = _dbSet.AsQueryable();

        // Text search (hotel name or city name) - case insensitive
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(lowerSearchTerm) ||
                h.City.Name.ToLower().Contains(lowerSearchTerm));
        }

        // City filter
        if (cityId.HasValue)
        {
            query = query.Where(h => h.CityId == cityId.Value);
        }

        // Rating filters
        if (minRating.HasValue)
        {
            query = query.Where(h => h.Rating >= minRating.Value);
        }
        if (maxRating.HasValue)
        {
            query = query.Where(h => h.Rating <= maxRating.Value);
        }

        // Availability and capacity filters
        if (checkInDate.HasValue && checkOutDate.HasValue && adults.HasValue && children.HasValue)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= adults.Value &&
                r.RoomType.MaxChildren >= children.Value &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < checkOutDate.Value && b.CheckOutDate > checkInDate.Value)));
        }

        return await query
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .OrderByDescending(h => h.Rating)
            .ThenBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelSuggestionsAsync(string searchText, int maxResults = 10)
    {
        var searchTerm = searchText.ToLower();
        return await _dbSet
            .Where(h => h.Name.ToLower().Contains(searchTerm) || h.City.Name.ToLower().Contains(searchTerm))
            .Include(h => h.City)
            .OrderBy(h => h.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Hotel> Hotels, int TotalCount)> SearchHotelsWithPaginationAsync(SearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        // Text search (hotel name or city name) - case insensitive
        if (criteria.HasSearch)
        {
            var searchTerm = criteria.SearchText!.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(searchTerm) ||
                h.City.Name.ToLower().Contains(searchTerm));
        }

        // City filter
        if (criteria.CityId.HasValue)
        {
            query = query.Where(h => h.CityId == criteria.CityId.Value);
        }

        // Rating filters
        if (criteria.HasRatingFilter)
        {
            if (criteria.MinRating.HasValue)
            {
                query = query.Where(h => h.Rating >= criteria.MinRating.Value);
            }
            if (criteria.MaxRating.HasValue)
            {
                query = query.Where(h => h.Rating <= criteria.MaxRating.Value);
            }
        }

        // Availability and capacity filters
        if (criteria.HasDateRange && criteria.HasGuestRequirements)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= criteria.Adults &&
                r.RoomType.MaxChildren >= criteria.Children &&
                !r.Bookings.Any(b =>
                    b.CheckInDate < criteria.CheckOutDate!.Value &&
                    b.CheckOutDate > criteria.CheckInDate!.Value)));
        }
        else if (criteria.HasGuestRequirements)
        {
            query = query.Where(h => h.Rooms.Any(r =>
                r.RoomType.MaxAdults >= criteria.Adults &&
                r.RoomType.MaxChildren >= criteria.Children));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = criteria.SortBy.ToLower() switch
        {
            "price" => query.OrderBy(h => h.Rooms.Min(r => r.RoomType.PricePerNight)),
            "rating" => query.OrderByDescending(h => h.Rating),
            "name" => query.OrderBy(h => h.Name),
            _ => query.OrderByDescending(h => h.Rating).ThenBy(h => h.Name) // Default: Relevance
        };

        // Apply pagination
        var hotels = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Bookings)
            .Include(h => h.Images)
            .ToListAsync();

        return (hotels, totalCount);
    }

    public async Task<Hotel?> GetHotelWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Where(h => h.Id == id)
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Hotel>> GetHotelsWithDetailsAsync(IEnumerable<Guid> hotelIds)
    {
        return await _dbSet
            .Where(h => hotelIds.Contains(h.Id))
            .Include(h => h.City)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Images)
            .ToListAsync();
    }

    public async Task<Hotel?> GetHotelWithRoomsAndBookingsAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(h => h.Id == hotelId)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Bookings)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }

    public async Task<Hotel?> GetHotelWithImagesAsync(Guid hotelId)
    {
        return await _dbSet
            .Where(h => h.Id == hotelId)
            .Include(h => h.Images)
            .FirstOrDefaultAsync();
    }
}