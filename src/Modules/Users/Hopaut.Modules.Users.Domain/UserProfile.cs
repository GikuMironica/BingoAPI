using System.Diagnostics.CodeAnalysis;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Users.Domain;

public readonly record struct UserProfileId(Guid Value) : IStronglyTypedId<UserProfileId, Guid>
{
    public static UserProfileId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public static UserProfileId From(Guid value) => new(value);

    public static UserProfileId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<UserProfileId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out UserProfileId result) =>
        StronglyTypedId.TryParseGuid(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out UserProfileId result) => StronglyTypedId.TryParseGuid(s, out result);

    public override string ToString() => Value.ToString();
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

    public static UserProfile Create(string identityUserId, string firstName, string lastName, long registrationTimeStamp, DateTimeOffset now)
    {
        return new UserProfile
        {
            Id = UserProfileId.New(now),
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
