using HomeBridge.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Data;

// Seeds roles, two demo accounts, and a starter set of listings on first run.
public static class DbSeeder
{
    public const string AdminRole = "Administrator";
    public const string ApplicantRole = "Applicant";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply any pending migrations so the schema exists before we touch data.
        await context.Database.MigrateAsync();

        foreach (var role in new[] { AdminRole, ApplicantRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, "admin@homebridge.org", "Admin#12345",
            "Site Administrator", AdminRole);

        var applicant = await EnsureUserAsync(userManager, "applicant@example.com", "Applicant#123",
            "Jordan Taylor", ApplicantRole, householdSize: 3);

        // Only seed sample stock once.
        if (!await context.Listings.AnyAsync())
        {
            context.Listings.AddRange(SampleListings(admin.Id));
            await context.SaveChangesAsync();
        }

        // Seed a few demo applications + a shortlist so the dashboards show realistic data.
        if (!await context.Applications.AnyAsync())
        {
            var listings = await context.Listings.OrderBy(l => l.Id).Take(6).ToListAsync();
            if (listings.Count >= 4)
            {
                var now = DateTime.UtcNow;
                context.Applications.AddRange(
                    new HousingApplication
                    {
                        ListingId = listings[1].Id, ApplicantId = applicant.Id, HouseholdSize = 3,
                        Message = "I am a single parent with two young children currently in temporary accommodation. This accessible, ground-floor home near schools and a clinic would give us much-needed stability.",
                        Status = ApplicationStatus.Pending, DateSubmitted = now.AddDays(-2)
                    },
                    new HousingApplication
                    {
                        ListingId = listings[3].Id, ApplicantId = applicant.Id, HouseholdSize = 1,
                        Message = "I am a full-time TAFE student on a low income looking for affordable, stable housing close to campus so I can finish my studies.",
                        Status = ApplicationStatus.Approved, DateSubmitted = now.AddDays(-9), DateReviewed = now.AddDays(-6)
                    },
                    new HousingApplication
                    {
                        ListingId = listings[4].Id, ApplicantId = applicant.Id, HouseholdSize = 3,
                        Message = "Our family is escaping an unsafe situation and needs short-term crisis accommodation while we get back on our feet. Any support would mean a great deal.",
                        Status = ApplicationStatus.Pending, DateSubmitted = now.AddHours(-20)
                    });

                context.SavedListings.AddRange(
                    new SavedListing { UserId = applicant.Id, ListingId = listings[0].Id, DateSaved = now.AddDays(-1) },
                    new SavedListing { UserId = applicant.Id, ListingId = listings[2].Id, DateSaved = now.AddHours(-5) });

                await context.SaveChangesAsync();
            }
        }
    }

