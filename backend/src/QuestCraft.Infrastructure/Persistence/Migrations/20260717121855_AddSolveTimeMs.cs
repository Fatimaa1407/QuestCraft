using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSolveTimeMs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SolveTimeMs",
                table: "ChallengeSubmissions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolveTimeMs",
                table: "ChallengeSubmissions");
        }
    }
}
