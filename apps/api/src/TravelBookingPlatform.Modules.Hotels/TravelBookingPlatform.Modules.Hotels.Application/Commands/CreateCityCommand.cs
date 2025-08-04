using TravelBookingPlatform.Core.Application.Commands;
using TravelBookingPlatform.Modules.Hotels.Application.DTOs;

namespace TravelBookingPlatform.Modules.Hotels.Application.Commands;

public class CreateCityCommand : ICommand<CityDto>
{
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
}