import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type { AppNotification } from '../types/notification';

export async function getNotifications(params: { unreadOnly?: boolean; page?: number; pageSize?: number } = {}): Promise<PagedResult<AppNotification>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<AppNotification>>>('/api/notifications', { params });
  return (
    data.data ?? {
      items: [],
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
      totalCount: 0,
      totalPages: 0,
    }
  );
}

export async function markNotificationRead(id: number): Promise<void> {
  await apiClient.put(`/api/notifications/${id}/read`);
}
