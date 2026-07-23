import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type {
  CategoryDto,
  ChallengeCommentThreadDto,
  ChallengeDetailDto,
  ChallengeListItemDto,
  ChallengeStatsDto,
  DailyPuzzleDto,
  DifficultyDto,
} from '../types/challenge';

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

export async function getCategories(): Promise<CategoryDto[]> {
  const { data } = await apiClient.get<ApiResponse<CategoryDto[]>>('/api/categories');
  return data.data ?? [];
}

export async function getDifficulties(): Promise<DifficultyDto[]> {
  const { data } = await apiClient.get<ApiResponse<DifficultyDto[]>>('/api/difficulties');
  return data.data ?? [];
}

export async function getChallengeStats(id: number): Promise<ChallengeStatsDto> {
  const { data } = await apiClient.get<ApiResponse<ChallengeStatsDto>>(`/api/challenges/${id}/stats`);
  return data.data ?? { solverCount: 0, totalSubmissions: 0, acceptanceRate: 0, averageSolveTimeMs: null };
}

export async function getChallengeComments(id: number): Promise<ChallengeCommentThreadDto[]> {
  const { data } = await apiClient.get<ApiResponse<ChallengeCommentThreadDto[]>>(`/api/challenges/${id}/comments`);
  return data.data ?? [];
}

export async function postChallengeComment(
  id: number,
  payload: { content: string; isSpoiler: boolean; parentCommentId?: number | null },
): Promise<void> {
  await apiClient.post(`/api/challenges/${id}/comments`, payload);
}

export async function getDailyPuzzle(): Promise<DailyPuzzleDto> {
  const { data } = await apiClient.get<ApiResponse<DailyPuzzleDto>>('/api/challenges/daily-puzzle');
  return data.data ?? { challengeId: null, title: null, difficulty: null, solvedToday: false };
}
