export function FloatingXp({ xp }: { xp: number }) {
  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-1/3 z-[60] flex justify-center">
      <span className="animate-float-up-fade flex items-center gap-1.5 text-3xl font-bold text-app-accent drop-shadow-lg dark:text-app-accent-2">
        ⭐ +{xp} XP
      </span>
    </div>
  );
}
