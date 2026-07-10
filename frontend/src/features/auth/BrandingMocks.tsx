// Purely decorative "hero" mockups for the auth branding panel — not wired to real data.
import type { CSSProperties } from 'react';

function floatStyle(rotate: string, delay: string): CSSProperties {
  return { '--rotate': rotate, animationDelay: delay } as CSSProperties;
}

export function EditorMock({ className = '' }: { className?: string }) {
  return (
    <div
      className={`animate-float overflow-hidden rounded-xl border border-white/10 bg-slate-900/90 shadow-2xl shadow-black/40 backdrop-blur ${className}`}
      style={floatStyle('0deg', '0.5s')}
    >
      <div className="flex items-center gap-1.5 border-b border-white/10 px-3 py-2">
        <span className="h-2.5 w-2.5 rounded-full bg-red-400/80" />
        <span className="h-2.5 w-2.5 rounded-full bg-yellow-400/80" />
        <span className="h-2.5 w-2.5 rounded-full bg-green-400/80" />
        <span className="ml-2 text-[10px] text-slate-400">Solution.cs</span>
      </div>
      <pre className="px-3 py-3 font-mono text-[11px] leading-relaxed text-slate-300">
        <span className="text-indigo-400">class</span> <span className="text-cyan-300">Solution</span>
        {'\n{\n    '}
        <span className="text-indigo-400">static</span> <span className="text-indigo-400">void</span>{' '}
        <span className="text-cyan-300">Main</span>()
        {'\n    {\n        '}
        Console.<span className="text-cyan-300">WriteLine</span>(<span className="text-emerald-300">"Accepted"</span>);
        <span className="animate-blink text-white">|</span>
        {'\n    }\n}'}
      </pre>
    </div>
  );
}

export function StatsMock({ className = '' }: { className?: string }) {
  return (
    <div
      className={`animate-float rounded-xl border border-white/10 bg-white/10 p-4 shadow-2xl shadow-black/40 backdrop-blur ${className}`}
      style={floatStyle('0deg', '2s')}
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-cyan-400 text-sm font-bold text-white">
            12
          </div>
          <div>
            <p className="text-[10px] text-slate-400">Level</p>
            <p className="text-sm font-semibold text-white">Kod Ustası</p>
          </div>
        </div>
        <span className="text-[10px] text-slate-400">3200/4000 XP</span>
      </div>
      <div className="mt-3 h-1.5 w-full overflow-hidden rounded-full bg-white/10">
        <div className="h-full w-4/5 rounded-full bg-gradient-to-r from-indigo-400 to-cyan-400" />
      </div>
    </div>
  );
}

export function LeaderboardMock({ className = '' }: { className?: string }) {
  const rows = [
    { medal: '🥇', name: 'aysel_07', xp: 5120 },
    { medal: '🥈', name: 'kodcu99', xp: 4870 },
    { medal: '🥉', name: 'nizami.dev', xp: 4510 },
  ];

  return (
    <div
      className={`animate-float rounded-xl border border-white/10 bg-white/10 p-4 shadow-2xl shadow-black/40 backdrop-blur ${className}`}
      style={floatStyle('0deg', '3.5s')}
    >
      <p className="mb-2 text-[10px] font-medium text-slate-400">🏆 Leaderboard</p>
      <ul className="space-y-1.5">
        {rows.map((row) => (
          <li key={row.name} className="flex items-center justify-between text-xs text-white">
            <span className="flex items-center gap-1.5">
              <span>{row.medal}</span>
              {row.name}
            </span>
            <span className="text-slate-400">{row.xp} XP</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
