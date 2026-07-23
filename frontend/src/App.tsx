import { lazy, Suspense } from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Loader2 } from 'lucide-react';
import { queryClient } from './app/queryClient';
import { AuthInitializer } from './app/AuthInitializer';
import { ProtectedRoute } from './components/layout/ProtectedRoute';
import { AppLayout } from './components/layout/AppLayout';

// Every page is a separate chunk, loaded on first navigation instead of all upfront — the eager
// version of this file put all 30 pages (Monaco editor, charts, the whole admin section, ...) in
// one ~1.3MB bundle. Named exports need the `.then(m => ({ default: m.X }))` adapter since
// React.lazy only accepts a module with a default export.
const LoginPage = lazy(() => import('./features/auth/LoginPage').then((m) => ({ default: m.LoginPage })));
const RegisterPage = lazy(() => import('./features/auth/RegisterPage').then((m) => ({ default: m.RegisterPage })));
const LandingPage = lazy(() => import('./features/landing/LandingPage').then((m) => ({ default: m.LandingPage })));
const DashboardPage = lazy(() => import('./features/dashboard/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const ChallengesListPage = lazy(() => import('./features/challenges/ChallengesListPage').then((m) => ({ default: m.ChallengesListPage })));
const ChallengeDetailPage = lazy(() => import('./features/challenges/ChallengeDetailPage').then((m) => ({ default: m.ChallengeDetailPage })));
const SpeedChallengePage = lazy(() => import('./features/challenges/SpeedChallengePage').then((m) => ({ default: m.SpeedChallengePage })));
const QuizListPage = lazy(() => import('./features/quiz/QuizListPage').then((m) => ({ default: m.QuizListPage })));
const QuizAttemptPage = lazy(() => import('./features/quiz/QuizAttemptPage').then((m) => ({ default: m.QuizAttemptPage })));
const ShopPage = lazy(() => import('./features/shop/ShopPage').then((m) => ({ default: m.ShopPage })));
const LeaderboardPage = lazy(() => import('./features/leaderboard/LeaderboardPage').then((m) => ({ default: m.LeaderboardPage })));
const FriendsPage = lazy(() => import('./features/friends/FriendsPage').then((m) => ({ default: m.FriendsPage })));
const ChatPage = lazy(() => import('./features/chat/ChatPage').then((m) => ({ default: m.ChatPage })));
const BattleLobbyPage = lazy(() => import('./features/battles/BattleLobbyPage').then((m) => ({ default: m.BattleLobbyPage })));
const BattleRoomPage = lazy(() => import('./features/battles/BattleRoomPage').then((m) => ({ default: m.BattleRoomPage })));
const AchievementsPage = lazy(() => import('./features/achievements/AchievementsPage').then((m) => ({ default: m.AchievementsPage })));
const ProfilePage = lazy(() => import('./features/profile/ProfilePage').then((m) => ({ default: m.ProfilePage })));
const StatisticsPage = lazy(() => import('./features/statistics/StatisticsPage').then((m) => ({ default: m.StatisticsPage })));
const SettingsPage = lazy(() => import('./features/settings/SettingsPage').then((m) => ({ default: m.SettingsPage })));
const AdminDashboardPage = lazy(() => import('./features/admin/AdminDashboardPage').then((m) => ({ default: m.AdminDashboardPage })));
const CategoriesAdminPage = lazy(() => import('./features/admin/CategoriesAdminPage').then((m) => ({ default: m.CategoriesAdminPage })));
const DifficultiesAdminPage = lazy(() => import('./features/admin/DifficultiesAdminPage').then((m) => ({ default: m.DifficultiesAdminPage })));
const MarketplaceAdminPage = lazy(() => import('./features/admin/MarketplaceAdminPage').then((m) => ({ default: m.MarketplaceAdminPage })));
const AchievementsAdminPage = lazy(() => import('./features/admin/AchievementsAdminPage').then((m) => ({ default: m.AchievementsAdminPage })));
const DailyQuestsAdminPage = lazy(() => import('./features/admin/DailyQuestsAdminPage').then((m) => ({ default: m.DailyQuestsAdminPage })));
const ChallengesAdminPage = lazy(() => import('./features/admin/ChallengesAdminPage').then((m) => ({ default: m.ChallengesAdminPage })));
const BattlePoolAdminPage = lazy(() => import('./features/admin/BattlePoolAdminPage').then((m) => ({ default: m.BattlePoolAdminPage })));
const ChallengeEditPage = lazy(() => import('./features/admin/ChallengeEditPage').then((m) => ({ default: m.ChallengeEditPage })));
const QuizzesAdminPage = lazy(() => import('./features/admin/QuizzesAdminPage').then((m) => ({ default: m.QuizzesAdminPage })));
const QuizEditPage = lazy(() => import('./features/admin/QuizEditPage').then((m) => ({ default: m.QuizEditPage })));
const ExcelToolsPage = lazy(() => import('./features/admin/ExcelToolsPage').then((m) => ({ default: m.ExcelToolsPage })));
const AuditLogPage = lazy(() => import('./features/admin/AuditLogPage').then((m) => ({ default: m.AuditLogPage })));
const UsersAdminPage = lazy(() => import('./features/admin/UsersAdminPage').then((m) => ({ default: m.UsersAdminPage })));
const ActivityTodayAdminPage = lazy(() => import('./features/admin/ActivityTodayAdminPage').then((m) => ({ default: m.ActivityTodayAdminPage })));
const SeasonalEventsAdminPage = lazy(() => import('./features/admin/SeasonalEventsAdminPage').then((m) => ({ default: m.SeasonalEventsAdminPage })));

function RouteFallback() {
  return (
    <div className="flex min-h-[40vh] items-center justify-center">
      <Loader2 size={28} className="animate-spin text-slate-400 dark:text-slate-600" />
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthInitializer>
          <Suspense fallback={<RouteFallback />}>
            <Routes>
              <Route path="/welcome" element={<LandingPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              <Route element={<ProtectedRoute />}>
                <Route element={<AppLayout />}>
                  <Route path="/" element={<DashboardPage />} />
                  <Route path="/challenges" element={<ChallengesListPage />} />
                  <Route path="/challenges/:id" element={<ChallengeDetailPage />} />
                  <Route path="/challenges/:id/speed" element={<SpeedChallengePage />} />
                  <Route path="/practice" element={<QuizListPage />} />
                  <Route path="/practice/:id" element={<QuizAttemptPage />} />
                  <Route path="/leaderboard" element={<LeaderboardPage />} />
                  <Route path="/friends" element={<FriendsPage />} />
                  <Route path="/chat" element={<ChatPage />} />
                  <Route path="/chat/:userId" element={<ChatPage />} />
                  <Route path="/battles" element={<BattleLobbyPage />} />
                  <Route path="/battles/:id" element={<BattleRoomPage />} />
                  <Route path="/achievements" element={<AchievementsPage />} />
                  <Route path="/shop" element={<ShopPage />} />
                  <Route path="/profile" element={<ProfilePage />} />
                  <Route path="/statistics" element={<StatisticsPage />} />
                  <Route path="/settings" element={<SettingsPage />} />

                  <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                    <Route path="/admin" element={<AdminDashboardPage />} />
                    <Route path="/admin/challenges" element={<ChallengesAdminPage />} />
                    <Route path="/admin/challenges/:id" element={<ChallengeEditPage />} />
                    <Route path="/admin/battle-pool" element={<BattlePoolAdminPage />} />
                    <Route path="/admin/quizzes" element={<QuizzesAdminPage />} />
                    <Route path="/admin/quizzes/:id" element={<QuizEditPage />} />
                    <Route path="/admin/categories" element={<CategoriesAdminPage />} />
                    <Route path="/admin/difficulties" element={<DifficultiesAdminPage />} />
                    <Route path="/admin/marketplace" element={<MarketplaceAdminPage />} />
                    <Route path="/admin/achievements" element={<AchievementsAdminPage />} />
                    <Route path="/admin/daily-quests" element={<DailyQuestsAdminPage />} />
                    <Route path="/admin/excel" element={<ExcelToolsPage />} />
                    <Route path="/admin/audit-log" element={<AuditLogPage />} />
                    <Route path="/admin/users" element={<UsersAdminPage />} />
                    <Route path="/admin/activity-today" element={<ActivityTodayAdminPage />} />
                    <Route path="/admin/seasonal-events" element={<SeasonalEventsAdminPage />} />
                  </Route>
                </Route>
              </Route>

              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Suspense>
        </AuthInitializer>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
