import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { CheckCircle2, PenLine, Search, Zap, type LucideIcon } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { cascadeGroup, cascadeItem, cascadeItemDelayed, landingEase, TIMELINE_START } from './landingMotion';

const tints = {
  blue: 'bg-blue-500/[0.14] text-blue-400',
  cyan: 'bg-cyan-500/[0.14] text-cyan-400',
  emerald: 'bg-emerald-500/[0.14] text-emerald-400',
  amber: 'bg-amber-500/[0.14] text-amber-400',
} as const;

const steps: { icon: LucideIcon; tint: keyof typeof tints; titleKey: string; descKey: string }[] = [
  { icon: Search, tint: 'blue', titleKey: 'landing.how.step1Title', descKey: 'landing.how.step1Desc' },
  { icon: PenLine, tint: 'cyan', titleKey: 'landing.how.step2Title', descKey: 'landing.how.step2Desc' },
  { icon: CheckCircle2, tint: 'emerald', titleKey: 'landing.how.step3Title', descKey: 'landing.how.step3Desc' },
  { icon: Zap, tint: 'amber', titleKey: 'landing.how.step4Title', descKey: 'landing.how.step4Desc' },
];

export function HowItWorks() {
  const { t } = useTranslation();

  return (
    <section id="how" className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(TIMELINE_START)}
          className="max-w-[640px]"
        >
          <span className="font-mono text-xs uppercase tracking-[0.14em] text-cyan-400">{t('landing.how.eyebrow')}</span>
          <h2 className="mt-3.5 text-[1.9rem] font-extrabold tracking-tight text-white sm:text-[2.5rem]" style={{ textWrap: 'balance' }}>
            {t('landing.how.title')}
          </h2>
          <p className="mt-3.5 text-[15.5px] leading-relaxed text-slate-400">{t('landing.how.subtitle')}</p>
        </motion.div>

        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeGroup(TIMELINE_START)}
          className="relative mt-12 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
        >
          <div className="pointer-events-none absolute inset-x-16 top-[43px] hidden h-px overflow-hidden bg-white/[0.08] lg:block">
            <motion.div
              initial={{ scaleX: 0 }}
              animate={{ scaleX: 1 }}
              transition={{ duration: 1.1, delay: TIMELINE_START, ease: landingEase }}
              style={{ transformOrigin: 'left' }}
              className="h-full w-full bg-gradient-to-r from-blue-500 via-cyan-400 to-emerald-400"
            />
          </div>

          {steps.map((step, index) => (
            <motion.div key={step.titleKey} variants={cascadeItem} className="relative z-10">
              <GlassCard className="p-6">
                <span className={`flex h-[38px] w-[38px] items-center justify-center rounded-[10px] ${tints[step.tint]}`}>
                  <step.icon size={18} />
                </span>
                <span className="mt-4 block font-mono text-[11.5px] tracking-[0.1em] text-cyan-400">
                  {t('landing.how.stepLabel')} 0{index + 1}
                </span>
                <h3 className="mt-2 text-[15.5px] font-bold text-white">{t(step.titleKey)}</h3>
                <p className="mt-2 text-[13px] leading-relaxed text-slate-400">{t(step.descKey)}</p>
              </GlassCard>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
