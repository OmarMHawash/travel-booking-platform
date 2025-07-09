### Testing Guidelines

- Unit tests for business logic and handlers
- Integration tests for API endpoints
- Repository pattern for testability
- Mock external dependencies

high-quality unit and integration tests for .NET applications following the exact patterns and conventions established in the TravelBookingPlatform project.

## Testing Stack and Packages

When generating test code, always use these packages with the specific versions:

- **xUnit** (2.9.2) - Primary testing framework with `<Using Include="Xunit" />` global usings
- **FluentAssertions** (8.4.0) - For readable and expressive assertions
- **NSubstitute** (5.3.0) - For mocking dependencies with clean syntax
- **AutoFixture** (4.18.1) - For generating test data
- **Microsoft.AspNetCore.Mvc.Testing** (8.0.0) - For integration tests
- **Microsoft.NET.Test.Sdk** (17.12.0) - Test SDK
- **coverlet.collector** (6.0.2) - Code coverage

## Code Generation Instructions

### 1. Unit Test Generation Template

When generating unit tests, follow this EXACT structure:

```csharp
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using [relevant domain namespaces];
using [relevant application namespaces];

namespace [TestNamespace];

public class [ClassUnderTest]Tests
{
    private readonly [IDependency] _dependency;
    private readonly [ClassUnderTest] _classUnderTest;
    private readonly Fixture _fixture;

    public [ClassUnderTest]Tests()
    {
        _fixture = new Fixture();
        _dependency = Substitute.For<[IDependency]>();

        // AutoMapper manual configuration for testing
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceType, DestinationType>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _classUnderTest = new [ClassUnderTest](_dependency, mapper);
    }

    [Fact]
    public async Task Handle_Should[ExpectedBehavior]_When[Condition]()
    {
        // Arrange
        var input = _fixture.Build<InputType>()
            .With(x => x.Property, "SpecificValue")
            .Create();

        // Act
        var result = await _classUnderTest.Method(input, CancellationToken.None);

        // Assert
        await _dependency.Received(1).Method(Arg.Is<EntityType>(e =>
            e.Property1 == input.Property1 &&
            e.Property2 == input.Property2));

        result.Should().NotBeNull();
        result.Property.Should().Be(input.Property);
    }
}
```

### 2. Validator Test Generation Template

When generating validator tests, follow this EXACT structure:

```csharp
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using [relevant namespaces];

namespace [TestNamespace];

public class [ValidatorName]Tests
{
    private readonly [IDependency] _mockDependency;
    private readonly [ValidatorName] _validator;
    private readonly Fixture _fixture = new();

    public [ValidatorName]Tests()
    {
        _mockDependency = Substitute.For<[IDependency]>();

        // Setup default behavior - configure mocks for happy path
        _mockDependency.ExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>())
            .Returns(false);

        _validator = new [ValidatorName](_mockDependency);
    }

    [Fact]
    public async Task Validator_ShouldHaveNoError_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<CommandType>()
            .With(x => x.Property, "ValidValue")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validator_ShouldHaveError_WhenPropertyIsInvalid(string? invalidValue)
    {
        // Arrange
        var command = _fixture.Build<CommandType>()
            .With(x => x.Property, invalidValue!)
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Property");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenPropertyIsTooLong()
    {
        // Arrange
        var command = _fixture.Build<CommandType>()
            .With(x => x.Property, new string('a', 101)) // Exceeds limit
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Property");
    }

    [Fact]
    public async Task Validator_ShouldHaveError_WhenPropertyAlreadyExists()
    {
        // Arrange
        _mockDependency.ExistsAsync("ExistingValue", Arg.Any<Guid?>())
            .Returns(true);

        var command = _fixture.Build<CommandType>()
            .With(x => x.Property, "ExistingValue")
            .Create();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Property" && e.ErrorMessage.Contains("already exists"));
    }
}
```

### 3. Integration Test Generation Template

When generating integration tests, follow this EXACT structure:

