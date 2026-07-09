using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoinEarned",
                table: "ChallengeSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "XpEarned",
                table: "ChallengeSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoinEarned",
                table: "ChallengeSubmissions");

            migrationBuilder.DropColumn(
                name: "XpEarned",
                table: "ChallengeSubmissions");
        }
    }
}
