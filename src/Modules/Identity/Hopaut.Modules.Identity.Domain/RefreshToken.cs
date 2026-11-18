using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hopaut.Modules.Identity.Domain;

public class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Token { get; set; } = default!;

    public string JwtId { get; set; } = default!;
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Used { get; set; }
    public bool Invalidated { get; set; }

    public string UserId { get; set; } = default!;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = default!;
}
