# Implementation Plan: Personalized Recently Visited

> **Feature**: Home Page - Personalized Recently Visited  
> **Status**: Planning  
> **Created**: 2025-01-08  
> **Planner**: System

## Background and Motivation

The current home page provides generic content (featured deals and popular destinations) for all users. Adding personalized "Recently Visited" functionality will:

1. **Enhance User Experience**: Show users content relevant to their browsing history
2. **Increase Engagement**: Help users quickly find hotels they've previously viewed
3. **Improve Conversion**: Reduce friction in rebooking or revisiting interesting hotels
4. **Personalization Foundation**: Establish user activity tracking infrastructure for future features

### Current Home Page Implementation

The existing `/api/v1/search/home-page` endpoint returns:

```json
{
  "FeaturedDeals": [...],
  "PopularDestinations": [...],
  "LastUpdated": "timestamp"
}
```

### Target Enhancement

Enhanced home page with personalized content:

```json
{
  "FeaturedDeals": [...],
  "PopularDestinations": [...],
  "RecentlyVisited": [...],  // NEW: User's recently viewed hotels
  "LastUpdated": "timestamp"
}
```

## Key Challenges and Analysis

### 1. **User Activity Tracking Infrastructure Missing**

- **Challenge**: No system currently tracks user interactions with hotels
- **Impact**: Need to build user activity tracking from scratch
- **Approach**: Implement lightweight activity logging with minimal performance impact

### 2. **Hotel Detail Views Not Implemented**

- **Challenge**: "Recently visited" typically means "recently viewed hotel details"
- **Current State**: Hotel search exists, but no hotel detail endpoint
- **Approach**: Implement hotel detail endpoint first, then add view tracking

### 3. **User Context in Home Page**

- **Challenge**: Current home page is anonymous/generic
- **Current State**: No user authentication required for home page
- **Approach**: Make recently visited optional (authenticated users only)

### 4. **Performance Considerations**

- **Challenge**: User activity tracking shouldn't impact search performance
- **Approach**: Asynchronous activity logging with background processing

### 5. **Data Privacy and Retention**

- **Challenge**: User activity data requires privacy considerations
- **Approach**: Configurable retention periods and user control

## High-level Task Breakdown

### ✅ **Prerequisites (Analysis Complete)**

- [x] **Current Architecture Analysis**: Understand existing home page and user systems
- [x] **Dependency Mapping**: Identify missing components (hotel details, activity tracking)
- [x] **Performance Impact Assessment**: Ensure minimal impact on existing functionality

### 📋 **Phase 1: Foundation - Hotel Detail Views (Required Dependency)**

- [ ] **Task 1.1: Hotel Detail Domain Query**

  - **Objective**: Create `GetHotelByIdQuery` and handler
  - **Success Criteria**: Can retrieve detailed hotel information by ID
  - **Dependencies**: None (domain entities exist)
  - **Effort**: 2 hours

- [ ] **Task 1.2: Hotel Detail API Endpoint**

  - **Objective**: Implement `GET /api/v1/hotels/{id}` endpoint
  - **Success Criteria**: Returns hotel details with related data (city, rooms, room types)
  - **Dependencies**: Task 1.1
  - **Effort**: 2 hours

- [ ] **Task 1.3: Hotel Detail Integration Tests**
  - **Objective**: Test hotel detail endpoint with authentication and authorization
  - **Success Criteria**: Comprehensive test coverage for hotel detail functionality
  - **Dependencies**: Task 1.2
  - **Effort**: 1 hour

### 📋 **Phase 2: User Activity Tracking Infrastructure**

- [ ] **Task 2.1: User Activity Domain Entity**

  - **Objective**: Create `UserActivity` entity for tracking user interactions
  - **Success Criteria**: Entity supports hotel views, searches, and other activity types
  - **Dependencies**: None
  - **Effort**: 2 hours
  - **Details**:
    ```csharp
    public class UserActivity : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public ActivityType Type { get; private set; } // HotelView, Search, etc.
        public Guid? TargetId { get; private set; } // Hotel ID, City ID, etc.
        public string? TargetType { get; private set; } // "Hotel", "City", etc.
        public string? Metadata { get; private set; } // JSON for additional context
        public DateTime ActivityDate { get; private set; }
    }
    ```

