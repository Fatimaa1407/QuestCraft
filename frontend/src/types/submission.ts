export type SubmissionVerdict = 'Pending' | 'Accepted' | 'WrongAnswer' | 'TimeLimitExceeded' | 'RuntimeError' | 'CompileError';

export interface SubmissionListItem {
  id: number;
  challengeId: number;
  challengeTitle: string;
  verdict: SubmissionVerdict;
  submittedAt: string;
  executionTimeMs: number;
}

export interface RunTestResultDto {
  passed: boolean;
  input: string;
  expectedOutput: string;
  actualOutput: string;
  executionTimeMs: number;
}

export interface RunResultDto {
  verdict: SubmissionVerdict;
  executionTimeMs: number;
  compileErrorMessage: string | null;
  results: RunTestResultDto[];
}

export interface SubmissionTestResultDto {
  isHidden: boolean;
  passed: boolean;
  input: string | null;
  expectedOutput: string | null;
  actualOutput: string | null;
}

export interface SubmissionResultDto {
  submissionId: number;
  verdict: SubmissionVerdict;
  passedTestCases: number;
  totalTestCases: number;
  executionTimeMs: number;
  memoryUsedKb: number;
  xpEarned: number;
  coinEarned: number;
  compileErrorMessage: string | null;
  results: SubmissionTestResultDto[];
  newAchievements: string[];
  totalXp: number;
  totalCoins: number;
  level: number;
}
