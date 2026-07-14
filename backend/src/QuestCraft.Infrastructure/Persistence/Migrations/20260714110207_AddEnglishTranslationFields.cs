using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishTranslationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationEn",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextEn",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextEn",
                table: "QuestionOptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "MarketplaceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "MarketplaceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "DailyQuestTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "DailyQuestTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstraintsEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HintEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InputFormatEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputFormatEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Achievements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ExplanationEn",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TextEn",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TextEn",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "DailyQuestTemplates");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "DailyQuestTemplates");

            migrationBuilder.DropColumn(
                name: "ConstraintsEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "HintEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "InputFormatEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OutputFormatEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Achievements");
        }
    }
}
