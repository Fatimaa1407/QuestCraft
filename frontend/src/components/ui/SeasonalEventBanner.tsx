import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AnimatePresence, motion } from 'framer-motion';
import { X } from 'lucide-react';
import { getCurrentSeasonalEvent } from '../../api/gamification';

export function SeasonalEventBanner() {
  const [dismissedId, setDismissedId] = useState<number | null>(null);
  const eventQuery = useQuery({
    queryKey: ['seasonal-event', 'current'],
    queryFn: getCurrentSeasonalEvent,
    staleTime: 5 * 60 * 1000,
  });

  const event = eventQuery.data;
  const isVisible = Boolean(event && event.id !== dismissedId);

  return (
    <AnimatePresence initial={false}>
      {isVisible && event && (
        <motion.div
          initial={{ height: 0, opacity: 0 }}
          animate={{ height: 'auto', opacity: 1 }}
          exit={{ height: 0, opacity: 0 }}
          transition={{ duration: 0.25 }}
          className="overflow-hidden"
        >
          <div className="flex items-center gap-3 bg-gradient-to-r from-violet-500 to-fuchsia-500 px-5 py-2.5 text-sm text-white sm:px-8">
            {event.emoji && <span className="text-lg">{event.emoji}</span>}
            <span className="min-w-0 flex-1 truncate">
              <span className="font-semibold">{event.name}</span>
              {event.description && <span className="ml-1.5 opacity-90">{event.description}</span>}
            </span>
            <button
              type="button"
              onClick={() => setDismissedId(event.id)}
              className="shrink-0 rounded-full p-1 text-white/80 transition hover:bg-white/15 hover:text-white"
              aria-label="Dismiss"
            >
              <X size={14} />
            </button>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
