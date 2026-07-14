import type { CSSProperties } from 'react';

// Positioned in the side gutters (between the sidebar and the centered max-w-6xl content column)
// so they never end up hidden behind page cards, regardless of how much content a page has.
const snippets: Array<{ lines: string[]; top: string; left: string; rotate: string; delay: string }> = [
  {
    lines: ['public class Quest', '{', '    int Xp = 0;', '}'],
    top: '22%',
    left: '6%',
    rotate: '-6deg',
    delay: '0s',
  },
  {
    lines: ['if (streak > 0)', '    LevelUp();'],
    top: '58%',
    left: '91%',
    rotate: '5deg',
    delay: '2.4s',
  },
];

export function AmbientGlow() {
  return (
    <div className="pointer-events-none fixed inset-0 z-0 overflow-hidden">
      {/* Faint grid pattern, same language as the auth pages. */}
      <div
        className="absolute inset-0 opacity-[0.05] dark:opacity-[0.09]"
        style={{
          backgroundImage:
            'linear-gradient(to right, currentColor 1px, transparent 1px), linear-gradient(to bottom, currentColor 1px, transparent 1px)',
          backgroundSize: '48px 48px',
        }}
      />

      {/* Soft radial glows for depth. */}
      <div className="absolute -top-20 left-1/4 h-[32rem] w-[32rem] rounded-full bg-blue-500/[0.18] blur-3xl dark:bg-blue-500/[0.28]" />
      <div className="absolute top-1/3 -right-24 h-[26rem] w-[26rem] rounded-full bg-cyan-500/[0.15] blur-3xl dark:bg-cyan-500/[0.24]" />
      <div className="absolute bottom-0 left-1/2 h-[24rem] w-[24rem] -translate-x-1/2 rounded-full bg-blue-400/[0.08] blur-3xl dark:bg-indigo-500/[0.14]" />

      {/* Film-grain texture for tactility. */}
      <div className="noise-overlay absolute inset-0" />

      {/* Sparse floating code snippets — same idea as CodeBackdrop, toned down. */}
      {snippets.map((snippet, index) => (
        <pre
          key={index}
          className="animate-float absolute select-none whitespace-pre rounded-lg border border-slate-900/15 bg-slate-900/[0.04] px-3 py-2 font-mono text-[11px] leading-relaxed text-slate-900 opacity-80 dark:border-white/15 dark:bg-white/[0.04] dark:text-white dark:opacity-60"
          style={
            {
              top: snippet.top,
              left: snippet.left,
              '--rotate': snippet.rotate,
              animationDelay: snippet.delay,
            } as CSSProperties
          }
        >
          {snippet.lines.join('\n')}
        </pre>
      ))}
    </div>
  );
}
