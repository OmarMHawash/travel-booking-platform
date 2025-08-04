using AutoMapper;
using MediatR;
using TravelBookingPlatform.Core.Domain;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;
using TravelBookingPlatform.Modules.Hotels.Domain.Entities;
using TravelBookingPlatform.Modules.Hotels.Domain.Repositories;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands.Handlers;

public class CreateCityCommandHandler: IRequestHandler<CreateCityCommand, CityDto>
{
    private readonly ICityRepository _cityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCityCommandHandler(ICityRepository cityRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cityRepository = cityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CityDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        City city = new City(request.Name, request.Country, request.PostCode);
            
        await _cityRepository.AddAsync(city);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        return _mapper.Map<CityDto>(city);
    }
}