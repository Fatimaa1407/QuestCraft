export interface MyProfile {
  bio: string | null;
  avatarUrl: string | null;
  isGameComplete: boolean;
}

export interface PublicProfile {
  userId: number;
  username: string;
  level: number;
  xp: number;
  bio: string | null;
  avatarUrl: string | null;
  frameImageUrl: string | null;
  bannerImageUrl: string | null;
  titleText: string | null;
  badgeImageUrl: string | null;
  badgeName: string | null;
  joinedAt: string;
  isGameComplete: boolean;
}
