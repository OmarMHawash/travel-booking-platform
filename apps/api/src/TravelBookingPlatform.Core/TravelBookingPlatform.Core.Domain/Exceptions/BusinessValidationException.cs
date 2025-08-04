using System;

namespace TravelBookingPlatform.Core.Domain.Exceptions;

public class BusinessValidationException : Exception
{
    public string? PropertyName { get; }

    public BusinessValidationException(string message) : base(message)
    {
    }

    public BusinessValidationException(string message, string propertyName) : base(message)
    {
        PropertyName = propertyName;
    }

    public BusinessValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}