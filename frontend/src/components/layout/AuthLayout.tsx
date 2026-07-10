import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Code2, Sparkles, Swords, Trophy } from 'lucide-react';
import { CodeBackdrop } from '../ui/CodeBackdrop';
import { ParticleField } from '../ui/ParticleField';
import { ThemeSwitcher } from '../ui/ThemeSwitcher';
import { LanguageSwitcher } from '../ui/LanguageSwitcher';
import { EditorMock, LeaderboardMock, StatsMock } from '../../features/auth/BrandingMocks';

// Clean 50/50 split: hero content + mockups live only in the left half, the form card stays in
// the right half. Flat deep-navy canvas (not a saturated gradient) with indigo/cyan glow accents
// from CodeBackdrop carries the color — GitHub/Linear/Vercel-style technical atmosphere rather
// than a game-like purple wash. The card itself is a mostly-opaque "elevated surface", not glass.
export function AuthLayout({ children }: { children: ReactNode }) {
  const { t } = useTranslation();

  return (
    <div className="relative flex min-h-svh flex-col overflow-hidden bg-slate-950">
      <ParticleField />

      <div className="relative z-20 flex items-center justify-between px-6 py-6 sm:px-10">
        <div className="flex items-center gap-2 text-lg font-semibold text-white">
          <Code2 size={22} className="text-cyan-400" />
          {t('app.name')}
        </div>
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeSwitcher />
        </div>
      </div>

      <div className="relative z-10 flex flex-1 flex-col lg:flex-row">
        {/* Left: hero */}
        <div className="relative flex flex-1 flex-col justify-center gap-8 overflow-hidden px-8 py-12 lg:px-16">
          <CodeBackdrop />

          <div className="relative z-10 max-w-md">
            <h2 className="text-4xl font-semibold leading-tight text-white">{t('auth.branding.tagline')}</h2>
            <ul className="mt-6 space-y-3 text-sm text-slate-300">
              <li className="flex items-center gap-2">
                <Sparkles size={16} className="text-cyan-400" /> {t('auth.branding.feature1')}
              </li>
              <li className="flex items-center gap-2">
                <Trophy size={16} className="text-cyan-400" /> {t('auth.branding.feature2')}
              </li>
              <li className="flex items-center gap-2">
                <Swords size={16} className="text-cyan-400" /> {t('auth.branding.feature3')}
              </li>
            </ul>
          </div>

          {/* One diagonal cascade, each card offset consistently down-right from the last so the
              three read as a single composition. Editor is sized up to stay the dominant piece;
              XP/Leaderboard are smaller supporting cards that only clip its bottom edge slightly. */}
          <div className="relative z-10 hidden h-96 max-w-md xl:block">
            <EditorMock className="absolute left-0 top-0 z-10 w-80" />
            <StatsMock className="absolute left-24 top-36 z-20 w-56" />
            <LeaderboardMock className="absolute left-36 top-56 z-30 w-56" />
          </div>
        </div>

        {/* Right: form, vertically centered */}
        <div className="relative flex flex-1 items-center justify-center px-6 py-12 lg:px-12">
          <div className="w-full max-w-[21rem] rounded-2xl border border-slate-200 bg-white p-7 shadow-2xl shadow-black/40 dark:border-slate-700/80 dark:bg-slate-800/95 dark:shadow-black/60 dark:ring-1 dark:ring-white/5">
            {children}
          </div>
        </div>
      </div>
    </div>
  );
}
