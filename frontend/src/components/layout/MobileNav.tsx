import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { LayoutDashboard, Swords, Dumbbell, User, Trophy, Award, ShoppingBag, LogOut, ShieldCheck, MoreHorizontal, X, Users, MessageCircle, Gamepad2 } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { logout } from '../../api/auth';
import { Z_INDEX } from '../../styles/zIndex';

// Primary tabs shown directly in the bottom bar — the four most-used destinations.
const tabItems = [
  { to: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard, end: true },
  { to: '/challenges', labelKey: 'nav.challenges', icon: Swords, end: false },
  { to: '/practice', labelKey: 'nav.practice', icon: Dumbbell, end: false },
  { to: '/profile', labelKey: 'nav.profile', icon: User, end: false },
];

// Everything else lives behind "More" — mirrors Sidebar.tsx's remaining items.
const moreItems = [
  { to: '/leaderboard', labelKey: 'nav.leaderboard', icon: Trophy, end: false },
  { to: '/friends', labelKey: 'nav.friends', icon: Users, end: false },
  { to: '/chat', labelKey: 'nav.chat', icon: MessageCircle, end: false },
  { to: '/battles', labelKey: 'nav.battles', icon: Gamepad2, end: false },
  { to: '/achievements', labelKey: 'nav.achievements', icon: Award, end: false },
  { to: '/shop', labelKey: 'nav.shop', icon: ShoppingBag, end: false },
];

const adminMoreItem = { to: '/admin', labelKey: 'nav.admin', icon: ShieldCheck, end: false };

const MotionNavLink = motion.create(NavLink);

function tabLinkClass(isActive: boolean) {
  return `flex h-full flex-1 flex-col items-center justify-center gap-1 text-[11px] font-medium transition-colors ${
    isActive
      ? 'text-blue-600 dark:text-cyan-400'
      : 'text-slate-400 hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300'
  }`;
}

export function MobileNav() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const [isMoreOpen, setIsMoreOpen] = useState(false);
  const visibleMoreItems = user?.role === 'Admin' ? [...moreItems, adminMoreItem] : moreItems;

  const handleLogout = async () => {
    setIsMoreOpen(false);
    try {
      await logout();
    } finally {
      clearAuth();
    }
  };

  return (
    <>
      <nav
        className="fixed inset-x-0 bottom-0 flex h-16 items-stretch border-t border-slate-200/70 bg-white/85 backdrop-blur-xl sm:hidden dark:border-white/[0.06] dark:bg-app-card-dark/85"
        style={{ zIndex: Z_INDEX.mobileNav, paddingBottom: 'env(safe-area-inset-bottom)' }}
        aria-label={t('nav.mobileNavLabel')}
      >
        {tabItems.map((item) => (
          <NavLink key={item.to} to={item.to} end={item.end} className={({ isActive }) => tabLinkClass(isActive)}>
            {({ isActive }) => (
              <>
                <item.icon size={20} strokeWidth={isActive ? 2.4 : 2} />
                <span>{t(item.labelKey)}</span>
              </>
            )}
          </NavLink>
        ))}
        <button
          type="button"
          onClick={() => setIsMoreOpen(true)}
          className={tabLinkClass(isMoreOpen)}
          aria-haspopup="dialog"
          aria-expanded={isMoreOpen}
        >
          <MoreHorizontal size={20} strokeWidth={isMoreOpen ? 2.4 : 2} />
          <span>{t('nav.more')}</span>
        </button>
      </nav>

      <MoreDrawer
        isOpen={isMoreOpen}
        onClose={() => setIsMoreOpen(false)}
        items={visibleMoreItems}
        onLogout={handleLogout}
      />
    </>
  );
}

interface MoreDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  items: typeof moreItems;
  onLogout: () => void;
}

function MoreDrawer({ isOpen, onClose, items, onLogout }: MoreDrawerProps) {
  const { t } = useTranslation();

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
          className="fixed inset-0 flex items-end bg-slate-950/50 backdrop-blur-sm sm:hidden"
          style={{ zIndex: Z_INDEX.modal }}
          onClick={onClose}
        >
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: 'spring', stiffness: 340, damping: 32 }}
            onClick={(e) => e.stopPropagation()}
            className="w-full rounded-t-3xl border-t border-slate-200/70 bg-white/95 p-4 pb-[calc(1rem+env(safe-area-inset-bottom))] backdrop-blur-xl dark:border-white/[0.08] dark:bg-slate-900/95"
          >
            <div className="mb-3 flex items-center justify-between px-1">
              <h2 className="text-sm font-semibold text-slate-900 dark:text-white">{t('nav.more')}</h2>
              <button
                type="button"
                onClick={onClose}
                aria-label={t('common.close')}
                className="flex h-8 w-8 items-center justify-center rounded-full text-slate-400 transition hover:bg-slate-100 hover:text-slate-700 dark:hover:bg-white/10 dark:hover:text-slate-200"
              >
                <X size={16} />
              </button>
            </div>

            <div className="grid grid-cols-1 gap-1">
              {items.map((item) => (
                <MotionNavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  onClick={onClose}
                  whileTap={{ scale: 0.98 }}
                  className={({ isActive }: { isActive: boolean }) =>
                    `flex items-center gap-3 rounded-2xl px-3 py-3 text-sm font-medium transition-colors ${
                      isActive
                        ? 'bg-blue-500/15 text-blue-600 dark:bg-blue-500/20 dark:text-cyan-400'
                        : 'text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-white/5'
                    }`
                  }
                >
                  <item.icon size={18} strokeWidth={2} />
                  {t(item.labelKey)}
                </MotionNavLink>
              ))}

              <motion.button
                type="button"
                onClick={onLogout}
                whileTap={{ scale: 0.98 }}
                className="flex items-center gap-3 rounded-2xl px-3 py-3 text-left text-sm font-medium text-red-600 transition-colors hover:bg-red-500/10 dark:text-red-400 dark:hover:bg-red-500/10"
              >
                <LogOut size={18} strokeWidth={2} />
                {t('nav.logout')}
              </motion.button>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
