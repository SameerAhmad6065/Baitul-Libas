using TailorBooking.Models;
using TailorBooking.Models.Enums;

namespace TailorBooking.Areas.Admin.ViewModels;

public class AdminBookingListViewModel
{
    public List<Booking> Bookings { get; set; } = [];

    public string? SearchTerm { get; set; }

    public BookingStatus? Status { get; set; }

    public int TotalBookings { get; set; }

    public int PendingBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CompletedBookings { get; set; }

    public int CancelledBookings { get; set; }
}
