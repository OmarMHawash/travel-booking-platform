namespace TravelBookingPlatform.Modules.Hotels.Application.DTOs;

public class CityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}