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
}

export interface PurchaseResultDto {
  purchaseId: number;
  itemName: string;
  pricePaid: number;
  remainingCoins: number;
}

export interface MyPurchaseDto {
  id: number;
  itemName: string;
  itemType: string;
  pricePaid: number;
  purchasedAt: string;
  isEquipped: boolean;
}

export const EQUIPABLE_ITEM_TYPES = ['ProfileFrame', 'Title', 'Theme'];
