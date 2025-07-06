# LLM Testing Code Generation Prompt

## Overview

This prompt template is designed to help LLMs generate high-quality unit and integration tests for .NET applications following the patterns and conventions established in the TravelBookingPlatform project.

## Testing Stack and Packages

When generating test code, always use these packages:

- **xUnit** - Primary testing framework
- **FluentAssertions** - For readable and expressive assertions
- **NSubstitute** - For mocking dependencies with clean syntax
- **AutoFixture** - For generating test data
- **Microsoft.AspNetCore.Mvc.Testing** - For integration tests
- **Microsoft.NET.Test.Sdk** - Test SDK
- **coverlet.collector** - Code coverage

## Code Generation Instructions

### 1. Unit Test Generation Template

When generating unit tests, follow this structure:

```csharp
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using [relevant namespaces];

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
        _classUnderTest = new [ClassUnderTest](_dependency);
    }

    [Fact]
    public async Task [MethodName]_Should[ExpectedBehavior]_When[Condition]()
    {
        // Arrange
        var input = _fixture.Create<[InputType]>();
        _dependency.Method().Returns(expectedResult);

        // Act
        var result = await _classUnderTest.Method(input);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(expectedValue);
        await _dependency.Received(1).Method();
    }
}
```

### 2. Integration Test Generation Template

When generating integration tests, follow this structure:

```csharp
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace [TestNamespace];

public class [ControllerName]IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Fixture _fixture;

    public [ControllerName]IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task [HttpMethod][EndpointName]_Should[ExpectedBehavior]_When[Condition]()
    {
        // Arrange
        var requestData = _fixture.Create<[RequestType]>();

        // Act
        var response = await _client.[HttpMethod]("/api/[endpoint]", requestData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.[ExpectedStatus]);
        var result = await response.Content.ReadFromJsonAsync<[ResponseType]>();
        result.Should().NotBeNull();
    }
}
```

## Key Testing Patterns and Best Practices

### 1. Test Naming Convention

- Use descriptive method names: `MethodName_Should[ExpectedBehavior]_When[Condition]`
- Examples:
  - `Handle_ShouldCreateCity_WhenValidCommandProvided`
  - `Validate_ShouldReturnError_WhenNameIsEmpty`
  - `GetAllCities_ShouldReturnSuccessStatusCode_WhenCalled`

### 2. AutoFixture Usage Patterns

**For Simple Objects:**

```csharp
var command = _fixture.Create<CreateCityCommand>();
```

**For Complex Objects with Specific Properties:**

```csharp
var command = _fixture.Build<CreateCityCommand>()
    .With(x => x.Name, "Specific Value")
    .With(x => x.Country, "Test Country")
    .Create();
```

**For Entities with Constructor Requirements:**

```csharp
var cities = new List<City>
{
    new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
    new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>())
};
```

**For Validation-Aware Test Data:**

```csharp
var randomName = _fixture.Create<string>();
var command = _fixture.Build<CreateCityCommand>()
    .With(x => x.Name, randomName.Length > 100 ? randomName[..50] : randomName)
    .Create();
```

### 3. FluentAssertions Patterns

**Basic Assertions:**

```csharp
result.Should().NotBeNull();
result.Should().BeOfType<ExpectedType>();
result.Should().Be(expectedValue);
```

**Collection Assertions:**

```csharp
result.Should().HaveCount(2);
result.Should().BeEmpty();
result.Should().Contain(item => item.Property == expectedValue);
result.Should().AllSatisfy(item => item.Property.Should().NotBeNullOrEmpty());
```

**Validation Assertions:**

```csharp
result.IsValid.Should().BeTrue();
result.Errors.Should().BeEmpty();
result.Errors.Should().Contain(e => e.PropertyName == "PropertyName");
```

### 4. NSubstitute Mocking Patterns

**Setup Return Values:**

```csharp
_repository.GetAsync(Arg.Any<int>()).Returns(Task.FromResult(expectedEntity));
_repository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Entity>>(entities));
```

**Verify Method Calls:**

```csharp
await _repository.Received(1).AddAsync(Arg.Any<Entity>());
await _repository.Received(1).AddAsync(Arg.Is<Entity>(e => e.Property == expectedValue));
_repository.DidNotReceive().DeleteAsync(Arg.Any<int>());
```

### 5. Test Categories and Scenarios

