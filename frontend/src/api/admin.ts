import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { CategoryDto, ChallengeDetailDto, ChallengeListItemDto, DifficultyDto, HiddenTestCaseDto, TestCaseDto } from '../types/challenge';
import type { MarketplaceItemDto } from '../types/marketplace';
import type {
  AchievementAdminDto,
  AdminActivityItemDto,
  AdminDashboardSummaryDto,
  AdminUserListItemDto,
  AuditLogDto,
  DailyQuestTemplateAdminDto,
  ExcelImportResultDto,
  QuizAdminDetailDto,
  SeasonalEventDto,
} from '../types/admin';
import type { QuizListItemDto } from '../types/quiz';

// --- Challenges ---
export interface ChallengePayload {
  title: string;
  description: string;
  categoryId: number;
  difficultyId: number;
  timeLimitMs: number;
  memoryLimitMb: number;
  xpReward: number;
  coinReward: number;
  starterCode: string;
  constraints: string | null;
  inputFormat: string | null;
  outputFormat: string | null;
  sampleInput: string | null;
  sampleOutput: string | null;
  isPublished: boolean;
  requiredLevel: number;
  titleEn: string | null;
  descriptionEn: string | null;
  constraintsEn: string | null;
  inputFormatEn: string | null;
  outputFormatEn: string | null;
  starterCodeEn: string | null;
  tags: string | null;
  isBattleOnly: boolean;
}
export async function getChallengeAdminById(id: number) {
  const { data } = await apiClient.get<ApiResponse<ChallengeDetailDto>>(`/api/challenges/${id}`);
  return data.data;
}
export async function createChallenge(payload: ChallengePayload) {
  const { data } = await apiClient.post<ApiResponse<ChallengeDetailDto>>('/api/challenges', payload);
  return data.data;
}
export async function updateChallenge(id: number, payload: ChallengePayload) {
  const { data } = await apiClient.put<ApiResponse<ChallengeDetailDto>>(`/api/challenges/${id}`, payload);
  return data.data;
}
export async function deleteChallenge(id: number) {
  await apiClient.delete(`/api/challenges/${id}`);
}
export async function getDeletedChallenges() {
  const { data } = await apiClient.get<ApiResponse<ChallengeListItemDto[]>>('/api/challenges/deleted');
  return data.data ?? [];
}
export async function restoreChallenge(id: number) {
  const { data } = await apiClient.post<ApiResponse<ChallengeListItemDto>>(`/api/challenges/${id}/restore`);
  return data.data;
}
export async function getBattlePoolChallenges() {
  const { data } = await apiClient.get<ApiResponse<ChallengeListItemDto[]>>('/api/challenges/battle-pool');
  return data.data ?? [];
}

export interface TestCasePayload {
  input: string;
  expectedOutput: string;
  orderIndex: number;
  isHidden: boolean;
  weight: number;
}
export async function addTestCase(challengeId: number, payload: TestCasePayload) {
  const { data } = await apiClient.post<ApiResponse<number>>(`/api/challenges/${challengeId}/test-cases`, payload);
  return data.data;
}
export async function updateTestCase(id: number, payload: TestCasePayload) {
  await apiClient.put(`/api/challenges/test-cases/${id}`, payload);
}
export async function deleteTestCase(id: number, isHidden: boolean) {
  await apiClient.delete(`/api/challenges/test-cases/${id}`, { params: { isHidden } });
}
export type { TestCaseDto, HiddenTestCaseDto };

// --- Quizzes (no soft-delete/restore workflow) ---
export interface QuizPayload {
  title: string;
  categoryId: number | null;
  xpReward: number;
  isPublished: boolean;
  requiredLevel: number;
  titleEn: string | null;
  tags: string | null;
}
export async function getQuizAdminById(id: number) {
  const { data } = await apiClient.get<ApiResponse<QuizAdminDetailDto>>(`/api/quizzes/${id}/admin`);
  return data.data;
}
export async function createQuiz(payload: QuizPayload) {
  const { data } = await apiClient.post<ApiResponse<QuizListItemDto>>('/api/quizzes', payload);
  return data.data;
}
export async function updateQuiz(id: number, payload: QuizPayload) {
  const { data } = await apiClient.put<ApiResponse<QuizListItemDto>>(`/api/quizzes/${id}`, payload);
  return data.data;
}
export async function deleteQuiz(id: number) {
  await apiClient.delete(`/api/quizzes/${id}`);
}

export interface QuestionOptionInputPayload {
  text: string;
  isCorrect: boolean;
  textEn: string | null;
}
export interface QuestionPayload {
  text: string;
  explanation: string | null;
  options: QuestionOptionInputPayload[];
  textEn: string | null;
  explanationEn: string | null;
}
export async function addQuestion(quizId: number, payload: QuestionPayload) {
  const { data } = await apiClient.post<ApiResponse<number>>(`/api/quizzes/${quizId}/questions`, payload);
  return data.data;
}
export async function updateQuestion(id: number, payload: QuestionPayload) {
  await apiClient.put(`/api/quizzes/questions/${id}`, payload);
}
export async function deleteQuestion(id: number) {
  await apiClient.delete(`/api/quizzes/questions/${id}`);
}

