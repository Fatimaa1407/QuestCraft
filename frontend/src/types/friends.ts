export interface FriendDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  xp: number;
  frameImageUrl: string | null;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
}

export interface FriendRequestDto {
  id: number;
  requesterId: number;
  requesterUsername: string;
  requesterAvatarUrl: string | null;
  requesterLevel: number;
  createdAt: string;
  requesterFrameImageUrl: string | null;
  requesterTitleText: string | null;
  requesterBadgeImageUrl: string | null;
  requesterBadgeName: string | null;
}

export type FriendStatus = 'None' | 'PendingSent' | 'PendingReceived' | 'Friends' | 'Declined' | 'Self';

export interface UserSearchResultDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  level: number;
  friendStatus: FriendStatus;
  frameImageUrl: string | null;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
}
