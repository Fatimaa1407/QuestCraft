import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Code2 } from 'lucide-react';
import { cascadeItemDelayed, FOOTER_START } from './landingMotion';

export function LandingFooter() {
  const { t } = useTranslation();
  const year = new Date().getFullYear();

  return (
    <motion.footer
      initial="hidden"
      animate="show"
      variants={cascadeItemDelayed(FOOTER_START)}
      className="border-t border-white/[0.08] py-16"
    >
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <div className="grid grid-cols-1 gap-10 sm:grid-cols-3">
          <div className="sm:col-span-1">
            <div className="flex items-center gap-2.5 text-[17px] font-bold tracking-tight text-white">
              <span className="flex h-8 w-8 items-center justify-center rounded-[9px] bg-gradient-to-br from-blue-500 to-cyan-400 text-[#051019]">
                <Code2 size={18} />
              </span>
              QuestCraft
            </div>
            <p className="mt-3 max-w-[30ch] text-[13.5px] leading-relaxed text-slate-400">{t('landing.footer.tagline')}</p>
          </div>

          <div>
            <h4 className="font-mono text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">
              {t('landing.footer.platformHeading')}
            </h4>
            <ul className="mt-4 flex flex-col gap-2.5 text-[13.5px] text-slate-400">
              <li><a href="#features" className="transition hover:text-white">{t('landing.footer.platformChallenges')}</a></li>
              <li><a href="#features" className="transition hover:text-white">{t('landing.footer.platformQuizzes')}</a></li>
              <li><a href="#features" className="transition hover:text-white">{t('landing.footer.platformLeaderboard')}</a></li>
              <li><a href="#features" className="transition hover:text-white">{t('landing.footer.platformMarketplace')}</a></li>
            </ul>
          </div>

          <div>
            <h4 className="font-mono text-xs font-semibold uppercase tracking-[0.08em] text-slate-500">
              {t('landing.footer.resourcesHeading')}
            </h4>
            <ul className="mt-4 flex flex-col gap-2.5 text-[13.5px] text-slate-400">
              <li><a href="#how" className="transition hover:text-white">{t('landing.footer.resourcesHow')}</a></li>
              <li><a href="#faq" className="transition hover:text-white">{t('landing.footer.resourcesFaq')}</a></li>
              <li><Link to="/login" className="transition hover:text-white">{t('landing.nav.login')}</Link></li>
              <li><Link to="/register" className="transition hover:text-white">{t('landing.nav.cta')}</Link></li>
            </ul>
          </div>
        </div>

        <div className="mt-14 border-t border-white/[0.08] pt-6 text-[12.5px] text-slate-500">
          {t('landing.footer.copyright', { year })}
        </div>
      </div>
    </motion.footer>
  );
}
