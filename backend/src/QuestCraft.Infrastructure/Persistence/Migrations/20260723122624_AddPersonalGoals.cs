using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyGoalBattles",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyGoalChallenges",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyGoalXp",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDailyPuzzle",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChallengeComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsSpoiler = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeComments_ChallengeComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ChallengeComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChallengeComments_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChallengeComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_ChallengeId_CreatedAt",
                table: "ChallengeComments",
                columns: new[] { "ChallengeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_ParentCommentId",
                table: "ChallengeComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeComments_UserId",
                table: "ChallengeComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeComments");

            migrationBuilder.DropColumn(
                name: "DailyGoalBattles",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DailyGoalChallenges",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DailyGoalXp",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsDailyPuzzle",
                table: "Challenges");
        }
    }
}
