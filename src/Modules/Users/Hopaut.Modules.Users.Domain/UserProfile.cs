using Hopaut.SharedKernel;

namespace Hopaut.Modules.Users.Domain;

public readonly record struct UserProfileId(Guid Value)
{
    public static UserProfileId New() => new(Guid.NewGuid());
}

/// <summary>
/// User profile aggregate — owns profile-related fields split from Identity's AppUser.
/// Keyed by Identity user id (string mapped to Guid).
/// </summary>
public sealed class UserProfile : AggregateRoot<UserProfileId>
{
    public string IdentityUserId { get; private set; } = default!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string ProfilePicture { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public long RegistrationTimeStamp { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(string identityUserId, string firstName, string lastName, long registrationTimeStamp)
    {
        return new UserProfile
        {
            Id = UserProfileId.New(),
            IdentityUserId = identityUserId,
            FirstName = firstName,
            LastName = lastName,
            RegistrationTimeStamp = registrationTimeStamp
        };
    }

    public void UpdateProfile(string firstName, string lastName, string description)
    {
        FirstName = firstName;
        LastName = lastName;
        Description = description;
    }

    public void UpdateProfilePicture(string pictureUrl)
    {
        ProfilePicture = pictureUrl;
    }
}