    // Creates the user with the given role if it does not already exist; returns it either way.
    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string role, int householdSize = 1)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
            return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            HouseholdSize = householdSize
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static List<HousingListing> SampleListings(string adminId)
    {
        var listings = new List<HousingListing>
        {
            new()
            {
                Title = "Sunny 2-bedroom unit near transport",
                Description = "Bright, recently repainted unit a short walk from the train station, shops and a community health centre. Heating included; small balcony with morning sun.",
                AddressLine = "14/22 Station Street", Suburb = "Footscray", State = "VIC", Postcode = "3011",
                PropertyType = PropertyType.Unit, Bedrooms = 2, Bathrooms = 1, WeeklyRent = 310m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Ground-floor accessible apartment",
                Description = "Step-free entry, widened doorways and a roll-in shower. Close to bus routes and a medical clinic. Suitable for tenants with mobility needs.",
                AddressLine = "3 Marlborough Court", Suburb = "Blacktown", State = "NSW", Postcode = "2148",
                PropertyType = PropertyType.Apartment, Bedrooms = 1, Bathrooms = 1, WeeklyRent = 280m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "3-bedroom family house with yard",
                Description = "Spacious weatherboard home with a fenced backyard, close to two primary schools and a public library. Long-term tenancy preferred for families.",
                AddressLine = "8 Wattle Grove", Suburb = "Elizabeth", State = "SA", Postcode = "5112",
                PropertyType = PropertyType.House, Bedrooms = 3, Bathrooms = 1, WeeklyRent = 360m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1568605114967-8130f3a36994?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Studio for single occupant",
                Description = "Compact, low-cost studio with a kitchenette and good natural light. Walking distance to TAFE and free community meals on weekdays.",
                AddressLine = "501/12 Hope Lane", Suburb = "Fortitude Valley", State = "QLD", Postcode = "4006",
                PropertyType = PropertyType.Studio, Bedrooms = 0, Bathrooms = 1, WeeklyRent = 220m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1554995207-c18c203602cb?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Emergency crisis accommodation",
                Description = "Short-stay crisis room with shared facilities and on-site support workers. Priority for people escaping family violence or facing homelessness.",
                AddressLine = "27 Safehaven Road", Suburb = "Dandenong", State = "VIC", Postcode = "3175",
                PropertyType = PropertyType.Emergency, Bedrooms = 1, Bathrooms = 1, WeeklyRent = 0m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Affordable townhouse, recently renovated",
                Description = "Two-storey townhouse with a new kitchen and reverse-cycle air conditioning. Quiet street near parks and a bulk-billing GP.",
                AddressLine = "5/9 Rosella Way", Suburb = "Mirrabooka", State = "WA", Postcode = "6061",
                PropertyType = PropertyType.Townhouse, Bedrooms = 2, Bathrooms = 2, WeeklyRent = 340m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1576941089067-2de3c901e126?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Spacious 4-bedroom family home",
                Description = "Large brick home with two living areas and a covered carport, on a quiet cul-de-sac near schools and a community centre. Ideal for a bigger household.",
                AddressLine = "12 Banksia Crescent", Suburb = "Logan Central", State = "QLD", Postcode = "4114",
                PropertyType = PropertyType.House, Bedrooms = 4, Bathrooms = 2, WeeklyRent = 395m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Accessible 2-bedroom unit, lift access",
                Description = "First-floor unit in a small block with a lift, grab rails in the bathroom and a level entry. Close to a hospital and frequent bus services.",
                AddressLine = "7/40 Park Terrace", Suburb = "Salisbury", State = "SA", Postcode = "5108",
                PropertyType = PropertyType.Unit, Bedrooms = 2, Bathrooms = 1, WeeklyRent = 300m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1502005229762-cf1b2da7c5d6?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Quiet 1-bedroom apartment",
                Description = "Low-maintenance apartment with secure parking and an on-site laundry. A short walk to the shopping centre and a free weekly community lunch.",
                AddressLine = "204/15 Garden Avenue", Suburb = "Broadmeadows", State = "VIC", Postcode = "3047",
                PropertyType = PropertyType.Apartment, Bedrooms = 1, Bathrooms = 1, WeeklyRent = 260m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "3-bedroom townhouse, pet friendly",
                Description = "Modern townhouse with a small courtyard and room for a pet. Walking distance to a primary school, parkland and the train line.",
                AddressLine = "9/3 Acacia Street", Suburb = "Mount Druitt", State = "NSW", Postcode = "2770",
                PropertyType = PropertyType.Townhouse, Bedrooms = 3, Bathrooms = 2, WeeklyRent = 370m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1583608205776-bfd35f0d9f83?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Accessible 3-bedroom home near the bay",
                Description = "Single-level home with wide hallways, a step-free shower and an enclosed yard. Close to the foreshore, a medical centre and TAFE.",
                AddressLine = "21 Beach Road", Suburb = "Frankston", State = "VIC", Postcode = "3199",
                PropertyType = PropertyType.House, Bedrooms = 3, Bathrooms = 2, WeeklyRent = 410m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1430285561322-7808604715df?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Budget studio close to the city",
                Description = "Affordable studio with a compact kitchenette, ideal for a single person or student. Frequent buses to the CBD and a library next door.",
                AddressLine = "8/61 Henry Street", Suburb = "Penrith", State = "NSW", Postcode = "2750",
                PropertyType = PropertyType.Studio, Bedrooms = 0, Bathrooms = 1, WeeklyRent = 240m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Shared room in a supported house",
                Description = "A single room in a friendly shared house with communal kitchen, bathroom and a visiting support worker. Suited to people rebuilding stability.",
                AddressLine = "33 Kingfisher Drive", Suburb = "Woodridge", State = "QLD", Postcode = "4114",
                PropertyType = PropertyType.SharedRoom, Bedrooms = 1, Bathrooms = 1, WeeklyRent = 160m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1585412727339-54e4bae3bbf9?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Family home with large backyard",
                Description = "Three-bedroom home with a big, fully fenced yard and a garden shed. Both pets and children welcome; close to schools and a bulk-billing clinic.",
                AddressLine = "4 Carramar Court", Suburb = "Davoren Park", State = "SA", Postcode = "5113",
                PropertyType = PropertyType.House, Bedrooms = 3, Bathrooms = 1, WeeklyRent = 330m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Modern 2-bedroom townhouse",
                Description = "Near-new townhouse with an open-plan living area and split-system cooling. Quiet estate close to public transport and a shopping precinct.",
                AddressLine = "11/2 Wirraway Way", Suburb = "Werribee", State = "VIC", Postcode = "3030",
                PropertyType = PropertyType.Townhouse, Bedrooms = 2, Bathrooms = 2, WeeklyRent = 350m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Accessible 2-bedroom apartment",
                Description = "Ground-floor apartment with level access, a wet-room bathroom and a quiet aspect. Walking distance to the station, library and medical centre.",
                AddressLine = "1/88 Macquarie Street", Suburb = "Liverpool", State = "NSW", Postcode = "2170",
                PropertyType = PropertyType.Apartment, Bedrooms = 2, Bathrooms = 1, WeeklyRent = 320m,
                IsAvailable = true, IsAccessible = true, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1567496898669-ee935f5f647a?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "Cosy 1-bedroom unit",
                Description = "Tidy unit with a separate kitchen and a small shared garden. Close to a multicultural community hub, shops and frequent trains.",
                AddressLine = "6/19 John Street", Suburb = "Cabramatta", State = "NSW", Postcode = "2166",
                PropertyType = PropertyType.Unit, Bedrooms = 1, Bathrooms = 1, WeeklyRent = 270m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = false,
                ImageUrl = "https://images.unsplash.com/photo-1502005097973-6a7082348e28?auto=format&fit=crop&w=1200&q=60"
            },
            new()
            {
                Title = "2-bedroom house, pets welcome",
                Description = "Comfortable home with a verandah and a fenced yard for a pet. Close to the town centre, schools and a community health service.",
                AddressLine = "17 Limestone Street", Suburb = "Ipswich", State = "QLD", Postcode = "4305",
                PropertyType = PropertyType.House, Bedrooms = 2, Bathrooms = 1, WeeklyRent = 300m,
                IsAvailable = true, IsAccessible = false, AcceptsPets = true,
                ImageUrl = "https://images.unsplash.com/photo-1449844908441-8829872d2607?auto=format&fit=crop&w=1200&q=60"
            }
        };

        // Stagger the listing dates so "recently listed" ordering and dates look realistic.
        var today = DateTime.UtcNow;
        for (var i = 0; i < listings.Count; i++)
        {
            listings[i].CreatedById = adminId;
            listings[i].DateListed = today.AddDays(-i);
        }

        return listings;
    }
}
