using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TailorBooking.Areas.Admin.ViewModels;
using TailorBooking.Data;
using TailorBooking.Models;
using TailorBooking.Models.Enums;

namespace TailorBooking.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        ApplicationDbContext context,
        ILogger<BookingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ======================================================
    // GET: /Admin/Bookings
    // ======================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        BookingStatus? status)
    {
        IQueryable<Booking> query = _context.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string search = searchTerm.Trim();

            query = query.Where(b =>
                b.BookingNumber.Contains(search) ||
                b.CustomerName.Contains(search) ||
                b.Email.Contains(search) ||
                b.WhatsAppNumber.Contains(search) ||
                b.City.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        List<Booking> bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        AdminBookingListViewModel model = new()
        {
            Bookings = bookings,
            SearchTerm = searchTerm,
            Status = status,

            TotalBookings =
                await _context.Bookings.CountAsync(),

            PendingBookings =
                await _context.Bookings.CountAsync(x =>
                    x.Status == BookingStatus.Pending),

            ConfirmedBookings =
                await _context.Bookings.CountAsync(x =>
                    x.Status == BookingStatus.Confirmed),

            CompletedBookings =
                await _context.Bookings.CountAsync(x =>
                    x.Status == BookingStatus.Completed),

            CancelledBookings =
                await _context.Bookings.CountAsync(x =>
                    x.Status == BookingStatus.Cancelled)
        };

        return View(model);
    }

    // ======================================================
    // GET: /Admin/Bookings/Details
    // ======================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        Booking? booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (booking is null)
            return NotFound();

        AdminBookingDetailsViewModel model = new()
        {
            Booking = booking,
            Status = booking.Status,
            AdminNotes = booking.AdminNotes,
            WhatsAppUrl = CreateWhatsAppUrl(
                booking.WhatsAppNumber,
                booking)
        };

        return View(model);
    }

    // ======================================================
    // POST: Update Booking
    // ======================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int id,
        AdminBookingDetailsViewModel model)
    {
        if (id != model.Booking.Id)
            return BadRequest();

        Booking? booking = await _context.Bookings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (booking is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Booking = booking;

            model.WhatsAppUrl = CreateWhatsAppUrl(
                booking.WhatsAppNumber,
                booking);

            return View("Details", model);
        }

        try
        {
            booking.Status = model.Status;

            booking.AdminNotes =
                string.IsNullOrWhiteSpace(model.AdminNotes)
                    ? null
                    : model.AdminNotes.Trim();

            // Uncomment if admin can edit appointment date/time

            /*
            booking.PreferredDate = model.PreferredDate;
            booking.PreferredTime = model.PreferredTime;
            */

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Booking updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = booking.Id });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Unable to update booking {BookingId}.",
                id);

            ModelState.AddModelError(
                string.Empty,
                "The booking could not be updated.");

            model.Booking = booking;

            model.WhatsAppUrl = CreateWhatsAppUrl(
                booking.WhatsAppNumber,
                booking);

            return View("Details", model);
        }
    }

    // ======================================================
    // WhatsApp URL
    // ======================================================

    private static string CreateWhatsAppUrl(
        string phoneNumber,
        Booking booking)
    {
        string normalizedNumber = new string(
            phoneNumber
                .Where(char.IsDigit)
                .ToArray());

        // Only convert local UAE mobile format (05XXXXXXXX, 10 digits)
        // to international (9715XXXXXXXX). Any other number is left
        // as-is, since it may already include its own country code
        // (e.g. a Pakistani number typed as 923115385206, or a UK
        // number as 447...), and blindly forcing +971 onto every
        // number that happens to start with "0" breaks those.

        bool looksLikeUaeLocalMobile =
            normalizedNumber.Length == 10 &&
            normalizedNumber.StartsWith("05");

        if (looksLikeUaeLocalMobile)
        {
            normalizedNumber = $"971{normalizedNumber[1..]}";
        }

        string message =
            $"Hello {booking.CustomerName}, " +
            $"we are contacting you regarding your appointment " +
            $"{booking.BookingNumber} " +
            $"scheduled on " +
            $"{booking.PreferredDate:dd MMMM yyyy} " +
            $"at {booking.PreferredTime:hh\\:mm}.";

        return $"https://wa.me/{normalizedNumber}?text={Uri.EscapeDataString(message)}";
    }
    // ======================================================
    // POST: Delete Booking
    // ======================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (booking is null)
        {
            TempData["ErrorMessage"] = "Booking not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Booking {booking.BookingNumber} deleted successfully.";

            _logger.LogInformation(
                "Booking {BookingNumber} deleted by admin.",
                booking.BookingNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting booking {BookingId}",
                id);

            TempData["ErrorMessage"] =
                "Unable to delete booking.";
        }

        return RedirectToAction(nameof(Index));
    }
}
