using AutoFixture;
using NSubstitute;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Modules.IntegrationTests.Helpers;

public static class RepositoryMockFactory
{
    /// <summary>
    /// Sets up ICityRepository mock with common behaviors for successful operations
    /// </summary>
    public static void SetupCityRepositoryDefaults(ICityRepository mockRepository, IFixture fixture)
    {
        // Setup default returns for common queries
        mockRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<City>>(new List<City>()));

        // Setup exists checks to return false by default (no conflicts)
        mockRepository.NameExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
        mockRepository.PostCodeExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);

        // Setup search methods
        mockRepository.GetCitySuggestionsAsync(Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult<IReadOnlyList<City>>(new List<City>()));
        mockRepository.GetPopularDestinationsAsync(Arg.Any<int>()).Returns(Task.FromResult<IReadOnlyList<City>>(new List<City>()));
        mockRepository.SearchCitiesAsync(Arg.Any<string>()).Returns(Task.FromResult<IReadOnlyList<City>>(new List<City>()));

        // Setup GetByNameAsync to return null by default
        mockRepository.GetByNameAsync(Arg.Any<string>()).Returns((City?)null);
    }

    /// <summary>
    /// Sets up ICityRepository mock to return specific cities for GetAllAsync
    /// </summary>
    public static void SetupCityRepositoryWithCities(ICityRepository mockRepository, IList<City> cities)
    {
        mockRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<City>>(cities.ToList()));
    }

    /// <summary>
    /// Sets up ICityRepository mock to simulate a city name conflict
    /// </summary>
    public static void SetupCityRepositoryNameConflict(ICityRepository mockRepository, string conflictingName)
    {
        mockRepository.NameExistsAsync(conflictingName, Arg.Any<Guid?>()).Returns(true);
    }

    /// <summary>
    /// Sets up IDealRepository mock with common behaviors for successful operations
    /// </summary>
    public static void SetupDealRepositoryDefaults(IDealRepository mockRepository, IFixture fixture)
    {
        // Setup default returns for common queries
        mockRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Deal>>(new List<Deal>()));
        mockRepository.GetFeaturedDealsAsync(Arg.Any<int>()).Returns(Task.FromResult(new List<Deal>()));
        mockRepository.GetActiveDealsAsync().Returns(Task.FromResult(new List<Deal>()));
        mockRepository.GetDealsByHotelAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<Deal>()));

        // Setup single deal queries
        mockRepository.GetDealWithDetailsAsync(Arg.Any<Guid>()).Returns((Deal?)null);
        mockRepository.GetByHotelAndTitleAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns((Deal?)null);
        mockRepository.GetOverlappingDealAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((Deal?)null);

        // Setup boolean checks
        mockRepository.HasActiveDealForRoomTypeAsync(Arg.Any<Guid>()).Returns(false);
        mockRepository.GetActiveFeaturedDealsCountAsync(Arg.Any<Guid>()).Returns(0);
    }

    /// <summary>
    /// Sets up IDealRepository mock to return specific deals for GetAllAsync
    /// </summary>
    public static void SetupDealRepositoryWithDeals(IDealRepository mockRepository, IList<Deal> deals)
    {
        mockRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Deal>>(deals.ToList()));
    }

    /// <summary>
    /// Sets up IDealRepository mock to return a specific deal by ID
    /// </summary>
    public static void SetupDealRepositoryWithSpecificDeal(IDealRepository mockRepository, Deal deal)
    {
        mockRepository.GetByIdAsync(deal.Id).Returns(deal);
        mockRepository.GetDealWithDetailsAsync(deal.Id).Returns(deal);
    }

    /// <summary>
    /// Sets up IUserRepository mock with common behaviors for successful operations
    /// </summary>
    public static void SetupUserRepositoryDefaults(IUserRepository mockRepository, IFixture fixture)
    {
        // Setup default returns for common queries
        mockRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<User>>(new List<User>()));

        // Setup user lookup methods
        mockRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        mockRepository.GetByUsernameAsync(Arg.Any<string>()).Returns((User?)null);

        // Setup exists checks to return false by default (no conflicts)
        mockRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        mockRepository.UsernameExistsAsync(Arg.Any<string>()).Returns(false);
    }

    /// <summary>
    /// Sets up IUserRepository mock to return a specific user by email
    /// </summary>
    public static void SetupUserRepositoryWithUser(IUserRepository mockRepository, User user)
    {
        mockRepository.GetByIdAsync(user.Id).Returns(user);
        mockRepository.GetByEmailAsync(user.Email.Value).Returns(user);
        if (!string.IsNullOrEmpty(user.Username))
        {
            mockRepository.GetByUsernameAsync(user.Username).Returns(user);
        }
    }

    /// <summary>
    /// Sets up IUserRepository mock to simulate email/username conflicts
    /// </summary>
    public static void SetupUserRepositoryEmailConflict(IUserRepository mockRepository, string email)
    {
        mockRepository.EmailExistsAsync(email).Returns(true);
    }

    public static void SetupUserRepositoryUsernameConflict(IUserRepository mockRepository, string username)
    {
        mockRepository.UsernameExistsAsync(username).Returns(true);
    }

    /// <summary>
    /// Sets up IUnitOfWork mock with successful save behavior
    /// </summary>
    public static void SetupUnitOfWorkDefaults(IUnitOfWork mockUnitOfWork)
    {
        mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    /// <summary>
    /// Sets up IUnitOfWork mock to simulate save failure
    /// </summary>
    public static void SetupUnitOfWorkFailure(IUnitOfWork mockUnitOfWork)
    {
        mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(0);
    }

    /// <summary>
    /// Resets all mocks to their default behaviors
    /// </summary>
    public static void ResetAllMocks(
        ICityRepository mockCityRepository,
        IDealRepository mockDealRepository,
        IUserRepository mockUserRepository,
        IUnitOfWork mockUnitOfWork,
        IFixture fixture)
    {
        mockCityRepository.ClearReceivedCalls();
        mockDealRepository.ClearReceivedCalls();
        mockUserRepository.ClearReceivedCalls();
        mockUnitOfWork.ClearReceivedCalls();

        SetupCityRepositoryDefaults(mockCityRepository, fixture);
        SetupDealRepositoryDefaults(mockDealRepository, fixture);
        SetupUserRepositoryDefaults(mockUserRepository, fixture);
        SetupUnitOfWorkDefaults(mockUnitOfWork);
    }
}