```csharp
using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using [relevant DTO namespaces];

namespace [TestNamespace];

public class [FeatureName]IntegrationTests : IntegrationTestBase
{
    private readonly Fixture _fixture;

    public [FeatureName]IntegrationTests(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Get[Resource]_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/[resource]");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create[Resource]_ShouldReturnCreatedStatusCode_WhenValidDataProvided()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 chars for uniqueness
        var createDto = _fixture.Build<Create[Resource]Dto>()
            .With(x => x.Name, $"Test Name {uniqueId}")
            .With(x => x.Property, $"Test Value {uniqueId}")
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/[resource]", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdResource = await response.Content.ReadFromJsonAsync<[Resource]Dto>();
        createdResource.Should().NotBeNull();
        createdResource.Name.Should().Be(createDto.Name);
        createdResource.Property.Should().Be(createDto.Property);
    }

    [Fact]
    public async Task Create[Resource]_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        var invalidDto = _fixture.Build<Create[Resource]Dto>()
            .With(x => x.Name, "") // Invalid empty name
            .Create();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/[resource]", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Key Testing Patterns and Best Practices

### 1. Test Naming Convention

Use descriptive method names following this EXACT pattern:

- **Command Handlers:** `Handle_Should[ExpectedBehavior]_When[Condition]`
- **Query Handlers:** `Handle_Should[ExpectedBehavior]_When[Condition]`
- **Validators:** `Validator_ShouldHaveError_When[Condition]` or `Validator_ShouldHaveNoError_When[Condition]`
- **Integration Tests:** `[HttpMethod][Resource]_Should[ExpectedBehavior]_When[Condition]`

**Examples from actual codebase:**

- `Handle_ShouldCreateCityAndSaveChanges_WhenCommandIsValid`
- `Handle_ShouldReturnListOfCityDtos_WhenCitiesExist`
- `Validator_ShouldHaveError_WhenNameIsInvalid`
- `CreateCity_ShouldReturnCreatedStatusCode_WhenValidCityProvided`

### 2. AutoFixture Usage Patterns

**For Simple Random Data:**

```csharp
var command = _fixture.Create<CreateCommand>();
var query = _fixture.Create<GetAllQuery>();
```

**For Specific Property Values:**

```csharp
var command = _fixture.Build<CreateCommand>()
    .With(x => x.Name, "Paris")
    .With(x => x.Country, "France")
    .Create();
```

**For Entities with Constructor Parameters:**

```csharp
var cities = new List<City>
{
    new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
    new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>())
};
```

**For Integration Tests with Unique Values:**

```csharp
var uniqueId = Guid.NewGuid().ToString("N")[..8];
var createDto = _fixture.Build<CreateCityDto>()
    .With(x => x.Name, $"Test City {uniqueId}")
    .With(x => x.Country, $"Test Country {uniqueId}")
    .Create();
```

**For Validation-Aware Data Generation:**

```csharp
var randomName = _fixture.Create<string>();
var command = _fixture.Build<CreateCommand>()
    .With(x => x.Name, randomName.Length > 100 ? randomName[..50] : randomName)
    .Create();
```

### 3. FluentAssertions Patterns

**Basic Entity Assertions:**

```csharp
result.Should().NotBeNull();
result.Name.Should().Be(expected.Name);
result.Country.Should().Be(expected.Country);
```

**Collection Assertions:**

```csharp
result.Should().HaveCount(2);
result.Should().BeEmpty();
result.Should().AllSatisfy(dto =>
{
    dto.Name.Should().NotBeNullOrEmpty();
    dto.Country.Should().NotBeNullOrEmpty();
});
```

**Validation Result Assertions:**

```csharp
result.IsValid.Should().BeTrue();
result.Errors.Should().BeEmpty();
result.Errors.Should().Contain(e => e.PropertyName == "PropertyName");
result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("already exists"));
```

**HTTP Response Assertions:**

```csharp
response.StatusCode.Should().Be(HttpStatusCode.Created);
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
```

### 4. NSubstitute Mocking Patterns

**Setup Repository Returns:**

```csharp
_repository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Entity>>(entities));
_repository.ExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
```

**Verify Method Calls with Complex Arguments:**

```csharp
await _repository.Received(1).AddAsync(Arg.Is<City>(c =>
    c.Name == command.Name &&
    c.Country == command.Country &&
    c.PostCode == command.PostCode));
