using System.ComponentModel.DataAnnotations;

namespace HomeBridge.Dtos;

// Landing-page headline figures plus a few featured homes.
public class HomeStatsDto
{
    public int AvailableCount { get; set; }
    public int SuburbCount { get; set; }
    public int AccessibleCount { get; set; }
    public IReadOnlyList<ListingDto> FeaturedListings { get; set; } = new List<ListingDto>();
}

// Snapshot for the admin dashboard.
public class AdminDashboardDto
{
    public int TotalListings { get; set; }
    public int AvailableListings { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RegisteredApplicants { get; set; }
    public IReadOnlyList<AdminApplicationDto> RecentApplications { get; set; } = new List<AdminApplicationDto>();
}

// Contact form — validated server-side, not persisted (mirrors the original).
public class ContactRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}

// A selectable enum option (value + human label) for dropdowns on the client.
public class EnumOptionDto
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
