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
  isPinned: boolean;
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

export interface HeatmapDay {
  date: string;
  count: number;
}

export interface StreakDto {
  currentStreak: number;
  longestStreak: number;
  lastActivityDate: string | null;
  activeDatesLast30: HeatmapDay[];
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

export interface DailyLoginRewardDto {
  alreadyClaimed: boolean;
  coinsAwarded: number;
  xpAwarded: number;
  wasMysteryBonus: boolean;
  newCoinsTotal: number;
  newXpTotal: number;
}

export interface RecommendedChallenge {
  id: number;
  title: string;
  difficulty: string;
  xpReward: number;
}

export interface RecommendationDto {
  weakCategoryName: string | null;
  acceptanceRate: number | null;
  challenges: RecommendedChallenge[];
}

export interface PersonalGoalsProgressDto {
  challengeGoal: number | null;
  challengesDoneToday: number;
  xpGoal: number | null;
  xpToday: number;
  battleGoal: number | null;
  battlesToday: number;
}

export interface HeatmapDayDetail {
  date: string;
  count: number;
  challengesSolved: number;
  xpEarned: number;
  codingTimeMs: number;
}

export interface MyStatisticsDto {
  challengesSolved: number;
  quizzesCompleted: number;
  successRatePercent: number;
  averageSolveTimeMs: number | null;
  totalCodingTimeMs: number;
  currentStreak: number;
  longestStreak: number;
  totalXp: number;
  totalCoinsEarned: number;
  battlesWon: number;
  rank: number;
  totalUsers: number;
}
