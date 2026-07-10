import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type { CategoryDto, ChallengeDetailDto, ChallengeListItemDto, DifficultyDto } from '../types/challenge';

export interface ChallengeFilters {
  categoryId?: number;
  difficultyId?: number;
  search?: string;
  page?: number;
  pageSize?: number;
}

export async function getChallenges(filters: ChallengeFilters = {}): Promise<PagedResult<ChallengeListItemDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<ChallengeListItemDto>>>('/api/challenges', {
    params: { page: 1, pageSize: 20, ...filters },
  });
  return data.data ?? { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };
}

export async function getChallengeById(id: number): Promise<ChallengeDetailDto | null> {
  const { data } = await apiClient.get<ApiResponse<ChallengeDetailDto>>(`/api/challenges/${id}`);
  return data.data;
}

export async function unlockHint(id: number): Promise<string | null> {
  const { data } = await apiClient.post<ApiResponse<string>>(`/api/challenges/${id}/hint/unlock`);
  return data.data;
}

export async function getCategories(): Promise<CategoryDto[]> {
  const { data } = await apiClient.get<ApiResponse<CategoryDto[]>>('/api/categories');
  return data.data ?? [];
}

export async function getDifficulties(): Promise<DifficultyDto[]> {
  const { data } = await apiClient.get<ApiResponse<DifficultyDto[]>>('/api/difficulties');
  return data.data ?? [];
}
