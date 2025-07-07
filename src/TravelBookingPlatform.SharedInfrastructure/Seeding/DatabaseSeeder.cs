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
            await SeedDealsAsync();

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
            new User("admin", "admin@travelbooking.com", BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role.Admin),
            new User("john.doe", "john.doe@email.com", BCrypt.Net.BCrypt.HashPassword("User123!"), Role.TypicalUser),
            new User("jane.smith", "jane.smith@email.com", BCrypt.Net.BCrypt.HashPassword("User123!"), Role.TypicalUser),
            new User("bob.johnson", "bob.johnson@email.com", BCrypt.Net.BCrypt.HashPassword("User123!"), Role.TypicalUser),
            new User("alice.williams", "alice.williams@email.com", BCrypt.Net.BCrypt.HashPassword("User123!"), Role.TypicalUser)
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
            // Budget Tier (Under $100)
            new RoomType("Hostel Dorm", 45.00m, 1, 0),
            new RoomType("Economy Single", 75.00m, 1, 0),
            new RoomType("Budget Double", 95.00m, 2, 0),

            // Standard Tier ($100-200)
            new RoomType("Standard Single", 120.00m, 1, 0),
            new RoomType("Standard Double", 160.00m, 2, 1),
            new RoomType("Twin Room", 140.00m, 2, 0),
            new RoomType("Compact Triple", 180.00m, 3, 0),

            // Deluxe Tier ($200-400)
            new RoomType("Deluxe Double", 240.00m, 2, 2),
            new RoomType("Superior Twin", 220.00m, 2, 1),
            new RoomType("Family Room", 320.00m, 4, 2),
            new RoomType("Studio Apartment", 280.00m, 2, 1),
            new RoomType("Business Room", 350.00m, 2, 0),

            // Premium Tier ($400-700)
            new RoomType("Junior Suite", 450.00m, 2, 2),
            new RoomType("Executive Double", 520.00m, 2, 1),
            new RoomType("Premium Family", 580.00m, 4, 3),
            new RoomType("Ocean View Suite", 620.00m, 2, 2),
            new RoomType("City View Suite", 480.00m, 2, 1),

            // Luxury Tier ($700-1500)
            new RoomType("Executive Suite", 850.00m, 4, 2),
            new RoomType("Penthouse Junior", 980.00m, 2, 1),
            new RoomType("Royal Suite", 1200.00m, 4, 3),
            new RoomType("Ambassador Suite", 1100.00m, 3, 2),

            // Ultra-Luxury Tier ($1500+)
            new RoomType("Presidential Suite", 1800.00m, 6, 4),
            new RoomType("Imperial Suite", 2200.00m, 8, 4),
            new RoomType("Royal Penthouse", 3500.00m, 6, 2),

            // Special Categories
            new RoomType("Connecting Rooms", 420.00m, 4, 4),
            new RoomType("Accessible Room", 180.00m, 2, 1),
            new RoomType("Pet-Friendly Room", 200.00m, 2, 2),
            new RoomType("Extended Stay Suite", 380.00m, 2, 2)
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
            new Hotel("Dubai Marina Resort", "Spectacular beachfront resort with world-class amenities and marina views.", 4.5m, dubai.Id, "https://example.com/marina-resort.jpg"),
            new Hotel("Downtown Budget Inn", "Affordable accommodation in the heart of Dubai with essential amenities.", 3.2m, dubai.Id, "https://example.com/dubai-budget.jpg")
        });

        // Sydney Hotels
        var sydney = cities.First(c => c.Name == "Sydney");
        hotels.AddRange(new[]
        {
            new Hotel("Sydney Harbour Luxury", "Premium waterfront hotel with Opera House views and world-class dining.", 4.7m, sydney.Id, "https://example.com/sydney-harbour.jpg"),
            new Hotel("Bondi Beach Resort", "Relaxed beachfront hotel perfect for surfers and beach lovers.", 4.0m, sydney.Id, "https://example.com/bondi.jpg"),
            new Hotel("CBD Business Center", "Modern business hotel in the financial district with conference facilities.", 3.8m, sydney.Id, "https://example.com/sydney-business.jpg")
        });

        // Barcelona Hotels
        var barcelona = cities.First(c => c.Name == "Barcelona");
        hotels.AddRange(new[]
        {
            new Hotel("Gaudi Palace Hotel", "Artistic luxury hotel inspired by Barcelona's architectural heritage.", 4.6m, barcelona.Id, "https://example.com/gaudi-palace.jpg"),
            new Hotel("Gothic Quarter Boutique", "Charming hotel in the historic heart of Barcelona.", 4.1m, barcelona.Id, "https://example.com/gothic-quarter.jpg"),
            new Hotel("Beach Front Barcelona", "Modern hotel near the beach with rooftop pool and city views.", 3.9m, barcelona.Id, "https://example.com/barcelona-beach.jpg")
        });

        // Rome Hotels
        var rome = cities.First(c => c.Name == "Rome");
        hotels.AddRange(new[]
        {
            new Hotel("Imperial Roman Grand", "Luxury hotel near the Colosseum with classical elegance.", 4.8m, rome.Id, "https://example.com/imperial-rome.jpg"),
            new Hotel("Vatican Hill Hotel", "Premium hotel with Vatican views and papal suite.", 4.4m, rome.Id, "https://example.com/vatican-hill.jpg"),
            new Hotel("Trastevere Inn", "Cozy boutique hotel in the charming Trastevere district.", 3.7m, rome.Id, "https://example.com/trastevere.jpg")
        });

        // Amsterdam Hotels
        var amsterdam = cities.First(c => c.Name == "Amsterdam");
        hotels.AddRange(new[]
        {
            new Hotel("Canal House Luxury", "Historic canal-side hotel with authentic Dutch charm.", 4.5m, amsterdam.Id, "https://example.com/canal-house.jpg"),
            new Hotel("Museum Quarter Hotel", "Contemporary hotel near world-famous museums.", 4.0m, amsterdam.Id, "https://example.com/museum-quarter.jpg"),
            new Hotel("Amsterdam Central Lodge", "Budget-friendly hostel-hotel hybrid for backpackers.", 3.1m, amsterdam.Id, "https://example.com/amsterdam-lodge.jpg")
        });

        // Singapore Hotels
        var singapore = cities.First(c => c.Name == "Singapore");
        hotels.AddRange(new[]
        {
            new Hotel("Marina Bay Skyline", "Ultra-modern luxury hotel with infinity pool and city views.", 4.9m, singapore.Id, "https://example.com/marina-bay.jpg"),
            new Hotel("Sentosa Island Resort", "Tropical paradise resort with theme park access.", 4.3m, singapore.Id, "https://example.com/sentosa.jpg"),
            new Hotel("Chinatown Heritage Hotel", "Cultural hotel celebrating Singapore's diverse heritage.", 3.6m, singapore.Id, "https://example.com/chinatown.jpg")
        });

        _context.Hotels.AddRange(hotels);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} hotels across {CityCount} cities.", hotels.Count, cities.Count);
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
            _logger.LogInformation("Creating rooms for hotel: {HotelName} (Rating: {Rating})", hotel.Name, hotel.Rating);

            if (hotel.Rating < 4.0m) // Budget Hotels
            {
                SeedBudgetHotelRooms(hotel, roomTypes, rooms);
            }
            else if (hotel.Rating >= 4.0m && hotel.Rating < 4.5m) // Business Hotels
            {
                SeedBusinessHotelRooms(hotel, roomTypes, rooms);
            }
            else if (hotel.Rating >= 4.5m && hotel.Rating < 4.8m) // Premium Hotels
            {
                SeedPremiumHotelRooms(hotel, roomTypes, rooms);
            }
            else // Luxury Hotels (4.8+)
            {
                SeedLuxuryHotelRooms(hotel, roomTypes, rooms);
            }
        }

        _context.Rooms.AddRange(rooms);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} rooms across {HotelCount} hotels.", rooms.Count, hotels.Count);
    }

    private void SeedBudgetHotelRooms(Hotel hotel, List<RoomType> roomTypes, List<Room> rooms)
    {
        var budgetDouble = roomTypes.First(rt => rt.Name == "Budget Double");
        var economySingle = roomTypes.First(rt => rt.Name == "Economy Single");
        var standardSingle = roomTypes.First(rt => rt.Name == "Standard Single");
        var standardDouble = roomTypes.First(rt => rt.Name == "Standard Double");
        var twinRoom = roomTypes.First(rt => rt.Name == "Twin Room");

        // 4 floors, mostly budget and standard rooms
        for (int floor = 1; floor <= 4; floor++)
        {
            for (int roomNum = 1; roomNum <= 12; roomNum++)
            {
                var roomNumber = $"{floor}{roomNum:D2}";
                RoomType selectedRoomType;

                if (roomNum <= 4)
                    selectedRoomType = economySingle;
                else if (roomNum <= 7)
                    selectedRoomType = budgetDouble;
                else if (roomNum <= 9)
                    selectedRoomType = standardSingle;
                else if (roomNum <= 11)
                    selectedRoomType = standardDouble;
                else
                    selectedRoomType = twinRoom;

                rooms.Add(new Room(roomNumber, hotel.Id, selectedRoomType.Id));
            }
        }

        // Add some accessible rooms
        var accessibleRoom = roomTypes.First(rt => rt.Name == "Accessible Room");
        rooms.Add(new Room("101A", hotel.Id, accessibleRoom.Id));
        rooms.Add(new Room("201A", hotel.Id, accessibleRoom.Id));
    }

    private void SeedBusinessHotelRooms(Hotel hotel, List<RoomType> roomTypes, List<Room> rooms)
    {
        var standardSingle = roomTypes.First(rt => rt.Name == "Standard Single");
        var standardDouble = roomTypes.First(rt => rt.Name == "Standard Double");
        var twinRoom = roomTypes.First(rt => rt.Name == "Twin Room");
        var businessRoom = roomTypes.First(rt => rt.Name == "Business Room");
        var deluxeDouble = roomTypes.First(rt => rt.Name == "Deluxe Double");
        var superiorTwin = roomTypes.First(rt => rt.Name == "Superior Twin");
        var studioApartment = roomTypes.First(rt => rt.Name == "Studio Apartment");

        // 6 floors with mix of business and standard rooms
        for (int floor = 1; floor <= 6; floor++)
        {
            for (int roomNum = 1; roomNum <= 10; roomNum++)
            {
                var roomNumber = $"{floor}{roomNum:D2}";
                RoomType selectedRoomType;

                if (floor <= 2) // Lower floors - standard rooms
                {
                    if (roomNum <= 4)
                        selectedRoomType = standardSingle;
                    else if (roomNum <= 7)
                        selectedRoomType = standardDouble;
                    else
                        selectedRoomType = twinRoom;
                }
                else if (floor <= 4) // Mid floors - business mix
                {
                    if (roomNum <= 3)
                        selectedRoomType = businessRoom;
                    else if (roomNum <= 6)
                        selectedRoomType = deluxeDouble;
                    else if (roomNum <= 8)
                        selectedRoomType = superiorTwin;
                    else
                        selectedRoomType = studioApartment;
                }
                else // Upper floors - premium business
                {
                    if (roomNum <= 5)
                        selectedRoomType = businessRoom;
                    else if (roomNum <= 8)
                        selectedRoomType = deluxeDouble;
                    else
                        selectedRoomType = studioApartment;
                }

                rooms.Add(new Room(roomNumber, hotel.Id, selectedRoomType.Id));
            }
        }

        // Add family and special rooms
        var familyRoom = roomTypes.First(rt => rt.Name == "Family Room");
        var accessibleRoom = roomTypes.First(rt => rt.Name == "Accessible Room");
        rooms.Add(new Room("701", hotel.Id, familyRoom.Id));
        rooms.Add(new Room("702", hotel.Id, familyRoom.Id));
        rooms.Add(new Room("101A", hotel.Id, accessibleRoom.Id));
    }

    private void SeedPremiumHotelRooms(Hotel hotel, List<RoomType> roomTypes, List<Room> rooms)
    {
        var standardDouble = roomTypes.First(rt => rt.Name == "Standard Double");
        var deluxeDouble = roomTypes.First(rt => rt.Name == "Deluxe Double");
        var superiorTwin = roomTypes.First(rt => rt.Name == "Superior Twin");
        var familyRoom = roomTypes.First(rt => rt.Name == "Family Room");
        var juniorSuite = roomTypes.First(rt => rt.Name == "Junior Suite");
        var cityViewSuite = roomTypes.First(rt => rt.Name == "City View Suite");
        var executiveDouble = roomTypes.First(rt => rt.Name == "Executive Double");
        var premiumFamily = roomTypes.First(rt => rt.Name == "Premium Family");
        var executiveSuite = roomTypes.First(rt => rt.Name == "Executive Suite");

        // 8 floors with increasing luxury by floor
        for (int floor = 1; floor <= 8; floor++)
        {
            for (int roomNum = 1; roomNum <= 8; roomNum++)
            {
                var roomNumber = $"{floor}{roomNum:D2}";
                RoomType selectedRoomType;

                if (floor <= 2) // Standard floors
                {
                    if (roomNum <= 4)
                        selectedRoomType = standardDouble;
                    else if (roomNum <= 6)
                        selectedRoomType = deluxeDouble;
                    else
                        selectedRoomType = superiorTwin;
                }
                else if (floor <= 4) // Deluxe floors
                {
                    if (roomNum <= 3)
                        selectedRoomType = deluxeDouble;
                    else if (roomNum <= 5)
                        selectedRoomType = superiorTwin;
                    else if (roomNum <= 6)
                        selectedRoomType = familyRoom;
                    else
                        selectedRoomType = juniorSuite;
                }
                else if (floor <= 6) // Premium floors
                {
                    if (roomNum <= 2)
                        selectedRoomType = juniorSuite;
                    else if (roomNum <= 4)
                        selectedRoomType = cityViewSuite;
                    else if (roomNum <= 6)
                        selectedRoomType = executiveDouble;
                    else
                        selectedRoomType = premiumFamily;
                }
                else // Executive floors
                {
                    if (roomNum <= 4)
                        selectedRoomType = executiveDouble;
                    else if (roomNum <= 6)
                        selectedRoomType = cityViewSuite;
                    else
                        selectedRoomType = executiveSuite;
                }

                rooms.Add(new Room(roomNumber, hotel.Id, selectedRoomType.Id));
            }
        }

        // Special rooms
        var accessibleRoom = roomTypes.First(rt => rt.Name == "Accessible Room");
        var petFriendlyRoom = roomTypes.First(rt => rt.Name == "Pet-Friendly Room");
        var connectingRooms = roomTypes.First(rt => rt.Name == "Connecting Rooms");

        rooms.Add(new Room("101A", hotel.Id, accessibleRoom.Id));
        rooms.Add(new Room("201A", hotel.Id, accessibleRoom.Id));
        rooms.Add(new Room("301P", hotel.Id, petFriendlyRoom.Id));
        rooms.Add(new Room("401C", hotel.Id, connectingRooms.Id));
    }

    private void SeedLuxuryHotelRooms(Hotel hotel, List<RoomType> roomTypes, List<Room> rooms)
    {
        var deluxeDouble = roomTypes.First(rt => rt.Name == "Deluxe Double");
        var juniorSuite = roomTypes.First(rt => rt.Name == "Junior Suite");
        var cityViewSuite = roomTypes.First(rt => rt.Name == "City View Suite");
        var oceanViewSuite = roomTypes.First(rt => rt.Name == "Ocean View Suite");
        var executiveDouble = roomTypes.First(rt => rt.Name == "Executive Double");
        var premiumFamily = roomTypes.First(rt => rt.Name == "Premium Family");
        var executiveSuite = roomTypes.First(rt => rt.Name == "Executive Suite");
        var penthouseJunior = roomTypes.First(rt => rt.Name == "Penthouse Junior");
        var royalSuite = roomTypes.First(rt => rt.Name == "Royal Suite");
        var ambassadorSuite = roomTypes.First(rt => rt.Name == "Ambassador Suite");
        var presidentialSuite = roomTypes.First(rt => rt.Name == "Presidential Suite");
        var imperialSuite = roomTypes.First(rt => rt.Name == "Imperial Suite");
        var royalPenthouse = roomTypes.First(rt => rt.Name == "Royal Penthouse");

        // 12 floors with ultra-luxury distribution
        for (int floor = 1; floor <= 12; floor++)
        {
            int roomsPerFloor = floor <= 6 ? 10 : (floor <= 9 ? 6 : 4); // Fewer rooms on higher floors

            for (int roomNum = 1; roomNum <= roomsPerFloor; roomNum++)
            {
                var roomNumber = $"{floor}{roomNum:D2}";
                RoomType selectedRoomType;

                if (floor <= 2) // Deluxe floors
                {
                    if (roomNum <= 6)
                        selectedRoomType = deluxeDouble;
                    else
                        selectedRoomType = juniorSuite;
                }
                else if (floor <= 4) // Premium floors
                {
                    if (roomNum <= 4)
                        selectedRoomType = juniorSuite;
                    else if (roomNum <= 7)
                        selectedRoomType = cityViewSuite;
                    else
                        selectedRoomType = oceanViewSuite;
                }
                else if (floor <= 6) // Executive floors
                {
                    if (roomNum <= 3)
                        selectedRoomType = oceanViewSuite;
                    else if (roomNum <= 6)
                        selectedRoomType = executiveDouble;
                    else if (roomNum <= 8)
                        selectedRoomType = premiumFamily;
                    else
                        selectedRoomType = executiveSuite;
                }
                else if (floor <= 9) // Luxury floors
                {
                    if (roomNum <= 2)
                        selectedRoomType = executiveSuite;
                    else if (roomNum <= 4)
                        selectedRoomType = penthouseJunior;
                    else
                        selectedRoomType = royalSuite;
                }
                else if (floor <= 11) // Ultra-luxury floors
                {
                    if (roomNum <= 2)
                        selectedRoomType = royalSuite;
                    else
                        selectedRoomType = ambassadorSuite;
                }
                else // Penthouse floor
                {
                    if (roomNum == 1)
                        selectedRoomType = presidentialSuite;
                    else if (roomNum == 2)
                        selectedRoomType = imperialSuite;
                    else
                        selectedRoomType = royalPenthouse;
                }

                rooms.Add(new Room(roomNumber, hotel.Id, selectedRoomType.Id));
            }
        }

        // Special luxury amenities
        var accessibleRoom = roomTypes.First(rt => rt.Name == "Accessible Room");
        var petFriendlyRoom = roomTypes.First(rt => rt.Name == "Pet-Friendly Room");
        var connectingRooms = roomTypes.First(rt => rt.Name == "Connecting Rooms");
        var extendedStaySuite = roomTypes.First(rt => rt.Name == "Extended Stay Suite");

        rooms.Add(new Room("101A", hotel.Id, accessibleRoom.Id));
        rooms.Add(new Room("201A", hotel.Id, accessibleRoom.Id));
        rooms.Add(new Room("301P", hotel.Id, petFriendlyRoom.Id));
        rooms.Add(new Room("302P", hotel.Id, petFriendlyRoom.Id));
        rooms.Add(new Room("401C", hotel.Id, connectingRooms.Id));
        rooms.Add(new Room("501E", hotel.Id, extendedStaySuite.Id));
        rooms.Add(new Room("502E", hotel.Id, extendedStaySuite.Id));
    }

    private async Task SeedBookingsAsync()
    {
        if (await _context.Bookings.AnyAsync())
        {
            _logger.LogInformation("Bookings already exist. Skipping booking seeding.");
            return;
        }

        var rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();
        var users = await _context.Users.Where(u => u.Role == Role.TypicalUser).ToListAsync();
        var bookings = new List<Booking>();

        var random = new Random(42); // Fixed seed for consistent data

        // Create strategic booking patterns for testing

        // 1. Past bookings (completed stays)
        await CreatePastBookings(rooms, users, bookings, random);

        // 2. Current bookings (ongoing stays)
        await CreateCurrentBookings(rooms, users, bookings, random);

        // 3. Near future bookings (next 30 days)
        await CreateNearFutureBookings(rooms, users, bookings, random);

        // 4. Far future bookings (31-180 days)
        await CreateFarFutureBookings(rooms, users, bookings, random);

        // 5. Weekend and holiday pattern bookings
        await CreateWeekendBookings(rooms, users, bookings, random);

        // 6. Extended stay bookings
        await CreateExtendedStayBookings(rooms, users, bookings, random);

        if (bookings.Any())
        {
            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} strategic bookings.", bookings.Count);
        }
    }

    private async Task CreatePastBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create 50 past bookings (last 6 months)
        for (int i = 0; i < 50; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            var daysBack = random.Next(30, 180); // 30-180 days ago
            var checkInDate = DateTime.Today.AddDays(-daysBack);
            var nights = random.Next(1, 14); // 1-14 nights
            var checkOutDate = checkInDate.AddDays(nights);

            // Ensure checkout is in the past
            if (checkOutDate < DateTime.Today)
            {
                try
                {
                    var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                    bookings.Add(booking);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Skipped invalid past booking: {Message}", ex.Message);
                }
            }
        }
    }

    private async Task CreateCurrentBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create 15 current bookings (checked in, not yet checked out)
        for (int i = 0; i < 15; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            var daysBack = random.Next(1, 7); // Checked in 1-7 days ago
            var checkInDate = DateTime.Today.AddDays(-daysBack);
            var nightsRemaining = random.Next(1, 10); // 1-10 nights remaining
            var checkOutDate = DateTime.Today.AddDays(nightsRemaining);

            try
            {
                var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                bookings.Add(booking);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Skipped invalid current booking: {Message}", ex.Message);
            }
        }
    }

    private async Task CreateNearFutureBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create 80 bookings for the next 30 days
        for (int i = 0; i < 80; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            var daysAhead = random.Next(1, 30); // 1-30 days from now
            var checkInDate = DateTime.Today.AddDays(daysAhead);
            var nights = GetNightsBasedOnRoomType(room, random);
            var checkOutDate = checkInDate.AddDays(nights);

            // Check if room is available for this period
            if (!HasConflictingBooking(bookings, room.Id, checkInDate, checkOutDate))
            {
                try
                {
                    var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                    bookings.Add(booking);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Skipped invalid near future booking: {Message}", ex.Message);
                }
            }
        }
    }

    private async Task CreateFarFutureBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create 60 bookings for 31-180 days in the future
        for (int i = 0; i < 60; i++)
        {
            var room = rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            var daysAhead = random.Next(31, 180); // 31-180 days from now
            var checkInDate = DateTime.Today.AddDays(daysAhead);
            var nights = GetNightsBasedOnRoomType(room, random);
            var checkOutDate = checkInDate.AddDays(nights);

            // Check if room is available for this period
            if (!HasConflictingBooking(bookings, room.Id, checkInDate, checkOutDate))
            {
                try
                {
                    var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                    bookings.Add(booking);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Skipped invalid far future booking: {Message}", ex.Message);
                }
            }
        }
    }

    private async Task CreateWeekendBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create bookings specifically for weekends over the next 8 weeks
        var currentDate = DateTime.Today;

        for (int week = 0; week < 8; week++)
        {
            // Find the next Friday
            var friday = currentDate.AddDays(((int)DayOfWeek.Friday - (int)currentDate.DayOfWeek + 7) % 7 + (week * 7));

            // Create some weekend bookings (Friday-Sunday or Saturday-Monday)
            for (int i = 0; i < random.Next(3, 8); i++)
            {
                var room = rooms[random.Next(rooms.Count)];
                var user = users[random.Next(users.Count)];

                var checkInDate = random.Next(2) == 0 ? friday : friday.AddDays(1); // Friday or Saturday
                var checkOutDate = checkInDate.AddDays(random.Next(2, 4)); // 2-3 nights

                if (!HasConflictingBooking(bookings, room.Id, checkInDate, checkOutDate))
                {
                    try
                    {
                        var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                        bookings.Add(booking);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning("Skipped invalid weekend booking: {Message}", ex.Message);
                    }
                }
            }
        }
    }

    private async Task CreateExtendedStayBookings(List<Room> rooms, List<User> users, List<Booking> bookings, Random random)
    {
        // Create some extended stay bookings (7+ nights) for business travelers
        var extendedStayRooms = rooms.Where(r =>
            r.RoomType.Name.Contains("Suite") ||
            r.RoomType.Name.Contains("Extended") ||
            r.RoomType.Name.Contains("Business") ||
            r.RoomType.Name.Contains("Studio")).ToList();

        for (int i = 0; i < 20; i++)
        {
            var room = extendedStayRooms.Any() ? extendedStayRooms[random.Next(extendedStayRooms.Count)] : rooms[random.Next(rooms.Count)];
            var user = users[random.Next(users.Count)];

            var daysAhead = random.Next(1, 90); // 1-90 days from now
            var checkInDate = DateTime.Today.AddDays(daysAhead);
            var nights = random.Next(7, 30); // 7-30 nights for extended stays
            var checkOutDate = checkInDate.AddDays(nights);

            if (!HasConflictingBooking(bookings, room.Id, checkInDate, checkOutDate))
            {
                try
                {
                    var booking = new Booking(checkInDate, checkOutDate, room.Id, user.Id);
                    bookings.Add(booking);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Skipped invalid extended stay booking: {Message}", ex.Message);
                }
            }
        }
    }

    private int GetNightsBasedOnRoomType(Room room, Random random)
    {
        // Adjust stay length based on room type
        if (room.RoomType.Name.Contains("Presidential") || room.RoomType.Name.Contains("Imperial") || room.RoomType.Name.Contains("Royal"))
            return random.Next(3, 10); // 3-9 nights for ultra-luxury
        else if (room.RoomType.Name.Contains("Suite") || room.RoomType.Name.Contains("Executive"))
            return random.Next(2, 7); // 2-6 nights for suites
        else if (room.RoomType.Name.Contains("Family") || room.RoomType.Name.Contains("Connecting"))
            return random.Next(3, 8); // 3-7 nights for families
        else if (room.RoomType.Name.Contains("Business"))
            return random.Next(1, 5); // 1-4 nights for business
        else if (room.RoomType.Name.Contains("Hostel") || room.RoomType.Name.Contains("Economy"))
            return random.Next(1, 4); // 1-3 nights for budget
        else
            return random.Next(1, 6); // 1-5 nights for standard
    }

    private bool HasConflictingBooking(List<Booking> existingBookings, Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        return existingBookings.Any(b =>
            b.RoomId == roomId &&
            b.CheckInDate < checkOut &&
            b.CheckOutDate > checkIn);
    }

    private async Task SeedDealsAsync()
    {
        if (await _context.Deals.AnyAsync())
        {
            _logger.LogInformation("Deals already exist. Skipping deal seeding.");
            return;
        }

        var hotels = await _context.Hotels.ToListAsync();
        var roomTypes = await _context.RoomTypes.ToListAsync();
        var deals = new List<Deal>();

        // Get specific room types for different deal categories
        var standardDouble = roomTypes.First(rt => rt.Name == "Standard Double");
        var deluxeDouble = roomTypes.First(rt => rt.Name == "Deluxe Double");
        var juniorSuite = roomTypes.First(rt => rt.Name == "Junior Suite");
        var executiveSuite = roomTypes.First(rt => rt.Name == "Executive Suite");
        var familyRoom = roomTypes.First(rt => rt.Name == "Family Room");

        // Featured Deals (for home page)
        deals.Add(new Deal(
            "New York Winter Getaway",
            "Experience the magic of New York in winter! Stay at The Plaza Hotel with exclusive amenities and complimentary breakfast.",
            hotels.First(h => h.Name == "The Plaza Hotel").Id,
            350.00m, // Original price
            249.00m, // Discounted price
            DateTime.Today.AddDays(-5), // Valid from (already started)
            DateTime.Today.AddDays(45), // Valid to
            true, // IsFeatured
            deluxeDouble.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "London Royal Experience",
            "Live like royalty at The Ritz London! Includes afternoon tea, spa access, and premium room upgrade.",
            hotels.First(h => h.Name == "The Ritz London").Id,
            550.00m,
            399.00m,
            DateTime.Today.AddDays(-2),
            DateTime.Today.AddDays(60),
            true, // IsFeatured
            juniorSuite.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Dubai Luxury Escape",
            "Ultimate luxury at Burj Al Arab! All-inclusive package with private beach access and butler service.",
            hotels.First(h => h.Name == "Burj Al Arab").Id,
            1200.00m,
            899.00m,
            DateTime.Today.AddDays(-1),
            DateTime.Today.AddDays(30),
            true, // IsFeatured
            executiveSuite.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Paris Romance Package",
            "Romantic getaway in the City of Love! Stay at Hotel de Crillon with champagne, roses, and Seine cruise.",
            hotels.First(h => h.Name == "Hotel de Crillon").Id,
            420.00m,
            299.00m,
            DateTime.Today,
            DateTime.Today.AddDays(40),
            true, // IsFeatured
            deluxeDouble.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Tokyo Business Special",
            "Perfect for business travelers! Tokyo Grand Hotel with meeting room access and airport transfer.",
            hotels.First(h => h.Name == "Tokyo Grand Hotel").Id,
            280.00m,
            199.00m,
            DateTime.Today.AddDays(-3),
            DateTime.Today.AddDays(50),
            true, // IsFeatured
            standardDouble.Id // RoomTypeId
        ));

        // Regular Deals (not featured)
        deals.Add(new Deal(
            "Manhattan Business Special",
            "Extended stay discount for business travelers with complimentary WiFi and meeting room access.",
            hotels.First(h => h.Name == "Manhattan Business Hotel").Id,
            220.00m,
            179.00m,
            DateTime.Today.AddDays(5),
            DateTime.Today.AddDays(90),
            false, // Not featured
            standardDouble.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "London Bridge Family Deal",
            "Perfect for families visiting London! Spacious family rooms with Thames views and kids activities.",
            hotels.First(h => h.Name == "London Bridge Hotel").Id,
            380.00m,
            299.00m,
            DateTime.Today.AddDays(10),
            DateTime.Today.AddDays(75),
            false,
            familyRoom.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Marais Art District Special",
            "Discover Paris art scene! Boutique hotel stay with museum passes and guided art tours included.",
            hotels.First(h => h.Name == "Boutique Marais Hotel").Id,
            180.00m,
            149.00m,
            DateTime.Today.AddDays(7),
            DateTime.Today.AddDays(80),
            false,
            standardDouble.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Shibuya Tech Experience",
            "Modern Tokyo adventure with latest tech amenities and VR entertainment room access.",
            hotels.First(h => h.Name == "Shibuya Business Tower").Id,
            300.00m,
            229.00m,
            DateTime.Today.AddDays(12),
            DateTime.Today.AddDays(65),
            false,
            deluxeDouble.Id // RoomTypeId
        ));

        deals.Add(new Deal(
            "Dubai Marina Sunset Package",
            "Spectacular marina views with sunset yacht tour and beachfront dining experience.",
            hotels.First(h => h.Name == "Dubai Marina Resort").Id,
            450.00m,
            349.00m,
            DateTime.Today.AddDays(15),
            DateTime.Today.AddDays(55),
            false,
            juniorSuite.Id // RoomTypeId
        ));

        // Some expired deals (for testing)
        var expiredDeal = new Deal(
            "Christmas Special (Expired)",
            "Holiday season special that has ended - for testing expired deals functionality.",
            hotels.First(h => h.Name == "The Plaza Hotel").Id,
            200.00m,
            149.00m,
            DateTime.Today.AddDays(-30),
            DateTime.Today.AddDays(-5), // Expired
            false,
            standardDouble.Id // RoomTypeId
        );
        expiredDeal.Deactivate(); // Make it inactive
        deals.Add(expiredDeal);

        // Future deals (for testing)
        deals.Add(new Deal(
            "Summer Early Bird Special",
            "Book early for summer vacation! Limited time offer starting next month.",
            hotels.First(h => h.Name == "The Ritz London").Id,
            600.00m,
            449.00m,
            DateTime.Today.AddDays(30), // Starts in future
            DateTime.Today.AddDays(120),
            false,
            executiveSuite.Id // RoomTypeId
        ));

        _context.Deals.AddRange(deals);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} deals.", deals.Count);
    }
}