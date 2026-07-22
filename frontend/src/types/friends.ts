export interface FriendDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  xp: number;
  frameImageUrl: string | null;
}

export interface FriendRequestDto {
  id: number;
  requesterId: number;
  requesterUsername: string;
  requesterAvatarUrl: string | null;
  requesterLevel: number;
  createdAt: string;
  requesterFrameImageUrl: string | null;
}

export type FriendStatus = 'None' | 'PendingSent' | 'PendingReceived' | 'Friends' | 'Self';

export interface UserSearchResultDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  friendStatus: FriendStatus;
  frameImageUrl: string | null;
}
