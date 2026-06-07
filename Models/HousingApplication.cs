using System.ComponentModel.DataAnnotations;

namespace HomeBridge.Models;

// A tenancy application submitted by an applicant against one listing.
public class HousingApplication
{
    public int Id { get; set; }

    [Required]
    public int ListingId { get; set; }
    public HousingListing? Listing { get; set; }

    // Owning applicant (Identity user id).
    [Required]
    public string ApplicantId { get; set; } = string.Empty;
    public ApplicationUser? Applicant { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "Please write between 20 and 1000 characters about your situation.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Your message")]
    public string Message { get; set; } = string.Empty;

    [Range(1, 20)]
    [Display(Name = "Household size")]
    public int HouseholdSize { get; set; } = 1;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [Display(Name = "Submitted")]
    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

    [Display(Name = "Reviewed")]
    public DateTime? DateReviewed { get; set; }
}
