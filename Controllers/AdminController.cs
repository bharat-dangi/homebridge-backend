using HomeBridge.Data;
using HomeBridge.Dtos;
using HomeBridge.Helpers;
using HomeBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Controllers;

// Staff-only area (dashboard, listing CRUD, application review); locked to the Administrator role.
[ApiController]
[Authorize(Roles = DbSeeder.AdminRole)]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ---- Dashboard ----

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> Dashboard()
    {
        var applicants = await _userManager.GetUsersInRoleAsync(DbSeeder.ApplicantRole);

        var recent = await _db.Applications
            .Include(a => a.Listing)
            .Include(a => a.Applicant)
            .OrderByDescending(a => a.DateSubmitted)
            .Take(5)
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalListings = await _db.Listings.CountAsync(),
            AvailableListings = await _db.Listings.CountAsync(l => l.IsAvailable),
            TotalApplications = await _db.Applications.CountAsync(),
            PendingApplications = await _db.Applications.CountAsync(a => a.Status == ApplicationStatus.Pending),
            ApprovedApplications = await _db.Applications.CountAsync(a => a.Status == ApplicationStatus.Approved),
            RegisteredApplicants = applicants.Count,
            RecentApplications = recent.Select(a => a.ToAdminDto()).ToList()
        };
    }

    // ---- Listing management ----

    // GET /api/admin/listings  → all listings (including hidden), newest first.
    [HttpGet("listings")]
    public async Task<ActionResult<IEnumerable<ListingDto>>> Listings()
    {
        var listings = await _db.Listings.OrderByDescending(l => l.DateListed).ToListAsync();
        return listings.Select(l => l.ToDto()).ToList();
    }

    [HttpGet("listings/{id:int}")]
    public async Task<ActionResult<ListingDto>> GetListing(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        return listing is null ? NotFound() : listing.ToDto();
    }

    [HttpPost("listings")]
    public async Task<ActionResult<ListingDto>> Create(ListingInput input)
    {
        var listing = new HousingListing
        {
            DateListed = DateTime.UtcNow,
            CreatedById = _userManager.GetUserId(User)
        };
        listing.Apply(input);

        _db.Listings.Add(listing);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing.ToDto());
    }

    [HttpPut("listings/{id:int}")]
    public async Task<ActionResult<ListingDto>> Update(int id, ListingInput input)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.Apply(input);
        await _db.SaveChangesAsync();
        return listing.ToDto();
    }

    [HttpPost("listings/{id:int}/toggle-availability")]
    public async Task<ActionResult<ListingDto>> ToggleAvailability(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        listing.IsAvailable = !listing.IsAvailable;
        await _db.SaveChangesAsync();
        return listing.ToDto();
    }

    [HttpDelete("listings/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null)
            return NotFound();

        // Cascade delete removes any applications tied to this listing.
        _db.Listings.Remove(listing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Application review ----

    // GET /api/admin/applications?status=Pending
    [HttpGet("applications")]
    public async Task<ActionResult<IEnumerable<AdminApplicationDto>>> Applications([FromQuery] ApplicationStatus? status)
    {
        var query = _db.Applications
            .Include(a => a.Listing)
            .Include(a => a.Applicant)
            .AsQueryable();

        if (status is not null)
            query = query.Where(a => a.Status == status);

        var applications = await query.OrderByDescending(a => a.DateSubmitted).ToListAsync();
        return applications.Select(a => a.ToAdminDto()).ToList();
    }

    // PUT /api/admin/applications/{id}/status
    [HttpPut("applications/{id:int}/status")]
    public async Task<ActionResult<AdminApplicationDto>> SetStatus(int id, SetStatusRequest request)
    {
        var application = await _db.Applications
            .Include(a => a.Listing)
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (application is null)
            return NotFound();

        application.Status = request.Status;
        application.DateReviewed = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return application.ToAdminDto();
    }
}
