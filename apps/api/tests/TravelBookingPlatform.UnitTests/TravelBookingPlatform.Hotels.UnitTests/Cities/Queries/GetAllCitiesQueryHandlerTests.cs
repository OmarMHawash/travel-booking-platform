using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Application.Queries;
using TravelBookingPlatform.Modules.Hotels.Application.Queries.Handlers;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Hotels.UnitTests.Cities.Queries;

public class GetAllCitiesQueryHandlerTests
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;
    private readonly GetAllCitiesQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetAllCitiesQueryHandlerTests()
    {
        _fixture = new Fixture();
        _cityRepository = Substitute.For<ICityRepository>();

        var mappingConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<City, CityDto>();
        }, NullLoggerFactory.Instance);
        _mapper = mappingConfig.CreateMapper();

        _handler = new GetAllCitiesQueryHandler(_cityRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfCityDtos_WhenCitiesExist()
    {
        // Arrange
        var query = _fixture.Create<GetAllCitiesQuery>();
        var cities = new List<City>
        {
            new("London", "UK", "SW1A 0AA"),
            new("Tokyo", "Japan", "100-0001")
        };

        _cityRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<City>>(cities));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // 1. Verify the repository method was called.
        await _cityRepository.Received(1).GetAllAsync();

        // 2. Check the result.
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeOfType<List<CityDto>>();
        result.First().Name.Should().Be("London");
        result.First().Country.Should().Be("UK");
        result.First().PostCode.Should().Be("SW1A 0AA");
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfCityDtos_WhenCitiesExistWithRandomData()
    {
        // Arrange - Using AutoFixture to generate random cities
        var query = _fixture.Create<GetAllCitiesQuery>();
        var cities = new List<City>
        {
            new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
            new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
            new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
            new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()),
            new(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>())
        };

        _cityRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<City>>(cities));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cityRepository.Received(1).GetAllAsync();
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().BeOfType<List<CityDto>>();
        result.Should().AllSatisfy(dto =>
        {
            dto.Name.Should().NotBeNullOrEmpty();
            dto.Country.Should().NotBeNullOrEmpty();
            dto.PostCode.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCitiesExist()
    {
        // Arrange
        var query = _fixture.Create<GetAllCitiesQuery>();
        var emptyList = new List<City>();

        _cityRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<City>>(emptyList));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cityRepository.Received(1).GetAllAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}