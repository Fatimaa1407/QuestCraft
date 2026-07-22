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
  newAchievements: string[];
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

export interface LevelProgress {
  level: number;
  challengesCompleted: number;
  challengesTotal: number;
  quizzesCompleted: number;
  quizzesTotal: number;
  overallCompleted: number;
  overallTotal: number;
  isMaxLevel: boolean;
}

export type LeaderboardPeriod = 'Daily' | 'Weekly' | 'Monthly' | 'AllTime';

export interface LeaderboardEntry {
  rank: number;
  userId: number;
  username: string;
  avatarUrl: string | null;
  xp: number;
  level: number;
  frameImageUrl: string | null;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
}

export interface XpDayPoint {
  date: string; // "YYYY-MM-DD"
  xp: number;
}

export interface CategoryProgress {
  categoryName: string;
  completed: number;
  total: number;
}

export interface DashboardAnalyticsDto {
  xpLast30Days: XpDayPoint[];
  categoryProgress: CategoryProgress[];
  activeDaysLast30: number;
}

export interface StreakDto {
  currentStreak: number;
  longestStreak: number;
  lastActivityDate: string | null;
  activeDatesLast30: string[];
}

export interface CurrentSeasonalEventDto {
  id: number;
  name: string;
  description: string | null;
  emoji: string | null;
  endDate: string;
}

export interface MyRankDto {
  rank: number;
  totalUsers: number;
  xp: number;
  level: number;
}
