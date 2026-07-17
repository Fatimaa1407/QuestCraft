import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { BattleDto, BattleSubmissionResultDto, BattleSummaryDto } from '../types/battle';

export async function getMyBattles(): Promise<BattleSummaryDto[]> {
  const { data } = await apiClient.get<ApiResponse<BattleSummaryDto[]>>('/api/battles/mine');
  return data.data ?? [];
}

export async function getOpenRooms(): Promise<BattleSummaryDto[]> {
  const { data } = await apiClient.get<ApiResponse<BattleSummaryDto[]>>('/api/battles/open-rooms');
  return data.data ?? [];
}

export async function getBattleByCode(code: string): Promise<BattleDto | null> {
  const { data } = await apiClient.get<ApiResponse<BattleDto>>(`/api/battles/by-code/${code}`);
  return data.data;
}

export async function getBattle(id: number): Promise<BattleDto | null> {
  const { data } = await apiClient.get<ApiResponse<BattleDto>>(`/api/battles/${id}`);
  return data.data;
}

export async function createDuelBattle(opponentUserId: number): Promise<BattleDto | null> {
  const { data } = await apiClient.post<ApiResponse<BattleDto>>('/api/battles/duel', { opponentUserId });
  return data.data;
}

export async function createRoomBattle(maxPlayers: number): Promise<BattleDto | null> {
  const { data } = await apiClient.post<ApiResponse<BattleDto>>('/api/battles/room', { maxPlayers });
  return data.data;
}

export async function joinBattle(id: number): Promise<BattleDto | null> {
  const { data } = await apiClient.post<ApiResponse<BattleDto>>(`/api/battles/${id}/join`);
  return data.data;
}

export async function startBattle(id: number): Promise<BattleDto | null> {
  const { data } = await apiClient.post<ApiResponse<BattleDto>>(`/api/battles/${id}/start`);
  return data.data;
}

export async function cancelBattle(id: number): Promise<void> {
  await apiClient.post(`/api/battles/${id}/cancel`);
}

export async function submitBattleSolution(id: number, sourceCode: string): Promise<BattleSubmissionResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<BattleSubmissionResultDto>>(`/api/battles/${id}/submit`, { sourceCode });
  return data.data;
}
