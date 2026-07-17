import { apiClient } from './client';
import type { ApiResponse, PagedResult } from '../types/api';
import type { ChatMessageDto, ConversationDto } from '../types/chat';

export async function getConversations(): Promise<ConversationDto[]> {
  const { data } = await apiClient.get<ApiResponse<ConversationDto[]>>('/api/chat/conversations');
  return data.data ?? [];
}

export async function getConversation(withUserId: number, page = 1, pageSize = 30): Promise<PagedResult<ChatMessageDto>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<ChatMessageDto>>>(`/api/chat/${withUserId}`, {
    params: { page, pageSize },
  });
  return data.data ?? { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
}

export async function sendChatMessage(withUserId: number, content: string): Promise<ChatMessageDto | null> {
  const { data } = await apiClient.post<ApiResponse<ChatMessageDto>>(`/api/chat/${withUserId}`, { content });
  return data.data;
}

export async function markConversationRead(withUserId: number): Promise<void> {
  await apiClient.post(`/api/chat/${withUserId}/read`);
}
