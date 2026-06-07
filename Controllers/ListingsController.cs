using HomeBridge.Data;
using HomeBridge.Dtos;
using HomeBridge.Helpers;
using HomeBridge.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Controllers;

// Public, read-only browsing of available homes — no authentication required.
[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ListingsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET /api/listings?keyword=&suburb=&maxRent=&minBedrooms=&propertyType=&accessibleOnly=&petsOnly=&sort=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<PagedResult<ListingDto>>> Search([FromQuery] ListingSearchRequest filter)
    {
        var query = _db.Listings.Where(l => l.IsAvailable);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            // ILIKE = case-insensitive partial match (Postgres). %term% matches anywhere in the text.
            var pattern = $"%{EscapeLike(filter.Keyword.Trim())}%";
            query = query.Where(l =>
                EF.Functions.ILike(l.Title, pattern) ||
                EF.Functions.ILike(l.Description, pattern) ||
                EF.Functions.ILike(l.Suburb, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Suburb))
        {
            var suburbPattern = $"%{EscapeLike(filter.Suburb.Trim())}%";
            query = query.Where(l => EF.Functions.ILike(l.Suburb, suburbPattern));
        }

        if (filter.MaxRent is > 0)
            query = query.Where(l => l.WeeklyRent <= filter.MaxRent);

        if (filter.MinBedrooms is > 0)
            query = query.Where(l => l.Bedrooms >= filter.MinBedrooms);

        if (filter.PropertyType is not null)
            query = query.Where(l => l.PropertyType == filter.PropertyType);

        if (filter.AccessibleOnly)
            query = query.Where(l => l.IsAccessible);

        if (filter.PetsOnly)
            query = query.Where(l => l.AcceptsPets);

        query = filter.Sort switch
        {
            ListingSort.RentLowToHigh => query.OrderBy(l => l.WeeklyRent),
            ListingSort.RentHighToLow => query.OrderByDescending(l => l.WeeklyRent),
            ListingSort.BedroomsDesc => query.OrderByDescending(l => l.Bedrooms).ThenByDescending(l => l.DateListed),
            _ => query.OrderByDescending(l => l.DateListed)
        };

        var pageSize = Math.Clamp(filter.PageSize, 1, 48);
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var page = Math.Max(1, filter.Page);
        if (totalPages > 0)
            page = Math.Min(page, totalPages);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ListingDto>
        {
            Items = items.Select(l => l.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalResults = total,
            SavedListingIds = (await SavedIdsForCurrentUserAsync()).ToList()
        };
    }

    // GET /api/listings/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ListingDetailsDto>> Details(int id)
    {
        var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == id);
        if (listing is null)
            return NotFound();

        var dto = new ListingDetailsDto { Listing = listing.ToDto() };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User)!;
            dto.IsSaved = await _db.SavedListings.AnyAsync(s => s.UserId == userId && s.ListingId == id);
            dto.HasApplied = await _db.Applications.AnyAsync(a =>
                a.ApplicantId == userId && a.ListingId == id && a.Status != ApplicationStatus.Withdrawn);
        }

        return dto;
    }

    // Listing ids the signed-in user has shortlisted (empty when anonymous).
    private async Task<HashSet<int>> SavedIdsForCurrentUserAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return new HashSet<int>();

        var userId = _userManager.GetUserId(User)!;
        var ids = await _db.SavedListings
            .Where(s => s.UserId == userId)
            .Select(s => s.ListingId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    // Escapes LIKE/ILIKE wildcards so a user's % or _ is matched literally (escape char: \).
    private static string EscapeLike(string input) => input
        .Replace("\\", "\\\\")
        .Replace("%", "\\%")
        .Replace("_", "\\_");
}
