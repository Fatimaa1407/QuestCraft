import { apiClient } from './client';
import type { ApiResponse } from '../types/api';
import type { FriendDto, FriendRequestDto, UserSearchResultDto } from '../types/friends';

export async function getFriends(): Promise<FriendDto[]> {
  const { data } = await apiClient.get<ApiResponse<FriendDto[]>>('/api/friends');
  return data.data ?? [];
}

export async function getIncomingFriendRequests(): Promise<FriendRequestDto[]> {
  const { data } = await apiClient.get<ApiResponse<FriendRequestDto[]>>('/api/friends/requests');
  return data.data ?? [];
}

export async function searchUsers(query: string): Promise<UserSearchResultDto[]> {
  const { data } = await apiClient.get<ApiResponse<UserSearchResultDto[]>>('/api/friends/search', { params: { query } });
  return data.data ?? [];
}

export async function sendFriendRequest(addresseeUserId: number): Promise<void> {
  await apiClient.post('/api/friends/requests', { addresseeUserId });
}

export async function respondFriendRequest(id: number, accept: boolean): Promise<void> {
  await apiClient.post(`/api/friends/requests/${id}/respond`, { accept });
}

export async function removeFriend(userId: number): Promise<void> {
  await apiClient.delete(`/api/friends/${userId}`);
}
