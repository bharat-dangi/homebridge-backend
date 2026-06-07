using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HomeBridge.Models;

// Extends the built-in Identity user with the profile fields HomeBridge needs.
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Household size must be between 1 and 20.")]
    [Display(Name = "Household size")]
    public int HouseholdSize { get; set; } = 1;

    [Display(Name = "Joined")]
    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;

    // Applications and shortlist entries owned by this user.
    public ICollection<HousingApplication> Applications { get; set; } = new List<HousingApplication>();
    public ICollection<SavedListing> SavedListings { get; set; } = new List<SavedListing>();
}
