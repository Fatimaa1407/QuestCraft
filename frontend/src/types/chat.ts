export interface ChatMessageDto {
  id: number;
  senderId: number;
  recipientId: number;
  content: string;
  imageDataUrl: string | null;
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
  friendFrameImageUrl: string | null;
  friendTitleText: string | null;
  friendBadgeImageUrl: string | null;
  friendBadgeName: string | null;
}
