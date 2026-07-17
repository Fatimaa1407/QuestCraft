import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { ArrowRight, Check, Circle, Zap } from 'lucide-react';
import { GlassCard } from '../../components/ui/GlassCard';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { cascadeItemDelayed, HERO_DELAYS, MotionLink } from './landingMotion';
import { useCountUp } from './useCountUp';

type TokenKind = 'kw' | 'type' | 'fn' | 'num';
type Token = { text: string; kind?: TokenKind };

const tokenClass: Record<TokenKind, string> = {
  kw: 'text-violet-400',
  type: 'text-cyan-400',
  fn: 'text-blue-400',
  num: 'text-amber-400',
};

const codeLines: Token[][] = [
  [{ text: 'public ', kind: 'kw' }, { text: 'int ', kind: 'type' }, { text: 'Fibonacci', kind: 'fn' }, { text: '(int n)' }],
  [{ text: '{' }],
  [{ text: '    if', kind: 'kw' }, { text: ' (n <= ' }, { text: '1', kind: 'num' }, { text: ') ' }, { text: 'return', kind: 'kw' }, { text: ' n;' }],
  [{ text: '    return', kind: 'kw' }, { text: ' ' }, { text: 'Fibonacci', kind: 'fn' }, { text: '(n-' }, { text: '1', kind: 'num' }, { text: ')' }],
  [{ text: '         + ' }, { text: 'Fibonacci', kind: 'fn' }, { text: '(n-' }, { text: '2', kind: 'num' }, { text: ');' }],
  [{ text: '}' }],
];

function CodeLine({ tokens }: { tokens: Token[] }) {
  return (
    <div>
      {tokens.map((token, i) => (
        <span key={i} className={token.kind ? tokenClass[token.kind] : undefined}>
          {token.text}
        </span>
      ))}
    </div>
  );
}

const statDefs = [
  { value: 20, suffix: '', labelKey: 'landing.hero.stat1Label' },
  { value: 60, suffix: '+', labelKey: 'landing.hero.stat2Label' },
  { value: 30, suffix: '+', labelKey: 'landing.hero.stat3Label' },
  { value: 6, suffix: '', labelKey: 'landing.hero.stat4Label' },
];

function HeroStat({ value, suffix, label }: { value: number; suffix: string; label: string }) {
  const animated = useCountUp(value, 1200, HERO_DELAYS.stats);
  return (
    <div>
      <div className="font-mono text-[22px] font-bold tracking-tight text-white tabular-nums">
        {animated}
        {suffix}
      </div>
      <div className="mt-1 text-[12.5px] text-slate-500">{label}</div>
    </div>
  );
}

type Phase = 'typing' | 'checking' | 'done';

