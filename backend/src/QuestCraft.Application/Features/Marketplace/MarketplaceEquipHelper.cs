using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

// Shared by EquipItemCommand and every auto-equip-on-acquire path (purchase, bundle, mystery box)
// so the type -> UserProfile slot mapping lives in exactly one place.
public static class MarketplaceEquipHelper
{
    public static bool IsEquipable(string itemTypeName) => itemTypeName switch
    {
        "ProfileFrame" or "Title" or "Theme" or "Avatar" or "ProfileBanner" or "Badge" => true,
        _ => false,
    };

    public static void Equip(UserProfile profile, MarketplaceItem item)
    {
        switch (item.ItemType.Name)
        {
            case "ProfileFrame":
                profile.EquippedFrameId = item.Id;
                break;
            case "Title":
                profile.EquippedTitleId = item.Id;
                break;
            case "Theme":
                profile.EquippedThemeId = item.Id;
                break;
            case "Avatar":
                profile.EquippedAvatarId = item.Id;
                break;
            case "ProfileBanner":
                profile.EquippedBannerId = item.Id;
                break;
            case "Badge":
                profile.EquippedBadgeId = item.Id;
                break;
            default:
                throw new BadRequestException("Bu tip məhsul taxıla bilməz.");
        }
    }
}
