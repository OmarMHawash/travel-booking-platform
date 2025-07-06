using TravelBookingPlatform.Modules.IntegrationTests;

namespace TravelBookingPlatform.Identity.IntegrationTests;

public class IdentityIntegrationTestBase : IClassFixture<IdentityIntegrationTestWebApplicationFactory>
{
    protected readonly IdentityIntegrationTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    public IdentityIntegrationTestBase(IdentityIntegrationTestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}

public class IdentityIntegrationTestWebApplicationFactory : IntegrationTestWebApplicationFactory
{
    public IdentityIntegrationTestWebApplicationFactory()
    {
        // Disable test authentication for Identity tests - use real JWT
        UseTestAuthentication = false;
    }
}