export function HeroSection() {
  const { t } = useTranslation();
  const [phase, setPhase] = useState<Phase>('typing');
  const [visibleLines, setVisibleLines] = useState(0);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const xpRemaining = useAnimatedNumber(phase === 'done' ? 780 : 220, 700);

  useEffect(() => {
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduced) {
      setVisibleLines(codeLines.length);
      setPhase('done');
      return;
    }

    const timeouts: number[] = [];
    let line = 0;
    const typeNext = () => {
      line += 1;
      setVisibleLines(line);
      if (line < codeLines.length) {
        timeouts.push(window.setTimeout(typeNext, 130 + Math.random() * 90));
      } else {
        timeouts.push(
          window.setTimeout(() => {
            setPhase('checking');
            timeouts.push(window.setTimeout(() => setPhase('done'), 650));
          }, 350),
        );
      }
    };
    timeouts.push(window.setTimeout(typeNext, (HERO_DELAYS.editor + 0.5) * 1000));

    return () => timeouts.forEach((id) => window.clearTimeout(id));
  }, []);

  useEffect(() => {
    if (phase !== 'done') return;
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const canvas = canvasRef.current;
    if (reduced || !canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    const dpr = window.devicePixelRatio || 1;
    const w = canvas.clientWidth;
    const h = canvas.clientHeight;
    canvas.width = w * dpr;
    canvas.height = h * dpr;
    ctx.scale(dpr, dpr);

    const colors = ['#3b82f6', '#22d3ee', '#f59e0b', '#10b981'];
    const particles = Array.from({ length: 36 }, (_, i) => {
      const angle = Math.random() * Math.PI * 2;
      const speed = 1.5 + Math.random() * 3;
      return {
        x: w * 0.5,
        y: h * 0.62,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed - 2,
        size: 2 + Math.random() * 3,
        color: colors[i % colors.length],
      };
    });

    let start: number | null = null;
    let frameId: number;
    const frame = (ts: number) => {
      if (start === null) start = ts;
      const elapsed = ts - start;
      ctx.clearRect(0, 0, w, h);
      particles.forEach((p) => {
        p.vy += 0.06;
        p.x += p.vx;
        p.y += p.vy;
        ctx.globalAlpha = Math.max(0, 1 - elapsed / 1100);
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x, p.y, p.size, p.size);
      });
      ctx.globalAlpha = 1;
      if (elapsed < 1100) {
        frameId = requestAnimationFrame(frame);
      } else {
        ctx.clearRect(0, 0, w, h);
      }
    };
    frameId = requestAnimationFrame(frame);
    return () => cancelAnimationFrame(frameId);
  }, [phase]);

  return (
    <section className="relative overflow-hidden pt-[152px] pb-20 sm:pt-[168px] sm:pb-24">
      <div className="animate-glow-drift pointer-events-none absolute -left-24 -top-40 h-[30rem] w-[30rem] rounded-full bg-blue-500/20 blur-[70px]" />
      <div
        className="animate-glow-drift pointer-events-none absolute -right-32 top-16 h-[24rem] w-[24rem] rounded-full bg-cyan-400/[0.18] blur-[70px]"
        style={{ animationDuration: '46s' }}
      />

      <div className="relative z-10 mx-auto grid max-w-6xl grid-cols-1 items-center gap-10 px-5 sm:px-8 lg:grid-cols-[1.05fr_0.95fr] lg:gap-14">
        <div>
          <motion.span
            initial="hidden"
            animate="show"
            variants={cascadeItemDelayed(HERO_DELAYS.badge)}
            className="inline-flex items-center gap-2 font-mono text-xs uppercase tracking-[0.14em] text-cyan-400 before:h-1.5 before:w-1.5 before:rounded-full before:bg-cyan-400 before:shadow-[0_0_10px_2px_rgba(34,211,238,0.7)]"
          >
            {t('landing.hero.eyebrow')}
          </motion.span>

          <motion.h1
            initial="hidden"
            animate="show"
            variants={cascadeItemDelayed(HERO_DELAYS.title)}
            className="mt-4 text-[2.5rem] font-extrabold leading-[1.06] tracking-tight text-white sm:text-[3.4rem]"
            style={{ textWrap: 'balance' }}
          >
            {t('landing.hero.titleLine1')}
            <br />
            {t('landing.hero.titleLine2')}{' '}
            <span className="bg-gradient-to-r from-blue-500 to-cyan-400 bg-clip-text text-transparent">
              {t('landing.hero.titleGrad')}
            </span>
          </motion.h1>

          <motion.p
            initial="hidden"
            animate="show"
            variants={cascadeItemDelayed(HERO_DELAYS.description)}
            className="mt-5 max-w-[46ch] text-[17px] leading-relaxed text-slate-400"
          >
            {t('landing.hero.lead')}
          </motion.p>

          <motion.div
            initial="hidden"
            animate="show"
            variants={cascadeItemDelayed(HERO_DELAYS.cta)}
            className="mt-7 flex flex-wrap items-center gap-3"
          >
            <MotionLink
              to="/register"
              whileHover={{ scale: 1.03, boxShadow: '0 20px 40px -12px rgba(34,211,238,0.55)' }}
              whileTap={{ scale: 0.98 }}
              transition={{ duration: 0.25, ease: 'easeOut' }}
              className="group inline-flex items-center gap-2 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-6 py-3 text-sm font-semibold text-[#051019] shadow-[0_14px_30px_-12px_rgba(34,211,238,0.45)]"
            >
              {t('landing.hero.ctaPrimary')}
              <ArrowRight size={16} className="transition-transform group-hover:translate-x-0.5" />
            </MotionLink>
            <motion.a
              href="#how"
              whileHover={{ scale: 1.03 }}
              whileTap={{ scale: 0.98 }}
              transition={{ duration: 0.25, ease: 'easeOut' }}
              className="rounded-lg border border-white/[0.16] bg-white/[0.04] px-6 py-3 text-sm font-medium text-white transition-colors hover:bg-white/[0.08]"
            >
              {t('landing.hero.ctaSecondary')}
            </motion.a>
          </motion.div>

          <motion.div
            initial="hidden"
            animate="show"
            variants={cascadeItemDelayed(HERO_DELAYS.stats)}
            className="mt-11 flex flex-wrap gap-8 sm:gap-10"
          >
            {statDefs.map((stat) => (
              <HeroStat key={stat.labelKey} value={stat.value} suffix={stat.suffix} label={t(stat.labelKey)} />
            ))}
          </motion.div>
        </div>

        <motion.div
          initial={{ opacity: 0, x: 40, scale: 0.96 }}
          animate={{ opacity: 1, x: 0, scale: 1 }}
          transition={{ duration: 0.5, delay: HERO_DELAYS.editor, ease: 'easeOut' }}
          className="relative mx-auto w-full max-w-[460px]"
        >
          <div className="overflow-hidden rounded-2xl border border-white/[0.16] bg-slate-900/80 shadow-[0_40px_70px_-30px_rgba(0,0,0,0.65)] backdrop-blur-xl">
            <div className="flex items-center gap-2 border-b border-white/[0.08] px-3.5 py-2.5">
              <span className="h-2.5 w-2.5 rounded-full bg-red-400" />
              <span className="h-2.5 w-2.5 rounded-full bg-amber-400" />
              <span className="h-2.5 w-2.5 rounded-full bg-emerald-400" />
              <span className="ml-2 font-mono text-xs text-slate-500">{t('landing.hero.editorFile')}</span>
            </div>
            <div className="min-h-[168px] px-4 py-4 font-mono text-[13.5px] leading-[1.75] text-slate-300">
              {codeLines.slice(0, visibleLines).map((tokens, i) => (
                <CodeLine key={i} tokens={tokens} />
              ))}
              {phase === 'typing' && visibleLines < codeLines.length && (
                <span className="inline-block h-[15px] w-[7px] -translate-y-[-2px] animate-pulse bg-cyan-400 align-middle" />
              )}
            </div>
            <div className="flex items-center justify-between gap-3 border-t border-white/[0.08] bg-white/[0.02] px-4 py-3">
              <span
                className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 font-mono text-[12.5px] font-semibold transition-colors ${
                  phase === 'done' ? 'bg-emerald-500/15 text-emerald-400' : 'bg-white/[0.06] text-slate-500'
                }`}
              >
                {phase === 'done' ? <Check size={12} /> : <Circle size={12} />}
                {phase === 'typing' ? t('landing.hero.verdictPending') : phase === 'checking' ? t('landing.hero.verdictChecking') : t('landing.hero.verdictAccepted')}
              </span>
              <span
                className={`flex gap-2.5 font-mono text-[12.5px] transition-all duration-500 ${
                  phase === 'done' ? 'translate-y-0 opacity-100' : 'translate-y-1 opacity-0'
                }`}
              >
                <span className="text-cyan-400">{t('landing.hero.rewardXp')}</span>
                <span className="text-amber-400">{t('landing.hero.rewardCoin')}</span>
              </span>
            </div>
          </div>

          <GlassCard
            hoverLift={false}
            className={`absolute -bottom-6 -right-3.5 w-52 p-3.5 transition-transform duration-500 ${phase === 'done' ? 'scale-[1.03]' : ''}`}
          >
            <div className="flex items-center justify-between">
              <span className="font-mono text-xl font-bold text-white">
                <span className="text-sm font-medium text-slate-500">12</span> &rarr; {phase === 'done' ? '13' : '12'}
              </span>
              <Zap size={18} className="text-amber-400" />
            </div>
            <div className="mt-2.5 h-1.5 overflow-hidden rounded-full bg-white/[0.08]">
              <div
                className="h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-400 transition-[width] duration-700 ease-out"
                style={{ width: phase === 'done' ? '22%' : '78%' }}
              />
            </div>
            <div className="mt-2 text-[11px] text-slate-500">
              {t('landing.hero.levelCap', { xp: xpRemaining })}
            </div>
          </GlassCard>

          <canvas ref={canvasRef} className="pointer-events-none absolute -inset-16 z-10" />
        </motion.div>
      </div>
    </section>
  );
}
