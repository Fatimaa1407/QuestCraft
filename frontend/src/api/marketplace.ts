import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type {
  EquippedCosmeticsDto, ItemTypeDto, MarketplaceItemDto, MyPurchaseDto, PurchaseResultDto,
  MarketplaceBundleDto, BundlePurchaseResultDto, MysteryBoxResultDto,
} from '../types/marketplace';

export async function getMarketplaceItems(typeId?: number): Promise<MarketplaceItemDto[]> {
  const { data } = await apiClient.get<ApiResponse<MarketplaceItemDto[]>>('/api/marketplace/items', {
    params: typeId ? { typeId } : undefined,
  });
  return data.data ?? [];
}

export async function getItemTypes(): Promise<ItemTypeDto[]> {
  const { data } = await apiClient.get<ApiResponse<ItemTypeDto[]>>('/api/marketplace/item-types');
  return data.data ?? [];
}

export async function purchaseItem(id: number): Promise<PurchaseResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<PurchaseResultDto>>(`/api/marketplace/items/${id}/purchase`);
  return data.data;
}

export async function equipItem(id: number): Promise<void> {
  await apiClient.post(`/api/marketplace/items/${id}/equip`);
}

export async function unequipItem(id: number): Promise<void> {
  await apiClient.post(`/api/marketplace/items/${id}/unequip`);
}

export async function getMyPurchases(): Promise<MyPurchaseDto[]> {
  const { data } = await apiClient.get<ApiResponse<MyPurchaseDto[]>>('/api/marketplace/my-purchases');
  return data.data ?? [];
}

export async function getMyEquippedCosmetics(): Promise<EquippedCosmeticsDto | null> {
  const { data } = await apiClient.get<ApiResponse<EquippedCosmeticsDto>>('/api/profile/me/equipped');
  return data.data;
}

export async function toggleWishlist(itemId: number): Promise<boolean> {
  const { data } = await apiClient.post<ApiResponse<boolean>>(`/api/marketplace/items/${itemId}/wishlist`);
  return data.data ?? false;
}

export async function getMyWishlist(): Promise<MarketplaceItemDto[]> {
  const { data } = await apiClient.get<ApiResponse<MarketplaceItemDto[]>>('/api/marketplace/wishlist');
  return data.data ?? [];
}

export async function getBundles(): Promise<MarketplaceBundleDto[]> {
  const { data } = await apiClient.get<ApiResponse<MarketplaceBundleDto[]>>('/api/marketplace/bundles');
  return data.data ?? [];
}

export async function purchaseBundle(id: number): Promise<BundlePurchaseResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<BundlePurchaseResultDto>>(`/api/marketplace/bundles/${id}/purchase`);
  return data.data;
}

export async function openMysteryBox(): Promise<MysteryBoxResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<MysteryBoxResultDto>>('/api/marketplace/mystery-box/open');
  return data.data;
}
