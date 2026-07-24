using System.ComponentModel.DataAnnotations;

namespace TailorBooking.ViewModels;

public class BookingCreateViewModel
    : IValidatableObject
{
    [Required(
        ErrorMessage = "RequiredFullName")]
    [StringLength(100)]
    [Display(Name = "FullName")]
    public string CustomerName { get; set; }
        = string.Empty;

    [Required(
        ErrorMessage = "RequiredEmail")]
    [EmailAddress(
        ErrorMessage = "InvalidEmail")]
    [StringLength(150)]
    [Display(Name = "EmailAddress")]
    public string Email { get; set; }
        = string.Empty;

    // intl-tel-input on the client always submits E.164 format:
    // a leading "+", country code, then subscriber number, e.g.
    // +971501234567 or +923115385206. This regex is the server-side
    // enforcement of that same shape — never trust client JS alone,
    // since the form can be posted to directly without it running.
    [Required(
        ErrorMessage = "RequiredWhatsApp")]
    [RegularExpression(
        @"^\+[1-9]\d{6,14}$",
        ErrorMessage = "InvalidWhatsApp")]
    [StringLength(20)]
    [Display(Name = "WhatsAppNumber")]
    public string WhatsAppNumber { get; set; }
        = string.Empty;

    [Required(
        ErrorMessage = "RequiredArea")]
    [StringLength(100)]
    [Display(Name = "DubaiArea")]
    public string City { get; set; }
        = string.Empty;

    [Required(
        ErrorMessage = "RequiredAddress")]
    [StringLength(300)]
    [Display(Name = "CompleteAddress")]
    public string Address { get; set; }
        = string.Empty;

    [Required(
        ErrorMessage = "RequiredDate")]
    [DataType(DataType.Date)]
    [Display(Name = "PreferredDate")]
    public DateOnly? PreferredDate { get; set; }

    [Required(
        ErrorMessage = "RequiredTime")]
    [DataType(DataType.Time)]
    [Display(Name = "PreferredTime")]
    public TimeOnly? PreferredTime { get; set; }

    [StringLength(1000)]
    [Display(Name = "AdditionalNotes")]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PreferredDate.HasValue &&
            PreferredDate.Value <
                DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult(
                "PastDateNotAllowed",
                [nameof(PreferredDate)]);
        }
    }
}
