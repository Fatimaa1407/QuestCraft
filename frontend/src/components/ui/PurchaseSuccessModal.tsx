import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import { AnimatePresence, motion } from 'framer-motion';
import { PartyPopper, Sparkles } from 'lucide-react';
import { Z_INDEX } from '../../styles/zIndex';
import { Confetti } from './Confetti';
import { getRarity, RARITY_STYLES } from '../../utils/rarity';

interface PurchaseSuccessModalProps {
  isOpen: boolean;
  itemName: string;
  itemType: string;
  imageUrl: string | null;
  pricePaid: number;
  isEquipable: boolean;
  isEquipping: boolean;
  onEquipNow: () => void;
  onClose: () => void;
}

// Deliberately a bigger, more celebratory moment than the inline card shimmer already used in
// ShopPage — the coin spend should feel like an event, not just a state flip. Modeled on
// LevelUpModal's portal/confetti pattern rather than the plain Modal.tsx dialog.
export function PurchaseSuccessModal({
  isOpen,
  itemName,
  itemType,
  imageUrl,
  pricePaid,
  isEquipable,
  isEquipping,
  onEquipNow,
  onClose,
}: PurchaseSuccessModalProps) {
  const { t } = useTranslation();
  const rarity = getRarity(pricePaid);
  const rarityStyle = RARITY_STYLES[rarity];

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0" style={{ zIndex: Z_INDEX.modal }}>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.25 }}
            className="absolute inset-0 bg-slate-950/70 backdrop-blur-md"
            onClick={onClose}
          />

          <div className="pointer-events-none absolute inset-0 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, y: 24, scale: 0.9 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 16, scale: 0.92 }}
              transition={{ type: 'spring', stiffness: 280, damping: 22 }}
              className="pointer-events-auto relative w-full max-w-sm overflow-hidden rounded-[28px] border border-white/20 bg-white/85 p-8 text-center shadow-[0_40px_80px_-24px_rgba(0,0,0,0.5)] backdrop-blur-2xl dark:border-white/10 dark:bg-slate-900/85"
            >
              <motion.span
                initial={{ scale: 0, rotate: -15 }}
                animate={{ scale: 1, rotate: 0 }}
                transition={{ delay: 0.1, type: 'spring', stiffness: 300, damping: 16 }}
                className="mx-auto flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br from-app-accent to-app-accent-2 text-white shadow-lg"
              >
                <PartyPopper size={20} />
              </motion.span>

              <motion.h2
                initial={{ opacity: 0, y: 6 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2, duration: 0.35 }}
                className="mt-3 text-lg font-bold text-slate-900 dark:text-white"
              >
                {t('shop.newItemTitle')}
              </motion.h2>

              <motion.div
                initial={{ opacity: 0, scale: 0.85 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.3, type: 'spring', stiffness: 260, damping: 20 }}
                className="mx-auto mt-5 flex h-24 w-24 items-center justify-center rounded-3xl shadow-lg"
                style={{ border: `2px solid ${rarityStyle.borderColor}` }}
              >
                {imageUrl ? (
                  <img src={imageUrl} alt="" className="h-full w-full rounded-[22px] object-cover" />
                ) : (
                  <Sparkles size={34} className={rarityStyle.text} />
                )}
              </motion.div>

              <motion.p
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.4, duration: 0.35 }}
                className="mt-4 text-base font-semibold text-slate-900 dark:text-white"
              >
                {itemName}
              </motion.p>
              <p className="mt-1 flex items-center justify-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
                <span>{itemType}</span>
                <span className="text-slate-300 dark:text-slate-700">·</span>
                <span className={`flex items-center gap-1 font-medium ${rarityStyle.text}`}>
                  <span className={`h-1.5 w-1.5 rounded-full ${rarityStyle.dot}`} />
                  {rarityStyle.labelAz}
                </span>
              </p>

              <div className="mt-6 flex flex-col gap-2">
                {isEquipable && (
                  <motion.button
                    type="button"
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.97 }}
                    onClick={onEquipNow}
                    disabled={isEquipping}
                    className="w-full rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2.5 text-sm font-semibold text-white shadow-lg transition-shadow hover:shadow-xl disabled:opacity-60"
                  >
                    {isEquipping ? t('shop.equipping') : t('shop.equipNow')}
                  </motion.button>
                )}
                <button
                  type="button"
                  onClick={onClose}
                  className="w-full rounded-full px-5 py-2.5 text-sm font-medium text-slate-500 transition hover:bg-slate-500/10 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
                >
                  {isEquipable ? t('shop.maybeLater') : t('shop.close')}
                </button>
              </div>
            </motion.div>
          </div>

          <Confetti count={40} style={{ zIndex: Z_INDEX.modal + 1 }} />
        </div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
