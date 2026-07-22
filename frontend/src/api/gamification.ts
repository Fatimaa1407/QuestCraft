import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type {
  Achievement,
  ClaimDailyQuestResult,
  CurrentSeasonalEventDto,
  DailyLoginRewardDto,
  DailyQuest,
  DashboardAnalyticsDto,
  LeaderboardEntry,
  LeaderboardPeriod,
  LevelProgress,
  MyRankDto,
  MyStatisticsDto,
  StreakDto,
} from '../types/gamification';

export async function getDailyQuests(): Promise<DailyQuest[]> {
  const { data } = await apiClient.get<ApiResponse<DailyQuest[]>>('/api/gamification/daily-quests');
  return data.data ?? [];
}

export async function getLevelProgress(): Promise<LevelProgress | null> {
  const { data } = await apiClient.get<ApiResponse<LevelProgress>>('/api/gamification/level-progress');
  return data.data;
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

export async function getAchievements(): Promise<Achievement[]> {
  const { data } = await apiClient.get<ApiResponse<Achievement[]>>('/api/gamification/achievements');
  return data.data ?? [];
}

export async function getDashboardAnalytics(): Promise<DashboardAnalyticsDto> {
  const { data } = await apiClient.get<ApiResponse<DashboardAnalyticsDto>>('/api/gamification/analytics');
  return data.data ?? { xpLast30Days: [], categoryProgress: [], activeDaysLast30: 0 };
}

export async function getMyStreak(): Promise<StreakDto> {
  const { data } = await apiClient.get<ApiResponse<StreakDto>>('/api/gamification/streak');
  return data.data ?? { currentStreak: 0, longestStreak: 0, lastActivityDate: null, activeDatesLast30: [] };
}

export async function getMyRank(period: LeaderboardPeriod = 'AllTime'): Promise<MyRankDto> {
  const { data } = await apiClient.get<ApiResponse<MyRankDto>>('/api/gamification/my-rank', {
    params: { period },
  });
  return data.data ?? { rank: 0, totalUsers: 0, xp: 0, level: 1 };
}

export async function getCurrentSeasonalEvent(): Promise<CurrentSeasonalEventDto | null> {
  const { data } = await apiClient.get<ApiResponse<CurrentSeasonalEventDto | null>>('/api/seasonal-events/current');
  return data.data;
}

export async function claimDailyLoginReward(): Promise<DailyLoginRewardDto | null> {
  const { data } = await apiClient.post<ApiResponse<DailyLoginRewardDto>>('/api/gamification/daily-login-reward/claim');
  return data.data;
}

export async function getMyStatistics(): Promise<MyStatisticsDto | null> {
  const { data } = await apiClient.get<ApiResponse<MyStatisticsDto>>('/api/gamification/statistics');
  return data.data;
}

export async function pinAchievement(achievementId: number): Promise<void> {
  await apiClient.post(`/api/gamification/achievements/${achievementId}/pin`);
}

export async function unpinAchievement(achievementId: number): Promise<void> {
  await apiClient.post(`/api/gamification/achievements/${achievementId}/unpin`);
}
