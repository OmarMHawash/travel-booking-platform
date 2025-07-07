# Travel Booking Platform - Backend API Specification

## Architecture Overview

- **Framework**: .NET Core Web API
- **Architecture**: Clean Architecture with Modular Monolith
- **Authentication**: JWT-based with Role-Based Access Control (RBAC)
- **Database**: Entity Framework Core with SQL Server
- **Patterns**: CQRS, Repository Pattern, Unit of Work

## Core Modules

1. **Identity Module**: User authentication and authorization
2. **Hotels Module**: Hotel, room, and booking management
3. **Shared Infrastructure**: Cross-cutting concerns

## Domain Entities & Relationships

### Identity Domain

```csharp
// User entity with role-based access
User {
    Id: Guid (PK)
    Email: string (unique)
    PasswordHash: string
    FirstName: string
    LastName: string
    Role: enum (Admin, User)
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

// Roles enum
Role { Admin, User }
```

### Hotels Domain

```csharp
City {
    Id: Guid (PK)
    Name: string
    Country: string
    PostOffice: string
    CreatedAt: DateTime
    UpdatedAt: DateTime
    // Navigation: Hotels[]
}

Hotel {
    Id: Guid (PK)
    Name: string
    Description: string
    StarRating: int (1-5)
    CityId: Guid (FK)
    Address: string
    Latitude: decimal
    Longitude: decimal
    CreatedAt: DateTime
    UpdatedAt: DateTime
    // Navigation: City, Rooms[], Bookings[]
}

RoomType {
    Id: Guid (PK)
    Name: string (Single, Double, Suite, etc.)
    Description: string
    BasePrice: decimal
    MaxAdults: int
    MaxChildren: int
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

Room {
    Id: Guid (PK)
    RoomNumber: string
    HotelId: Guid (FK)
    RoomTypeId: Guid (FK)
    IsAvailable: bool
    CreatedAt: DateTime
    UpdatedAt: DateTime
    // Navigation: Hotel, RoomType, Bookings[]
}

Booking {
    Id: Guid (PK)
    UserId: Guid (FK)
    HotelId: Guid (FK)
    RoomId: Guid (FK)
    CheckInDate: DateTime
    CheckOutDate: DateTime
    TotalPrice: decimal
    Status: enum (Pending, Confirmed, Cancelled)
    SpecialRequests: string
    Adults: int
    Children: int
    CreatedAt: DateTime
    UpdatedAt: DateTime
    // Navigation: User, Hotel, Room
}
```

## API Endpoints Structure

### Authentication Endpoints

```
POST /api/auth/register          - User registration
POST /api/auth/login            - User login (returns JWT)
POST /api/auth/change-password  - Change user password
```

### User Management Endpoints

```
GET  /api/users/{id}            - Get user by ID
GET  /api/users/email/{email}   - Get user by email
```

### Cities Endpoints

```
GET    /api/cities              - Get all cities
POST   /api/cities              - Create city (Admin only)
PUT    /api/cities/{id}         - Update city (Admin only)
DELETE /api/cities/{id}         - Delete city (Admin only)
```

### Hotels Search & Discovery

```
GET /api/search/hotels          - Search hotels with filters
GET /api/search/suggestions     - Get search suggestions
GET /api/destinations/popular   - Get popular destinations
```

### Hotels Management (Admin)

```
GET    /api/hotels              - Get all hotels
POST   /api/hotels              - Create hotel (Admin only)
PUT    /api/hotels/{id}         - Update hotel (Admin only)
DELETE /api/hotels/{id}         - Delete hotel (Admin only)
```

### Rooms Management (Admin)

```
GET    /api/rooms               - Get all rooms
POST   /api/rooms               - Create room (Admin only)
PUT    /api/rooms/{id}          - Update room (Admin only)
DELETE /api/rooms/{id}          - Delete room (Admin only)
```

### Booking Endpoints

```
GET  /api/bookings              - Get user bookings
POST /api/bookings              - Create booking
GET  /api/bookings/{id}         - Get booking details
PUT  /api/bookings/{id}/cancel  - Cancel booking
```

## Business Rules & Validation

### Authentication

- Email must be unique and valid format
- Password must be at least 8 characters with complexity requirements
- JWT tokens expire after configurable time period
- Role-based authorization for admin endpoints

### Hotels & Search

- Star rating must be between 1-5
- Search supports filtering by: price range, star rating, city, check-in/out dates
- Hotels must belong to valid cities
- Room capacity validation (adults + children <= room max capacity)

### Bookings

- Check-out date must be after check-in date
- Rooms must be available for selected dates
- Price calculation based on room type and number of nights
- Booking status workflow: Pending → Confirmed/Cancelled

### Admin Operations

- Only users with Admin role can perform CRUD operations on cities, hotels, and rooms
- Soft delete for maintaining data integrity
- Audit trail for all administrative changes

## Technical Requirements

### Security

- JWT authentication with role-based authorization
- Password hashing using bcrypt or similar
- Input validation and sanitization
- HTTPS enforcement
- CORS configuration for frontend integration

### Database

- Entity Framework Core with migrations
- Database seeding for initial data
- Proper indexing for search performance
- Foreign key constraints and data integrity

### Error Handling

- Global exception handling middleware
- Structured logging with Serilog
- Consistent error response format
- Validation error details

### Performance

- Async/await patterns throughout
- Efficient database queries with proper includes
- Pagination for large result sets
- Caching for frequently accessed data

### Testing

- Unit tests for business logic and handlers
- Integration tests for API endpoints
- Repository pattern for testability
- Mock external dependencies

## Response DTOs Structure

### Common Response Patterns

```csharp
// Standard API response wrapper
ApiResponse<T> {
    Data: T
    Success: bool
    Message: string
    Errors: string[]
}

// Paginated responses
PagedResponse<T> {
    Data: List<T>
    TotalCount: int
    PageNumber: int
    PageSize: int
    HasNextPage: bool
    HasPreviousPage: bool
}
```

### Key DTOs

- AuthResponseDto (includes token and user info)
- UserDto, CityDto, HotelDto, RoomDto, BookingDto
- SearchRequestDto, SearchResultDto
- CreateCityDto, CreateHotelDto, CreateRoomDto, CreateBookingDto

This specification provides the complete backend requirements for generating a production-ready .NET Core API following clean architecture principles.