```

**Verify Simple Method Calls:**

```csharp
await _repository.Received(1).GetAllAsync();
await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
```

### 5. AutoMapper Configuration Pattern

For all tests requiring mapping, use this EXACT pattern:

```csharp
var mappingConfig = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<SourceType, DestinationType>();
    cfg.CreateMap<AnotherSource, AnotherDestination>();
}, NullLoggerFactory.Instance);
var mapper = mappingConfig.CreateMapper();
```

### 6. Test Categories and Required Scenarios

**Command Handler Tests Must Include:**

1. **Happy Path:** Valid command creates entity and saves changes
2. **AutoFixture Variation:** Test with randomly generated data
3. **Verification:** Repository and UnitOfWork method calls
4. **Return Value:** Correct DTO mapping

**Query Handler Tests Must Include:**

1. **Happy Path:** Returns correct DTOs when entities exist
2. **AutoFixture Variation:** Test with random data generation
3. **Empty Case:** Returns empty list when no entities exist
4. **Collection Validation:** All DTOs have required properties

**Validator Tests Must Include:**

1. **Valid Case:** No errors for valid input
2. **AutoFixture Valid:** No errors with random valid data
3. **Null/Empty:** Theory tests for null, empty, whitespace
4. **Length Validation:** Too long values
5. **Uniqueness:** Already exists scenarios

**Integration Tests Must Include:**

1. **GET Success:** Returns OK status code
2. **POST Success:** Returns Created with valid data
3. **POST Failure:** Returns BadRequest with invalid data
4. **Response Validation:** Correct DTO structure

### 7. Integration Test Base Class Pattern

Use the established `IntegrationTestBase` class:

```csharp
public class [Feature]IntegrationTests : IntegrationTestBase
{
    private readonly Fixture _fixture;

    public [Feature]IntegrationTests(IntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    // Test methods use Client property from base class
}
```

### 8. Mocked Integration Testing Patterns

**Overview:**
Mocked integration tests provide faster execution by replacing database interactions with repository mocks while still testing the full API pipeline including controllers, validation, and error handling.

#### 8.1. MockedIntegrationTestBase Pattern

Use `MockedIntegrationTestBase` for repository-mocked integration tests:

```csharp
using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using [relevant DTO namespaces];

namespace [TestNamespace];

public class Mocked[Feature]IntegrationTests : MockedIntegrationTestBase
{
    private readonly Fixture _fixture;

    public Mocked[Feature]IntegrationTests(MockedIntegrationTestWebApplicationFactory factory) : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetAll[Resource]_ShouldReturnOk_WhenRepositoryHasData()
    {
        // Arrange
        var entities = TestDataBuilders.CreateCities(2);
        RepositoryMocks.CityRepository.GetAllAsync()
            .Returns(Task.FromResult<IReadOnlyList<City>>(entities));

        // Act
        var response = await Client.GetAsync("/api/v1/cities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        result.Should().HaveCount(2);
    }
}
```

#### 8.2. RepositoryMockFactory Usage

**Centralized Mock Management:**

```csharp
// Access pre-configured mocks through RepositoryMocks property
RepositoryMocks.CityRepository.GetAllAsync().Returns(cities);
RepositoryMocks.UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
RepositoryMocks.UserRepository.ExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);
```

**Mock State Reset Pattern:**

```csharp
[Fact]
public async Task Test_Method()
{
    // CRITICAL: Clear previous mock setups to avoid state pollution
    RepositoryMocks.CityRepository.ClearReceivedCalls();

    // Setup fresh mock behavior for this test
    RepositoryMocks.CityRepository.ExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>())
        .Returns(false);

