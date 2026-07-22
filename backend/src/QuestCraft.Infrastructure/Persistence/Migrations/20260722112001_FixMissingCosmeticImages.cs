using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingCosmeticImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The original "Qızıl Çərçivə" (ProfileFrame) and "İlk Addım Nişanı" (Badge) seed rows
            // were created with ImageUrl = NULL, back when only Shop cards rendered these items
            // (an icon fallback was fine there). Now that FramedAvatar overlays the equipped
            // frame/badge image directly onto the avatar everywhere, a NULL image means the
            // cosmetic is invisible once equipped — so backfill real art for the two items still
            // missing one, guarded so this only ever touches rows that still have no image.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM MarketplaceItems WHERE Name = N'Qızıl Çərçivə' AND ImageUrl IS NULL)
BEGIN
    UPDATE MarketplaceItems
    SET ImageUrl = N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZmRlNjhhIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiNmNTllMGIiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiNiNDUzMDkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjY0IiByPSI1OCIgZmlsbD0ibm9uZSIgc3Ryb2tlPSJ1cmwoI2cpIiBzdHJva2Utd2lkdGg9IjciLz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjYiIHI9IjQiIGZpbGw9IiNmZGU2OGEiLz4KICA8Y2lyY2xlIGN4PSI2NCIgY3k9IjEyMiIgcj0iNCIgZmlsbD0iI2I0NTMwOSIvPgogIDxjaXJjbGUgY3g9IjYiIGN5PSI2NCIgcj0iNCIgZmlsbD0iI2Y1OWUwYiIvPgogIDxjaXJjbGUgY3g9IjEyMiIgY3k9IjY0IiByPSI0IiBmaWxsPSIjZjU5ZTBiIi8+Cjwvc3ZnPg=='
    WHERE Name = N'Qızıl Çərçivə' AND ImageUrl IS NULL;
END

IF EXISTS (SELECT 1 FROM MarketplaceItems WHERE Name = N'İlk Addım Nişanı' AND ImageUrl IS NULL)
BEGIN
    UPDATE MarketplaceItems
    SET ImageUrl = N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2NCIgaGVpZ2h0PSI2NCIgdmlld0JveD0iMCAwIDY0IDY0Ij4KICA8ZGVmcz48bGluZWFyR3JhZGllbnQgaWQ9ImciIHgxPSIwIiB5MT0iMCIgeDI9IjEiIHkyPSIxIj4KICAgIDxzdG9wIG9mZnNldD0iMCUiIHN0b3AtY29sb3I9IiMzNGQzOTkiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMwNTk2NjkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8Y2lyY2xlIGN4PSIzMiIgY3k9IjMyIiByPSIzMCIgZmlsbD0idXJsKCNnKSIvPgogIDx0ZXh0IHg9IjMyIiB5PSI0MiIgZm9udC1zaXplPSIyOCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+8J+RozwvdGV4dD4KPC9zdmc+'
    WHERE Name = N'İlk Addım Nişanı' AND ImageUrl IS NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE MarketplaceItems SET ImageUrl = NULL WHERE Name = N'Qızıl Çərçivə';
UPDATE MarketplaceItems SET ImageUrl = NULL WHERE Name = N'İlk Addım Nişanı';
");
        }
    }
}
