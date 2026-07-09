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
    public DbSet<ScreeningSurvey> ScreeningSurveys { get; set; }
    public DbSet<ScreeningSurveySection> ScreeningSurveySections { get; set; }
    public DbSet<ScreeningSurveyQuestion> ScreeningSurveyQuestions { get; set; }
    public DbSet<ScreeningSurveyOption> ScreeningSurveyOptions { get; set; }
    public DbSet<ScreeningSubmission> ScreeningSubmissions { get; set; }
    public DbSet<ScreeningAnswer> ScreeningAnswers { get; set; }
    public DbSet<ScreeningSectionScore> ScreeningSectionScores { get; set; }

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

        modelBuilder.Entity<ScreeningSurveySection>()
            .HasOne(s => s.Survey)
            .WithMany(sv => sv.Sections)
            .HasForeignKey(s => s.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScreeningSurveyQuestion>()
            .HasOne(q => q.Section)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScreeningSurveyOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScreeningSubmission>()
            .HasOne(s => s.Survey)
            .WithMany(sv => sv.Submissions)
            .HasForeignKey(s => s.SurveyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ScreeningSubmission>()
            .HasIndex(s => new { s.SurveyId, s.Email })
            .IsUnique()
            .HasFilter("[IsCompleted] = 1");

        modelBuilder.Entity<ScreeningSubmission>()
            .Property(s => s.Percentage)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ScreeningAnswer>()
            .HasOne(a => a.Submission)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScreeningAnswer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ScreeningAnswer>()
            .HasOne(a => a.Option)
            .WithMany(o => o.Answers)
            .HasForeignKey(a => a.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ScreeningSectionScore>()
            .HasOne(ss => ss.Submission)
            .WithMany(s => s.SectionScores)
            .HasForeignKey(ss => ss.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScreeningSectionScore>()
            .HasOne(ss => ss.Section)
            .WithMany(s => s.SectionScores)
            .HasForeignKey(ss => ss.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
