using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Hotels.UnitTests.Cities.Commands;

public class CreateCityCommandHandlerTests
{
    private readonly ICityRepository _cityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCityCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateCityCommandHandlerTests()
    {
        // Setup AutoFixture for test data generation
        _fixture = new Fixture();

        // Arrange (common setup)
        _cityRepository = Substitute.For<ICityRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        // AutoMapper setup for testing. We can manually configure the mapping.
        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreateCityCommand, City>();
            cfg.CreateMap<City, CityDto>();
        }, NullLoggerFactory.Instance);
        var mapper = mappingConfig.CreateMapper();

        _handler = new CreateCityCommandHandler(_cityRepository, _unitOfWork, mapper);
    }

    [Fact]
    public async Task Handle_ShouldCreateCityAndSaveChanges_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Build<CreateCityCommand>()
            .With(x => x.Name, "Paris")
            .With(x => x.Country, "France")
            .With(x => x.PostCode, "75001")
            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 1. Verify that AddAsync was called on the repository with any City object.
        await _cityRepository.Received(1).AddAsync(Arg.Is<City>(c =>
            c.Name == command.Name &&
            c.Country == command.Country &&
            c.PostCode == command.PostCode
        ));

        // 2. Verify that SaveChangesAsync was called on the Unit of Work.
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);

        // 3. Check the returned DTO for correctness.
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Country.Should().Be(command.Country);
        result.PostCode.Should().Be(command.PostCode);
    }

    [Fact]
    public async Task Handle_ShouldCreateCityWithRandomData_WhenUsingAutoFixture()
    {
        // Arrange - Using AutoFixture to generate random test data
        var command = _fixture.Create<CreateCityCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _cityRepository.Received(1).AddAsync(Arg.Any<City>());
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Country.Should().Be(command.Country);
        result.PostCode.Should().Be(command.PostCode);
    }
}