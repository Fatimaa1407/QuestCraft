using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBattleConcurrencyGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParticipantCount",
                table: "Battles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Battles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill from the real participant count for any battles that already exist, rather than
            // leaving them all at the column default of 0.
            migrationBuilder.Sql(@"
                UPDATE b SET b.ParticipantCount = pc.Cnt
                FROM Battles b
                INNER JOIN (SELECT BattleId, COUNT(*) AS Cnt FROM BattleParticipants GROUP BY BattleId) pc ON pc.BattleId = b.Id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantCount",
                table: "Battles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Battles");
        }
    }
}