- [ ] **Task 2.2: User Activity Repository**

  - **Objective**: Implement repository for user activity persistence and queries
  - **Success Criteria**: Can save, retrieve, and query user activities efficiently
  - **Dependencies**: Task 2.1
  - **Effort**: 2 hours

- [ ] **Task 2.3: Activity Tracking Service**

  - **Objective**: Create service for asynchronous activity logging
  - **Success Criteria**: Non-blocking activity tracking with proper error handling
  - **Dependencies**: Task 2.2
  - **Effort**: 3 hours

- [ ] **Task 2.4: Database Migration for User Activity**
  - **Objective**: Create EF Core migration for user activity table
  - **Success Criteria**: Database schema supports user activity tracking with proper indexes
  - **Dependencies**: Task 2.1
  - **Effort**: 1 hour

### 📋 **Phase 3: Recently Visited Implementation**

- [ ] **Task 3.1: Recently Visited Query and Handler**

  - **Objective**: Create `GetRecentlyVisitedHotelsQuery` and handler
  - **Success Criteria**: Returns user's recently viewed hotels with configurable limit
  - **Dependencies**: Task 2.2
  - **Effort**: 2 hours

- [ ] **Task 3.2: Enhanced Home Page Query**

  - **Objective**: Update home page query to include recently visited data
  - **Success Criteria**: Home page returns recently visited hotels for authenticated users
  - **Dependencies**: Task 3.1
  - **Effort**: 2 hours

- [ ] **Task 3.3: Hotel View Tracking Integration**
  - **Objective**: Add activity tracking to hotel detail endpoint
  - **Success Criteria**: Viewing hotel details automatically logs user activity
  - **Dependencies**: Task 1.2, Task 2.3
  - **Effort**: 1 hour

### 📋 **Phase 4: Testing and Validation**

- [ ] **Task 4.1: User Activity Unit Tests**

  - **Objective**: Comprehensive unit tests for activity tracking components
  - **Success Criteria**: 90%+ test coverage for new functionality
  - **Dependencies**: Phase 2 complete
  - **Effort**: 3 hours

- [ ] **Task 4.2: Recently Visited Integration Tests**

  - **Objective**: End-to-end tests for recently visited functionality
  - **Success Criteria**: Tests cover authenticated and anonymous users, edge cases
  - **Dependencies**: Phase 3 complete
  - **Effort**: 2 hours

- [ ] **Task 4.3: Performance Testing**
  - **Objective**: Verify activity tracking doesn't impact application performance
  - **Success Criteria**: Response times remain within acceptable limits
  - **Dependencies**: All phases complete
  - **Effort**: 2 hours

### 📋 **Phase 5: Configuration and Optimization**

- [ ] **Task 5.1: Configuration Settings**

  - **Objective**: Add configurable settings for activity tracking and retention
  - **Success Criteria**: Admins can configure tracking behavior and data retention
  - **Dependencies**: Core functionality complete
  - **Effort**: 1 hour

- [ ] **Task 5.2: Privacy Controls**
  - **Objective**: Allow users to clear their activity history
  - **Success Criteria**: Users can manage their recently visited data
  - **Dependencies**: Task 5.1
  - **Effort**: 2 hours

## Technical Specifications

### User Activity Entity Design

```csharp
public class UserActivity : AggregateRoot
{
    public Guid UserId { get; private set; }
    public ActivityType Type { get; private set; }
    public Guid? TargetId { get; private set; }
    public string? TargetType { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime ActivityDate { get; private set; }

    // Methods for creating different activity types
    public static UserActivity CreateHotelView(Guid userId, Guid hotelId);
    public static UserActivity CreateSearch(Guid userId, string searchTerm, Guid? cityId = null);
}

public enum ActivityType
{
    HotelView = 1,
    Search = 2,
    BookingView = 3  // For future use
}
```

