using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;
using TravelBookingPlatform.Modules.Identity.Domain.Repositories;

namespace TravelBookingPlatform.Modules.IntegrationTests;

public class MockedIntegrationTestBase : IClassFixture<MockedIntegrationTestWebApplicationFactory>
{
    protected readonly MockedIntegrationTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    // Repository mocks available to all test classes
    protected readonly ICityRepository MockCityRepository;
    protected readonly IDealRepository MockDealRepository;
    protected readonly IHotelRepository MockHotelRepository;
    protected readonly IBookingRepository MockBookingRepository;
    protected readonly IRoomRepository MockRoomRepository;
    protected readonly IRoomTypeRepository MockRoomTypeRepository;
    protected readonly IUserRepository MockUserRepository;
    protected readonly IUnitOfWork MockUnitOfWork;

    public MockedIntegrationTestBase(MockedIntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        // Get the mocked repositories from the test factory
        MockCityRepository = factory.MockCityRepository;
        MockDealRepository = factory.MockDealRepository;
        MockHotelRepository = factory.MockHotelRepository;
        MockBookingRepository = factory.MockBookingRepository;
        MockRoomRepository = factory.MockRoomRepository;
        MockRoomTypeRepository = factory.MockRoomTypeRepository;
        MockUserRepository = factory.MockUserRepository;
        MockUnitOfWork = factory.MockUnitOfWork;
    }
}

public class MockedIntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    public bool UseTestAuthentication { get; set; } = true;

    // Repository mocks that can be accessed by test classes
    public ICityRepository MockCityRepository { get; private set; } = null!;
    public IDealRepository MockDealRepository { get; private set; } = null!;
    public IHotelRepository MockHotelRepository { get; private set; } = null!;
    public IBookingRepository MockBookingRepository { get; private set; } = null!;
    public IRoomRepository MockRoomRepository { get; private set; } = null!;
    public IRoomTypeRepository MockRoomTypeRepository { get; private set; } = null!;
    public IUserRepository MockUserRepository { get; private set; } = null!;
    public IUnitOfWork MockUnitOfWork { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddJsonFile("appsettings.MockedTesting.json", optional: true);
        });

        builder.ConfigureServices(services =>
        {
            // Create repository mocks
            CreateRepositoryMocks();

            // Remove real repository registrations and replace with mocks
            ReplaceRepositoriesWithMocks(services);

            if (UseTestAuthentication)
            {
                // Add test authentication for non-Identity tests
                services.AddAuthentication("Test")
                    .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
                services.AddAuthorization();
            }
        });

        builder.UseEnvironment("MockedTesting");
    }

    private void CreateRepositoryMocks()
    {
        MockCityRepository = Substitute.For<ICityRepository>();
        MockDealRepository = Substitute.For<IDealRepository>();
        MockHotelRepository = Substitute.For<IHotelRepository>();
        MockBookingRepository = Substitute.For<IBookingRepository>();
        MockRoomRepository = Substitute.For<IRoomRepository>();
        MockRoomTypeRepository = Substitute.For<IRoomTypeRepository>();
        MockUserRepository = Substitute.For<IUserRepository>();
        MockUnitOfWork = Substitute.For<IUnitOfWork>();

        // Setup default behaviors for UnitOfWork
        MockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    private void ReplaceRepositoriesWithMocks(IServiceCollection services)
    {
        // Remove real repository registrations
        var repositoryTypes = new[]
        {
            typeof(ICityRepository),
            typeof(IDealRepository),
            typeof(IHotelRepository),
            typeof(IBookingRepository),
            typeof(IRoomRepository),
            typeof(IRoomTypeRepository),
            typeof(IUserRepository),
            typeof(IUnitOfWork)
        };

        foreach (var repositoryType in repositoryTypes)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == repositoryType);
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }

        // Register mocked repositories
        services.AddSingleton(MockCityRepository);
        services.AddSingleton(MockDealRepository);
        services.AddSingleton(MockHotelRepository);
        services.AddSingleton(MockBookingRepository);
        services.AddSingleton(MockRoomRepository);
        services.AddSingleton(MockRoomTypeRepository);
        services.AddSingleton(MockUserRepository);
        services.AddSingleton(MockUnitOfWork);
    }
}

// TestAuthenticationSchemeOptions and TestAuthenticationHandler are already defined in IntegrationTestBase.cs