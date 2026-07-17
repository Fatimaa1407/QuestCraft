export interface FriendDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  xp: number;
}

export interface FriendRequestDto {
  id: number;
  requesterId: number;
  requesterUsername: string;
  requesterAvatarUrl: string | null;
  requesterLevel: number;
  createdAt: string;
}

export type FriendStatus = 'None' | 'PendingSent' | 'PendingReceived' | 'Friends' | 'Self';

export interface UserSearchResultDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  friendStatus: FriendStatus;
}
