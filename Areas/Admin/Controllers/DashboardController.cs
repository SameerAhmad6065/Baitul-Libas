using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TailorBooking.Data;
using TailorBooking.Models.Enums;

namespace TailorBooking.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalBookings =
            await _context.Bookings.CountAsync();

        ViewBag.PendingBookings =
            await _context.Bookings.CountAsync(booking =>
                booking.Status == BookingStatus.Pending);

        ViewBag.ConfirmedBookings =
            await _context.Bookings.CountAsync(booking =>
                booking.Status == BookingStatus.Confirmed);

        ViewBag.CompletedBookings =
            await _context.Bookings.CountAsync(booking =>
                booking.Status == BookingStatus.Completed);

        ViewBag.RecentBookings =
            await _context.Bookings
                .AsNoTracking()
                .OrderByDescending(booking =>
                    booking.CreatedAt)
                .Take(5)
                .ToListAsync();

        return View();
    }
}