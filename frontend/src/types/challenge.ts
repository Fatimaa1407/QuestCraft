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
  hint: string | null;
  hasHint: boolean;
  isHintUnlocked: boolean;
  isPublished: boolean;
  requiredLevel: number;
  testCases: TestCaseDto[];
  hiddenTestCases: HiddenTestCaseDto[] | null;
  isAlreadySolved: boolean;
}
