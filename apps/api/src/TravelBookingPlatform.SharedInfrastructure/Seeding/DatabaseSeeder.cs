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
            await SeedHotelImagesAsync();
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
            new RoomType("Hostel Dorm", "A bed in a shared dormitory. Perfect for solo travelers on a tight budget. Includes locker and shared bathroom access.", 45.00m, 1, 0, "https://example.com/images/room-types/hostel-dorm.jpg"),
            new RoomType("Economy Single", "A small, private room with a single bed and essential amenities. Ideal for a short, budget-friendly stay.", 75.00m, 1, 0, "https://example.com/images/room-types/economy-single.jpg"),
            new RoomType("Budget Double", "A basic private room with a double bed, suitable for couples or solo travelers wanting more space.", 95.00m, 2, 0, "https://example.com/images/room-types/budget-double.jpg"),

            // Standard Tier ($100-200)
            new RoomType("Standard Single", "A comfortable room with a single bed, work desk, and en-suite bathroom. Great for solo business or leisure travelers.", 120.00m, 1, 0, "https://example.com/images/room-types/standard-single.jpg"),
            new RoomType("Standard Double", "Our most popular room. Features a queen-sized bed, modern decor, and all standard amenities for a comfortable stay.", 160.00m, 2, 1, "https://example.com/images/room-types/standard-double.jpg"),
            new RoomType("Twin Room", "Features two separate single beds, perfect for friends or colleagues traveling together.", 140.00m, 2, 0, "https://example.com/images/room-types/twin-room.jpg"),
            new RoomType("Compact Triple", "A cozy room with three single beds or one double and one single, designed for small groups or families.", 180.00m, 3, 0, "https://example.com/images/room-types/compact-triple.jpg"),

            // Deluxe Tier ($200-400)
            new RoomType("Deluxe Double", "A spacious, elegantly appointed room with a king-sized bed, premium linens, and enhanced amenities.", 240.00m, 2, 2, "https://example.com/images/room-types/deluxe-double.jpg"),
            new RoomType("Superior Twin", "An upgraded twin room with more space, better views, and premium bathroom amenities.", 220.00m, 2, 1, "https://example.com/images/room-types/superior-twin.jpg"),
            new RoomType("Family Room", "A large room with multiple bedding options, designed to comfortably accommodate a family of four.", 320.00m, 4, 2, "https://example.com/images/room-types/family-room.jpg"),
            new RoomType("Studio Apartment", "Features a kitchenette and living area, offering the comforts of home for a longer stay.", 280.00m, 2, 1, "https://example.com/images/room-types/studio-apartment.jpg"),
            new RoomType("Business Room", "Designed for the corporate traveler, featuring a large work desk, high-speed internet, and ergonomic chair.", 350.00m, 2, 0, "https://example.com/images/room-types/business-room.jpg"),

            // Premium Tier ($400-700)
            new RoomType("Junior Suite", "A large, open-plan suite with a distinct living area, king-sized bed, and luxurious bathroom.", 450.00m, 2, 2, "https://example.com/images/room-types/junior-suite.jpg"),
            new RoomType("Executive Double", "Located on higher floors with exclusive lounge access, complimentary breakfast, and evening cocktails.", 520.00m, 2, 1, "https://example.com/images/room-types/executive-double.jpg"),
            new RoomType("Premium Family", "A multi-room suite designed for families, featuring separate sleeping areas for parents and children.", 580.00m, 4, 3, "https://example.com/images/room-types/premium-family.jpg"),
            new RoomType("Ocean View Suite", "A stunning suite offering panoramic views of the ocean from a private balcony.", 620.00m, 2, 2, "https://example.com/images/room-types/ocean-view-suite.jpg"),
            new RoomType("City View Suite", "Enjoy breathtaking views of the city skyline from this high-floor suite with floor-to-ceiling windows.", 480.00m, 2, 1, "https://example.com/images/room-types/city-view-suite.jpg"),

            // Luxury Tier ($700-1500)
            new RoomType("Executive Suite", "A one-bedroom suite with a separate living room, dining area, and oversized bathroom with a soaking tub.", 850.00m, 4, 2, "https://example.com/images/room-types/executive-suite.jpg"),
            new RoomType("Penthouse Junior", "Located on the top floors, this suite offers premium luxury, exclusive services, and the best views.", 980.00m, 2, 1, "https://example.com/images/room-types/penthouse-junior.jpg"),
            new RoomType("Royal Suite", "An opulent suite featuring lavish decor, multiple bedrooms, a private terrace, and dedicated butler service.", 1200.00m, 4, 3, "https://example.com/images/room-types/royal-suite.jpg"),
            new RoomType("Ambassador Suite", "A residence-style suite perfect for diplomacy or entertainment, with a formal dining room and reception area.", 1100.00m, 3, 2, "https://example.com/images/room-types/ambassador-suite.jpg"),

            // Ultra-Luxury Tier ($1500+)
            new RoomType("Presidential Suite", "The pinnacle of luxury. A magnificent suite with multiple bedrooms, private gym, and 24/7 butler service.", 1800.00m, 6, 4, "https://example.com/images/room-types/presidential-suite.jpg"),
            new RoomType("Imperial Suite", "A palace within the hotel. Spanning an entire wing, this suite offers unparalleled space, privacy, and opulence.", 2200.00m, 8, 4, "https://example.com/images/room-types/imperial-suite.jpg"),
            new RoomType("Royal Penthouse", "The ultimate hotel experience. A two-story penthouse with a private rooftop pool, cinema, and staff.", 3500.00m, 6, 2, "https://example.com/images/room-types/royal-penthouse.jpg"),

            // Special Categories
            new RoomType("Connecting Rooms", "Two interconnecting rooms that can be booked together, ideal for larger families or groups needing privacy and proximity.", 420.00m, 4, 4, "https://example.com/images/room-types/connecting-rooms.jpg"),
            new RoomType("Accessible Room", "Thoughtfully designed for guests with mobility needs, featuring wider doorways and a roll-in shower.", 180.00m, 2, 1, "https://example.com/images/room-types/accessible-room.jpg"),
            new RoomType("Pet-Friendly Room", "Bring your furry friend along! This room includes a pet bed, bowls, and easy access to outdoor areas.", 200.00m, 2, 2, "https://example.com/images/room-types/pet-friendly-room.jpg"),
            new RoomType("Extended Stay Suite", "A fully-equipped suite with a kitchen, laundry, and weekly housekeeping, designed for stays of a week or more.", 380.00m, 2, 2, "https://example.com/images/room-types/extended-stay-suite.jpg")
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
            new Hotel("The Plaza Hotel", "Luxury hotel in the heart of Manhattan with iconic views and world-class amenities.", 4.8m, newYork.Id),
            new Hotel("Manhattan Business Hotel", "Modern business hotel perfect for corporate travelers with state-of-the-art facilities.", 4.2m, newYork.Id)
        });

        // London Hotels
        var london = cities.First(c => c.Name == "London");
        hotels.AddRange(new[]
        {
            new Hotel("The Ritz London", "Legendary luxury hotel offering timeless elegance and exceptional service since 1906.", 4.9m, london.Id),
            new Hotel("London Bridge Hotel", "Contemporary hotel with stunning Thames views and easy access to major attractions.", 4.1m, london.Id)
        });

        // Paris Hotels
        var paris = cities.First(c => c.Name == "Paris");
        hotels.AddRange(new[]
        {
            new Hotel("Hotel de Crillon", "Palatial hotel on Place de la Concorde combining French elegance with modern luxury.", 4.7m, paris.Id),
            new Hotel("Boutique Marais Hotel", "Charming boutique hotel in the historic Marais district with artistic flair.", 4.3m, paris.Id)
        });

        // Tokyo Hotels
        var tokyo = cities.First(c => c.Name == "Tokyo");
        hotels.AddRange(new[]
        {
            new Hotel("Tokyo Grand Hotel", "Premium hotel offering traditional Japanese hospitality with modern amenities.", 4.6m, tokyo.Id),
            new Hotel("Shibuya Business Tower", "Ultra-modern hotel in the heart of Shibuya with cutting-edge technology.", 4.4m, tokyo.Id)
        });

        // Dubai Hotels
        var dubai = cities.First(c => c.Name == "Dubai");
        hotels.AddRange(new[]
        {
            new Hotel("Burj Al Arab", "Iconic sail-shaped luxury hotel offering unparalleled opulence and personalized service.", 5.0m, dubai.Id),
            new Hotel("Dubai Marina Resort", "Spectacular beachfront resort with world-class amenities and marina views.", 4.5m, dubai.Id),
            new Hotel("Downtown Budget Inn", "Affordable accommodation in the heart of Dubai with essential amenities.", 3.2m, dubai.Id)
        });

        // Sydney Hotels
        var sydney = cities.First(c => c.Name == "Sydney");
        hotels.AddRange(new[]
        {
            new Hotel("Sydney Harbour Luxury", "Premium waterfront hotel with Opera House views and world-class dining.", 4.7m, sydney.Id),
            new Hotel("Bondi Beach Resort", "Relaxed beachfront hotel perfect for surfers and beach lovers.", 4.0m, sydney.Id),
            new Hotel("CBD Business Center", "Modern business hotel in the financial district with conference facilities.", 3.8m, sydney.Id)
        });

        // Barcelona Hotels
        var barcelona = cities.First(c => c.Name == "Barcelona");
        hotels.AddRange(new[]
        {
            new Hotel("Gaudi Palace Hotel", "Artistic luxury hotel inspired by Barcelona's architectural heritage.", 4.6m, barcelona.Id),
            new Hotel("Gothic Quarter Boutique", "Charming hotel in the historic heart of Barcelona.", 4.1m, barcelona.Id),
            new Hotel("Beach Front Barcelona", "Modern hotel near the beach with rooftop pool and city views.", 3.9m, barcelona.Id)
        });

        // Rome Hotels
        var rome = cities.First(c => c.Name == "Rome");
        hotels.AddRange(new[]
        {
            new Hotel("Imperial Roman Grand", "Luxury hotel near the Colosseum with classical elegance.", 4.8m, rome.Id),
            new Hotel("Vatican Hill Hotel", "Premium hotel with Vatican views and papal suite.", 4.4m, rome.Id),
            new Hotel("Trastevere Inn", "Cozy boutique hotel in the charming Trastevere district.", 3.7m, rome.Id)
        });

        // Amsterdam Hotels
        var amsterdam = cities.First(c => c.Name == "Amsterdam");
        hotels.AddRange(new[]
        {
            new Hotel("Canal House Luxury", "Historic canal-side hotel with authentic Dutch charm.", 4.5m, amsterdam.Id),
            new Hotel("Museum Quarter Hotel", "Contemporary hotel near world-famous museums.", 4.0m, amsterdam.Id),
            new Hotel("Amsterdam Central Lodge", "Budget-friendly hostel-hotel hybrid for backpackers.", 3.1m, amsterdam.Id)
        });

        // Singapore Hotels
        var singapore = cities.First(c => c.Name == "Singapore");
        hotels.AddRange(new[]
        {
            new Hotel("Marina Bay Skyline", "Ultra-modern luxury hotel with infinity pool and city views.", 4.9m, singapore.Id),
            new Hotel("Sentosa Island Resort", "Tropical paradise resort with theme park access.", 4.3m, singapore.Id),
            new Hotel("Chinatown Heritage Hotel", "Cultural hotel celebrating Singapore's diverse heritage.", 3.6m, singapore.Id)
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

    private async Task SeedHotelImagesAsync()
    {
        if (await _context.HotelImages.AnyAsync())
        {
            _logger.LogInformation("Hotel images already exist. Skipping image seeding.");
            return;
        }

        var hotels = await _context.Hotels.ToListAsync();
        var images = new List<HotelImage>();

        foreach (var hotel in hotels)
        {
            images.Add(new HotelImage(hotel.Id, $"https://example.com/images/hotels/{hotel.Name.ToLower().Replace(' ', '-')}-cover.jpg", $"The beautiful exterior of {hotel.Name}", 0, true)); // Cover Image
            images.Add(new HotelImage(hotel.Id, $"https://example.com/images/hotels/{hotel.Name.ToLower().Replace(' ', '-')}-lobby.jpg", "Our elegant lobby and reception area", 1));
            images.Add(new HotelImage(hotel.Id, $"https://example.com/images/hotels/{hotel.Name.ToLower().Replace(' ', '-')}-room.jpg", "A view of one of our deluxe rooms", 2));
            images.Add(new HotelImage(hotel.Id, $"https://example.com/images/hotels/{hotel.Name.ToLower().Replace(' ', '-')}-pool.jpg", "The stunning poolside area", 3));
            images.Add(new HotelImage(hotel.Id, $"https://example.com/images/hotels/{hotel.Name.ToLower().Replace(' ', '-')}-restaurant.jpg", "Fine dining at our in-house restaurant", 4));
        }

        _context.HotelImages.AddRange(images);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} images for {HotelCount} hotels.", images.Count, hotels.Count);
    }
}