using TravelBookingPlatform.Core.Domain.Entities;

namespace TravelBookingPlatform.Modules.Hotels.Domain.Entities;

public class City : AggregateRoot
{
    public string Name { get; private set; }
    public string Country { get; private set; }
    public string PostCode { get; private set; }

    // For EF Core
    private City() { }

    public City(string name, string country, string postCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));
        if (string.IsNullOrWhiteSpace(postCode))
            throw new ArgumentException("Post code cannot be empty.", nameof(postCode));

        Name = name;
        Country = country;
        PostCode = postCode;
    }
    

    public void Update(string name, string country, string postCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));
        if (string.IsNullOrWhiteSpace(postCode))
            throw new ArgumentException("Post code cannot be empty.", nameof(postCode));

        Name = name;
        Country = country;
        PostCode = postCode;
        MarkAsUpdated();
    }
}