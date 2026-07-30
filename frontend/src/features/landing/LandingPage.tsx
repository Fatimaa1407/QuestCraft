import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { MotionConfig, motion, useReducedMotion, useScroll, useTransform } from 'framer-motion';
import { LandingNav } from './LandingNav';
import { HeroSection } from './HeroSection';
import { FeatureBento } from './FeatureBento';
import { HowItWorks } from './HowItWorks';
import { AudienceTabs } from './AudienceTabs';
import { Testimonials } from './Testimonials';
import { FaqSection } from './FaqSection';
import { CtaBanner } from './CtaBanner';
import { LandingFooter } from './LandingFooter';

export function LandingPage() {
  const { t } = useTranslation();
  const reduceMotion = useReducedMotion();
  const { scrollYProgress } = useScroll();
  const gridY = useTransform(scrollYProgress, [0, 1], reduceMotion ? [0, 0] : [0, 48]);

  // Scoped smooth-scroll: only while this pre-auth page is mounted, so the
  // authenticated app's own scroll containers aren't affected.
  useEffect(() => {
    const root = document.documentElement;
    root.classList.add('scroll-smooth');
    return () => root.classList.remove('scroll-smooth');
  }, []);

  return (
    <MotionConfig reducedMotion="user">
      <div className="relative min-h-svh overflow-x-hidden bg-app-bg text-slate-900 dark:bg-[linear-gradient(160deg,#0b1220_0%,#0d1526_46%,#080c16_100%)] dark:text-slate-50">
        <motion.div
          className="pointer-events-none absolute inset-0 text-slate-900 opacity-[0.035] dark:text-white dark:opacity-[0.05]"
          style={{
            backgroundImage:
              'linear-gradient(to right, currentColor 1px, transparent 1px), linear-gradient(to bottom, currentColor 1px, transparent 1px)',
            backgroundSize: '48px 48px',
            y: gridY,
          }}
        />
        <div
          className="pointer-events-none absolute inset-0 opacity-[0.12]"
          style={{
            background:
              'radial-gradient(1100px 620px at 82% -8%, rgba(34,211,238,0.14), transparent 60%), radial-gradient(900px 560px at 10% 8%, rgba(59,130,246,0.16), transparent 60%)',
          }}
        />

        <a
          href="#main"
          className="fixed left-4 top-4 z-[100] -translate-y-24 rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white transition-transform focus:translate-y-0 dark:bg-white dark:text-slate-900"
        >
          {t('landing.nav.skipLink')}
        </a>

        <LandingNav />

        <main id="main" className="relative z-10">
          <HeroSection />
          <FeatureBento />
          <HowItWorks />
          <AudienceTabs />
          <Testimonials />
          <FaqSection />
          <CtaBanner />
        </main>

        <div className="relative z-10">
          <LandingFooter />
        </div>
      </div>
    </MotionConfig>
  );
}
