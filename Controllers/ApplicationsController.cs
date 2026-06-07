using HomeBridge.Data;
using HomeBridge.Dtos;
using HomeBridge.Helpers;
using HomeBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Controllers;

// Everything a signed-in applicant does with applications: list, apply, withdraw.
[ApiController]
[Authorize]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET /api/applications/mine
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> Mine()
    {
        var userId = _userManager.GetUserId(User)!;
        var applications = await _db.Applications
            .Include(a => a.Listing)
            .Where(a => a.ApplicantId == userId)
            .OrderByDescending(a => a.DateSubmitted)
            .ToListAsync();

        return applications.Select(a => a.ToDto()).ToList();
    }

    // POST /api/applications
    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Apply(ApplyRequest request)
    {
        var listing = await _db.Listings.FirstOrDefaultAsync(l => l.Id == request.ListingId && l.IsAvailable);
        if (listing is null)
            return NotFound(new { message = "That home is no longer available." });

        var userId = _userManager.GetUserId(User)!;
        var alreadyApplied = await _db.Applications.AnyAsync(a =>
            a.ApplicantId == userId && a.ListingId == request.ListingId && a.Status != ApplicationStatus.Withdrawn);
        if (alreadyApplied)
            return Conflict(new { message = "You already have an active application for this home." });

        var application = new HousingApplication
        {
            ListingId = request.ListingId,
            ApplicantId = userId,
            Message = request.Message,
            HouseholdSize = request.HouseholdSize
        };
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();

        application.Listing = listing;
        return CreatedAtAction(nameof(Mine), application.ToDto());
    }

    // POST /api/applications/{id}/withdraw
    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == userId);
        if (application is null)
            return NotFound();

        if (application.Status is not (ApplicationStatus.Pending or ApplicationStatus.Approved))
            return BadRequest(new { message = "This application can no longer be withdrawn." });

        application.Status = ApplicationStatus.Withdrawn;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Your application has been withdrawn." });
    }
}
