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

      {/* Soft radial glows for depth — each drifts extremely slowly (32–60s loops, negative
          delays so they're already out of phase on mount) so the lighting reads as alive
          without ever looking like a moving decoration. Inert under prefers-reduced-motion. */}
      <div
        className="animate-glow-drift absolute -top-20 left-1/4 h-[32rem] w-[32rem] rounded-full bg-blue-500/[0.18] blur-3xl dark:bg-blue-500/[0.28]"
        style={{ '--drift-x': '5%', '--drift-y': '-4%', '--drift-duration': '34s', animationDelay: '-6s' } as CSSProperties}
      />
      <div
        className="animate-glow-drift absolute top-1/3 -right-24 h-[26rem] w-[26rem] rounded-full bg-cyan-500/[0.15] blur-3xl dark:bg-cyan-500/[0.24]"
        style={{ '--drift-x': '-4%', '--drift-y': '5%', '--drift-duration': '40s', animationDelay: '-18s' } as CSSProperties}
      />
      <div
        className="animate-glow-drift absolute bottom-0 left-1/2 h-[24rem] w-[24rem] -translate-x-1/2 rounded-full bg-blue-400/[0.08] blur-3xl dark:bg-cyan-500/[0.12]"
        style={{ '--drift-x': '3%', '--drift-y': '-5%', '--drift-duration': '28s', animationDelay: '-12s' } as CSSProperties}
      />

      {/* Two extra, larger, even softer blobs purely for depth — same technique, slower still. */}
      <div
        className="animate-glow-drift absolute top-2/3 left-[8%] h-[30rem] w-[30rem] rounded-full bg-blue-400/[0.06] blur-3xl dark:bg-blue-400/[0.10]"
        style={{ '--drift-x': '-6%', '--drift-y': '-3%', '--drift-duration': '52s', animationDelay: '-24s' } as CSSProperties}
      />
      <div
        className="animate-glow-drift absolute -top-10 right-[12%] h-[22rem] w-[22rem] rounded-full bg-cyan-400/[0.07] blur-3xl dark:bg-cyan-400/[0.11]"
        style={{ '--drift-x': '4%', '--drift-y': '6%', '--drift-duration': '60s', animationDelay: '-38s' } as CSSProperties}
      />

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
