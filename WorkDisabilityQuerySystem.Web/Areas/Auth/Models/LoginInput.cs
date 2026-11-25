using System.ComponentModel.DataAnnotations;

namespace WorkDisabilityQuerySystem.Web.Areas.Auth.Models;

public sealed class LoginInput
{
    [Required(ErrorMessage = "請輸入帳號。")]
    [StringLength(50, ErrorMessage = "帳號最多 50 個字元。")]
    [Display(Name = "帳號")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼。")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
