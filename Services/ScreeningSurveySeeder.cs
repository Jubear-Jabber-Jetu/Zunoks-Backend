using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public class ScreeningSurveySeeder
{
    private readonly ZunoksDbContext _context;
    private readonly ILogger<ScreeningSurveySeeder> _logger;

    private static readonly string[] StandardOptions =
    {
        "Expert", "Advanced", "Intermediate", "Basic", "None",
    };

    private static readonly int[] StandardScores = { 5, 4, 3, 2, 1 };

    public ScreeningSurveySeeder(ZunoksDbContext context, ILogger<ScreeningSurveySeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.ScreeningSurveys.AnyAsync())
            return;

        var survey = new ScreeningSurvey
        {
            Title = "Digital Marketing Manager Screening Survey",
            Description = "Assess core technical skills, leadership capabilities, and tool proficiency for Digital Marketing Manager candidates.",
            IsActive = true,
            CreatedAt = BangladeshTime.Now(),
        };

        var sections = new (string Part, string Category, bool IsScored, string[] Questions)[]
        {
            ("Part 1 - Core Technical Skills", "Technical", true, new[]
            {
                "SEO & Local SEO",
                "Google Business Profile",
                "Website Management",
                "Paid Advertising",
                "Social Media & Content Marketing",
                "Analytics & Reporting",
            }),
            ("Part 2 - Management & Strategic Leadership", "Leadership", true, new[]
            {
                "Team Leadership",
                "Project Management",
                "Client Communication",
                "Strategic Thinking",
            }),
            ("Part 3 - Tool Proficiency", "Tool", true, new[]
            {
                "Google Analytics",
                "WordPress",
                "SEMRush / Ahrefs",
                "Canva",
                "Brevo",
            }),
            ("Part 4 - General Information", "General", false, new[]
            {
                "Notice Period",
                "Portfolio Links",
                "Expected Salary",
            }),
        };

        var sectionOrder = 1;
        foreach (var (part, category, isScored, questions) in sections)
        {
            var section = new ScreeningSurveySection
            {
                Title = part,
                PartLabel = part.Split(" - ").FirstOrDefault() ?? part,
                Category = category,
                SortOrder = sectionOrder++,
                IsScored = isScored,
            };

            var questionOrder = 1;
            foreach (var questionText in questions)
            {
                var isText = !isScored;
                var question = new ScreeningSurveyQuestion
                {
                    Text = questionText,
                    QuestionType = isText ? "Text" : "Radio",
                    SortOrder = questionOrder++,
                    IsRequired = true,
                    IsScored = isScored,
                };

                if (!isText)
                {
                    for (var i = 0; i < StandardOptions.Length; i++)
                    {
                        question.Options.Add(new ScreeningSurveyOption
                        {
                            Label = StandardOptions[i],
                            Score = StandardScores[i],
                            SortOrder = i + 1,
                        });
                    }
                }

                section.Questions.Add(question);
            }

            survey.Sections.Add(section);
        }

        _context.ScreeningSurveys.Add(survey);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded Digital Marketing Manager Screening Survey (Id={Id})", survey.Id);
    }
}
