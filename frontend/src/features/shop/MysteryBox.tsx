import { useState } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AnimatePresence, motion } from 'framer-motion';
import { Gift, Sparkles, PartyPopper, Check } from 'lucide-react';
import { openMysteryBox } from '../../api/marketplace';
import { useAuthStore } from '../../app/authStore';
import { showToast } from '../../app/toastStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { playSuccessSound, playErrorSound } from '../../utils/sounds';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { RARITY_STYLES, type Rarity } from '../../utils/rarity';
import { GlassCard } from '../../components/ui/GlassCard';
import { Confetti } from '../../components/ui/Confetti';
import { Z_INDEX } from '../../styles/zIndex';
import { buttonTap } from '../../utils/motion';
import type { MysteryBoxResultDto } from '../../types/marketplace';

const MYSTERY_BOX_COST = 100;

export function MysteryBox() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const updateUser = useAuthStore((s) => s.updateUser);
  const user = useAuthStore((s) => s.user);
  const [result, setResult] = useState<MysteryBoxResultDto | null>(null);
  const [isShaking, setIsShaking] = useState(false);

  const canAfford = (user?.coins ?? 0) >= MYSTERY_BOX_COST;

  const openMutation = useMutation({
    mutationFn: openMysteryBox,
    onSuccess: (data) => {
      if (!data) return;
      setIsShaking(false);
      updateUser({ coins: data.remainingCoins });
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'my-purchases'] });
      queryClient.invalidateQueries({ queryKey: ['profile', 'equipped'] });
      if (!data.nothingLeft) {
        playSuccessSound();
        if (data.autoEquipped) {
          showToast({ title: t('shop.toastEquipped'), message: data.itemName ?? undefined, imageUrl: data.imageUrl, emoji: '✨' });
        }
      }
      setResult(data);
    },
    onError: (err) => {
      setIsShaking(false);
      playErrorSound();
      showToast({ title: getApiErrorMessage(err, t('shop.actionError')), emoji: '⚠️' });
    },
  });

  const handleOpen = () => {
    setIsShaking(true);
    setTimeout(() => openMutation.mutate(), 550);
  };

  const rarity: Rarity | null = result?.rarity ? (result.rarity as Rarity) : null;
  const rarityStyle = rarity ? RARITY_STYLES[rarity] : null;
  const animatedCoins = useAnimatedNumber(result ? result.remainingCoins : user?.coins ?? 0);

  return (
    <>
      <GlassCard hoverLift={false} className="relative overflow-hidden p-6 text-center" style={{ border: '2px solid rgba(217, 70, 239, 0.35)' }}>
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-fuchsia-500/[0.07] via-transparent to-violet-500/[0.07]" />
        <motion.div
          animate={isShaking ? { rotate: [0, -8, 8, -8, 8, 0], scale: [1, 1.05, 1.05, 1.05, 1.05, 1] } : {}}
          transition={{ duration: 0.5 }}
          className="relative mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-fuchsia-500 to-violet-500 text-white shadow-lg"
        >
          <Gift size={28} />
        </motion.div>
        <h3 className="relative mt-3 text-base font-bold text-slate-900 dark:text-slate-100">{t('shop.mysteryBox')}</h3>
        <p className="relative mt-1 text-xs text-slate-500 dark:text-slate-400">{t('shop.mysteryBoxCost', { cost: MYSTERY_BOX_COST })}</p>
        <motion.button
          {...buttonTap}
          onClick={handleOpen}
          disabled={openMutation.isPending || !canAfford}
          className="relative mt-4 w-full rounded-full bg-gradient-to-r from-fuchsia-500 to-violet-500 px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-fuchsia-500/30 disabled:opacity-50"
        >
          {openMutation.isPending ? t('shop.mysteryBoxOpening') : t('shop.mysteryBoxOpen')}
        </motion.button>
      </GlassCard>

      {createPortal(
        <AnimatePresence>
          {result && (
            <div className="fixed inset-0" style={{ zIndex: Z_INDEX.modal }}>
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.25 }}
                className="absolute inset-0 bg-slate-950/70 backdrop-blur-md"
                onClick={() => setResult(null)}
              />
              <div className="pointer-events-none absolute inset-0 flex items-center justify-center p-4">
                <motion.div
                  initial={{ opacity: 0, y: 24, scale: 0.9 }}
                  animate={{ opacity: 1, y: 0, scale: 1 }}
                  exit={{ opacity: 0, y: 16, scale: 0.92 }}
                  transition={{ type: 'spring', stiffness: 280, damping: 22 }}
                  className="pointer-events-auto relative w-full max-w-sm overflow-hidden rounded-[28px] border border-white/20 bg-white/85 p-8 text-center shadow-[0_40px_80px_-24px_rgba(0,0,0,0.5)] backdrop-blur-2xl dark:border-white/10 dark:bg-slate-900/85"
                >
                  {result.nothingLeft ? (
                    <>
                      <span className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-slate-500/10 text-slate-400">
                        <Gift size={26} />
                      </span>
                      <h2 className="mt-4 text-lg font-bold text-slate-900 dark:text-white">{t('shop.mysteryBoxNothingLeft')}</h2>
                      <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('shop.mysteryBoxNothingLeftHint')}</p>
                    </>
                  ) : (
                    <>
                      <motion.span
                        initial={{ scale: 0, rotate: -15 }}
                        animate={{ scale: 1, rotate: 0 }}
                        transition={{ delay: 0.1, type: 'spring', stiffness: 300, damping: 16 }}
                        className="mx-auto flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-fuchsia-500 to-violet-500 text-white shadow-lg"
                      >
                        <PartyPopper size={20} />
                      </motion.span>
                      <motion.h2
                        initial={{ opacity: 0, y: 6 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.2, duration: 0.35 }}
                        className="mt-3 text-lg font-bold text-slate-900 dark:text-white"
                      >
                        {t('shop.mysteryBoxWon')}
                      </motion.h2>

                      <motion.div
                        initial={{ opacity: 0, scale: 0.85 }}
                        animate={{ opacity: 1, scale: 1 }}
                        transition={{ delay: 0.3, type: 'spring', stiffness: 260, damping: 20 }}
                        className="relative mx-auto mt-5 flex h-24 w-24 items-center justify-center rounded-3xl shadow-lg"
                        style={{ border: `2px solid ${rarityStyle?.borderColor}` }}
                      >
                        {rarityStyle?.ringClass && <div className={`pointer-events-none absolute inset-0 rounded-3xl ${rarityStyle.ringClass}`} />}
                        {result.imageUrl ? (
                          <img src={result.imageUrl} alt="" className="relative h-full w-full rounded-[22px] object-cover" />
                        ) : (
                          <Sparkles size={34} className={`relative ${rarityStyle?.text}`} />
                        )}
                      </motion.div>

                      <motion.p
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        transition={{ delay: 0.4, duration: 0.35 }}
                        className="mt-4 text-base font-semibold text-slate-900 dark:text-white"
                      >
                        {result.itemName}
                      </motion.p>
                      <p className="mt-1 flex items-center justify-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
                        <span>{result.itemType}</span>
                        <span className="text-slate-300 dark:text-slate-700">·</span>
                        <span className={`flex items-center gap-1 font-medium ${rarityStyle?.text}`}>
                          <span className={`h-1.5 w-1.5 rounded-full ${rarityStyle?.dot}`} />
                          {rarityStyle?.labelAz}
                        </span>
                      </p>

                      {result.autoEquipped && (
                        <motion.p
                          initial={{ opacity: 0, y: 6 }}
                          animate={{ opacity: 1, y: 0 }}
                          transition={{ delay: 0.5, duration: 0.3 }}
                          className="mt-3 flex items-center justify-center gap-1.5 text-xs font-semibold text-emerald-600 dark:text-emerald-400"
                        >
                          <Check size={13} />
                          {t('shop.autoEquipped')}
                        </motion.p>
                      )}

                      <p className="mt-4 flex items-center justify-center gap-1 text-sm font-semibold text-amber-600 dark:text-amber-400">
                        🪙 {animatedCoins}
                      </p>
                    </>
                  )}

                  <button
                    type="button"
                    onClick={() => setResult(null)}
                    className="mt-6 w-full rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2.5 text-sm font-semibold text-white shadow-lg transition-shadow hover:shadow-xl"
                  >
                    {t('shop.close')}
                  </button>
                </motion.div>
              </div>
              {!result.nothingLeft && <Confetti count={40} style={{ zIndex: Z_INDEX.modal + 1 }} />}
            </div>
          )}
        </AnimatePresence>,
        document.body,
      )}
    </>
  );
}
