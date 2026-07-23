export interface CategoryDto {
  id: number;
  name: string;
  description: string | null;
  iconUrl: string | null;
}

export interface DifficultyDto {
  id: number;
  name: string;
  color: string | null;
  xpMultiplier: number;
}

export interface ChallengeListItemDto {
  id: number;
  title: string;
  category: string;
  difficulty: string;
  xpReward: number;
  coinReward: number;
  isPublished: boolean;
  requiredLevel: number;
  isLocked: boolean;
  tags: string | null;
  isBattleOnly: boolean;
}

export interface TestCaseDto {
  id: number;
  input: string;
  expectedOutput: string;
  orderIndex: number;
}

export interface HiddenTestCaseDto {
  id: number;
  input: string;
  expectedOutput: string;
  orderIndex: number;
  weight: number;
}

export interface ChallengeStatsDto {
  solverCount: number;
  totalSubmissions: number;
  acceptanceRate: number;
  averageSolveTimeMs: number | null;
}

export interface ChallengeCommentDto {
  id: number;
  content: string;
  isSpoiler: boolean;
  createdAt: string;
  userId: number;
  username: string;
  avatarUrl: string | null;
  parentCommentId: number | null;
}

export interface ChallengeCommentThreadDto {
  comment: ChallengeCommentDto;
  replies: ChallengeCommentDto[];
}

export interface DailyPuzzleDto {
  challengeId: number | null;
  title: string | null;
  difficulty: string | null;
  solvedToday: boolean;
}

export interface ChallengeDetailDto {
  id: number;
  title: string;
  description: string;
  categoryId: number;
  category: string;
  difficultyId: number;
  difficulty: string;
  timeLimitMs: number;
  memoryLimitMb: number;
  xpReward: number;
  coinReward: number;
  starterCode: string;
  constraints: string | null;
  inputFormat: string | null;
  outputFormat: string | null;
  sampleInput: string | null;
  sampleOutput: string | null;
  isPublished: boolean;
  requiredLevel: number;
  testCases: TestCaseDto[];
  hiddenTestCases: HiddenTestCaseDto[] | null;
  isAlreadySolved: boolean;
  // Admin-only: raw (unresolved) English variants, populated only when the requester is an Admin.
  titleEn: string | null;
  descriptionEn: string | null;
  constraintsEn: string | null;
  inputFormatEn: string | null;
  outputFormatEn: string | null;
  starterCodeEn: string | null;
  tags: string | null;
  isBattleOnly: boolean;
}
