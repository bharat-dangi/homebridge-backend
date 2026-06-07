using System.ComponentModel.DataAnnotations;
using HomeBridge.Models;

namespace HomeBridge.Dtos;

// Full representation of a listing returned to clients.
public class ListingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public string PropertyTypeLabel { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal WeeklyRent { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsAccessible { get; set; }
    public bool AcceptsPets { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime DateListed { get; set; }
}

// Listing plus per-user state, for the details page.
public class ListingDetailsDto
{
    public ListingDto Listing { get; set; } = null!;
    public bool IsSaved { get; set; }
    public bool HasApplied { get; set; }
}

// Search/filter inputs bound from the query string.
public class ListingSearchRequest
{
    public string? Keyword { get; set; }
    public string? Suburb { get; set; }

    [Range(0, 5000)]
    public decimal? MaxRent { get; set; }

    public int? MinBedrooms { get; set; }
    public PropertyType? PropertyType { get; set; }
    public bool AccessibleOnly { get; set; }
    public bool PetsOnly { get; set; }
    public ListingSort Sort { get; set; } = ListingSort.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;
}

// How the browse endpoint sorts results (mirrors the original MVC enum).
public enum ListingSort
{
    Newest,
    RentLowToHigh,
    RentHighToLow,
    BedroomsDesc
}

// Generic paged envelope returned by list endpoints.
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalResults / (double)PageSize) : 0;
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    // Ids the signed-in user has shortlisted (empty when anonymous), for the save toggle.
    public IReadOnlyList<int> SavedListingIds { get; set; } = new List<int>();
}

// Admin create/update payload — same validation rules as the domain entity.
public class ListingInput
{
    [Required]
    [StringLength(120, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Suburb { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string State { get; set; } = "NSW";

    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Postcode must be 4 digits.")]
    public string Postcode { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

    [Range(0, 10, ErrorMessage = "Bedrooms must be between 0 and 10.")]
    public int Bedrooms { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Bathrooms must be between 1 and 10.")]
    public int Bathrooms { get; set; } = 1;

    [Range(0, 5000, ErrorMessage = "Weekly rent must be between $0 and $5000.")]
    public decimal WeeklyRent { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsAccessible { get; set; }
    public bool AcceptsPets { get; set; }

    [StringLength(400)]
    [Url]
    public string? ImageUrl { get; set; }
}
