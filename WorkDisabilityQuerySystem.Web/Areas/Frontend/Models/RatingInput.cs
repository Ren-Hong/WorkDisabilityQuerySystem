using System.ComponentModel.DataAnnotations;
namespace WorkDisabilityQuerySystem.Web.Areas.Frontend.Models;

public sealed class RatingInput
{
    [Required(ErrorMessage = "請從搜尋結果選取損傷項目。")]
    [RegularExpression(@"\d{2}(\.\d{2}){3}", ErrorMessage = "損傷編碼格式有誤。")]
    public string? ImpairmentCode { get; set; }

    [Required(ErrorMessage = "請從搜尋結果選取職業。")]
    public Guid? OccupationId { get; set; }

    [Required(ErrorMessage = "請輸入 WPI。")]
    [Range(1, 100, ErrorMessage = "WPI 必須為 1–100 的整數。")]
    public int? Wpi { get; set; }

    [Required(ErrorMessage = "請輸入受傷時年齡。")]
    [Range(0, int.MaxValue, ErrorMessage = "年齡必須為非負整數。")]
    public int? Age { get; set; }

    public string? AffectedFunction { get; set; }
}

