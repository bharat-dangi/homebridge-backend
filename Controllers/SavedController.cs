using HomeBridge.Data;
using HomeBridge.Dtos;
using HomeBridge.Helpers;
using HomeBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Controllers;

// The signed-in user's shortlist of saved homes.
[ApiController]
[Authorize]
[Route("api/saved")]
public class SavedController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SavedController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET /api/saved  → the user's shortlisted listings, newest first.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListingDto>>> Saved()
    {
        var userId = _userManager.GetUserId(User)!;
        var listings = await _db.SavedListings
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateSaved)
            .Select(s => s.Listing!)
            .ToListAsync();

        return listings.Select(l => l.ToDto()).ToList();
    }

    // POST /api/saved/{listingId}/toggle  → add/remove from shortlist; returns the new state.
    [HttpPost("{listingId:int}/toggle")]
    public async Task<ActionResult<object>> Toggle(int listingId)
    {
        var listingExists = await _db.Listings.AnyAsync(l => l.Id == listingId);
        if (!listingExists)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var saved = await _db.SavedListings.FirstOrDefaultAsync(s => s.UserId == userId && s.ListingId == listingId);

        bool isSaved;
        if (saved is null)
        {
            _db.SavedListings.Add(new SavedListing { UserId = userId, ListingId = listingId });
            isSaved = true;
        }
        else
        {
            _db.SavedListings.Remove(saved);
            isSaved = false;
        }
        await _db.SaveChangesAsync();

        return Ok(new { listingId, isSaved });
    }
}
