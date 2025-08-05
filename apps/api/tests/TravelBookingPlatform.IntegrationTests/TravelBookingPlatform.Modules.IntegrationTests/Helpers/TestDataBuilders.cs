using AutoFixture;
using BCrypt.Net;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.ValueObjects;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;
using TravelBookingPlatform.Modules.Identity.Application.Commands;
using TravelBookingPlatform.Modules.Identity.Application.DTOs;

namespace TravelBookingPlatform.Modules.IntegrationTests.Helpers;

public static class TestDataBuilders
{
    private static readonly Fixture _fixture = new();

    /// <summary>
    /// Creates a valid City entity with unique values
    /// </summary>
    public static City CreateValidCity(string? name = null, string? country = null, string? postCode = null)
    {
        // Use only valid characters (letters, spaces, hyphens, apostrophes, periods)
        var cityNames = new[] { "Paris", "London", "Madrid", "Rome", "Berlin", "Vienna", "Prague", "Dublin", "Athens", "Budapest", "New York", "Los Angeles", "San Francisco", "Las Vegas", "New Orleans", "Santa Fe", "Saint Petersburg", "Mont-Blanc", "O'Connor", "St. Moritz" };
        var countryNames = new[] { "France", "England", "Spain", "Italy", "Germany", "Austria", "Czech Republic", "Ireland", "Greece", "Hungary", "United States", "United Kingdom", "South Africa", "New Zealand", "Costa Rica", "Saudi Arabia", "South Korea", "North Macedonia" };
        var suffixes = new[] { " North", " South", " East", " West", " Central", " Old Town", " Downtown", " Heights", " Valley", " Hills", " Springs", " Bay", " Beach", " Point", " Ridge", " Park", " Center", " Square", " Village", " Gardens" };

        var random = new Random();
        var cityName = cityNames[random.Next(cityNames.Length)];
        var countryName = countryNames[random.Next(countryNames.Length)];

        // Sometimes add a suffix to make cities unique, but keep them business-valid
        if (random.Next(2) == 0)
        {
            cityName += suffixes[random.Next(suffixes.Length)];
        }

        return new City(
            name ?? cityName,
            country ?? countryName,
            postCode ?? $"{Random.Shared.Next(10000, 99999)}" // 5 digit postcode
        );
    }

    /// <summary>
    /// Creates multiple valid City entities
    /// </summary>
    public static List<City> CreateValidCities(int count = 3)
    {
        var cities = new List<City>();
        for (int i = 0; i < count; i++)
        {
            cities.Add(CreateValidCity());
        }
        return cities;
    }

    /// <summary>
    /// Creates a valid Deal entity with required relationships
    /// </summary>
    public static Deal CreateValidDeal(Guid? hotelId = null, Guid? roomTypeId = null, string? title = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var originalPrice = _fixture.Create<decimal>() % 1000 + 200; // Price between 200-1200
        var discountedPrice = originalPrice * 0.8m; // 20% discount

        return new Deal(
            title ?? $"Test Deal {uniqueId}",
            $"Test Deal Description {uniqueId}",
            hotelId ?? Guid.NewGuid(),
            originalPrice,
            discountedPrice,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            false, // isFeatured
            roomTypeId ?? Guid.NewGuid(),
            100, // maxBookings
            null // imageUrl
        );
    }

    /// <summary>
    /// Creates multiple valid Deal entities
    /// </summary>
    public static List<Deal> CreateValidDeals(int count = 3, Guid? hotelId = null)
    {
        var deals = new List<Deal>();
        for (int i = 0; i < count; i++)
        {
            deals.Add(CreateValidDeal(hotelId));
        }
        return deals;
    }

    /// <summary>
    /// Creates multiple featured Deal entities
    /// </summary>
    public static List<Deal> CreateFeaturedDeals(int count = 3, Guid? hotelId = null)
    {
        var deals = new List<Deal>();
        for (int i = 0; i < count; i++)
        {
            deals.Add(CreateFeaturedDeal(hotelId));
        }
        return deals;
    }

    /// <summary>
    /// Creates a valid User entity with proper email value object and BCrypt password hash
    /// </summary>
    public static User CreateValidUser(string? email = null, string? username = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var userEmail = email ?? $"testuser{uniqueId}@example.com";
        var userName = username ?? $"testuser{uniqueId}";
        var rawPassword = password ?? "ValidPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        return new User(userName, userEmail, passwordHash, Role.TypicalUser);
    }

