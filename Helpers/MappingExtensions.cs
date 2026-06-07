using HomeBridge.Dtos;
using HomeBridge.Models;

namespace HomeBridge.Helpers;

// Converts domain entities into the DTO shapes returned by the API.
public static class MappingExtensions
{
    public static ListingDto ToDto(this HousingListing l) => new()
    {
        Id = l.Id,
        Title = l.Title,
        Description = l.Description,
        AddressLine = l.AddressLine,
        Suburb = l.Suburb,
        State = l.State,
        Postcode = l.Postcode,
        Location = l.Location,
        PropertyType = l.PropertyType,
        PropertyTypeLabel = l.PropertyType.Humanize(),
        Bedrooms = l.Bedrooms,
        Bathrooms = l.Bathrooms,
        WeeklyRent = l.WeeklyRent,
        IsAvailable = l.IsAvailable,
        IsAccessible = l.IsAccessible,
        AcceptsPets = l.AcceptsPets,
        ImageUrl = l.ImageUrl,
        DateListed = l.DateListed
    };

    public static ApplicationDto ToDto(this HousingApplication a) => new()
    {
        Id = a.Id,
        ListingId = a.ListingId,
        ListingTitle = a.Listing?.Title ?? "(listing removed)",
        ListingLocation = a.Listing?.Location ?? string.Empty,
        ListingImageUrl = a.Listing?.ImageUrl,
        Message = a.Message,
        HouseholdSize = a.HouseholdSize,
        Status = a.Status,
        StatusLabel = a.Status.Humanize(),
        DateSubmitted = a.DateSubmitted,
        DateReviewed = a.DateReviewed,
        CanWithdraw = a.Status is ApplicationStatus.Pending or ApplicationStatus.Approved
    };

    public static AdminApplicationDto ToAdminDto(this HousingApplication a) => new()
    {
        Id = a.Id,
        ListingId = a.ListingId,
        ListingTitle = a.Listing?.Title ?? "(listing removed)",
        ListingLocation = a.Listing?.Location ?? string.Empty,
        ListingImageUrl = a.Listing?.ImageUrl,
        Message = a.Message,
        HouseholdSize = a.HouseholdSize,
        Status = a.Status,
        StatusLabel = a.Status.Humanize(),
        DateSubmitted = a.DateSubmitted,
        DateReviewed = a.DateReviewed,
        CanWithdraw = a.Status is ApplicationStatus.Pending or ApplicationStatus.Approved,
        ApplicantName = a.Applicant?.FullName ?? "(unknown)",
        ApplicantEmail = a.Applicant?.Email ?? string.Empty,
        ApplicantPhone = a.Applicant?.PhoneNumber
    };

    // Copies editable fields from an admin payload onto a listing entity.
    public static void Apply(this HousingListing l, ListingInput input)
    {
        l.Title = input.Title;
        l.Description = input.Description;
        l.AddressLine = input.AddressLine;
        l.Suburb = input.Suburb;
        l.State = input.State;
        l.Postcode = input.Postcode;
        l.PropertyType = input.PropertyType;
        l.Bedrooms = input.Bedrooms;
        l.Bathrooms = input.Bathrooms;
        l.WeeklyRent = input.WeeklyRent;
        l.IsAvailable = input.IsAvailable;
        l.IsAccessible = input.IsAccessible;
        l.AcceptsPets = input.AcceptsPets;
        l.ImageUrl = input.ImageUrl;
    }
}
