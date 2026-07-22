interface FramedAvatarProps {
  username: string;
  avatarUrl?: string | null;
  frameImageUrl?: string | null;
  size?: number;
  className?: string;
  /** Set false to suppress the animated ring behind the frame (e.g. dense list rows). */
  animated?: boolean;
}

// Shared avatar renderer: image or initial-letter fallback, with an optional equipped-frame ring
// overlaid on top plus a slow-spinning accent-colored glow ring behind it. Used everywhere an
// avatar appears (navbar, profile, leaderboard, battles, notifications) so both the frame cosmetic
// and its animation show up consistently in one place.
export function FramedAvatar({ username, avatarUrl, frameImageUrl, size = 36, className = '', animated = true }: FramedAvatarProps) {
  return (
    <span className={`relative inline-flex shrink-0 items-center justify-center ${className}`} style={{ width: size, height: size }}>
      {avatarUrl ? (
        <img src={avatarUrl} alt="" className="h-full w-full rounded-full object-cover" />
      ) : (
        <span
          className="flex h-full w-full items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 font-semibold text-white"
          style={{ fontSize: size * 0.4 }}
        >
          {(username || '?').charAt(0).toUpperCase()}
        </span>
      )}
      {frameImageUrl && (
        <>
          {animated && (
            <span
              className="animate-frame-spin pointer-events-none absolute rounded-full"
              style={{
                inset: '-22%',
                background:
                  'conic-gradient(from 0deg, var(--color-app-accent), var(--color-app-accent-2), transparent, var(--color-app-accent))',
                WebkitMask: 'radial-gradient(farthest-side, transparent calc(100% - 3px), #fff calc(100% - 2px))',
                mask: 'radial-gradient(farthest-side, transparent calc(100% - 3px), #fff calc(100% - 2px))',
              }}
            />
          )}
          <img
            src={frameImageUrl}
            alt=""
            className="pointer-events-none absolute inset-0 h-full w-full rounded-full"
            style={{ transform: 'scale(1.25)' }}
          />
        </>
      )}
    </span>
  );
}
