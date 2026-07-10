import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type {
  QuizAnswerInput,
  QuizAttemptListItem,
  QuizAttemptResultDto,
  QuizAttemptViewDto,
  QuizListItemDto,
} from '../types/quiz';

export async function getMyQuizAttempts(page = 1, pageSize = 5): Promise<PagedResult<QuizAttemptListItem>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<QuizAttemptListItem>>>('/api/quizzes/attempts/my', {
    params: { page, pageSize },
  });
  return data.data ?? { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
}

export interface QuizFilters {
  search?: string;
  page?: number;
  pageSize?: number;
}

export async function getQuizzes(filters: QuizFilters = {}): Promise<PagedResult<QuizListItemDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<QuizListItemDto>>>('/api/quizzes', {
    params: { page: 1, pageSize: 20, ...filters },
  });
  return data.data ?? { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };
}

export async function getQuizForAttempt(id: number): Promise<QuizAttemptViewDto | null> {
  const { data } = await apiClient.get<ApiResponse<QuizAttemptViewDto>>(`/api/quizzes/${id}`);
  return data.data;
}

export async function submitQuizAttempt(id: number, answers: QuizAnswerInput[]): Promise<QuizAttemptResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<QuizAttemptResultDto>>(`/api/quizzes/${id}/attempt`, { answers });
  return data.data;
}
