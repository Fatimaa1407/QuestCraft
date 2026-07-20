interface FramedAvatarProps {
  username: string;
  avatarUrl?: string | null;
  frameImageUrl?: string | null;
  size?: number;
  className?: string;
}

// Shared avatar renderer: image or initial-letter fallback, with an optional equipped-frame ring
// overlaid on top. Used everywhere an avatar appears (navbar, profile, leaderboard, battles,
// friends, chat, notifications) so the frame cosmetic shows up consistently in one place.
export function FramedAvatar({ username, avatarUrl, frameImageUrl, size = 36, className = '' }: FramedAvatarProps) {
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
        <img
          src={frameImageUrl}
          alt=""
          className="pointer-events-none absolute inset-0 h-full w-full rounded-full"
          style={{ transform: 'scale(1.25)' }}
        />
      )}
    </span>
  );
}
