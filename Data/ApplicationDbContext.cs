using HomeBridge.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeBridge.Data;

// EF Core context: Identity tables plus the three HomeBridge domain tables.
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<HousingListing> Listings => Set<HousingListing>();
    public DbSet<HousingApplication> Applications => Set<HousingApplication>();
    public DbSet<SavedListing> SavedListings => Set<SavedListing>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // An application belongs to one listing; remove applications if the listing is deleted.
        builder.Entity<HousingApplication>()
            .HasOne(a => a.Listing)
            .WithMany(l => l.Applications)
            .HasForeignKey(a => a.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Keep the applicant's account intact if a listing is removed elsewhere.
        builder.Entity<HousingApplication>()
            .HasOne(a => a.Applicant)
            .WithMany(u => u.Applications)
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can shortlist a given listing only once.
        builder.Entity<SavedListing>()
            .HasIndex(s => new { s.UserId, s.ListingId })
            .IsUnique();

        builder.Entity<SavedListing>()
            .HasOne(s => s.Listing)
            .WithMany()
            .HasForeignKey(s => s.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SavedListing>()
            .HasOne(s => s.User)
            .WithMany(u => u.SavedListings)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
