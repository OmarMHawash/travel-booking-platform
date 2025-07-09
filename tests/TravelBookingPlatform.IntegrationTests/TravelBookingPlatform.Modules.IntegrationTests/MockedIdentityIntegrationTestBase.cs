using TravelBookingPlatform.Modules.IntegrationTests;

namespace TravelBookingPlatform.Identity.IntegrationTests;

public class MockedIdentityIntegrationTestBase : IClassFixture<MockedIdentityIntegrationTestWebApplicationFactory>
{
    protected readonly MockedIdentityIntegrationTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    public MockedIdentityIntegrationTestBase(MockedIdentityIntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}

public class MockedIdentityIntegrationTestWebApplicationFactory : MockedIntegrationTestWebApplicationFactory
{
    public MockedIdentityIntegrationTestWebApplicationFactory()
    {
        // Disable test authentication for Identity tests - use real JWT for auth testing
        UseTestAuthentication = false;
    }
}