using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TailorBooking.Models;

namespace TailorBooking.Services;

public interface IEmailNotificationService
{
    Task NotifyNewBookingAsync(Booking booking);
}

public class EmailNotificationService : IEmailNotificationService
{
    private const string SendEmailUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _ownerEmail;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _httpClient = httpClient;

        _apiKey = configuration["Brevo:ApiKey"]
            ?? throw new InvalidOperationException("Brevo:ApiKey is not configured.");

        _fromEmail = configuration["Brevo:FromEmail"]
            ?? throw new InvalidOperationException("Brevo:FromEmail is not configured.");

        _fromName = configuration["Brevo:FromName"] ?? "Bait Al Libas";

        _ownerEmail = configuration["Brevo:OwnerEmail"]
            ?? throw new InvalidOperationException("Brevo:OwnerEmail is not configured.");

        _logger = logger;
    }

    public async Task NotifyNewBookingAsync(Booking booking)
    {
        try
        {
            string htmlContent =
                $"<h2>New Booking Received</h2>" +
                $"<table cellpadding='6' style='border-collapse:collapse;'>" +
                $"<tr><td><strong>Booking #</strong></td><td>{booking.BookingNumber}</td></tr>" +
                $"<tr><td><strong>Name</strong></td><td>{booking.CustomerName}</td></tr>" +
                $"<tr><td><strong>Email</strong></td><td>{booking.Email}</td></tr>" +
                $"<tr><td><strong>WhatsApp</strong></td><td>{booking.WhatsAppNumber}</td></tr>" +
                $"<tr><td><strong>City</strong></td><td>{booking.City}</td></tr>" +
                $"<tr><td><strong>Address</strong></td><td>{booking.Address}</td></tr>" +
                $"<tr><td><strong>Date</strong></td><td>{booking.PreferredDate:dd MMM yyyy}</td></tr>" +
                $"<tr><td><strong>Time</strong></td><td>{booking.PreferredTime:hh\\:mm}</td></tr>" +
                (string.IsNullOrWhiteSpace(booking.Notes)
                    ? ""
                    : $"<tr><td><strong>Notes</strong></td><td>{booking.Notes}</td></tr>") +
                $"</table>";

            string subject = $"New Booking — {booking.BookingNumber}";

            var payload = new
            {
                sender = new { email = _fromEmail, name = _fromName },
                to = new[] { new { email = _ownerEmail } },
                subject,
                htmlContent
            };

            string json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("api-key", _apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Booking notification email sent for {BookingNumber}. Status: {StatusCode}",
                    booking.BookingNumber,
                    response.StatusCode);
            }
            else
            {
                string body = await response.Content.ReadAsStringAsync();

                _logger.LogError(
                    "Booking notification email failed for {BookingNumber}. Status: {StatusCode}. Body: {Body}",
                    booking.BookingNumber,
                    response.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            // Notification failure should never block the booking itself.
            _logger.LogError(
                ex,
                "Failed to send booking notification email for {BookingNumber}.",
                booking.BookingNumber);
        }
    }
}
