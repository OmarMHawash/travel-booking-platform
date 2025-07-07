using System;

namespace TravelBookingPlatform.Core.Domain.Exceptions;

public class ForeignKeyViolationException : Exception
{
    public string EntityName { get; }
    public string ForeignKeyName { get; }
    public object ForeignKeyValue { get; }

    public ForeignKeyViolationException(string entityName, string foreignKeyName, object foreignKeyValue)
        : base($"The referenced {foreignKeyName} '{foreignKeyValue}' does not exist for {entityName}.")
    {
        EntityName = entityName;
        ForeignKeyName = foreignKeyName;
        ForeignKeyValue = foreignKeyValue;
    }

    public ForeignKeyViolationException(string message) : base(message)
    {
    }

    public ForeignKeyViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}