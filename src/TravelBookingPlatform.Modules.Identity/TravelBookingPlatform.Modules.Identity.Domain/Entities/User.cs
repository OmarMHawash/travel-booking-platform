using TravelBookingPlatform.Core.Domain.Entities;
using TravelBookingPlatform.Modules.Identity.Domain.Enums;
using TravelBookingPlatform.Modules.Identity.Domain.ValueObjects;

namespace TravelBookingPlatform.Modules.Identity.Domain.Entities;

public class User : AggregateRoot
{
    public string Username { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }

    // For EF Core
    private User() { }

    public User(string username, string email, string passwordHash, Role role = Role.TypicalUser)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Username = username;
        Email = new Email(email);
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void UpdateEmail(string newEmail)
    {
        Email = new Email(newEmail);
        MarkAsUpdated();
    }

    public void UpdateRole(Role newRole)
    {
        Role = newRole;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}