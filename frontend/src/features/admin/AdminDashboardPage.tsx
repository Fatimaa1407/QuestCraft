import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Swords, ListChecks, FolderTree, Gauge, ShoppingBag, Award, ClipboardList, FileSpreadsheet, ScrollText } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const sections = [
  { to: '/admin/challenges', icon: Swords, titleKey: 'admin.sections.challenges', descKey: 'admin.sections.challengesDesc' },
  { to: '/admin/quizzes', icon: ListChecks, titleKey: 'admin.sections.quizzes', descKey: 'admin.sections.quizzesDesc' },
  { to: '/admin/categories', icon: FolderTree, titleKey: 'admin.sections.categories', descKey: 'admin.sections.categoriesDesc' },
  { to: '/admin/difficulties', icon: Gauge, titleKey: 'admin.sections.difficulties', descKey: 'admin.sections.difficultiesDesc' },
  { to: '/admin/marketplace', icon: ShoppingBag, titleKey: 'admin.sections.marketplace', descKey: 'admin.sections.marketplaceDesc' },
  { to: '/admin/achievements', icon: Award, titleKey: 'admin.sections.achievements', descKey: 'admin.sections.achievementsDesc' },
  { to: '/admin/daily-quests', icon: ClipboardList, titleKey: 'admin.sections.dailyQuests', descKey: 'admin.sections.dailyQuestsDesc' },
  { to: '/admin/excel', icon: FileSpreadsheet, titleKey: 'admin.sections.excel', descKey: 'admin.sections.excelDesc' },
  { to: '/admin/audit-log', icon: ScrollText, titleKey: 'admin.sections.auditLog', descKey: 'admin.sections.auditLogDesc' },
];

export function AdminDashboardPage() {
  const { t } = useTranslation();

  return (
    <div>
      <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('admin.title')}</h1>
      <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('admin.subtitle')}</p>

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        animate="show"
        className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3"
      >
        {sections.map((section) => (
          <motion.div key={section.to} variants={fadeInUp}>
            <Link to={section.to}>
              <GlassCard className="flex items-start gap-4 p-5">
                <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-500 to-cyan-500 text-white shadow-lg">
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
    </div>
  );
}
