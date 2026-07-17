import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { ArrowRight } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { cascadeItemDelayed, CTA_START, MotionLink } from './landingMotion';

export function CtaBanner() {
  const { t } = useTranslation();

  return (
    <section className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div initial="hidden" animate="show" variants={cascadeItemDelayed(CTA_START)}>
          <GlassCard hoverLift={false} className="relative overflow-hidden px-8 py-14 text-center sm:px-12 sm:py-16">
            <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(600px_260px_at_50%_0%,rgba(34,211,238,0.16),transparent_70%)]" />
            <h2 className="relative text-[1.8rem] font-extrabold tracking-tight text-white sm:text-[2.4rem]" style={{ textWrap: 'balance' }}>
              {t('landing.cta.title')}
            </h2>
            <p className="relative mt-3 text-[15px] text-slate-400">{t('landing.cta.subtitle')}</p>
            <div className="relative mt-8 flex justify-center">
              <MotionLink
                to="/register"
                whileHover={{ scale: 1.03, boxShadow: '0 20px 40px -12px rgba(34,211,238,0.55)' }}
                whileTap={{ scale: 0.98 }}
                transition={{ duration: 0.25, ease: 'easeOut' }}
                className="group inline-flex items-center gap-2 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-6 py-3 text-sm font-semibold text-[#051019] shadow-[0_14px_30px_-12px_rgba(34,211,238,0.45)]"
              >
                {t('landing.cta.button')}
                <ArrowRight size={16} className="transition-transform group-hover:translate-x-0.5" />
              </MotionLink>
            </div>
            <p className="relative mt-3.5 text-xs text-slate-500">{t('landing.cta.note')}</p>
          </GlassCard>
        </motion.div>
      </div>
    </section>
  );
}
