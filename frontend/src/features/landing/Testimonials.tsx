import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Star } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { cardHoverTransition, cascadeGroup, cascadeItem, cascadeItemDelayed, TESTIMONIALS_START } from './landingMotion';

const reviews = ['review1', 'review2', 'review3', 'review4'] as const;

function Stars() {
  return (
    <div className="flex gap-0.5 text-amber-400">
      {Array.from({ length: 5 }, (_, i) => (
        <Star key={i} size={14} fill="currentColor" strokeWidth={0} />
      ))}
    </div>
  );
}

export function Testimonials() {
  const { t } = useTranslation();

  return (
    <section id="testimonials" className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(TESTIMONIALS_START)}
          className="max-w-[640px]"
        >
          <span className="font-mono text-xs uppercase tracking-[0.14em] text-cyan-400">{t('landing.testimonials.eyebrow')}</span>
          <h2 className="mt-3.5 text-[1.9rem] font-extrabold tracking-tight text-white sm:text-[2.5rem]" style={{ textWrap: 'balance' }}>
            {t('landing.testimonials.title')}
          </h2>
        </motion.div>

        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeGroup(TESTIMONIALS_START)}
          className="mt-12 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
        >
          {reviews.map((key) => (
            <motion.div key={key} variants={cascadeItem}>
              <GlassCard className="flex h-full flex-col gap-3.5 p-6">
                <Stars />
                <p className="text-[13.5px] leading-relaxed text-slate-200">&ldquo;{t(`landing.testimonials.${key}Quote`)}&rdquo;</p>
                <div className="font-mono text-[11px] tracking-wide text-slate-500">{t(`landing.testimonials.${key}Meta`)}</div>
                <div className="mt-auto flex items-center gap-2.5 border-t border-white/[0.08] pt-3.5">
                  <motion.span
                    whileHover={{ scale: 1.12 }}
                    transition={cardHoverTransition}
                    className="flex h-[34px] w-[34px] shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-400 text-[13px] font-bold text-[#051019]"
                  >
                    {t(`landing.testimonials.${key}Name`).charAt(0)}
                  </motion.span>
                  <div>
                    <div className="text-[13px] font-semibold text-white">{t(`landing.testimonials.${key}Name`)}</div>
                    <div className="font-mono text-[11.5px] text-slate-500">{t(`landing.testimonials.${key}Role`)}</div>
                  </div>
                </div>
              </GlassCard>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
