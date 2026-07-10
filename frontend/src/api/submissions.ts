import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type { RunResultDto, SubmissionListItem, SubmissionResultDto } from '../types/submission';

export async function getMySubmissions(page = 1, pageSize = 5): Promise<PagedResult<SubmissionListItem>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<SubmissionListItem>>>('/api/submissions/my', {
    params: { page, pageSize },
  });
  return data.data ?? { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
}

export async function runCode(challengeId: number, sourceCode: string): Promise<RunResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<RunResultDto>>('/api/submissions/run', { challengeId, sourceCode });
  return data.data;
}

export async function submitCode(challengeId: number, sourceCode: string): Promise<SubmissionResultDto | null> {
  const { data } = await apiClient.post<ApiResponse<SubmissionResultDto>>('/api/submissions/submit', { challengeId, sourceCode });
  return data.data;
}
