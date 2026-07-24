using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TailorBooking.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? WhatsAppNumber { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; }
    = new List<Booking>();

}