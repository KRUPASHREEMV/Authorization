using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoAuth.Domain.Entities;

namespace TodoAuth.Domain.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<EmailVerification> EmailVerifications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EmailVerification>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.OTPHash).IsRequired().HasMaxLength(128);
            b.Property(e => e.Email).IsRequired().HasMaxLength(256);
            b.Property(e => e.Purpose).IsRequired().HasMaxLength(50);
        });
    }
}
