import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Code2, LayoutDashboard, Swords, Dumbbell, Trophy, Award, User, ShoppingBag, LogOut, ShieldCheck, Users, MessageCircle, Gamepad2 } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { logout } from '../../api/auth';

const navItems = [
  { to: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard, end: true },
  { to: '/challenges', labelKey: 'nav.challenges', icon: Swords, end: false },
  { to: '/practice', labelKey: 'nav.practice', icon: Dumbbell, end: false },
  { to: '/leaderboard', labelKey: 'nav.leaderboard', icon: Trophy, end: false },
  { to: '/friends', labelKey: 'nav.friends', icon: Users, end: false },
  { to: '/chat', labelKey: 'nav.chat', icon: MessageCircle, end: false },
  { to: '/battles', labelKey: 'nav.battles', icon: Gamepad2, end: false },
  { to: '/achievements', labelKey: 'nav.achievements', icon: Award, end: false },
  { to: '/shop', labelKey: 'nav.shop', icon: ShoppingBag, end: false },
  { to: '/profile', labelKey: 'nav.profile', icon: User, end: false },
];

const adminNavItem = { to: '/admin', labelKey: 'nav.admin', icon: ShieldCheck, end: false };

const MotionNavLink = motion.create(NavLink);

export function Sidebar() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const visibleNavItems = user?.role === 'Admin' ? [...navItems, adminNavItem] : navItems;

  const handleLogout = async () => {
    try {
      await logout();
    } finally {
      clearAuth();
    }
  };

  return (
    <aside className="relative z-10 hidden w-20 shrink-0 flex-col items-center border-r border-slate-200/70 bg-white/70 py-5 backdrop-blur-xl sm:flex dark:border-white/[0.06] dark:bg-app-card-dark/60">
      <motion.div whileHover={{ scale: 1.06 }} whileTap={{ scale: 0.95 }}>
        <NavLink
          to="/"
          title={t('app.name')}
          className="mb-6 flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-app-accent to-app-accent-2 text-white shadow-lg shadow-app-accent/25"
        >
          <Code2 size={24} strokeWidth={2.25} />
        </NavLink>
      </motion.div>

      <nav className="flex flex-1 flex-col items-center gap-2">
        {visibleNavItems.map((item) => (
          <MotionNavLink
            key={item.to}
            to={item.to}
            end={item.end}
            title={t(item.labelKey)}
            whileHover={{ scale: 1.08 }}
            whileTap={{ scale: 0.94 }}
            className={({ isActive }: { isActive: boolean }) =>
              `relative flex h-12 w-12 items-center justify-center rounded-2xl transition-colors ${
                isActive
                  ? 'bg-app-accent/15 text-app-accent shadow-[0_0_16px_color-mix(in_srgb,var(--color-app-accent)_35%,transparent)] dark:bg-app-accent/20 dark:text-app-accent-2 dark:shadow-[0_0_16px_color-mix(in_srgb,var(--color-app-accent-2)_35%,transparent)]'
                  : 'text-slate-400 hover:bg-slate-100 hover:text-slate-700 dark:text-slate-500 dark:hover:bg-white/5 dark:hover:text-slate-200'
              }`
            }
          >
            {({ isActive }: { isActive: boolean }) => (
              <>
                {isActive && (
                  <motion.span
                    layoutId="sidebar-active-indicator"
                    className="absolute -left-2.5 h-5 w-1 rounded-full bg-gradient-to-b from-app-accent to-app-accent-2"
                    transition={{ type: 'spring', stiffness: 380, damping: 30 }}
                  />
                )}
                <item.icon size={22} strokeWidth={2} />
              </>
            )}
          </MotionNavLink>
        ))}
      </nav>

      <motion.button
        onClick={handleLogout}
        title={t('nav.logout')}
        whileHover={{ scale: 1.08 }}
        whileTap={{ scale: 0.94 }}
        className="flex h-12 w-12 items-center justify-center rounded-2xl text-slate-400 transition-colors hover:bg-red-500/10 hover:text-red-600 dark:text-slate-500 dark:hover:bg-red-500/10 dark:hover:text-red-400"
      >
        <LogOut size={22} strokeWidth={2} />
      </motion.button>
    </aside>
  );
}
