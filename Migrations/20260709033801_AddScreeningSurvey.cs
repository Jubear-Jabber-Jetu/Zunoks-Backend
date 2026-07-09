using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZunoksBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddScreeningSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScreeningSurveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSurveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    YearsOfExperience = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoticePeriod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PortfolioLinks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExpectedSalary = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSubmissions_ScreeningSurveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "ScreeningSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSurveySections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PartLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSurveySections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSurveySections_ScreeningSurveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "ScreeningSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSectionScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSectionScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSectionScores_ScreeningSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ScreeningSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScreeningSectionScores_ScreeningSurveySections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ScreeningSurveySections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSurveyQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSurveyQuestions_ScreeningSurveySections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ScreeningSurveySections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningSurveyOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSurveyOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSurveyOptions_ScreeningSurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ScreeningSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: true),
                    TextAnswer = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ScoreEarned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_ScreeningSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ScreeningSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_ScreeningSurveyOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "ScreeningSurveyOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_ScreeningSurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ScreeningSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_OptionId",
                table: "ScreeningAnswers",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_QuestionId",
                table: "ScreeningAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_SubmissionId",
                table: "ScreeningAnswers",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSectionScores_SectionId",
                table: "ScreeningSectionScores",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSectionScores_SubmissionId",
                table: "ScreeningSectionScores",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSubmissions_SurveyId_Email",
                table: "ScreeningSubmissions",
                columns: new[] { "SurveyId", "Email" },
                unique: true,
                filter: "[IsCompleted] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSurveyOptions_QuestionId",
                table: "ScreeningSurveyOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSurveyQuestions_SectionId",
                table: "ScreeningSurveyQuestions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSurveySections_SurveyId",
                table: "ScreeningSurveySections",
                column: "SurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScreeningAnswers");

            migrationBuilder.DropTable(
                name: "ScreeningSectionScores");

            migrationBuilder.DropTable(
                name: "ScreeningSurveyOptions");

            migrationBuilder.DropTable(
                name: "ScreeningSubmissions");

            migrationBuilder.DropTable(
                name: "ScreeningSurveyQuestions");

            migrationBuilder.DropTable(
                name: "ScreeningSurveySections");

            migrationBuilder.DropTable(
                name: "ScreeningSurveys");
        }
    }
}
