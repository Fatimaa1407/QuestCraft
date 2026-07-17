export interface AchievementAdminDto {
  id: number;
  name: string;
  nameEn: string | null;
  description: string;
  descriptionEn: string | null;
  iconUrl: string | null;
  conditionType: string;
  conditionValue: number;
  xpReward: number;
  coinReward: number;
  isActive: boolean;
}

export interface DailyQuestTemplateAdminDto {
  id: number;
  title: string;
  titleEn: string | null;
  description: string | null;
  descriptionEn: string | null;
  targetType: string;
  targetValue: number;
  xpReward: number;
  coinReward: number;
  isActive: boolean;
}

export interface QuestionOptionAdminDto {
  id: number;
  text: string;
  isCorrect: boolean;
  textEn: string | null;
}

export interface QuestionAdminDto {
  id: number;
  text: string;
  explanation: string | null;
  options: QuestionOptionAdminDto[];
  textEn: string | null;
  explanationEn: string | null;
}

export interface QuizAdminDetailDto {
  id: number;
  title: string;
  categoryId: number | null;
  category: string | null;
  xpReward: number;
  isPublished: boolean;
  requiredLevel: number;
  questions: QuestionAdminDto[];
  titleEn: string | null;
  tags: string | null;
}

export interface AuditLogDto {
  id: number;
  userId: number | null;
  username: string | null;
  action: string;
  entityName: string;
  entityId: number | null;
  newValues: string | null;
  timestamp: string;
  ipAddress: string | null;
}

export interface ExcelImportResultDto {
  totalRows: number;
  successRows: number;
  failedRows: number;
  errors: string[];
}

export interface AdminDashboardSummaryDto {
  totalUsers: number;
  totalChallenges: number;
  totalQuizzes: number;
  totalSubmissions: number;
  submissionsToday: number;
  newUsersThisWeek: number;
  activeUsersToday: number;
}

export interface AdminActivityItemDto {
  kind: 'Submission' | 'Quiz';
  userId: number;
  username: string;
  title: string;
  verdict: string | null;
  score: number | null;
  totalQuestions: number | null;
  timestamp: string;
}

export interface SeasonalEventDto {
  id: number;
  name: string;
  nameEn: string | null;
  description: string | null;
  descriptionEn: string | null;
  startDate: string;
  endDate: string;
  isActive: boolean;
  emoji: string | null;
}

export interface AdminUserListItemDto {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  level: number;
  xp: number;
  coins: number;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}
