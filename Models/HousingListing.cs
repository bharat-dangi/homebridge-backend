using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBridge.Models;

// A single affordable-housing property advertised on the site.
public class HousingListing
{
    public int Id { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Street address")]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Suburb { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    [Display(Name = "State")]
    public string State { get; set; } = "NSW";

    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be 4 digits.")]
    public string Postcode { get; set; } = string.Empty;

    [Display(Name = "Property type")]
    public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

    [Range(0, 10, ErrorMessage = "Bedrooms must be between 0 and 10.")]
    public int Bedrooms { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Bathrooms must be between 1 and 10.")]
    public int Bathrooms { get; set; } = 1;

    [Range(0, 5000, ErrorMessage = "Weekly rent must be between $0 and $5000.")]
    [Column(TypeName = "decimal(10,2)")]
    [DataType(DataType.Currency)]
    [Display(Name = "Weekly rent")]
    public decimal WeeklyRent { get; set; }

    [Display(Name = "Available now")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Wheelchair accessible")]
    public bool IsAccessible { get; set; }

    [Display(Name = "Pets allowed")]
    public bool AcceptsPets { get; set; }

    [StringLength(400)]
    [DataType(DataType.Url)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Date listed")]
    public DateTime DateListed { get; set; } = DateTime.UtcNow;

    // Administrator who created the listing (Identity user id).
    public string? CreatedById { get; set; }

    public ICollection<HousingApplication> Applications { get; set; } = new List<HousingApplication>();

    // Convenience for views: "Suburb STATE 1234".
    [NotMapped]
    public string Location => $"{Suburb} {State} {Postcode}";
}