**Always Include These Test Categories:**

1. **Happy Path Tests:**

   - Valid input scenarios
   - Successful operations
   - Expected return values

2. **Edge Case Tests:**

   - Boundary conditions
   - Null/empty inputs
   - Maximum/minimum values

3. **Error Handling Tests:**

   - Invalid input scenarios
   - Exception handling
   - Validation failures

4. **Integration Scenarios:**
   - HTTP status codes
   - Request/response validation
   - End-to-end workflows

### 6. Command Handler Test Pattern

```csharp
[Fact]
public async Task Handle_ShouldCreateEntity_WhenValidCommandProvided()
{
    // Arrange
    var command = _fixture.Create<CreateCommand>();

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    await _repository.Received(1).AddAsync(Arg.Is<Entity>(e =>
        e.Property1 == command.Property1 &&
        e.Property2 == command.Property2));
    await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);

    result.Should().NotBeNull();
    result.Property.Should().Be(command.Property);
}
```

### 7. Query Handler Test Pattern

```csharp
[Fact]
public async Task Handle_ShouldReturnEntities_WhenEntitiesExist()
{
    // Arrange
    var query = _fixture.Create<GetAllQuery>();
    var entities = _fixture.CreateMany<Entity>(3).ToList();
    _repository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Entity>>(entities));

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    await _repository.Received(1).GetAllAsync();
    result.Should().NotBeNull();
    result.Should().HaveCount(3);
    result.Should().AllSatisfy(dto => dto.Property.Should().NotBeNullOrEmpty());
}
```

### 8. Validator Test Pattern

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData(" ")]
public void Validate_ShouldHaveError_WhenPropertyIsInvalid(string invalidValue)
{
    // Arrange
    var command = _fixture.Build<Command>()
        .With(x => x.Property, invalidValue)
        .Create();

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Property));
}
```

### 9. Integration Test HTTP Patterns

```csharp
[Fact]
public async Task PostEndpoint_ShouldReturnCreated_WhenValidDataProvided()
{
    // Arrange
    var requestData = _fixture.Build<CreateDto>()
        .With(x => x.Name, "Valid Name")
        .Create();

    // Act
    var response = await _client.PostAsJsonAsync("/api/endpoint", requestData);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var result = await response.Content.ReadFromJsonAsync<ResponseDto>();
    result.Should().NotBeNull();
    result!.Name.Should().Be(requestData.Name);
}
```

## LLM Prompt Template

Use this template when requesting test generation:

---

**PROMPT:**

Generate comprehensive unit/integration tests for the following .NET class/controller following these requirements:

1. **Use the specified testing stack:** xUnit, FluentAssertions, NSubstitute, AutoFixture
2. **Follow naming conventions:** `MethodName_Should[ExpectedBehavior]_When[Condition]`
3. **Include test categories:** Happy path, edge cases, error handling
4. **Use AutoFixture patterns:** Generate test data appropriately, handle constructor constraints
5. **Use FluentAssertions:** Write readable, expressive assertions
6. **Use NSubstitute:** Mock dependencies with clean syntax, verify interactions
7. **Follow AAA pattern:** Clear Arrange, Act, Assert sections
8. **Constructor setup:** Initialize fixtures, mocks, and class under test
9. **Comprehensive coverage:** Test all public methods and scenarios

**Target Code:**
[Insert the class/controller code here]

**Additional Requirements:**

- Generate both positive and negative test cases
- Include boundary condition testing
- Use proper async/await patterns where applicable
- For integration tests, include HTTP status code validation
- Follow the established project patterns and conventions

---

## File Structure Conventions

```
tests/
├── TravelBookingPlatform.UnitTests/
│   ├── [Module].UnitTests/
│   │   ├── [Feature]/
│   │   │   ├── Commands/
│   │   │   │   ├── [CommandName]HandlerTests.cs
│   │   │   │   └── [CommandName]ValidatorTests.cs
│   │   │   └── Queries/
│   │   │       └── [QueryName]HandlerTests.cs
│   │   └── [Module].UnitTests.csproj
└── TravelBookingPlatform.IntegrationTests/
    ├── [Module].IntegrationTests/
    │   ├── [Feature]IntegrationTests.cs
    │   └── [Module].IntegrationTests.csproj
```

This prompt template ensures consistent, high-quality test generation that follows the established patterns and best practices in the TravelBookingPlatform project.
