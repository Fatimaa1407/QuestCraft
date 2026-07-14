export type NotificationType =
  | 'DailyQuestReminder'
  | 'AchievementUnlock'
  | 'ChallengeAccepted'
  | 'MarketplacePurchase'
  | 'SystemNotification'
  | 'LevelUp';

export interface AppNotification {
  id: number;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}
