using Microsoft.AspNetCore.Identity;

namespace Hopaut.Modules.Identity.Domain;

/// <summary>
/// Application user — auth-only fields. Profile fields (FirstName, LastName, etc.)
/// will live in the Users module after Phase 2.4.
/// </summary>
public class AppUser : IdentityUser
{
    // Profile fields kept temporarily until Users module extracts them (Step 2.4)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long RegistrationTimeStamp { get; set; }
}
