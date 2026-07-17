// Shimmer placeholder for loading states — matches the glass aesthetic (rounded, subtle
// surface tint) and is dark/light theme aware. Compose these into shapes that mirror the
// final layout (e.g. a row of bars for a chart, a grid of squares for a heatmap) rather than
// a single generic block, so the page doesn't "jump" once real content arrives.
export function Skeleton({ className = '' }: { className?: string }) {
  return (
    <div
      className={`relative overflow-hidden rounded-lg bg-slate-200/70 dark:bg-white/[0.06] ${className}`}
    >
      <div className="absolute inset-0 -translate-x-full bg-gradient-to-r from-transparent via-white/60 to-transparent dark:via-white/10 animate-shimmer-sweep-loop" />
    </div>
  );
}
