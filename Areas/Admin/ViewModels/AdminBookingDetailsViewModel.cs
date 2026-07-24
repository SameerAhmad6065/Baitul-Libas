using System.ComponentModel.DataAnnotations;
using TailorBooking.Models;
using TailorBooking.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TailorBooking.Areas.Admin.ViewModels;

public class AdminBookingDetailsViewModel
{
    [ValidateNever]
    public Booking Booking { get; set; } = null!;

    [Required(ErrorMessage = "Please select a booking status.")]
    [Display(Name = "Booking Status")]
    public BookingStatus Status { get; set; }

    [StringLength(
        1000,
        ErrorMessage = "Admin notes cannot exceed 1000 characters.")]
    [Display(Name = "Admin Notes")]
    public string? AdminNotes { get; set; }

    public string WhatsAppUrl { get; set; } = string.Empty;
}