// --- Categories ---
export async function createCategory(payload: { name: string; description: string | null; iconUrl: string | null }) {
  const { data } = await apiClient.post<ApiResponse<CategoryDto>>('/api/categories', payload);
  return data.data;
}
export async function updateCategory(id: number, payload: { name: string; description: string | null; iconUrl: string | null }) {
  const { data } = await apiClient.put<ApiResponse<CategoryDto>>(`/api/categories/${id}`, payload);
  return data.data;
}
export async function deleteCategory(id: number) {
  await apiClient.delete(`/api/categories/${id}`);
}
export async function getDeletedCategories() {
  const { data } = await apiClient.get<ApiResponse<CategoryDto[]>>('/api/categories/deleted');
  return data.data ?? [];
}
export async function restoreCategory(id: number) {
  const { data } = await apiClient.post<ApiResponse<CategoryDto>>(`/api/categories/${id}/restore`);
  return data.data;
}

// --- Difficulties ---
export async function createDifficulty(payload: { name: string; color: string | null; xpMultiplier: number }) {
  const { data } = await apiClient.post<ApiResponse<DifficultyDto>>('/api/difficulties', payload);
  return data.data;
}
export async function updateDifficulty(id: number, payload: { name: string; color: string | null; xpMultiplier: number }) {
  const { data } = await apiClient.put<ApiResponse<DifficultyDto>>(`/api/difficulties/${id}`, payload);
  return data.data;
}
export async function deleteDifficulty(id: number) {
  await apiClient.delete(`/api/difficulties/${id}`);
}
export async function getDeletedDifficulties() {
  const { data } = await apiClient.get<ApiResponse<DifficultyDto[]>>('/api/difficulties/deleted');
  return data.data ?? [];
}
export async function restoreDifficulty(id: number) {
  const { data } = await apiClient.post<ApiResponse<DifficultyDto>>(`/api/difficulties/${id}/restore`);
  return data.data;
}

// --- Marketplace items (no soft-delete/restore workflow — Delete just deactivates) ---
export interface MarketplaceItemPayload {
  name: string;
  description: string | null;
  itemTypeId: number;
  price: number;
  imageUrl: string | null;
  isActive: boolean;
  nameEn: string | null;
  descriptionEn: string | null;
}
export async function createMarketplaceItem(payload: MarketplaceItemPayload) {
  const { data } = await apiClient.post<ApiResponse<MarketplaceItemDto>>('/api/marketplace/items', payload);
  return data.data;
}
export async function updateMarketplaceItem(id: number, payload: MarketplaceItemPayload) {
  const { data } = await apiClient.put<ApiResponse<MarketplaceItemDto>>(`/api/marketplace/items/${id}`, payload);
  return data.data;
}
export async function deleteMarketplaceItem(id: number) {
  await apiClient.delete(`/api/marketplace/items/${id}`);
}

