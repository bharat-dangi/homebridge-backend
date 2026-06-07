using HomeBridge.Data;
using HomeBridge.Dtos;
using HomeBridge.Helpers;
using HomeBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Controllers;

// Public, supporting endpoints: landing-page stats, dropdown options and the contact form.
[ApiController]
[Route("api")]
public class MetaController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<MetaController> _logger;

    public MetaController(ApplicationDbContext db, ILogger<MetaController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/home  → featured homes + headline counts for the landing page.
    [HttpGet("home")]
    public async Task<ActionResult<HomeStatsDto>> Home()
    {
        var available = _db.Listings.Where(l => l.IsAvailable);

        var featured = await available
            .OrderByDescending(l => l.DateListed)
            .Take(3)
            .ToListAsync();

        return new HomeStatsDto
        {
            AvailableCount = await available.CountAsync(),
            SuburbCount = await available.Select(l => l.Suburb).Distinct().CountAsync(),
            AccessibleCount = await available.CountAsync(l => l.IsAccessible),
            FeaturedListings = featured.Select(l => l.ToDto()).ToList()
        };
    }

    // GET /api/options  → enum option lists (value + label) for the client's dropdowns.
    [HttpGet("options")]
    public ActionResult<object> Options() => Ok(new
    {
        propertyTypes = EnumOptions<PropertyType>(),
        sorts = EnumOptions<ListingSort>(),
        applicationStatuses = EnumOptions<ApplicationStatus>()
    });

    // POST /api/contact  → validated server-side; logged rather than emailed (kept simple by design).
    [HttpPost("contact")]
    public IActionResult Contact(ContactRequest request)
    {
        _logger.LogInformation("Contact message from {Email}: {Subject}", request.Email, request.Subject);
        return Ok(new { message = "Thanks for reaching out — our team will reply within two business days." });
    }

    private static List<EnumOptionDto> EnumOptions<TEnum>() where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(v => new EnumOptionDto
            {
                Value = Convert.ToInt32(v),
                Name = v.ToString(),
                Label = ((Enum)(object)v).Humanize()
            })
            .ToList();
}
