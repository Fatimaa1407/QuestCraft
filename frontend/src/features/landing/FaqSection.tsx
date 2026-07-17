import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronDown } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { cascadeGroup, cascadeItem, cascadeItemDelayed, FAQ_START } from './landingMotion';

const faqKeys = ['q1', 'q2', 'q3', 'q4', 'q5'] as const;

export function FaqSection() {
  const { t } = useTranslation();
  const [openIndex, setOpenIndex] = useState<number | null>(null);

  return (
    <section id="faq" className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(FAQ_START)}
          className="max-w-[640px]"
        >
          <span className="font-mono text-xs uppercase tracking-[0.14em] text-cyan-400">{t('landing.faq.eyebrow')}</span>
          <h2 className="mt-3.5 text-[1.9rem] font-extrabold tracking-tight text-white sm:text-[2.5rem]" style={{ textWrap: 'balance' }}>
            {t('landing.faq.title')}
          </h2>
        </motion.div>

        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeGroup(FAQ_START)}
          className="mt-10 flex max-w-[760px] flex-col gap-2.5"
        >
          {faqKeys.map((key, index) => {
            const isOpen = openIndex === index;
            const qId = `faq-q-${index}`;
            const aId = `faq-a-${index}`;
            return (
              <motion.div key={key} variants={cascadeItem}>
                <GlassCard
                  hoverLift={false}
                  className="px-6 transition-colors"
                  style={isOpen ? { borderColor: 'rgba(255,255,255,0.18)', background: 'rgba(255,255,255,0.04)' } : undefined}
                >
                  <button
                    id={qId}
                    type="button"
                    aria-expanded={isOpen}
                    aria-controls={aId}
                    onClick={() => setOpenIndex(isOpen ? null : index)}
                    className="flex w-full items-center justify-between gap-4 py-[18px] text-left text-[15px] font-semibold text-white"
                  >
                    <span>{t(`landing.faq.${key}`)}</span>
                    <ChevronDown
                      size={18}
                      className={`shrink-0 text-slate-500 transition-transform duration-300 ${isOpen ? 'rotate-180 text-cyan-400' : ''}`}
                    />
                  </button>
                  <AnimatePresence initial={false}>
                    {isOpen && (
                      <motion.div
                        id={aId}
                        role="region"
                        aria-labelledby={qId}
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: 'auto', opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.25 }}
                        className="overflow-hidden"
                      >
                        <p className="pb-[18px] text-[13.5px] leading-relaxed text-slate-400">
                          {t(`landing.faq.a${key.slice(1)}`)}
                        </p>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </GlassCard>
              </motion.div>
            );
          })}
        </motion.div>
      </div>
    </section>
  );
}
