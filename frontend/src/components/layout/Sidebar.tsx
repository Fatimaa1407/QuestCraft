import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Code2, LayoutDashboard, Swords, Dumbbell, Trophy, Award, User, ShoppingBag } from 'lucide-react';

const navItems = [
  { to: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard, end: true },
  { to: '/challenges', labelKey: 'nav.challenges', icon: Swords, end: false },
  { to: '/practice', labelKey: 'nav.practice', icon: Dumbbell, end: false },
  { to: '/leaderboard', labelKey: 'nav.leaderboard', icon: Trophy, end: false },
  { to: '/achievements', labelKey: 'nav.achievements', icon: Award, end: false },
  { to: '/shop', labelKey: 'nav.shop', icon: ShoppingBag, end: false },
  { to: '/profile', labelKey: 'nav.profile', icon: User, end: false },
];

export function Sidebar() {
  const { t } = useTranslation();

  return (
    <aside className="relative z-10 hidden w-20 shrink-0 flex-col items-center border-r border-slate-200/70 bg-white/70 py-5 backdrop-blur-xl sm:flex dark:border-white/[0.06] dark:bg-app-card-dark/60">
      <NavLink to="/" title={t('app.name')} className="mb-6 flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 text-white shadow-lg shadow-blue-500/25">
        <Code2 size={24} strokeWidth={2.25} />
      </NavLink>

      <nav className="flex flex-1 flex-col items-center gap-2">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            title={t(item.labelKey)}
            className={({ isActive }) =>
              `flex h-12 w-12 items-center justify-center rounded-2xl transition ${
                isActive
                  ? 'bg-blue-500/15 text-blue-600 dark:bg-blue-500/20 dark:text-cyan-400'
                  : 'text-slate-400 hover:bg-slate-100 hover:text-slate-700 dark:text-slate-500 dark:hover:bg-white/5 dark:hover:text-slate-200'
              }`
            }
          >
            <item.icon size={22} strokeWidth={2} />
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
