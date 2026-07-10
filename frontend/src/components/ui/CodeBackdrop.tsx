import type { CSSProperties } from 'react';

const snippets: Array<{ lang: string; lines: string[]; top: string; left: string; rotate: string; opacity: string; delay: string }> = [
  {
    lang: 'C#',
    lines: ['public class Solution', '{', '    static void Main()', '    {', '        // qazan XP!', '    }', '}'],
    top: '4%',
    left: '4%',
    rotate: '-6deg',
    opacity: 'opacity-20',
    delay: '0s',
  },
  {
    lang: 'Python',
    lines: ['def two_sum(nums, target):', '    seen = {}', '    for i, n in enumerate(nums):', '        ...'],
    top: '54%',
    left: '2%',
    rotate: '4deg',
    opacity: 'opacity-15',
    delay: '1.2s',
  },
  {
    lang: 'JavaScript',
    lines: ['const solve = (input) => {', '  return input', '    .split(" ")', '    .map(Number);', '};'],
    top: '8%',
    left: '58%',
    rotate: '5deg',
    opacity: 'opacity-15',
    delay: '2.4s',
  },
  {
    lang: 'C#',
    lines: ['int a = int.Parse(parts[0]);', 'int b = int.Parse(parts[1]);', 'Console.WriteLine(a + b);'],
    top: '68%',
    left: '52%',
    rotate: '-4deg',
    opacity: 'opacity-20',
    delay: '3.6s',
  },
  {
    lang: 'SQL',
    lines: ['SELECT Username, Xp', 'FROM Leaderboard', 'ORDER BY Xp DESC;'],
    top: '38%',
    left: '68%',
    rotate: '3deg',
    opacity: 'opacity-10',
    delay: '4.8s',
  },
];

export function CodeBackdrop() {
  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden">
      <div
        className="absolute inset-0 opacity-[0.07]"
        style={{
          backgroundImage:
            'linear-gradient(to right, white 1px, transparent 1px), linear-gradient(to bottom, white 1px, transparent 1px)',
          backgroundSize: '40px 40px',
        }}
      />
      {/* Primary indigo glow (brand) with a faint purple undertone — subtle, not dominant. */}
      <div className="absolute left-1/2 top-1/2 h-[36rem] w-[36rem] -translate-x-1/2 -translate-y-1/2 animate-pulse rounded-full bg-indigo-600/20 blur-3xl" />
      <div className="absolute left-1/3 top-1/2 h-72 w-72 -translate-x-1/2 -translate-y-1/2 rounded-full bg-violet-700/10 blur-3xl" />
      {/* Secondary cyan accent glow. */}
      <div className="animate-drift absolute -right-10 top-1/4 h-64 w-64 rounded-full bg-cyan-500/15 blur-3xl" />

      {snippets.map((snippet, index) => (
        <pre
          key={index}
          className={`animate-float absolute select-none whitespace-pre rounded-lg border border-white/10 bg-black/20 px-3 py-2 font-mono text-[11px] leading-relaxed text-white ${snippet.opacity}`}
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
