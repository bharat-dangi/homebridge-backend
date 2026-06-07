using System.ComponentModel.DataAnnotations;
using HomeBridge.Models;

namespace HomeBridge.Dtos;

// Payload an applicant posts to apply for a listing.
public class ApplyRequest
{
    [Required]
    public int ListingId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "Please write between 20 and 1000 characters about your situation.")]
    public string Message { get; set; } = string.Empty;

    [Range(1, 20)]
    public int HouseholdSize { get; set; } = 1;
}

// An application as seen by its owner (the applicant).
public class ApplicationDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string ListingLocation { get; set; } = string.Empty;
    public string? ListingImageUrl { get; set; }
    public string Message { get; set; } = string.Empty;
    public int HouseholdSize { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime DateSubmitted { get; set; }
    public DateTime? DateReviewed { get; set; }
    // True while the application can still be withdrawn (Pending or Approved).
    public bool CanWithdraw { get; set; }
}

// An application as seen by an administrator (adds applicant details).
public class AdminApplicationDto : ApplicationDto
{
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string? ApplicantPhone { get; set; }
}

// Admin sets an application's outcome.
public class SetStatusRequest
{
    [Required]
    public ApplicationStatus Status { get; set; }
}