    // Test implementation...
}
```

#### 8.3. TestDataBuilders Enhanced Patterns

**Validation-Aware Data Generation:**

```csharp
// For fields with strict validation rules (letters, spaces, hyphens, apostrophes, periods only)
public static string CreateValidName()
{
    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz '.'-";
    return new string(Enumerable.Repeat(chars, 10)
        .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
}

// For unique test data without numbers (to pass validation)
public static CreateCityDto CreateValidCityDto()
{
    var timestamp = DateTime.Now.Ticks.ToString()[^6..]; // Last 6 digits
    return new CreateCityDto
    {
        Name = $"TestCity{timestamp.Select(c => (char)('A' + (c - '0'))).ToArray()}",
        Country = $"TestCountry{timestamp.Select(c => (char)('A' + (c - '0'))).ToArray()}",
        PostCode = CreateValidPostCode()
    };
}
```

**Entity Collections with AutoFixture:**

```csharp
public static List<City> CreateCities(int count)
{
    var fixture = new Fixture();
    var cities = new List<City>();

    for (int i = 0; i < count; i++)
    {
        var validName = CreateValidName();
        var validCountry = CreateValidName();
        var validPostCode = CreateValidPostCode();

        cities.Add(new City(validName, validCountry, validPostCode));
    }

    return cities;
}
```

#### 8.4. Mock State Management Best Practices

**Problem: Mock State Pollution**

- Singleton mocks share state between tests
- Previous test setups can interfere with current test

**Solution: Explicit Mock Reset and Setup**

```csharp
[Fact]
public async Task CreateCity_ShouldReturnCreated_WhenValidData()
{
    // 1. Clear any previous mock setups
    RepositoryMocks.CityRepository.ClearReceivedCalls();

    // 2. Setup fresh behavior with Arg.Any<> to override previous setups
    RepositoryMocks.CityRepository.ExistsAsync(Arg.Any<string>(), Arg.Any<Guid?>())
        .Returns(false);

    // 3. Use unique test data for each test
    var createDto = TestDataBuilders.CreateValidCityDto();

    // 4. Act & Assert
    var response = await Client.PostAsJsonAsync("/api/v1/cities", createDto);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}

[Fact]
public async Task CreateCity_ShouldReturnBadRequest_WhenNameAlreadyExists()
{
    // 1. Clear previous state
    RepositoryMocks.CityRepository.ClearReceivedCalls();

    // 2. Setup specific behavior for this test
    RepositoryMocks.CityRepository.ExistsAsync("DuplicateName", Arg.Any<Guid?>())
        .Returns(true);

    // 3. Use data that matches the mock setup
    var createDto = new CreateCityDto
    {
        Name = "DuplicateName",
        Country = TestDataBuilders.CreateValidName(),
        PostCode = TestDataBuilders.CreateValidPostCode()
    };

    // 4. Act & Assert
    var response = await Client.PostAsJsonAsync("/api/v1/cities", createDto);
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

#### 8.5. Exception Handling Patterns

**Repository Exception to HTTP Status Mapping:**

```csharp
[Fact]
public async Task CreateCity_ShouldReturnInternalServerError_WhenRepositoryFails()
{
    // Use System.Exception for 500 status code
    // InvalidOperationException maps to 400 BadRequest
    RepositoryMocks.CityRepository.AddAsync(Arg.Any<City>())
        .Throws(new System.Exception("Database connection failed"));

    var createDto = TestDataBuilders.CreateValidCityDto();

    var response = await Client.PostAsJsonAsync("/api/v1/cities", createDto);

    response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
}

[Fact]
public async Task CreateCity_ShouldReturnBadRequest_WhenBusinessRuleViolated()
{
    // Business validation exceptions map to 400 BadRequest
    RepositoryMocks.CityRepository.AddAsync(Arg.Any<City>())
        .Throws(new BusinessValidationException("Business rule violated"));

    var createDto = TestDataBuilders.CreateValidCityDto();

    var response = await Client.PostAsJsonAsync("/api/v1/cities", createDto);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

#### 8.6. Data Generation Best Practices

**Avoid AutoFixture GUIDs and Timestamps in Validation-Sensitive Fields:**

```csharp
// ❌ BAD: Contains numbers and special characters that fail validation
var badDto = _fixture.Build<CreateCityDto>()
    .With(x => x.Name, Guid.NewGuid().ToString()) // Contains hyphens and numbers
    .Create();

// ✅ GOOD: Validation-aware data generation
var goodDto = _fixture.Build<CreateCityDto>()
    .With(x => x.Name, TestDataBuilders.CreateValidName())
    .With(x => x.Country, TestDataBuilders.CreateValidName())
    .With(x => x.PostCode, TestDataBuilders.CreateValidPostCode())
    .Create();
```

**Unique Data Generation for Parallel Tests:**

```csharp
public static CreateCityDto CreateUniqueCityDto()
{
    var uniqueSuffix = DateTime.UtcNow.Ticks.ToString()[^6..];
    var alphabeticSuffix = string.Join("", uniqueSuffix.Select(c => (char)('A' + (c - '0'))));

    return new CreateCityDto
    {
        Name = $"City{alphabeticSuffix}",
        Country = $"Country{alphabeticSuffix}",
        PostCode = $"PC{alphabeticSuffix}"
    };
}
```

#### 8.7. Required Test Scenarios for Mocked Integration Tests

**Must Include All These Scenarios:**

1. **GET Success with Data:**

   ```csharp
   // Repository returns data → 200 OK with DTOs
   ```

2. **GET Success with Empty Result:**

   ```csharp
   // Repository returns empty list → 200 OK with empty array
   ```

3. **POST Success:**

   ```csharp
   // Valid data + repository success → 201 Created with location header
   ```

4. **POST Validation Failure:**

   ```csharp
   // Invalid data → 400 BadRequest with validation errors
   ```

5. **POST Business Rule Violation:**

   ```csharp
   // Valid data but business rule fails → 400 BadRequest or 409 Conflict
   ```

6. **Repository Exception Handling:**

   ```csharp
   // Repository throws exception → 500 InternalServerError
   ```

7. **AutoFixture Data Handling:**

   ```csharp
   // Test with randomly generated but valid data
   ```

8. **Unique Constraint Violations:**
   ```csharp
   // Duplicate names, postcodes, etc. → 400 BadRequest
   ```

#### 8.8. Performance Benefits

**Mocked Integration Tests vs Database Integration Tests:**

- **Execution Time:** ~1.4s for 8 tests vs ~15s+ with database
- **Isolation:** No database state cleanup required
- **Reliability:** No database connection dependencies
- **Parallelization:** Tests can run in parallel without conflicts

**When to Use Each:**

- **Mocked Integration Tests:** Fast feedback, repository contract testing, business logic validation
- **Database Integration Tests:** End-to-end validation, database constraint testing, complex query testing

## Project Structure

Follow this EXACT file organization:

```
tests/
├── TravelBookingPlatform.UnitTests/
│   └── TravelBookingPlatform.[Module].UnitTests/
│       ├── [Feature]/
│       │   ├── Commands/
│       │   │   ├── [CommandName]HandlerTests.cs
│       │   │   └── [CommandName]ValidatorTests.cs
│       │   └── Queries/
│       │       └── [QueryName]HandlerTests.cs
│       └── TravelBookingPlatform.[Module].UnitTests.csproj
└── TravelBookingPlatform.IntegrationTests/
    ├── TravelBookingPlatform.Host.IntegrationTests/
    │   ├── SystemControllerIntegrationTests.cs
    │   ├── IntegrationTestBase.cs
    │   ├── IntegrationTestWebApplicationFactory.cs
    │   └── TravelBookingPlatform.Host.IntegrationTests.csproj
    └── TravelBookingPlatform.Modules.IntegrationTests/
        ├── [Feature]IntegrationTests.cs              <-- Database integration tests
        ├── Mocked[Feature]IntegrationTests.cs        <-- Repository-mocked integration tests
        ├── IntegrationTestBase.cs                    <-- Base for database tests
        ├── IntegrationTestWebApplicationFactory.cs   <-- Factory for database tests
        ├── MockedIntegrationTestBase.cs              <-- Base for mocked tests
        ├── MockedIntegrationTestWebApplicationFactory.cs <-- Factory for mocked tests
        ├── TestInfrastructure/
        │   ├── RepositoryMockFactory.cs              <-- Centralized mock creation
        │   └── TestDataBuilders.cs                   <-- Test data generation utilities
        └── TravelBookingPlatform.Modules.IntegrationTests.csproj
```

### File Descriptions

#### Database Integration Tests

- **`[Feature]IntegrationTests.cs`**: Full end-to-end tests with real database
- **`IntegrationTestBase.cs`**: Base class for database-dependent tests
- **`IntegrationTestWebApplicationFactory.cs`**: Test factory with real database setup

#### Mocked Integration Tests

- **`Mocked[Feature]IntegrationTests.cs`**: Fast tests with repository mocks
- **`MockedIntegrationTestBase.cs`**: Base class for repository-mocked tests
- **`MockedIntegrationTestWebApplicationFactory.cs`**: Test factory with mocked repositories

#### Test Infrastructure

- **`RepositoryMockFactory.cs`**: Centralized creation and management of repository mocks
- **`TestDataBuilders.cs`**: Utilities for generating test data with validation awareness

## LLM Generation Prompt Template

Use this template when requesting test generation:

---

**PROMPT:**

Generate comprehensive unit/integration tests for the following .NET class/controller following the TravelBookingPlatform project conventions:

1. **Use EXACT testing stack:** xUnit (2.9.2), FluentAssertions (8.4.0), NSubstitute (5.3.0), AutoFixture (4.18.1)
2. **Follow EXACT naming:** `Handle_Should[ExpectedBehavior]_When[Condition]` pattern
3. **Include ALL test categories:**
   - Happy path with explicit values
   - AutoFixture random data variation
   - Edge cases (empty, null, too long)
   - Error scenarios
4. **Use constructor pattern:**
   - Initialize `_fixture = new Fixture()`
   - Create all mocks with `Substitute.For<Interface>()`
   - Configure AutoMapper manually with `NullLoggerFactory.Instance`
5. **Use AutoFixture Build pattern:**
   - `.Build<Type>().With(x => x.Property, value).Create()`
   - Generate unique IDs for integration tests
6. **Follow FluentAssertions patterns:**
   - Chain assertions: `result.Property.Should().Be(expected)`
   - Collection assertions: `Should().AllSatisfy()`
7. **Use NSubstitute verification:**
   - `Received(1)` for call verification
   - `Arg.Is<Type>()` for complex argument matching
8. **Integration test patterns:**
   - **Database tests:** Inherit from `IntegrationTestBase` for end-to-end validation
   - **Mocked tests:** Inherit from `MockedIntegrationTestBase` for fast repository-mocked tests
   - Use unique identifiers to avoid test collisions
   - Test both success and failure HTTP scenarios
   - Apply mock state reset patterns: `RepositoryMocks.Repository.ClearReceivedCalls()`
   - Use validation-aware test data generation
9. **Reference the new test projects:**
   - at the end, you need to reference the new test projects in the solution file

### Proposed Architecture:

```
TravelBookingPlatform.sln
├── .editorconfig                                 <-- Shared Code Formatting Configuration
├── Directory.Build.props                         <-- Project-wide SDKs, common properties, analyzer rules, syntax/patterns check
├── src/
│   ├── TravelBookingPlatform.Core/               <-- The SHARED KERNEL (Most Stable, Least Dependencies)
│   │   ├── TravelBookingPlatform.Core.Domain/    <-- Core domain concepts (e.g., base Entity/AggregateRoot, ValueObject, IUnitOfWork interface, common Domain Event base classes)
│   │   └── TravelBookingPlatform.Core.Application/ <-- Base MediatR command/query interfaces, common DTOs, common abstractions for external services (if truly generic)
│   │
│   ├── TravelBookingPlatform.Modules.Identity/
│   │   ├── TravelBookingPlatform.Modules.Identity.Domain/      <-- Entities (User, Role), Value Objects, Domain Events specific to Identity
│   │   ├── TravelBookingPlatform.Modules.Identity.Application/ <-- Commands, Queries, Handlers (MediatR), DTOs, Application Services, Fluent Validations
│   │   ├── TravelBookingPlatform.Modules.Identity.Infrastructure/ <-- EF Core DbContext, Migrations, Repository implementations, specific Identity service implementations (e.g., JWT token service)
│   │   └── TravelBookingPlatform.Modules.Identity.Api/          <-- Controllers, API specific models, module-specific DI registrations (exposed as extension methods for Host)
│   │
│   ├── TravelBookingPlatform.Modules.Hotels/
│   │   ├── TravelBookingPlatform.Modules.Hotels.Domain/        <-- Entities (Hotel, Room, City, Booking), Value Objects, Domain Events specific to Hotels and Booking
│   │   ├── TravelBookingPlatform.Modules.Hotels.Application/   <-- Commands, Queries, Handlers (MediatR), DTOs, Application Services, Fluent Validations, AutoMapper profiles
│   │   ├── TravelBookingPlatform.Modules.Hotels.Infrastructure/ <-- EF Core DbContext, Migrations, Repository implementations, external API clients (e.g., Map service integration)
│   │   └── TravelBookingPlatform.Modules.Hotels.Api/            <-- Controllers, API specific models, module-specific DI registrations
│   │
│   ├── TravelBookingPlatform.Host/                 <-- THE MONOLITHIC ENTRY POINT
│   │   │                                           References all `TravelBookingPlatform.Modules.*.Api` projects.
│   │   │                                           Contains `Program.cs` for bootstrapping.
│   │   │                                           Configures Global DI, Serilog, Swagger/Swashbuckle (API Docs),
│   │   │                                           built in HTTP APIs ProblemDetails, API Versioning, Authentication/Authorization Middleware,
│   │   │                                           CORS, and any other global HTTP pipeline setup.
│   │   │
│   │   └── appsettings.json                        <-- Global configuration
│   │
│   └── TravelBookingPlatform.SharedInfrastructure/ <-- COMMON INFRASTRUCTURE & Cross-Cutting Concerns
│       │                                           References `TravelBookingPlatform.Core`.
│       │                                           Contains implementations of `IUnitOfWork`, generic repositories,
│       │                                           centralized EF Core ApplicationDbContext that includes all module entities,
│       │                                           database migrations, common utility extensions (e.g., for DateTime),
│       │                                           global authentication/authorization handlers (if not Identity-specific),
│       │                                           Serilog sinks configuration, Health Checks, Cache implementations (e.g., Redis setup),
│       │                                           message bus implementation (e.g., in-memory for monolith, or RabbitMQ/Kafka client for future).
│
tests/
    ├── TravelBookingPlatform.UnitTests/
    │   ├── TravelBookingPlatform.Core.UnitTests/
    │   ├── TravelBookingPlatform.Identity.UnitTests/
    │   └── TravelBookingPlatform.Hotels.UnitTests/
    │
    └── TravelBookingPlatform.IntegrationTests/
        ├── TravelBookingPlatform.Host.IntegrationTests/        <-- Full API pipeline, cross-module scenarios
        └── TravelBookingPlatform.Modules.IntegrationTests/     <-- Module-to-module interaction tests

```

### Justification and Integration of Requirements:

1.  **Modular Monolith & Microservices Readiness:**

- **Clear Bounded Contexts:** Each `TravelBookingPlatform.Modules.<ModuleName>` directory represents a distinct domain area.
- **Internal Layering:** The `Domain`, `Application`, `Infrastructure`, `Api` breakdown within each module promotes SoC and Clean Architecture principles.
  - `Domain`: Pure business logic, entities, value objects. No external dependencies.
  - `Application`: Orchestrates domain, handles use cases (CQRS via MediatR), defines DTOs. Depends on `Domain`.
  - `Infrastructure`: Concrete implementations of repository interfaces (from Domain), external service integrations (EF Core, Payment Gateways, Email), specific identity providers. Depends on `Application` and `Domain`.
  - `Api`: RESTful API controllers, Presentation Layer, Module-specific DI setup. Depends on `Application`.
- **Integrated Booking:** Booking functionality is implemented within the Hotels module since hotel bookings are domain-specific to the hotels context and share many common concerns (rooms, availability, pricing).
- **Future Split:** When moving to microservices, each `TravelBookingPlatform.Modules.<ModuleName>` folder (specifically its `Api` project and its dependencies: `Application`, `Domain`, `Infrastructure`) can be easily extracted into a separate repository and deployable service. Shared `Core` and `SharedInfrastructure` components might become NuGet packages or their functionalities re-evaluated for distributed contexts.

2.  **Technologies & Practices:**

- **.NET 8 (LTS), C# 12:** Standard setup, latest language features.
- **JWT, Identity:** Fully owned by `TravelBookingPlatform.Modules.Identity`.
- **EF Core, MS SQL:** Uses a centralized `ApplicationDbContext` in `SharedInfrastructure` that includes entities from all modules. This approach works well for the monolith and can be refactored for microservices later.
- **Swashbuckle (Swagger), API Versioning:** Configured in `TravelBookingPlatform.Host`. Each module's `Api` project exposes its controllers.
- **Hosting, Options, Logger (Microsoft.Extensions.Logging), Serilog:** `TravelBookingPlatform.Host` sets up the primary logging, `SharedInfrastructure` provides common Serilog sinks and configurations. `ILogger<T>` can be injected anywhere.
- **DI:** `TravelBookingPlatform.Host` is the composition root. Each module's `.Api` project contains an extension method (e.g., `AddIdentityModule(this IServiceCollection services)`) to register its services into the main container, promoting modularity.
- **Fluent Validation:** Resides in each module's `Application` layer, validating commands/queries.
- **Fluent Assertion:** Used in the `tests/` projects.
- **HTTP APIs ProblemDetails:** Configured in `TravelBookingPlatform.Host` for consistent API error responses (using the built-in dotnet framework).
- **xUnit:** The chosen testing framework across all test projects.
- **CQRS with MediatR:** `Core.Application` defines `IRequest`/`IRequestHandler`. Each module's `Application` layer implements commands, queries, and their handlers. This is key for internal communication and logic separation.
- **AutoMapper, DTOs:** Defined within each module's `Application` layer for mapping between domain entities and DTOs.
- **Shared Code Formatting Configuration (`.editorconfig`):** At the solution root for consistent code style.
- **Project Syntax/Patterns Check Properties (`Directory.Build.props`):** At the solution root for common SDK versions, static analysis rules (e.g., Roslyn analyzers), and C# language features.

3.  **Project Document (Features & Non-Functional):**

- **Admin Page for Easy Management:** The `Hotels` module contains the logic and API for managing Cities, Hotels, Rooms, and Bookings. This is a clear responsibility.
- **Booking Management:** Integrated within the Hotels module, including booking entities, repositories, and business logic for hotel reservations.
- **Payment Integration:** Can be implemented within `TravelBookingPlatform.Modules.Hotels.Infrastructure` or as a separate service integration.
- **Error Handling & Logging:** Robustly handled by `ProblemDetails` in `Host` and Serilog configured via `Host` and `SharedInfrastructure`.
- **Testing:** Comprehensive `UnitTests` for internal logic and `IntegrationTests` for cross-module interactions and end-to-end API flows.
- **Codebase Documentation, API Reference:** Swagger/Swashbuckle for API reference. In-code comments and project-level documentation (e.g., in a `docs/` folder).

4.  **Unit testing packages:**

    Microsoft.NET.Test.Sdk (usually included by default)
    xunit
    coverlet.collector
    FluentAssertions (for more readable assertions)
    NSubstitute
    AutoFixture (to generate test data)

### Current Implementation Status:

**Implemented Modules:**

- ✅ TravelBookingPlatform.Core (Domain, Application)
- ✅ TravelBookingPlatform.Modules.Identity (Domain, Application, Infrastructure, Api)
- ✅ TravelBookingPlatform.Modules.Hotels (Domain, Application, Infrastructure, Api) - includes Booking functionality
- ✅ TravelBookingPlatform.SharedInfrastructure (includes centralized persistence)
- ✅ TravelBookingPlatform.Host

**Test Coverage:**

- ✅ Unit Tests for Core, Identity, and Hotels modules
- ✅ Integration Tests for Host and cross-module scenarios
- ✅ Mocked Integration Test Infrastructure (MockedIntegrationTestBase, RepositoryMockFactory, TestDataBuilders)
- ✅ MockedCitiesIntegrationTests (8 tests, 1.4s execution time)
- ❌ MockedDealsIntegrationTests (not created yet)
- ❌ MockedUserIntegrationTests (deleted due to compilation errors, needs recreation)
- ❌ MockedAuthIntegrationTests (not created yet)

## Implementation Status and Lessons Learned

### ✅ Completed Infrastructure

**MockedIntegrationTestBase Pattern:**

- ✅ Base class with DI container and repository mocking setup
- ✅ Clean separation from database integration tests
- ✅ Centralized mock management through RepositoryMockFactory

**RepositoryMockFactory:**

- ✅ Centralized mock creation for IUserRepository, ICityRepository, IUnitOfWork
- ✅ Singleton pattern for consistent mock instances across tests
- ✅ Pre-configured with common default behaviors

**TestDataBuilders:**

- ✅ Enhanced with validation-aware data generation
- ✅ CreateValidName() for strict validation rules (letters, spaces, hyphens, apostrophes, periods only)
- ✅ CreateValidCityDto() with unique alphabetic identifiers
- ✅ CreateCities() for entity collections
- ✅ AutoFixture integration with custom constraints

### ✅ Proven Patterns - MockedCitiesIntegrationTests

**Successfully Resolved Challenges:**

1. **Mock State Pollution:**

   - **Problem:** Singleton mocks shared state between tests causing "city name already exists" failures
   - **Solution:** ClearReceivedCalls() and Arg.Any<>() patterns to override previous mock setups

2. **Validation Rule Compliance:**

   - **Problem:** AutoFixture GUIDs contained numbers/hyphens that failed validation
   - **Solution:** Custom alphabetic data generation respecting business rules

3. **Exception Mapping:**
   - **Problem:** InvalidOperationException mapped to 400 BadRequest instead of expected 500
   - **Solution:** Use System.Exception for 500 status codes, specific exceptions for business rule violations

**Test Coverage Achieved (8 tests, all passing):**

- GET /api/v1/cities with and without data
- POST /api/v1/cities with valid and invalid data
- Business rule violations (duplicate names/postcodes)
- Repository failure scenarios
- AutoFixture data compatibility

### ❌ Remaining Work

**Priority 1: Create MockedDealsIntegrationTests**

- Apply proven patterns from CitiesIntegrationTests
- Include deal-specific business rules (featured status, pricing validation)
- Test CRUD operations for deals

**Priority 2: Recreate MockedUserIntegrationTests**

- Resolve Identity module complexity (User entity, DTOs, commands)
- Map user creation, authentication flows
- Handle password validation and email constraints

**Priority 3: Create MockedAuthIntegrationTests**

- Test login/logout flows with mocked authentication service
- JWT token validation scenarios
- Authentication failure cases

### 🔍 Key Lessons Learned

1. **Mock State Management is Critical:**

   - Always use ClearReceivedCalls() at the start of each test
   - Use Arg.Any<>() to override previous mock setups
   - Generate unique test data for each test run

2. **Validation-Aware Data Generation:**

   - AutoFixture out-of-the-box doesn't respect business validation rules
   - Custom data builders are essential for fields with strict constraints
   - Alphabetic transformations work better than numeric identifiers

3. **Exception Handling Mapping:**

   - System.Exception → 500 InternalServerError
   - BusinessValidationException → 400 BadRequest
   - InvalidOperationException → 400 BadRequest (not 500)

4. **Performance Benefits:**

   - Mocked tests execute ~10x faster than database tests
   - Better suited for fast feedback during development
   - Database tests still valuable for end-to-end validation

5. **Test Isolation:**
   - Each test must be completely independent
   - Mock setup should be explicit and test-specific
   - Avoid shared test data that could cause race conditions
