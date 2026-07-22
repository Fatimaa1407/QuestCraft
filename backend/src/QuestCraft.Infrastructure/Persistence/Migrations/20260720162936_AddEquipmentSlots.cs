using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquippedAvatarId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquippedBadgeId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquippedBannerId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EquippedAvatarId",
                table: "UserProfiles",
                column: "EquippedAvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EquippedBadgeId",
                table: "UserProfiles",
                column: "EquippedBadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_EquippedBannerId",
                table: "UserProfiles",
                column: "EquippedBannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedAvatarId",
                table: "UserProfiles",
                column: "EquippedAvatarId",
                principalTable: "MarketplaceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedBadgeId",
                table: "UserProfiles",
                column: "EquippedBadgeId",
                principalTable: "MarketplaceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedBannerId",
                table: "UserProfiles",
                column: "EquippedBannerId",
                principalTable: "MarketplaceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Seed: new "ProfileBanner" item type, plus real purchasable items so every one of the
            // 6 cosmetic types (Avatar, ProfileFrame, ProfileBanner, Title, Badge, Theme) has something
            // to actually buy/equip. The original 7 types/5 items were seeded at runtime (not via a
            // migration), so their Ids aren't known here — types/theme items below are looked up by
            // Name the same way ApplicationDbContextSeeder does, to stay environment-independent.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM MarketplaceItemTypes WHERE Name = N'ProfileBanner')
BEGIN
    INSERT INTO MarketplaceItemTypes (Name, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'ProfileBanner', GETUTCDATE(), NULL, 0);
