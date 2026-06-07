namespace HomeBridge.Models;

// Join row backing the "shortlist" feature — a user bookmarking a listing.
public class SavedListing
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int ListingId { get; set; }
    public HousingListing? Listing { get; set; }

    public DateTime DateSaved { get; set; } = DateTime.UtcNow;
}
