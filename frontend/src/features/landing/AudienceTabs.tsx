import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Check } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { AUDIENCE_START, cascadeItemDelayed, landingEase, STEP } from './landingMotion';

type Audience = 'student' | 'teacher';

export function AudienceTabs() {
  const { t } = useTranslation();
  const [active, setActive] = useState<Audience>('student');

  const studentStats = [
    ['landing.audience.studentStat1Label', '12'],
    ['landing.audience.studentStat2Label', '37'],
    ['landing.audience.studentStat3Label', t('landing.audience.studentStat3Value')],
    ['landing.audience.studentStat4Label', '#14'],
  ] as const;
  const teacherStats = [
    ['landing.audience.teacherStat1Label', '9'],
    ['landing.audience.teacherStat2Label', '.xlsx'],
    ['landing.audience.teacherStat3Label', t('landing.audience.teacherStat3Value')],
    ['landing.audience.teacherStat4Label', 'Admin'],
  ] as const;

  const isStudent = active === 'student';
  const bullets = isStudent
    ? ['landing.audience.studentBullet1', 'landing.audience.studentBullet2', 'landing.audience.studentBullet3']
    : ['landing.audience.teacherBullet1', 'landing.audience.teacherBullet2', 'landing.audience.teacherBullet3'];
  const stats = isStudent ? studentStats : teacherStats;

  return (
    <section id="audience" className="py-24 sm:py-28">
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(AUDIENCE_START)}
          className="max-w-[640px]"
        >
          <span className="font-mono text-xs uppercase tracking-[0.14em] text-cyan-400">{t('landing.audience.eyebrow')}</span>
          <h2 className="mt-3.5 text-[1.9rem] font-extrabold tracking-tight text-white sm:text-[2.5rem]" style={{ textWrap: 'balance' }}>
            {t('landing.audience.title')}
          </h2>
        </motion.div>

        <motion.div
          initial="hidden"
          animate="show"
          variants={cascadeItemDelayed(AUDIENCE_START + STEP)}
          className="mt-8 inline-flex gap-1 rounded-full border border-white/[0.08] bg-white/[0.03] p-1"
          role="tablist"
        >
          <motion.button
            type="button"
            role="tab"
            aria-selected={isStudent}
            onClick={() => setActive('student')}
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.98 }}
            transition={{ duration: 0.25, ease: 'easeOut' }}
            className={`rounded-full px-4.5 py-2 text-[13.5px] font-semibold transition-colors ${
              isStudent ? 'bg-gradient-to-r from-blue-600 to-cyan-500 text-[#051019]' : 'text-slate-400 hover:bg-white/[0.06] hover:text-white'
            }`}
          >
            {t('landing.audience.tabStudent')}
          </motion.button>
          <motion.button
            type="button"
            role="tab"
            aria-selected={!isStudent}
            onClick={() => setActive('teacher')}
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.98 }}
            transition={{ duration: 0.25, ease: 'easeOut' }}
            className={`rounded-full px-4.5 py-2 text-[13.5px] font-semibold transition-colors ${
              !isStudent ? 'bg-gradient-to-r from-blue-600 to-cyan-500 text-[#051019]' : 'text-slate-400 hover:bg-white/[0.06] hover:text-white'
            }`}
          >
            {t('landing.audience.tabTeacher')}
          </motion.button>
        </motion.div>

        <motion.div
          key={active}
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4, ease: landingEase }}
          className="mt-8 grid grid-cols-1 items-center gap-10 lg:grid-cols-2"
        >
          <div>
            <h3 className="text-2xl font-extrabold tracking-tight text-white">
              {t(isStudent ? 'landing.audience.studentHeading' : 'landing.audience.teacherHeading')}
            </h3>
            <ul className="mt-4 flex flex-col gap-3.5">
              {bullets.map((key) => (
                <li key={key} className="flex items-start gap-2.5 text-[14.5px] leading-relaxed text-slate-400">
                  <Check size={18} className="mt-0.5 shrink-0 text-emerald-400" />
                  {t(key)}
                </li>
              ))}
            </ul>
          </div>
          <GlassCard hoverLift={false} className="p-7">
            {stats.map(([labelKey, value]) => (
              <div key={labelKey} className="flex items-center justify-between border-b border-white/[0.08] py-3 text-[13px] last:border-0">
                <span className="text-slate-400">{t(labelKey)}</span>
                <span className="font-mono text-cyan-400">{value}</span>
              </div>
            ))}
          </GlassCard>
        </motion.div>
      </div>
    </section>
  );
}
