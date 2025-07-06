# LLM Testing Code Generation Prompt

## Overview

This prompt template is designed to help LLMs generate high-quality unit and integration tests for .NET applications following the exact patterns and conventions established in the TravelBookingPlatform project.

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
    └── TravelBookingPlatform.[Module].IntegrationTests/
        ├── [Feature]IntegrationTests.cs
        ├── IntegrationTestBase.cs
        └── TravelBookingPlatform.[Module].IntegrationTests.csproj
```

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
   - Inherit from `IntegrationTestBase`
   - Use unique identifiers to avoid test collisions
   - Test both success and failure HTTP scenarios'
9. **Reference the new test projects:**
   - at the end, you need to reference the new test projects in the solution file

**Target Code:**
[Insert the class/controller code here]

---

This prompt ensures test generation that EXACTLY matches the patterns established in the TravelBookingPlatform codebase.
