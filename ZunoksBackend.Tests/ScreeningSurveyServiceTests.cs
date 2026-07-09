using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;
using ZunoksBackend.Services;

namespace ZunoksBackend.Tests;

public class ScreeningSurveyServiceTests
{
    private static ZunoksDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ZunoksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ZunoksDbContext(options);
    }

    private static async Task<ScreeningSurvey> SeedSurveyAsync(ZunoksDbContext context)
    {
        var survey = new ScreeningSurvey
        {
            Title = "Test Survey",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        var section = new ScreeningSurveySection
        {
            Title = "Technical",
            PartLabel = "Part 1",
            Category = "Technical",
            SortOrder = 1,
            IsScored = true,
        };

        var question = new ScreeningSurveyQuestion
        {
            Text = "SEO Skills",
            QuestionType = "Radio",
            SortOrder = 1,
            IsRequired = true,
            IsScored = true,
        };

        question.Options.Add(new ScreeningSurveyOption { Label = "Expert", Score = 5, SortOrder = 1 });
        question.Options.Add(new ScreeningSurveyOption { Label = "None", Score = 1, SortOrder = 2 });

        section.Questions.Add(question);
        survey.Sections.Add(section);
        context.ScreeningSurveys.Add(survey);
        await context.SaveChangesAsync();
        return survey;
    }

    [Fact]
    public async Task SubmitSurvey_CalculatesScoreCorrectly()
    {
        await using var context = CreateContext();
        var survey = await SeedSurveyAsync(context);
        var service = new ScreeningSurveyService(context);
        var optionId = survey.Sections.First().Questions.First().Options.First(o => o.Score == 5).Id;

        var submission = await service.SubmitSurveyAsync(new ScreeningSubmitDto
        {
            SurveyId = survey.Id,
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "01700000000",
            Answers = new List<ScreeningAnswerSubmitDto>
            {
                new() { QuestionId = survey.Sections.First().Questions.First().Id, OptionId = optionId },
            },
        });

        Assert.Equal(5, submission.TotalScore);
        Assert.Equal(5, submission.MaxScore);
        Assert.Equal(100m, submission.Percentage);
        Assert.True(submission.IsCompleted);
    }

    [Fact]
    public async Task SubmitSurvey_PreventsDuplicateEmail()
    {
        await using var context = CreateContext();
        var survey = await SeedSurveyAsync(context);
        var service = new ScreeningSurveyService(context);
        var questionId = survey.Sections.First().Questions.First().Id;
        var optionId = survey.Sections.First().Questions.First().Options.First().Id;

        var dto = new ScreeningSubmitDto
        {
            SurveyId = survey.Id,
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "01700000000",
            Answers = new List<ScreeningAnswerSubmitDto>
            {
                new() { QuestionId = questionId, OptionId = optionId },
            },
        };

        await service.SubmitSurveyAsync(dto);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitSurveyAsync(dto));
    }

    [Fact]
    public async Task SubmitSurvey_RequiresEmailAndPhone()
    {
        await using var context = CreateContext();
        var survey = await SeedSurveyAsync(context);
        var service = new ScreeningSurveyService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitSurveyAsync(new ScreeningSubmitDto
        {
            SurveyId = survey.Id,
            FullName = "Jane Doe",
            Email = "",
            Phone = "",
            Answers = new List<ScreeningAnswerSubmitDto>(),
        }));
    }
}
