export interface ItemTypeDto {
  id: number;
  name: string;
}

export interface MarketplaceItemDto {
  id: number;
  name: string;
  description: string | null;
  itemTypeId: number;
  itemType: string;
  price: number;
  imageUrl: string | null;
  isActive: boolean;
  isOwned: boolean;
  isFeatured: boolean;
  isWishlisted: boolean;
}

export interface PurchaseResultDto {
  purchaseId: number;
  marketplaceItemId: number;
  itemName: string;
  itemType: string;
  imageUrl: string | null;
  pricePaid: number;
  remainingCoins: number;
  autoEquipped: boolean;
}

export interface MyPurchaseDto {
  id: number;
  marketplaceItemId: number;
  itemName: string;
  itemTypeId: number;
  itemType: string;
  imageUrl: string | null;
  pricePaid: number;
  purchasedAt: string;
  isEquipped: boolean;
}

export interface EquippedCosmeticsDto {
  avatarUrl: string | null;
  frameImageUrl: string | null;
  bannerImageUrl: string | null;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
  themeItemId: number | null;
  themeName: string | null;
}

export const EQUIPABLE_ITEM_TYPES = ['Avatar', 'ProfileFrame', 'ProfileBanner', 'Title', 'Badge', 'Theme'];

export interface BundleItemDto {
  marketplaceItemId: number;
  name: string;
  imageUrl: string | null;
  price: number;
  itemType: string;
  isOwned: boolean;
}

export interface MarketplaceBundleDto {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string | null;
  bundlePrice: number;
  individualTotal: number;
  isOwnedFully: boolean;
  ownedCount: number;
  items: BundleItemDto[];
}

export interface BundlePurchaseResultDto {
  bundleId: number;
  bundleName: string;
  pricePaid: number;
  remainingCoins: number;
  grantedItemNames: string[];
}

export interface MysteryBoxResultDto {
  nothingLeft: boolean;
  itemId: number | null;
  itemName: string | null;
  itemType: string | null;
  imageUrl: string | null;
  rarity: string | null;
  autoEquipped: boolean;
  pricePaid: number;
  remainingCoins: number;
}
