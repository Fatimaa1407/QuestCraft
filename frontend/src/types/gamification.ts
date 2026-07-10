export interface DailyQuest {
  id: number;
  title: string;
  description: string | null;
  currentProgress: number;
  targetValue: number;
  isCompleted: boolean;
  rewardClaimed: boolean;
  xpReward: number;
  coinReward: number;
}

export interface ClaimDailyQuestResult {
  quest: DailyQuest;
  totalXp: number;
  totalCoins: number;
  level: number;
}

export interface Achievement {
  id: number;
  name: string;
  description: string;
  iconUrl: string | null;
  xpReward: number;
  coinReward: number;
  isUnlocked: boolean;
  unlockedAt: string | null;
}

export type LeaderboardPeriod = 'Daily' | 'Weekly' | 'Monthly' | 'AllTime';

export interface LeaderboardEntry {
  rank: number;
  userId: number;
  username: string;
  avatarUrl: string | null;
  xp: number;
  level: number;
}