// --- Achievements ---
export async function getAchievementsAdmin() {
  const { data } = await apiClient.get<ApiResponse<AchievementAdminDto[]>>('/api/achievements');
  return data.data ?? [];
}
export interface AchievementPayload {
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
export async function createAchievement(payload: AchievementPayload) {
  const { data } = await apiClient.post<ApiResponse<AchievementAdminDto>>('/api/achievements', payload);
  return data.data;
}
export async function updateAchievement(id: number, payload: AchievementPayload) {
  const { data } = await apiClient.put<ApiResponse<AchievementAdminDto>>(`/api/achievements/${id}`, payload);
  return data.data;
}
export async function deleteAchievement(id: number) {
  await apiClient.delete(`/api/achievements/${id}`);
}
export async function getDeletedAchievements() {
  const { data } = await apiClient.get<ApiResponse<AchievementAdminDto[]>>('/api/achievements/deleted');
  return data.data ?? [];
}
export async function restoreAchievement(id: number) {
  const { data } = await apiClient.post<ApiResponse<AchievementAdminDto>>(`/api/achievements/${id}/restore`);
  return data.data;
}

// --- Daily Quest Templates ---
export async function getDailyQuestTemplatesAdmin() {
  const { data } = await apiClient.get<ApiResponse<DailyQuestTemplateAdminDto[]>>('/api/daily-quest-templates');
  return data.data ?? [];
}
export interface DailyQuestTemplatePayload {
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
export async function createDailyQuestTemplate(payload: DailyQuestTemplatePayload) {
  const { data } = await apiClient.post<ApiResponse<DailyQuestTemplateAdminDto>>('/api/daily-quest-templates', payload);
  return data.data;
}
export async function updateDailyQuestTemplate(id: number, payload: DailyQuestTemplatePayload) {
  const { data } = await apiClient.put<ApiResponse<DailyQuestTemplateAdminDto>>(`/api/daily-quest-templates/${id}`, payload);
  return data.data;
}
export async function deleteDailyQuestTemplate(id: number) {
  await apiClient.delete(`/api/daily-quest-templates/${id}`);
}
export async function getDeletedDailyQuestTemplates() {
  const { data } = await apiClient.get<ApiResponse<DailyQuestTemplateAdminDto[]>>('/api/daily-quest-templates/deleted');
  return data.data ?? [];
}
export async function restoreDailyQuestTemplate(id: number) {
  const { data } = await apiClient.post<ApiResponse<DailyQuestTemplateAdminDto>>(`/api/daily-quest-templates/${id}/restore`);
  return data.data;
}

// --- Dashboard summary ---
export async function getAdminDashboardSummary(): Promise<AdminDashboardSummaryDto> {
  const { data } = await apiClient.get<ApiResponse<AdminDashboardSummaryDto>>('/api/admin/dashboard-summary');
  return (
    data.data ?? {
      totalUsers: 0,
      totalChallenges: 0,
      totalQuizzes: 0,
      totalSubmissions: 0,
      submissionsToday: 0,
      newUsersThisWeek: 0,
      activeUsersToday: 0,
    }
  );
}

// --- Seasonal events (no soft-delete/restore workflow — Delete just deactivates) ---
export interface SeasonalEventPayload {
  name: string;
  nameEn: string | null;
  description: string | null;
  descriptionEn: string | null;
  startDate: string;
  endDate: string;
  isActive: boolean;
  emoji: string | null;
}
export async function getSeasonalEvents() {
  const { data } = await apiClient.get<ApiResponse<SeasonalEventDto[]>>('/api/seasonal-events');
  return data.data ?? [];
}
export async function createSeasonalEvent(payload: SeasonalEventPayload) {
  const { data } = await apiClient.post<ApiResponse<SeasonalEventDto>>('/api/seasonal-events', payload);
  return data.data;
}
export async function updateSeasonalEvent(id: number, payload: SeasonalEventPayload) {
  const { data } = await apiClient.put<ApiResponse<SeasonalEventDto>>(`/api/seasonal-events/${id}`, payload);
  return data.data;
}
export async function deleteSeasonalEvent(id: number) {
  await apiClient.delete(`/api/seasonal-events/${id}`);
}

// --- Activity today ---
export async function getAdminActivityToday() {
  const { data } = await apiClient.get<ApiResponse<AdminActivityItemDto[]>>('/api/admin/activity-today');
  return data.data ?? [];
}

// --- Users ---
export async function getAdminUsers(page = 1, pageSize = 20, search = '') {
  const { data } = await apiClient.get<ApiResponse<{ items: AdminUserListItemDto[]; page: number; pageSize: number; totalCount: number; totalPages: number }>>(
    '/api/admin/users',
    { params: { page, pageSize, search: search || undefined } },
  );
  return data.data ?? { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
}
export async function updateUserRole(id: number, role: string) {
  const { data } = await apiClient.patch<ApiResponse<AdminUserListItemDto>>(`/api/admin/users/${id}/role`, { role });
  return data.data;
}
export async function updateUserActive(id: number, isActive: boolean) {
  const { data } = await apiClient.patch<ApiResponse<AdminUserListItemDto>>(`/api/admin/users/${id}/active`, { isActive });
  return data.data;
}

// --- Audit log ---
export async function getAuditLogs(page = 1, pageSize = 20) {
  const { data } = await apiClient.get<ApiResponse<{ items: AuditLogDto[]; page: number; pageSize: number; totalCount: number; totalPages: number }>>(
    '/api/admin/audit-logs',
    { params: { page, pageSize } },
  );
  return data.data ?? { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
}

// --- Excel import/export ---
export async function importChallenges(file: File): Promise<ExcelImportResultDto | null> {
  const formData = new FormData();
  formData.append('file', file);
  const { data } = await apiClient.post<ApiResponse<ExcelImportResultDto>>('/api/admin/excel/import/challenges', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return data.data;
}
export async function importTestCases(challengeId: number, file: File): Promise<ExcelImportResultDto | null> {
  const formData = new FormData();
  formData.append('file', file);
  const { data } = await apiClient.post<ApiResponse<ExcelImportResultDto>>(
    `/api/admin/excel/import/challenges/${challengeId}/test-cases`,
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } },
  );
  return data.data;
}
export async function importQuizQuestions(quizId: number, file: File): Promise<ExcelImportResultDto | null> {
  const formData = new FormData();
  formData.append('file', file);
  const { data } = await apiClient.post<ApiResponse<ExcelImportResultDto>>(
    `/api/admin/excel/import/quizzes/${quizId}/questions`,
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } },
  );
  return data.data;
}

export type ExcelExportKind = 'users' | 'challenges' | 'quiz-results' | 'leaderboard' | 'marketplace';

export async function downloadExcelExport(kind: ExcelExportKind, params?: Record<string, string>) {
  const response = await apiClient.get(`/api/admin/excel/export/${kind}`, { params, responseType: 'blob' });
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement('a');
  link.href = url;
  link.download = `${kind}.xlsx`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
