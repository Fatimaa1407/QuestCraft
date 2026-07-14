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
import { ProfilePage } from './features/profile/ProfilePage';
import { AdminDashboardPage } from './features/admin/AdminDashboardPage';
import { CategoriesAdminPage } from './features/admin/CategoriesAdminPage';
import { DifficultiesAdminPage } from './features/admin/DifficultiesAdminPage';
import { MarketplaceAdminPage } from './features/admin/MarketplaceAdminPage';
import { AchievementsAdminPage } from './features/admin/AchievementsAdminPage';
import { DailyQuestsAdminPage } from './features/admin/DailyQuestsAdminPage';
import { ChallengesAdminPage } from './features/admin/ChallengesAdminPage';
import { ChallengeEditPage } from './features/admin/ChallengeEditPage';
import { QuizzesAdminPage } from './features/admin/QuizzesAdminPage';
import { QuizEditPage } from './features/admin/QuizEditPage';
import { ExcelToolsPage } from './features/admin/ExcelToolsPage';
import { AuditLogPage } from './features/admin/AuditLogPage';

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
                <Route path="/profile" element={<ProfilePage />} />

                <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                  <Route path="/admin" element={<AdminDashboardPage />} />
                  <Route path="/admin/challenges" element={<ChallengesAdminPage />} />
                  <Route path="/admin/challenges/:id" element={<ChallengeEditPage />} />
                  <Route path="/admin/quizzes" element={<QuizzesAdminPage />} />
                  <Route path="/admin/quizzes/:id" element={<QuizEditPage />} />
                  <Route path="/admin/categories" element={<CategoriesAdminPage />} />
                  <Route path="/admin/difficulties" element={<DifficultiesAdminPage />} />
                  <Route path="/admin/marketplace" element={<MarketplaceAdminPage />} />
                  <Route path="/admin/achievements" element={<AchievementsAdminPage />} />
                  <Route path="/admin/daily-quests" element={<DailyQuestsAdminPage />} />
                  <Route path="/admin/excel" element={<ExcelToolsPage />} />
                  <Route path="/admin/audit-log" element={<AuditLogPage />} />
                </Route>
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
