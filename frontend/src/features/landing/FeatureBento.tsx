import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Award, ListChecks, ShoppingBag, Swords, Trophy, Zap, type LucideIcon } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { cardHoverTransition, cascadeGroup, cascadeItem, cascadeItemDelayed, FEATURES_START } from './landingMotion';

const tints = {
  blue: 'bg-blue-500/[0.14] text-blue-400',
  cyan: 'bg-cyan-500/[0.14] text-cyan-400',
  amber: 'bg-amber-500/[0.14] text-amber-400',
  emerald: 'bg-emerald-500/[0.14] text-emerald-400',
  violet: 'bg-violet-500/[0.14] text-violet-400',
} as const;

// Every feature card shares the same hover feedback: a 6px lift, a stronger cyan-tinted
// shadow, and the animated gradient border ring GlassCard already ships (glow prop) —
// all in one 250ms easeOut transition, matching the rest of the landing page.
const cardHover = {
  glow: true,
  whileHover: { y: -6, boxShadow: '0 30px 60px -28px rgba(34,211,238,0.32)' },
  transition: cardHoverTransition,
} as const;

function FeatureIcon({ icon: Icon, tint }: { icon: LucideIcon; tint: keyof typeof tints }) {
  return (
    <span className={`flex h-[42px] w-[42px] items-center justify-center rounded-[11px] ${tints[tint]}`}>
      <Icon size={20} />
    </span>
  );
}

export function FeatureBento() {
  const { t } = useTranslation();

  return (
    <section id="features" className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(FEATURES_START)}
          className="max-w-[640px]"
        >
          <span className="font-mono text-xs uppercase tracking-[0.14em] text-cyan-400">{t('landing.features.eyebrow')}</span>
          <h2 className="mt-3.5 text-[1.9rem] font-extrabold tracking-tight text-white sm:text-[2.5rem]" style={{ textWrap: 'balance' }}>
            {t('landing.features.title')}
          </h2>
          <p className="mt-3.5 text-[15.5px] leading-relaxed text-slate-400">{t('landing.features.subtitle')}</p>
        </motion.div>

        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeGroup(FEATURES_START)}
          className="mt-12 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4 lg:[grid-auto-rows:168px]"
        >
          <motion.div variants={cascadeItem} className="sm:col-span-2 lg:col-span-2 lg:row-span-2">
            <GlassCard {...cardHover} className="flex h-full flex-col justify-between p-6">
              <div>
                <FeatureIcon icon={Swords} tint="blue" />
                <h3 className="mt-3.5 text-[17px] font-bold tracking-tight text-white">{t('landing.features.challengesTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.challengesDesc')}</p>
              </div>
              <div className="mt-4 rounded-lg border border-white/[0.08] bg-black/20 px-3 py-2.5 font-mono text-xs text-slate-500">
                ✓ {t('landing.features.challengesSnippet')}
              </div>
            </GlassCard>
          </motion.div>

          <motion.div variants={cascadeItem}>
            <GlassCard {...cardHover} className="flex h-full flex-col justify-center gap-3 p-6">
              <FeatureIcon icon={ListChecks} tint="cyan" />
              <div>
                <h3 className="text-[17px] font-bold tracking-tight text-white">{t('landing.features.quizzesTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.quizzesDesc')}</p>
              </div>
            </GlassCard>
          </motion.div>

          <motion.div variants={cascadeItem}>
            <GlassCard {...cardHover} className="flex h-full flex-col justify-center gap-3 p-6">
              <FeatureIcon icon={Zap} tint="amber" />
              <div>
                <h3 className="text-[17px] font-bold tracking-tight text-white">{t('landing.features.xpTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.xpDesc')}</p>
              </div>
            </GlassCard>
          </motion.div>

          <motion.div variants={cascadeItem} className="sm:col-span-2 lg:col-span-2">
            <GlassCard {...cardHover} className="flex h-full flex-col justify-center gap-3 p-6">
              <FeatureIcon icon={Trophy} tint="emerald" />
              <div>
                <h3 className="text-[17px] font-bold tracking-tight text-white">{t('landing.features.leaderboardTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.leaderboardDesc')}</p>
              </div>
            </GlassCard>
          </motion.div>

          <motion.div variants={cascadeItem}>
            <GlassCard {...cardHover} className="flex h-full flex-col justify-center gap-3 p-6">
              <FeatureIcon icon={ShoppingBag} tint="violet" />
              <div>
                <h3 className="text-[17px] font-bold tracking-tight text-white">{t('landing.features.marketplaceTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.marketplaceDesc')}</p>
              </div>
            </GlassCard>
          </motion.div>

          <motion.div variants={cascadeItem}>
            <GlassCard {...cardHover} className="flex h-full flex-col justify-center gap-3 p-6">
              <FeatureIcon icon={Award} tint="blue" />
              <div>
                <h3 className="text-[17px] font-bold tracking-tight text-white">{t('landing.features.achievementsTitle')}</h3>
                <p className="mt-2 text-[13.5px] leading-relaxed text-slate-400">{t('landing.features.achievementsDesc')}</p>
              </div>
            </GlassCard>
          </motion.div>
        </motion.div>
      </div>
    </section>
  );
}
