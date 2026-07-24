using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace TailorBooking.Controllers;

public class LanguageController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(
        string culture,
        string? returnUrl)
    {
        string selectedCulture =
            culture == "ar" ? "ar" : "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider
                .DefaultCookieName,
            CookieRequestCultureProvider
                .MakeCookieValue(
                    new RequestCulture(
                        selectedCulture)),
            new CookieOptions
            {
                Expires =
                    DateTimeOffset.UtcNow.AddYears(1),

                IsEssential = true,

                HttpOnly = true,

                SameSite = SameSiteMode.Lax,

                Secure = Request.IsHttps
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(
            "Index",
            "Home");
    }
}