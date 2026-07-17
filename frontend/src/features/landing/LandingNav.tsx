import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Code2, Menu, X } from 'lucide-react';
import { ThemeSwitcher } from '../../components/ui/ThemeSwitcher';
import { LanguageSwitcher } from '../../components/ui/LanguageSwitcher';
import { useScrollSpy } from '../../utils/useScrollSpy';
import { MotionLink } from './landingMotion';

const sectionIds = ['features', 'how', 'audience', 'testimonials', 'faq'];

export function LandingNav() {
  const { t } = useTranslation();
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const activeId = useScrollSpy(sectionIds);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 8);
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const links = [
    { id: 'features', label: t('landing.nav.features') },
    { id: 'how', label: t('landing.nav.how') },
    { id: 'audience', label: t('landing.nav.audience') },
    { id: 'testimonials', label: t('landing.nav.testimonials') },
    { id: 'faq', label: t('landing.nav.faq') },
  ];

  return (
    <motion.header
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.5, ease: 'easeOut' }}
      className={`fixed inset-x-0 top-0 z-50 border-b transition-[background-color,border-color,box-shadow] duration-300 ${
        scrolled ? 'border-white/[0.08] bg-[#080c16]/80 shadow-[0_10px_34px_-24px_rgba(0,0,0,0.7)] backdrop-blur-xl' : 'border-transparent bg-transparent'
      }`}
    >
      <div className="mx-auto flex h-[72px] max-w-6xl items-center justify-between px-5 sm:px-8">
        <Link to="/welcome" className="flex items-center gap-2.5 text-[17px] font-bold tracking-tight text-white">
          <span className="flex h-8 w-8 items-center justify-center rounded-[9px] bg-gradient-to-br from-blue-500 to-cyan-400 text-[#051019]">
            <Code2 size={18} />
          </span>
          QuestCraft
        </Link>

        <nav className="hidden items-center gap-7 text-[14.5px] text-slate-400 lg:flex" aria-label={t('landing.nav.ariaLabel')}>
          {links.map((link) => (
            <a
              key={link.id}
              href={`#${link.id}`}
              className={`relative py-1 transition-colors after:absolute after:inset-x-0 after:-bottom-[3px] after:h-0.5 after:origin-left after:scale-x-0 after:rounded-full after:bg-gradient-to-r after:from-blue-500 after:to-cyan-400 after:transition-transform after:duration-300 hover:text-white hover:after:scale-x-100 ${
                activeId === link.id ? 'text-white after:scale-x-100' : ''
              }`}
            >
              {link.label}
            </a>
          ))}
        </nav>

        <div className="flex items-center gap-2">
          <div className="hidden items-center gap-2 lg:flex">
            <motion.div whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.98 }} transition={{ duration: 0.25, ease: 'easeOut' }}>
              <Link
                to="/login"
                className="block rounded-lg border border-white/[0.14] bg-white/[0.04] px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-white/[0.08]"
              >
                {t('landing.nav.login')}
              </Link>
            </motion.div>
            <MotionLink
              to="/register"
              whileHover={{ scale: 1.03, boxShadow: '0 16px 34px -12px rgba(34,211,238,0.5)' }}
              whileTap={{ scale: 0.98 }}
              transition={{ duration: 0.25, ease: 'easeOut' }}
              className="rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-4 py-2 text-sm font-semibold text-[#051019] shadow-[0_14px_30px_-12px_rgba(34,211,238,0.45)]"
            >
              {t('landing.nav.cta')}
            </MotionLink>
          </div>
          <ThemeSwitcher />
          <LanguageSwitcher />
          <button
            type="button"
            onClick={() => setMenuOpen((v) => !v)}
            aria-label={t('landing.nav.menuOpen')}
            aria-expanded={menuOpen}
            className="flex h-9 w-9 items-center justify-center rounded-full text-white lg:hidden"
          >
            {menuOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </div>

      <AnimatePresence>
        {menuOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.25 }}
            className="overflow-hidden border-b border-white/[0.08] bg-[#080c16]/95 backdrop-blur-xl lg:hidden"
          >
            <div className="flex flex-col gap-1 px-5 pb-6 pt-2 sm:px-8">
              {links.map((link) => (
                <a
                  key={link.id}
                  href={`#${link.id}`}
                  onClick={() => setMenuOpen(false)}
                  className="border-b border-white/[0.06] py-3 text-[15px] text-slate-300"
                >
                  {link.label}
                </a>
              ))}
              <Link
                to="/login"
                onClick={() => setMenuOpen(false)}
                className="mt-3 rounded-lg border border-white/[0.14] bg-white/[0.04] px-4 py-2.5 text-center text-sm font-medium text-white"
              >
                {t('landing.nav.login')}
              </Link>
              <Link
                to="/register"
                onClick={() => setMenuOpen(false)}
                className="mt-2 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-4 py-2.5 text-center text-sm font-semibold text-[#051019]"
              >
                {t('landing.nav.cta')}
              </Link>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.header>
  );
}
