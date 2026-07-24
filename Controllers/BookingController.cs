using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TailorBooking.Data;
using TailorBooking.Models;
using TailorBooking.Models.Enums;
using TailorBooking.Services;
using TailorBooking.ViewModels;

namespace TailorBooking.Controllers;

[AllowAnonymous]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BookingController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IEmailNotificationService _emailNotificationService;

    public BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<BookingController> logger,
        IStringLocalizer<SharedResource> localizer,
        IEmailNotificationService emailNotificationService)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _localizer = localizer;
        _emailNotificationService = emailNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new BookingCreateViewModel
        {
            PreferredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {
                model.CustomerName = user.FullName;
                model.Email = user.Email ?? string.Empty;
                model.WhatsAppNumber = user.WhatsAppNumber ?? string.Empty;
                model.City = user.City ?? string.Empty;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            string? customerId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                customerId = _userManager.GetUserId(User);
            }

            string bookingNumber = await GenerateUniqueBookingNumberAsync();

            var booking = new Booking
            {
                BookingNumber = bookingNumber,
                CustomerId = customerId,
                CustomerName = model.CustomerName.Trim(),
                Email = model.Email.Trim(),
                WhatsAppNumber = model.WhatsAppNumber.Trim(),
                City = model.City.Trim(),
                Address = model.Address.Trim(),
                PreferredDate = model.PreferredDate!.Value,
                PreferredTime = model.PreferredTime!.Value,
                Notes = model.Notes?.Trim(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _emailNotificationService.NotifyNewBookingAsync(booking);

            TempData["BookingNumber"] = booking.BookingNumber;

            return RedirectToAction(
                nameof(Confirmation),
                new { bookingNumber = booking.BookingNumber });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An error occurred while creating a booking.");

            ModelState.AddModelError(
                string.Empty,
                _localizer["BookingFailed"]);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string bookingNumber)
    {
        if (string.IsNullOrWhiteSpace(bookingNumber))
        {
            return RedirectToAction(nameof(Create));
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BookingNumber == bookingNumber);

        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    private async Task<string> GenerateUniqueBookingNumberAsync()
    {
        string bookingNumber;
        bool exists;

        do
        {
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            string randomPart = Random.Shared.Next(100000, 999999).ToString();

            bookingNumber = $"TB-{datePart}-{randomPart}";

            exists = await _context.Bookings
                .AnyAsync(x => x.BookingNumber == bookingNumber);

        } while (exists);

        return bookingNumber;
    }

}
