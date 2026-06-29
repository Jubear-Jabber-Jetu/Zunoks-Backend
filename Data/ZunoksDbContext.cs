using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Models;

namespace ZunoksBackend.Data;

public class ZunoksDbContext : IdentityDbContext<ApplicationUser>
{
    public ZunoksDbContext(DbContextOptions<ZunoksDbContext> options) : base(options)
    {
    }

    public DbSet<ZunoksSubmission> ZunoksSubmissions { get; set; }
    public DbSet<ZunoksResponse> ZunoksResponses { get; set; }
    public DbSet<SelectedModule> SelectedModules { get; set; }
    public DbSet<ReComLead> ReComLeads { get; set; }
    public DbSet<ReComLeadService> ReComLeadServices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ZunoksResponse>()
            .HasOne(r => r.ZunoksSubmission)
            .WithMany(s => s.Responses)
            .HasForeignKey(r => r.ZunoksSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SelectedModule>()
            .HasOne(m => m.ZunoksSubmission)
            .WithMany(s => s.SelectedModules)
            .HasForeignKey(m => m.ZunoksSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReComLeadService>()
            .HasOne(s => s.ReComLead)
            .WithMany(l => l.Services)
            .HasForeignKey(s => s.ReComLeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
