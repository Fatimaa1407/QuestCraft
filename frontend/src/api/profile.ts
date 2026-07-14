import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { MyProfile } from '../types/profile';

export async function getMyProfile(): Promise<MyProfile | null> {
  const { data } = await apiClient.get<ApiResponse<MyProfile>>('/api/profile');
  return data.data;
}

export async function updateMyProfile(payload: { bio: string | null; avatarUrl: string | null }): Promise<MyProfile | null> {
  const { data } = await apiClient.put<ApiResponse<MyProfile>>('/api/profile', payload);
  return data.data;
}
