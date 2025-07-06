using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;
using TravelBookingPlatform.Modules.Identity.Domain.ValueObjects;
using TravelBookingPlatform.SharedInfrastructure.Persistence;

namespace TravelBookingPlatform.SharedInfrastructure.Seeding;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            await SeedUsersAsync();
            await SeedCitiesAsync();
            await SeedRoomTypesAsync();
            await SeedHotelsAsync();
            await SeedRoomsAsync();
            await SeedBookingsAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync(u => u.Email.Value == "admin@travelbooking.com"))
        {
            _logger.LogInformation("Admin user already exists. Skipping user seeding.");
            return;
        }

        var users = new List<User>
        {
            new User("admin", "admin@travelbooking.com", "Admin123!", Role.Admin),
            new User("john.doe", "john.doe@email.com", "User123!", Role.TypicalUser),
            new User("jane.smith", "jane.smith@email.com", "User123!", Role.TypicalUser),
            new User("bob.johnson", "bob.johnson@email.com", "User123!", Role.TypicalUser),
            new User("alice.williams", "alice.williams@email.com", "User123!", Role.TypicalUser)
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} users.", users.Count);
    }

    private async Task SeedCitiesAsync()
    {
        if (await _context.Cities.AnyAsync())
        {
            _logger.LogInformation("Cities already exist. Skipping city seeding.");
            return;
        }

        var cities = new List<City>
        {
            new City("New York", "United States", "10001"),
            new City("London", "United Kingdom", "WC2N 5DU"),
            new City("Paris", "France", "75001"),
            new City("Tokyo", "Japan", "100-0001"),
            new City("Dubai", "United Arab Emirates", "00000"),
            new City("Sydney", "Australia", "2000"),
            new City("Barcelona", "Spain", "08001"),
            new City("Rome", "Italy", "00100"),
            new City("Amsterdam", "Netherlands", "1012"),
            new City("Singapore", "Singapore", "018989")
        };

        _context.Cities.AddRange(cities);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} cities.", cities.Count);
    }

    private async Task SeedRoomTypesAsync()
    {
        if (await _context.RoomTypes.AnyAsync())
        {
            _logger.LogInformation("Room types already exist. Skipping room type seeding.");
            return;
        }

        var roomTypes = new List<RoomType>
        {
            new RoomType("Standard Single", 120.00m, 1, 0),
            new RoomType("Standard Double", 180.00m, 2, 1),
            new RoomType("Deluxe Double", 250.00m, 2, 2),
            new RoomType("Family Room", 320.00m, 4, 2),
            new RoomType("Junior Suite", 420.00m, 2, 1),
            new RoomType("Executive Suite", 650.00m, 4, 2),
            new RoomType("Presidential Suite", 1200.00m, 6, 3),
            new RoomType("Economy Single", 85.00m, 1, 0),
            new RoomType("Twin Room", 160.00m, 2, 0),
            new RoomType("Connecting Rooms", 380.00m, 4, 4)
        };

        _context.RoomTypes.AddRange(roomTypes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} room types.", roomTypes.Count);
    }

    private async Task SeedHotelsAsync()
    {
        if (await _context.Hotels.AnyAsync())
        {
            _logger.LogInformation("Hotels already exist. Skipping hotel seeding.");
            return;
        }

        var cities = await _context.Cities.ToListAsync();
        var hotels = new List<Hotel>();

        // New York Hotels
        var newYork = cities.First(c => c.Name == "New York");
        hotels.AddRange(new[]
        {
            new Hotel("The Plaza Hotel", "Luxury hotel in the heart of Manhattan with iconic views and world-class amenities.", 4.8m, newYork.Id, "https://example.com/plaza.jpg"),
            new Hotel("Manhattan Business Hotel", "Modern business hotel perfect for corporate travelers with state-of-the-art facilities.", 4.2m, newYork.Id, "https://example.com/manhattan-business.jpg")
        });

        // London Hotels
        var london = cities.First(c => c.Name == "London");
        hotels.AddRange(new[]
        {
            new Hotel("The Ritz London", "Legendary luxury hotel offering timeless elegance and exceptional service since 1906.", 4.9m, london.Id, "https://example.com/ritz-london.jpg"),
            new Hotel("London Bridge Hotel", "Contemporary hotel with stunning Thames views and easy access to major attractions.", 4.1m, london.Id, "https://example.com/london-bridge.jpg")
        });

        // Paris Hotels
        var paris = cities.First(c => c.Name == "Paris");
        hotels.AddRange(new[]
        {
            new Hotel("Hotel de Crillon", "Palatial hotel on Place de la Concorde combining French elegance with modern luxury.", 4.7m, paris.Id, "https://example.com/crillon.jpg"),
            new Hotel("Boutique Marais Hotel", "Charming boutique hotel in the historic Marais district with artistic flair.", 4.3m, paris.Id, "https://example.com/marais.jpg")
        });

        // Tokyo Hotels
        var tokyo = cities.First(c => c.Name == "Tokyo");
        hotels.AddRange(new[]
        {
            new Hotel("Tokyo Grand Hotel", "Premium hotel offering traditional Japanese hospitality with modern amenities.", 4.6m, tokyo.Id, "https://example.com/tokyo-grand.jpg"),
            new Hotel("Shibuya Business Tower", "Ultra-modern hotel in the heart of Shibuya with cutting-edge technology.", 4.4m, tokyo.Id, "https://example.com/shibuya.jpg")
        });

        // Dubai Hotels
        var dubai = cities.First(c => c.Name == "Dubai");
        hotels.AddRange(new[]
        {
            new Hotel("Burj Al Arab", "Iconic sail-shaped luxury hotel offering unparalleled opulence and personalized service.", 5.0m, dubai.Id, "https://example.com/burj-al-arab.jpg"),
            new Hotel("Dubai Marina Resort", "Spectacular beachfront resort with world-class amenities and marina views.", 4.5m, dubai.Id, "https://example.com/marina-resort.jpg")
        });

        _context.Hotels.AddRange(hotels);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} hotels.", hotels.Count);
    }

    private async Task SeedRoomsAsync()
    {
        if (await _context.Rooms.AnyAsync())
        {
            _logger.LogInformation("Rooms already exist. Skipping room seeding.");
            return;
        }

        var hotels = await _context.Hotels.ToListAsync();
        var roomTypes = await _context.RoomTypes.ToListAsync();
        var rooms = new List<Room>();

        foreach (var hotel in hotels)
        {

            // Standard rooms (most hotels have these)
            var standardDouble = roomTypes.First(rt => rt.Name == "Standard Double");
            var standardSingle = roomTypes.First(rt => rt.Name == "Standard Single");
            var deluxeDouble = roomTypes.First(rt => rt.Name == "Deluxe Double");

            for (int floor = 1; floor <= 3; floor++)
            {
                for (int roomNum = 1; roomNum <= 10; roomNum++)
                {
                    var roomNumber = $"{floor}{roomNum:D2}";

                    // Mix of room types
                    RoomType selectedRoomType;
                    if (roomNum <= 6)
                        selectedRoomType = standardDouble;
                    else if (roomNum <= 8)
                        selectedRoomType = standardSingle;
                    else
                        selectedRoomType = deluxeDouble;

                    rooms.Add(new Room(roomNumber, hotel.Id, selectedRoomType.Id));
                }
            }

            // Add some premium rooms for luxury hotels
            if (hotel.Rating >= 4.5m)
            {
                var juniorSuite = roomTypes.First(rt => rt.Name == "Junior Suite");
                var executiveSuite = roomTypes.First(rt => rt.Name == "Executive Suite");
                var presidentialSuite = roomTypes.First(rt => rt.Name == "Presidential Suite");

                rooms.Add(new Room("401", hotel.Id, juniorSuite.Id));
                rooms.Add(new Room("402", hotel.Id, juniorSuite.Id));
                rooms.Add(new Room("501", hotel.Id, executiveSuite.Id));

                if (hotel.Rating >= 4.8m)
                {
                    rooms.Add(new Room("601", hotel.Id, presidentialSuite.Id));
                }
            }
        }

        _context.Rooms.AddRange(rooms);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} rooms.", rooms.Count);
    }

    private async Task SeedBookingsAsync()
    {
        if (await _context.Bookings.AnyAsync())
        {
            _logger.LogInformation("Bookings already exist. Skipping booking seeding.");
            return;
        }

        var rooms = await _context.Rooms.Take(20).ToListAsync(); // Take first 20 rooms
        var users = await _context.Users.Where(u => u.Role == Role.TypicalUser).ToListAsync();
        var bookings = new List<Booking>();

        var random = new Random(42); // Fixed seed for consistent data

        for (int i = 0; i < 15; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            // Create bookings for different time periods
            var checkInDate = DateTime.Today.AddDays(random.Next(1, 90)); // 1 to 90 days from now
            var nights = random.Next(1, 8); // 1 to 7 nights
            var checkOutDate = checkInDate.AddDays(nights);

            try
            {
                var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                bookings.Add(booking);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Skipped invalid booking: {Message}", ex.Message);
            }
        }

        if (bookings.Any())
        {
            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} bookings.", bookings.Count);
        }
    }
}