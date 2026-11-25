using System.ComponentModel.DataAnnotations;

namespace WorkDisabilityQuerySystem.Web.Areas.Backend.Models;

public sealed class UpdateAccountProfileInput
{
    [Required]
    public Guid? AccountId { get; set; }

    [Required]
    public string Level { get; set; } = "";
    [StringLength(100)]
    public string? DisplayName { get; set; }
}