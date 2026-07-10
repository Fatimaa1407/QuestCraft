import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { ClaimDailyQuestResult, DailyQuest, LeaderboardEntry, LeaderboardPeriod } from '../types/gamification';

export async function getDailyQuests(): Promise<DailyQuest[]> {
  const { data } = await apiClient.get<ApiResponse<DailyQuest[]>>('/api/gamification/daily-quests');
  return data.data ?? [];
}

export async function claimDailyQuest(id: number): Promise<ClaimDailyQuestResult | null> {
  const { data } = await apiClient.post<ApiResponse<ClaimDailyQuestResult>>(`/api/gamification/daily-quests/${id}/claim`);
  return data.data;
}

export async function getLeaderboard(period: LeaderboardPeriod = 'AllTime', top = 20): Promise<LeaderboardEntry[]> {
  const { data } = await apiClient.get<ApiResponse<LeaderboardEntry[]>>('/api/gamification/leaderboard', {
    params: { period, top },
  });
  return data.data ?? [];
}
