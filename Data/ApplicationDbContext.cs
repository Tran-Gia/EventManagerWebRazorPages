using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManagerWebRazorPage.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public new DbSet<User> Users { get; set; }
        public DbSet<EventItem> EventItems { get; set; }
        public DbSet<EventDetail> EventDetails { get; set; }
        public DbSet<EventItemOption> EventItemOptions { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }
        public DbSet<UserRegistrationConfirmation> UserRegistrationConfirmations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(b =>
                {
                    b.ToTable("Users");
                    b.HasAlternateKey("UserInternalId");
                });
            builder.Entity<IdentityRole>(b =>
            {
                b.ToTable("Roles");
            }); 
            builder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RoleClaims");
            });
            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaims");
            });
            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UserLogins");
            });
            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UserTokens");
            });
            builder.Entity<UserRegistration>(b =>
            {
                b.HasAlternateKey(x => new
                {
                    x.UserId,
                    x.EventId,
                    x.ItemId
                });
            });
            builder.Entity<UserRegistrationConfirmation>(b =>
            {
                b.Property(p => p.ConfirmStatus).HasConversion<int>().IsRequired();
            });
        }
    }
}
