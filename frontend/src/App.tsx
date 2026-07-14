import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './app/queryClient';
import { AuthInitializer } from './app/AuthInitializer';
import { ProtectedRoute } from './components/layout/ProtectedRoute';
import { AppLayout } from './components/layout/AppLayout';
import { LoginPage } from './features/auth/LoginPage';
import { RegisterPage } from './features/auth/RegisterPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { ChallengesListPage } from './features/challenges/ChallengesListPage';
import { ChallengeDetailPage } from './features/challenges/ChallengeDetailPage';
import { QuizListPage } from './features/quiz/QuizListPage';
import { QuizAttemptPage } from './features/quiz/QuizAttemptPage';
import { ShopPage } from './features/shop/ShopPage';
import { LeaderboardPage } from './features/leaderboard/LeaderboardPage';
import { AchievementsPage } from './features/achievements/AchievementsPage';
import { ComingSoonPage } from './features/misc/ComingSoonPage';

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthInitializer>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            <Route element={<ProtectedRoute />}>
              <Route element={<AppLayout />}>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/challenges" element={<ChallengesListPage />} />
                <Route path="/challenges/:id" element={<ChallengeDetailPage />} />
                <Route path="/practice" element={<QuizListPage />} />
                <Route path="/practice/:id" element={<QuizAttemptPage />} />
                <Route path="/leaderboard" element={<LeaderboardPage />} />
                <Route path="/achievements" element={<AchievementsPage />} />
                <Route path="/shop" element={<ShopPage />} />
                <Route path="/profile" element={<ComingSoonPage titleKey="nav.profile" />} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AuthInitializer>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
