namespace QuestCraft.Application.Features.Marketplace;

// Mirrors frontend/src/utils/rarity.ts's getRarity() thresholds exactly — keep both in sync.
// Used server-side only for Mystery Box weighted selection (rarity itself is still price-derived,
// there is no persisted Rarity column).
public static class MarketplaceRarityHelper
{
    public static string GetRarity(int price) => price switch
    {
        >= 200 => "Mythic",
        >= 150 => "Legendary",
        >= 100 => "Epic",
        >= 50 => "Rare",
        _ => "Common",
    };

    // Common items are far more likely than Mythic — gives the box a real "reveal" moment.
    public static int GetWeight(string rarity) => rarity switch
    {
        "Mythic" => 2,
        "Legendary" => 5,
        "Epic" => 13,
        "Rare" => 30,
        _ => 50,
    };
}
