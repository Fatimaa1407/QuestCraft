export interface ChatMessageDto {
  id: number;
  senderId: number;
  recipientId: number;
  content: string;
  createdAt: string;
  isRead: boolean;
}

export interface ConversationDto {
  friendUserId: number;
  friendUsername: string;
  friendAvatarUrl: string | null;
  lastMessage: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
}
