using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TailorBooking.Models.Enums;

namespace TailorBooking.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string BookingNumber { get; set; } = "";

    public string? CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public ApplicationUser? Customer { get; set; }

    [Required]
    public string CustomerName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string WhatsAppNumber { get; set; } = "";

    [Required]
    public string City { get; set; } = "";

    [Required]
    public string Address { get; set; } = "";

    [Required]
    public DateOnly PreferredDate { get; set; }

    [Required]
    public TimeOnly PreferredTime { get; set; }

    public string? Notes { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}