    /// <summary>
    /// Creates multiple valid User entities
    /// </summary>
    public static List<User> CreateValidUsers(int count = 3)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateValidUser());
        }
        return users;
    }

    /// <summary>
    /// Creates a User entity with specific role
    /// </summary>
    public static User CreateUserWithRole(Role role, string? email = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var userEmail = email ?? $"testuser{uniqueId}@example.com";
        var userName = $"testuser{uniqueId}";
        var rawPassword = password ?? "ValidPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        return new User(userName, userEmail, passwordHash, role);
    }

    /// <summary>
    /// Creates a featured Deal entity
    /// </summary>
    public static Deal CreateFeaturedDeal(Guid? hotelId = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var originalPrice = _fixture.Create<decimal>() % 1000 + 200;
        var discountedPrice = originalPrice * 0.8m;

        return new Deal(
            $"Featured Deal {uniqueId}",
            $"Featured Deal Description {uniqueId}",
            hotelId ?? Guid.NewGuid(),
            originalPrice,
            discountedPrice,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            true, // isFeatured
            Guid.NewGuid(),
            100,
            null
        );
    }

    /// <summary>
    /// Creates an inactive Deal entity
    /// </summary>
    public static Deal CreateInactiveDeal(Guid? hotelId = null)
    {
        var deal = CreateValidDeal(hotelId);
        deal.Deactivate();
        return deal;
    }

    /// <summary>
    /// Creates a Deal entity with specific date range
    /// </summary>
    public static Deal CreateDealWithDateRange(DateTime validFrom, DateTime validTo, Guid? hotelId = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var originalPrice = _fixture.Create<decimal>() % 1000 + 200;
        var discountedPrice = originalPrice * 0.8m;

        return new Deal(
            $"Date Range Deal {uniqueId}",
            $"Date Range Deal Description {uniqueId}",
            hotelId ?? Guid.NewGuid(),
            originalPrice,
            discountedPrice,
            validFrom,
            validTo,
            false,
            Guid.NewGuid(),
            100,
            null
        );
    }

    /// <summary>
    /// Creates a City entity with specific name (useful for testing conflicts)
    /// </summary>
    public static City CreateCityWithName(string name, string? country = null, string? postCode = null)
    {
        return CreateValidCity(name, country, postCode);
    }

    /// <summary>
    /// Creates a User entity with specific email (useful for testing conflicts)
    /// </summary>
    public static User CreateUserWithEmail(string email, string? username = null)
    {
        return CreateValidUser(email, username);
    }

    // ========== COMMAND/DTO BUILDERS ==========

    /// <summary>
    /// Creates a valid CreateCityCommand with business-compliant data
    /// </summary>
    public static CreateCityCommand CreateValidCityCommand(string? name = null, string? country = null, string? postCode = null)
    {
        var cityNames = new[] { "Paris", "London", "Madrid", "Rome", "Berlin", "Vienna", "Prague", "Dublin", "Athens", "Budapest", "New York", "Los Angeles", "San Francisco", "Las Vegas", "New Orleans", "Santa Fe", "Saint Petersburg", "Mont-Blanc", "O'Connor", "St. Moritz" };
        var countryNames = new[] { "France", "England", "Spain", "Italy", "Germany", "Austria", "Czech Republic", "Ireland", "Greece", "Hungary", "United States", "United Kingdom", "South Africa", "New Zealand", "Costa Rica", "Saudi Arabia", "South Korea", "North Macedonia" };
        var suffixes = new[] { " North", " South", " East", " West", " Central", " Old Town", " Downtown", " Heights", " Valley", " Hills", " Springs", " Bay", " Beach", " Point", " Ridge", " Park", " Center", " Square", " Village", " Gardens" };

        var random = new Random();
        var cityName = cityNames[random.Next(cityNames.Length)];
        var countryName = countryNames[random.Next(countryNames.Length)];

        // Sometimes add a suffix to make cities unique, but keep them business-valid
        if (random.Next(2) == 0)
        {
            cityName += suffixes[random.Next(suffixes.Length)];
        }

        return new CreateCityCommand
        {
            Name = name ?? cityName,
            Country = country ?? countryName,
            PostCode = postCode ?? $"{Random.Shared.Next(10000, 99999)}"
        };
    }

    /// <summary>
    /// Creates a valid RegisterUserCommand with BCrypt-compatible data
    /// </summary>
    public static RegisterUserCommand CreateValidRegisterUserCommand(string? email = null, string? username = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        return new RegisterUserCommand
        {
            Email = email ?? $"testuser{uniqueId}@example.com",
            Username = username ?? $"testuser{uniqueId}",
            Password = password ?? "ValidPassword123!"
        };
    }

    /// <summary>
    /// Creates a valid LoginCommand
    /// </summary>
    public static LoginCommand CreateValidLoginCommand(string? email = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        return new LoginCommand
        {
            Email = email ?? $"testuser{uniqueId}@example.com",
            Password = password ?? "ValidPassword123!"
        };
    }

    /// <summary>
    /// Creates a LoginCommand with specific credentials for an existing user
    /// </summary>
    public static LoginCommand CreateLoginCommandForUser(User user, string password = "ValidPassword123!")
    {
        return new LoginCommand
        {
            Email = user.Email.Value,
            Password = password
        };
    }

    /// <summary>
    /// Creates a RegisterUserCommand with specific email (for conflict testing)
    /// </summary>
    public static RegisterUserCommand CreateRegisterUserCommandWithEmail(string email)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        return new RegisterUserCommand
        {
            Email = email,
            Username = $"testuser{uniqueId}",
            Password = "ValidPassword123!"
        };
    }

    /// <summary>
    /// Creates a CreateCityCommand with specific postcode (for conflict testing)
    /// </summary>
    public static CreateCityCommand CreateCityCommandWithPostCode(string postCode)
    {
        var cityNames = new[] { "Paris", "London", "Madrid", "Rome", "Berlin" };
        var countryNames = new[] { "France", "England", "Spain", "Italy", "Germany" };

        var random = new Random();
        var randomSuffix = random.Next(1000, 9999);

        return new CreateCityCommand
        {
            Name = $"{cityNames[random.Next(cityNames.Length)]} {randomSuffix}",
            Country = $"{countryNames[random.Next(countryNames.Length)]} {randomSuffix}",
            PostCode = postCode
        };
    }

    /// <summary>
    /// Creates an invalid CreateCityCommand (for validation testing)
    /// </summary>
    public static CreateCityCommand CreateInvalidCityCommand()
    {
        return new CreateCityCommand
        {
            Name = "", // Invalid: empty name
            Country = "", // Invalid: empty country
            PostCode = "12" // Invalid: too short
        };
    }

    /// <summary>
    /// Gets the default test password used in all test users
    /// </summary>
    public static string GetDefaultTestPassword() => "ValidPassword123!";

    // ========== DEAL DTO BUILDERS ==========

    /// <summary>
    /// Creates a valid CreateDealDto with business-compliant data
    /// </summary>
    public static CreateDealDto CreateValidCreateDealDto(Guid? hotelId = null, Guid? roomTypeId = null, string? title = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var random = new Random();

        // Generate valid price relationship (discounted must be < original)
        var originalPrice = random.Next(100, 1000); // $100-$1000
        var discountedPrice = originalPrice * 0.8m; // 20% discount

        // Generate valid date range (ValidTo > ValidFrom, ValidFrom >= Today)
        var validFrom = DateTime.Today.AddDays(random.Next(1, 7)); // 1-7 days from now
        var validTo = validFrom.AddDays(random.Next(7, 30)); // 7-30 days from validFrom

        return new CreateDealDto
        {
            Title = title ?? $"Test Deal {uniqueId}",
            Description = $"Test Deal Description {uniqueId}",
            HotelId = hotelId ?? Guid.NewGuid(),
            RoomTypeId = roomTypeId ?? Guid.NewGuid(),
            OriginalPrice = originalPrice,
            DiscountedPrice = discountedPrice,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsFeatured = false,
            MaxBookings = random.Next(10, 100),
            ImageURL = null // Keep optional field null for simplicity
        };
    }

    /// <summary>
    /// Creates a valid UpdateDealDto with business-compliant data
    /// </summary>
    public static UpdateDealDto CreateValidUpdateDealDto(Guid? roomTypeId = null, string? title = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var random = new Random();

        var originalPrice = random.Next(100, 1000);
        var discountedPrice = originalPrice * 0.75m; // 25% discount

        var validFrom = DateTime.Today.AddDays(random.Next(1, 7));
        var validTo = validFrom.AddDays(random.Next(7, 30));

        return new UpdateDealDto
        {
            Title = title ?? $"Updated Deal {uniqueId}",
            Description = $"Updated Deal Description {uniqueId}",
            RoomTypeId = roomTypeId ?? Guid.NewGuid(),
            OriginalPrice = originalPrice,
            DiscountedPrice = discountedPrice,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsFeatured = false,
            MaxBookings = random.Next(10, 100),
            ImageURL = null
        };
    }

    /// <summary>
    /// Creates a CreateDealDto with featured status
    /// </summary>
    public static CreateDealDto CreateFeaturedCreateDealDto(Guid? hotelId = null)
    {
        var dealDto = CreateValidCreateDealDto(hotelId);
        dealDto.IsFeatured = true;
        dealDto.Title = $"Featured {dealDto.Title}";
        return dealDto;
    }

    /// <summary>
    /// Creates a CreateDealDto with specific title (for testing conflicts)
    /// </summary>
    public static CreateDealDto CreateDealDtoWithTitle(string title, Guid? hotelId = null)
    {
        var dealDto = CreateValidCreateDealDto(hotelId);
        dealDto.Title = title;
        return dealDto;
    }

    /// <summary>
    /// Creates a CreateDealDto with invalid price relationship (discounted >= original)
    /// </summary>
    public static CreateDealDto CreateDealDtoWithInvalidPrices()
    {
        var dealDto = CreateValidCreateDealDto();
        dealDto.OriginalPrice = 100m;
        dealDto.DiscountedPrice = 120m; // Invalid: higher than original
        return dealDto;
    }

    /// <summary>
    /// Creates a CreateDealDto with invalid date range (ValidTo <= ValidFrom)
    /// </summary>
    public static CreateDealDto CreateDealDtoWithInvalidDates()
    {
        var dealDto = CreateValidCreateDealDto();
        dealDto.ValidFrom = DateTime.Today.AddDays(10);
        dealDto.ValidTo = DateTime.Today.AddDays(5); // Invalid: earlier than ValidFrom
        return dealDto;
    }

    /// <summary>
    /// Creates a CreateDealDto with past ValidFrom date
    /// </summary>
    public static CreateDealDto CreateDealDtoWithPastDate()
    {
        var dealDto = CreateValidCreateDealDto();
        dealDto.ValidFrom = DateTime.Today.AddDays(-5); // Invalid: in the past
        dealDto.ValidTo = DateTime.Today.AddDays(10);
        return dealDto;
    }

    /// <summary>
    /// Creates an invalid CreateDealDto (for validation testing)
    /// </summary>
    public static CreateDealDto CreateInvalidCreateDealDto()
    {
        return new CreateDealDto
        {
            Title = "", // Invalid: empty title
            Description = "", // Invalid: empty description
            HotelId = Guid.Empty, // Invalid: empty GUID
            RoomTypeId = Guid.Empty, // Invalid: empty GUID
            OriginalPrice = 0, // Invalid: must be > 0
            DiscountedPrice = 0, // Invalid: must be > 0
            ValidFrom = DateTime.MinValue, // Invalid: empty date
            ValidTo = DateTime.MinValue, // Invalid: empty date
            IsFeatured = false,
            MaxBookings = 0, // Invalid: must be > 0
            ImageURL = "invalid-url" // Invalid: not a valid URL
        };
    }

    /// <summary>
    /// Creates a CreateDealDto with fields exceeding maximum length limits
    /// </summary>
    public static CreateDealDto CreateDealDtoWithTooLongFields()
    {
        var dealDto = CreateValidCreateDealDto();
        dealDto.Title = new string('a', 201); // Invalid: exceeds 200 char limit
        dealDto.Description = new string('b', 2001); // Invalid: exceeds 2000 char limit
        dealDto.ImageURL = "https://example.com/" + new string('c', 500); // Invalid: exceeds 500 char limit
        return dealDto;
    }

    /// <summary>
    /// Creates a CreateDealDto with extreme price values
    /// </summary>
    public static CreateDealDto CreateDealDtoWithExtremeValues()
    {
        var dealDto = CreateValidCreateDealDto();
        dealDto.OriginalPrice = 100001m; // Invalid: exceeds $100,000 limit
        dealDto.DiscountedPrice = 100001m; // Invalid: exceeds $100,000 limit
        dealDto.MaxBookings = 10001; // Invalid: exceeds 10,000 limit
        return dealDto;
    }

    /// <summary>
    /// Creates CreateDealCommand from CreateDealDto
    /// </summary>
    public static CreateDealCommand CreateValidCreateDealCommand(Guid? hotelId = null)
    {
        var dealDto = CreateValidCreateDealDto(hotelId);
        return new CreateDealCommand(dealDto);
    }

    /// <summary>
    /// Creates UpdateDealCommand from UpdateDealDto
    /// </summary>
    public static UpdateDealCommand CreateValidUpdateDealCommand(Guid dealId, Guid? roomTypeId = null)
    {
        var dealDto = CreateValidUpdateDealDto(roomTypeId);
        return new UpdateDealCommand(dealId, dealDto);
    }

    /// <summary>
    /// Creates a valid Hotel entity for mocking
    /// </summary>
    public static Hotel CreateValidHotel(Guid? cityId = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return new Hotel(
            $"Test Hotel {uniqueId}",
            $"Test Hotel Description {uniqueId}",
            4.5m, // Rating between 0-5
            cityId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates a valid RoomType entity for mocking
    /// </summary>
    public static RoomType CreateValidRoomType()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var random = new Random();
        return new RoomType(
            $"Test Room Type {uniqueId}",
            $"Test Room Type Description {uniqueId}",
            random.Next(50, 300), // Price per night between $50-$300
            random.Next(1, 4), // Max adults 1-4
            random.Next(0, 2),  // Max children 0-2
            null
        );
    }
}