### Recently Visited Response Structure

```csharp
public class RecentlyVisitedDto
{
    public Guid HotelId { get; set; }
    public string HotelName { get; set; }
    public string CityName { get; set; }
    public string Country { get; set; }
    public int StarRating { get; set; }
    public string ImageUrl { get; set; }
    public DateTime LastVisited { get; set; }
    public decimal? MinPrice { get; set; }
}
```

### Enhanced Home Page Response

```csharp
public class HomePageDataDto
{
    public List<FeaturedDealDto> FeaturedDeals { get; set; }
    public List<CityDto> PopularDestinations { get; set; }
    public List<RecentlyVisitedDto>? RecentlyVisited { get; set; } // Null for anonymous users
    public DateTime LastUpdated { get; set; }
}
```

## Success Criteria

### Functional Requirements

1. **Hotel Detail Views**: Users can view detailed hotel information
2. **Activity Tracking**: System tracks user hotel views automatically
3. **Recently Visited Display**: Authenticated users see recently visited hotels on home page
4. **Anonymous Users**: Feature gracefully handles non-authenticated users
5. **Performance**: Activity tracking adds <50ms to response times
6. **Data Management**: Users can clear their activity history

### Non-Functional Requirements

1. **Performance**: No noticeable impact on existing search functionality
2. **Privacy**: User activity data is properly secured and manageable
3. **Scalability**: Activity tracking scales with user base growth
4. **Maintainability**: Clean separation of concerns following existing architecture
5. **Testability**: Comprehensive test coverage for new functionality

### Acceptance Criteria

- [ ] Authenticated users see "Recently Visited" section on home page
- [ ] Recently visited shows last 5 hotels viewed by the user
- [ ] Anonymous users don't see recently visited section
- [ ] Hotel detail page tracking works seamlessly
- [ ] No performance degradation in existing endpoints
- [ ] All new functionality has appropriate test coverage
- [ ] Configuration allows customization of tracking behavior

## Risk Assessment

### High Risk

- **Performance Impact**: Activity tracking could slow down hotel detail views
  - **Mitigation**: Asynchronous processing, performance monitoring

### Medium Risk

- **Database Growth**: User activity data could grow rapidly
  - **Mitigation**: Configurable retention periods, data archiving

### Low Risk

- **User Privacy Concerns**: Activity tracking raises privacy questions
  - **Mitigation**: Clear privacy controls, user data management

## Dependencies and Prerequisites

### External Dependencies

- None (all functionality built on existing platform)

### Internal Dependencies

1. **Hotel Detail Endpoint**: Must be implemented first
2. **JWT Authentication**: Already exists, but needed for user context
3. **Entity Framework Core**: Already configured for new entities

### Technology Considerations

- **Async Processing**: Use background tasks for activity logging
- **Caching**: Consider caching recently visited data
- **Indexes**: Proper database indexing for performance

## Lessons Learned (To Be Updated During Implementation)

_(This section will be filled by the Executor during implementation)_

## Executor's Feedback or Assistance Requests

_(This section will be filled by the Executor during implementation)_

---

## Implementation Notes

### Architecture Alignment

This implementation follows the existing Clean Architecture patterns:

- **Domain**: New entities and business rules
- **Application**: Commands, queries, and handlers using MediatR
- **Infrastructure**: Repository implementations and data persistence
- **API**: Controller endpoints following existing conventions

### Integration Points

- **Home Page**: Extends existing `/api/v1/search/home-page` endpoint
- **Authentication**: Integrates with existing JWT authentication
- **Database**: Uses existing EF Core context and migration system
- **Testing**: Follows existing test patterns and infrastructure

### Future Enhancements

This foundation enables:

1. **Advanced Personalization**: ML-based recommendations
2. **Cross-Device Sync**: Activity tracking across user sessions
3. **Analytics**: User behavior analysis and insights
4. **Marketing**: Targeted deal recommendations based on history
