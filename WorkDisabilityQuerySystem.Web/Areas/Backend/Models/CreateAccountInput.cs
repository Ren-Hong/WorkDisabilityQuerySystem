using System.ComponentModel.DataAnnotations;

namespace WorkDisabilityQuerySystem.Web.Areas.Backend.Models;

public sealed class CreateAccountInput
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, StringLength(72, MinimumLength = 12)]
    public string Password { get; set; } = "";

    [Required]
    public string Level { get; set; } = "";
    [StringLength(100)]
    public string? DisplayName { get; set; }
}