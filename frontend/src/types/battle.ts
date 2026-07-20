export type BattleMode = 'Duel' | 'Room';
export type BattleStatus = 'Waiting' | 'InProgress' | 'Finished' | 'Cancelled';

export interface BattleParticipantDto {
  userId: number;
  username: string;
  avatarUrl: string | null;
  hasFinished: boolean;
  finishedAt: string | null;
  rank: number | null;
  passedTestCases: number;
  totalTestCases: number;
  frameImageUrl: string | null;
}

export interface BattleDto {
  id: number;
  mode: BattleMode;
  status: BattleStatus;
  challengeId: number;
  challengeTitle: string;
  timeLimitMs: number;
  hostUserId: number;
  invitedUserId: number | null;
  joinCode: string | null;
  maxPlayers: number;
  startedAt: string | null;
  endedAt: string | null;
  participants: BattleParticipantDto[];
}

export interface BattleSummaryDto {
  id: number;
  mode: BattleMode;
  status: BattleStatus;
  challengeTitle: string;
  playerCount: number;
  maxPlayers: number;
  joinCode: string | null;
  createdAt: string;
}

export interface BattleSubmissionResultDto {
  allPassed: boolean;
  passedTestCases: number;
  totalTestCases: number;
  compileErrorMessage: string | null;
  battle: BattleDto;
}
