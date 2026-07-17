import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import {
  Swords,
  ListChecks,
  FolderTree,
  Gauge,
  ShoppingBag,
  Award,
  ClipboardList,
  FileSpreadsheet,
  ScrollText,
  Users,
  ClipboardCheck,
  Activity,
  UserPlus,
  PartyPopper,
  Gamepad2,
} from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { StatCard } from '../../components/ui/StatCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { getAdminDashboardSummary } from '../../api/admin';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const sections = [
  { to: '/admin/users', icon: Users, titleKey: 'admin.sections.users', descKey: 'admin.sections.usersDesc' },
  { to: '/admin/challenges', icon: Swords, titleKey: 'admin.sections.challenges', descKey: 'admin.sections.challengesDesc' },
  { to: '/admin/quizzes', icon: ListChecks, titleKey: 'admin.sections.quizzes', descKey: 'admin.sections.quizzesDesc' },
  { to: '/admin/categories', icon: FolderTree, titleKey: 'admin.sections.categories', descKey: 'admin.sections.categoriesDesc' },
  { to: '/admin/difficulties', icon: Gauge, titleKey: 'admin.sections.difficulties', descKey: 'admin.sections.difficultiesDesc' },
  { to: '/admin/marketplace', icon: ShoppingBag, titleKey: 'admin.sections.marketplace', descKey: 'admin.sections.marketplaceDesc' },
  { to: '/admin/achievements', icon: Award, titleKey: 'admin.sections.achievements', descKey: 'admin.sections.achievementsDesc' },
  { to: '/admin/daily-quests', icon: ClipboardList, titleKey: 'admin.sections.dailyQuests', descKey: 'admin.sections.dailyQuestsDesc' },
  { to: '/admin/excel', icon: FileSpreadsheet, titleKey: 'admin.sections.excel', descKey: 'admin.sections.excelDesc' },
  { to: '/admin/audit-log', icon: ScrollText, titleKey: 'admin.sections.auditLog', descKey: 'admin.sections.auditLogDesc' },
  { to: '/admin/activity-today', icon: Activity, titleKey: 'admin.sections.activityToday', descKey: 'admin.sections.activityTodayDesc' },
  { to: '/admin/seasonal-events', icon: PartyPopper, titleKey: 'admin.sections.seasonalEvents', descKey: 'admin.sections.seasonalEventsDesc' },
  { to: '/admin/battle-pool', icon: Gamepad2, titleKey: 'admin.sections.battlePool', descKey: 'admin.sections.battlePoolDesc' },
];

export function AdminDashboardPage() {
  const { t } = useTranslation();
  const summaryQuery = useQuery({ queryKey: ['admin', 'dashboard-summary'], queryFn: getAdminDashboardSummary });
  const summary = summaryQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.title')}</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('admin.subtitle')}</p>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {summaryQuery.isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
              <GlassCard key={i} className="p-6">
                <Skeleton className="mb-4 h-11 w-11 rounded-xl" />
                <Skeleton className="h-3.5 w-20" />
                <Skeleton className="mt-2 h-7 w-14" />
              </GlassCard>
            ))
          : (
              <>
                <Link to="/admin/users">
                  <StatCard icon={Users} label={t('admin.dashboard.totalUsers')} value={summary?.totalUsers ?? 0} tint="blue" />
                </Link>
                <Link to="/admin/challenges">
                  <StatCard icon={Swords} label={t('admin.sections.challenges')} value={summary?.totalChallenges ?? 0} tint="cyan" />
                </Link>
                <Link to="/admin/quizzes">
                  <StatCard icon={ListChecks} label={t('admin.sections.quizzes')} value={summary?.totalQuizzes ?? 0} tint="cyan" />
                </Link>
                <Link to="/admin/activity-today">
                  <StatCard icon={ClipboardCheck} label={t('admin.dashboard.submissionsToday')} value={summary?.submissionsToday ?? 0} tint="amber" />
                </Link>
                <Link to="/admin/users">
                  <StatCard icon={Activity} label={t('admin.dashboard.activeUsersToday')} value={summary?.activeUsersToday ?? 0} tint="violet" />
                </Link>
                <Link to="/admin/users">
                  <StatCard icon={UserPlus} label={t('admin.dashboard.newUsersThisWeek')} value={summary?.newUsersThisWeek ?? 0} tint="emerald" />
                </Link>
              </>
            )}
      </motion.div>

      <motion.div variants={staggerContainer} className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <motion.div key={section.to} variants={fadeInUp}>
            <Link to={section.to}>
              <GlassCard className="flex items-start gap-4 p-6">
                <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-500 to-cyan-500 text-white shadow-lg">
                  <section.icon size={20} />
                </span>
                <div className="min-w-0">
                  <h2 className="font-semibold text-slate-900 dark:text-white">{t(section.titleKey)}</h2>
                  <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">{t(section.descKey)}</p>
                </div>
              </GlassCard>
            </Link>
          </motion.div>
        ))}
      </motion.div>
    </motion.div>
  );
}
