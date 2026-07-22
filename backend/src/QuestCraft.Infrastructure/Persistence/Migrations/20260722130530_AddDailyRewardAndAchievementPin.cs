using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyRewardAndAchievementPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastLoginRewardClaimedAt",
                table: "UserProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "UserAchievements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginRewardClaimedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "UserAchievements");
        }
    }
}