END
");

            migrationBuilder.Sql(@"
DECLARE @avatarTypeId INT = (SELECT Id FROM MarketplaceItemTypes WHERE Name = N'Avatar');
DECLARE @bannerTypeId INT = (SELECT Id FROM MarketplaceItemTypes WHERE Name = N'ProfileBanner');
DECLARE @themeTypeId INT = (SELECT Id FROM MarketplaceItemTypes WHERE Name = N'Theme');

IF @avatarTypeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MarketplaceItems WHERE Name = N'Robot Avatarı')
BEGIN
    INSERT INTO MarketplaceItems (Name, NameEn, Description, DescriptionEn, ItemTypeId, Price, ImageUrl, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES
    (N'Robot Avatarı', N'Robot Avatar', N'Futuristik robot avatarı.', N'A futuristic robot avatar.', @avatarTypeId, 60,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjM2I4MmY2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjMDZiNmQ0Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCfpJY8L3RleHQ+Cjwvc3ZnPg==',
        1, GETUTCDATE(), NULL, 0),
    (N'Astronavt Avatarı', N'Astronaut Avatar', N'Kosmosu fəth edən astronavt avatarı.', N'A space-conquering astronaut avatar.', @avatarTypeId, 90,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjOGI1Y2Y2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjZWM0ODk5Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCfmoA8L3RleHQ+Cjwvc3ZnPg==',
        1, GETUTCDATE(), NULL, 0),
    (N'Alov Avatarı', N'Flame Avatar', N'Alovlu bir avatar.', N'A fiery avatar.', @avatarTypeId, 90,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgiIGhlaWdodD0iMTI4IiB2aWV3Qm94PSIwIDAgMTI4IDEyOCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMSI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZjk3MzE2Ii8+PHN0b3Agb2Zmc2V0PSIxMDAlIiBzdG9wLWNvbG9yPSIjZWY0NDQ0Ii8+CiAgPC9saW5lYXJHcmFkaWVudD48L2RlZnM+CiAgPGNpcmNsZSBjeD0iNjQiIGN5PSI2NCIgcj0iNjQiIGZpbGw9InVybCgjZykiLz4KICA8dGV4dCB4PSI2NCIgeT0iODIiIGZvbnQtc2l6ZT0iNTYiIHRleHQtYW5jaG9yPSJtaWRkbGUiPvCflKU8L3RleHQ+Cjwvc3ZnPg==',
        1, GETUTCDATE(), NULL, 0);
END

IF @bannerTypeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MarketplaceItems WHERE Name = N'Gecə Səması Banneri')
BEGIN
    INSERT INTO MarketplaceItems (Name, NameEn, Description, DescriptionEn, ItemTypeId, Price, ImageUrl, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES
    (N'Gecə Səması Banneri', N'Night Sky Banner', N'Profil başlığını ulduzlu gecə rənginə boyayır.', N'Paints your profile header in starry-night colors.', @bannerTypeId, 70,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjMGYxNzJhIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiMzMTJlODEiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMxZTFiNGIiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=',
        1, GETUTCDATE(), NULL, 0),
    (N'Gündoğuşu Banneri', N'Sunrise Banner', N'İlıq narıncı-çəhrayı gündoğuşu rəngləri.', N'Warm orange-pink sunrise colors.', @bannerTypeId, 70,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjZjk3MzE2Ii8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiNmNDNmNWUiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiNlYzQ4OTkiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=',
        1, GETUTCDATE(), NULL, 0),
    (N'Meşə Banneri', N'Forest Banner', N'Sərin, canlı meşə yaşılı tonları.', N'Cool, vivid forest-green tones.', @bannerTypeId, 70,
        N'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4MDAiIGhlaWdodD0iMjAwIiB2aWV3Qm94PSIwIDAgODAwIDIwMCI+CiAgPGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJnIiB4MT0iMCIgeTE9IjAiIHgyPSIxIiB5Mj0iMCI+CiAgICA8c3RvcCBvZmZzZXQ9IjAlIiBzdG9wLWNvbG9yPSIjMTQ1MzJkIi8+PHN0b3Agb2Zmc2V0PSI1MCUiIHN0b3AtY29sb3I9IiMxNmEzNGEiLz48c3RvcCBvZmZzZXQ9IjEwMCUiIHN0b3AtY29sb3I9IiMyMmM1NWUiLz4KICA8L2xpbmVhckdyYWRpZW50PjwvZGVmcz4KICA8cmVjdCB3aWR0aD0iODAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0idXJsKCNnKSIvPgo8L3N2Zz4=',
        1, GETUTCDATE(), NULL, 0);
END

IF @themeTypeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MarketplaceItems WHERE Name = N'Bənövşəyi Tema')
BEGIN
    INSERT INTO MarketplaceItems (Name, NameEn, Description, DescriptionEn, ItemTypeId, Price, ImageUrl, IsActive, CreatedAt, UpdatedAt, IsDeleted)
    VALUES
    (N'Bənövşəyi Tema', N'Violet Theme', N'Dashboard aksent rənglərini bənövşəyi tonlara dəyişir.', N'Changes the dashboard accent colors to violet tones.', @themeTypeId, 80, NULL, 1, GETUTCDATE(), NULL, 0),
    (N'Narıncı Tema', N'Sunset Theme', N'Dashboard aksent rənglərini isti narıncı tonlara dəyişir.', N'Changes the dashboard accent colors to warm orange tones.', @themeTypeId, 80, NULL, 1, GETUTCDATE(), NULL, 0);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM MarketplaceItems WHERE Name IN (
    N'Robot Avatarı', N'Astronavt Avatarı', N'Alov Avatarı',
    N'Gecə Səması Banneri', N'Gündoğuşu Banneri', N'Meşə Banneri',
    N'Bənövşəyi Tema', N'Narıncı Tema');
DELETE FROM MarketplaceItemTypes WHERE Name = N'ProfileBanner';
");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedAvatarId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedBadgeId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_MarketplaceItems_EquippedBannerId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_EquippedAvatarId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_EquippedBadgeId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_EquippedBannerId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EquippedAvatarId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EquippedBadgeId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EquippedBannerId",
                table: "UserProfiles");
        }
    }
}
