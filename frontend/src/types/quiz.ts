export interface QuizAttemptListItem {
  id: number;
  quizId: number;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  xpEarned: number;
  completedAt: string;
}

export interface QuizListItemDto {
  id: number;
  title: string;
  category: string | null;
  xpReward: number;
  isPublished: boolean;
  questionCount: number;
}

export interface QuestionOptionDto {
  id: number;
  text: string;
}

export interface QuestionDto {
  id: number;
  text: string;
  options: QuestionOptionDto[];
}

export interface QuizAttemptViewDto {
  id: number;
  title: string;
  xpReward: number;
  questions: QuestionDto[];
}

export interface QuizAnswerInput {
  questionId: number;
  selectedOptionId: number | null;
}

export interface QuestionResultDto {
  questionId: number;
  text: string;
  isCorrect: boolean;
  selectedOptionId: number | null;
  correctOptionId: number;
  explanation: string | null;
}

export interface QuizAttemptResultDto {
  attemptId: number;
  score: number;
  totalQuestions: number;
  xpEarned: number;
  questions: QuestionResultDto[];
  newAchievements: string[];
}
