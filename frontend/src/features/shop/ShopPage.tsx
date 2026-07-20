import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Lightbulb, UserCircle, Frame, Image, Palette, Award, Type, ShoppingBag, Check, Lock, Wallet, Snowflake, X } from 'lucide-react';
import { getMarketplaceItems, getItemTypes, purchaseItem, equipItem, unequipItem, getMyPurchases } from '../../api/marketplace';
import { EQUIPABLE_ITEM_TYPES } from '../../types/marketplace';
import { getRarity, RARITY_STYLES } from '../../utils/rarity';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { Skeleton } from '../../components/ui/Skeleton';
import { EmptyState } from '../../components/ui/EmptyState';
import { getApiErrorMessage } from '../../utils/apiError';
import { playSuccessSound, playErrorSound } from '../../utils/sounds';
import { fadeInUp, staggerContainer, buttonTap } from '../../utils/motion';

const typeIcons: Record<string, typeof Lightbulb> = {
  Hint: Lightbulb,
  Avatar: UserCircle,
  ProfileFrame: Frame,
  ProfileBanner: Image,
  Theme: Palette,
  Badge: Award,
  Title: Type,
  StreakFreeze: Snowflake,
};

const typeEmoji: Record<string, string> = {
  Avatar: '😀',
  Badge: '🏅',
  Hint: '💡',
  ProfileFrame: '🖼️',
  ProfileBanner: '🏳️',
  Theme: '🎨',
  Title: '👑',
  StreakFreeze: '❄️',
};

export function ShopPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const updateUser = useAuthStore((s) => s.updateUser);
  const [typeId, setTypeId] = useState<number | undefined>(undefined);
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [justPurchasedId, setJustPurchasedId] = useState<number | null>(null);

  const animatedCoins = useAnimatedNumber(user?.coins ?? 0);

  const typesQuery = useQuery({ queryKey: ['marketplace', 'item-types'], queryFn: getItemTypes });
  const itemsQuery = useQuery({
    queryKey: ['marketplace', 'items', typeId],
    queryFn: () => getMarketplaceItems(typeId),
  });
  const purchasesQuery = useQuery({ queryKey: ['marketplace', 'my-purchases'], queryFn: getMyPurchases });
  const equippedItemIds = new Set((purchasesQuery.data ?? []).filter((p) => p.isEquipped).map((p) => p.marketplaceItemId));

  const invalidateEquipState = () => {
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'my-purchases'] });
    queryClient.invalidateQueries({ queryKey: ['profile', 'equipped'] });
  };

  const showFeedback = (type: 'success' | 'error', text: string) => {
    setFeedback({ type, text });
    setTimeout(() => setFeedback(null), 3000);
  };

  const purchaseMutation = useMutation({
    mutationFn: purchaseItem,
    onSuccess: (result, itemId) => {
      if (!result) return;
      updateUser({ coins: result.remainingCoins });
      invalidateEquipState();
      playSuccessSound();
      setJustPurchasedId(itemId);
      setTimeout(() => setJustPurchasedId(null), 1300);
      showFeedback('success', t('shop.purchaseSuccess', { name: result.itemName }));
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const equipMutation = useMutation({
    mutationFn: equipItem,
    onSuccess: () => {
      invalidateEquipState();
      playSuccessSound();
      showFeedback('success', t('shop.equipSuccess'));
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const unequipMutation = useMutation({
    mutationFn: unequipItem,
    onSuccess: () => {
      invalidateEquipState();
      playSuccessSound();
      showFeedback('success', t('shop.unequipSuccess'));
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const items = itemsQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp} className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('shop.title')}</h1>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('shop.subtitle')}</p>
        </div>
        <div className="flex items-center gap-3 rounded-2xl border border-amber-400/40 bg-amber-500/10 px-5 py-3">
          <Wallet size={20} className="text-amber-600 dark:text-amber-400" />
          <div>
            <p className="text-[11px] font-semibold uppercase tracking-wide text-amber-600/80 dark:text-amber-400/80">
              {t('shop.coinsLabel')}
            </p>
            <p className="flex items-center gap-1 text-xl font-bold text-amber-600 dark:text-amber-400">
              🪙 {animatedCoins}
            </p>
          </div>
        </div>
      </motion.div>

      {feedback && (
        <motion.p
          initial={{ opacity: 0, y: -6 }}
          animate={{ opacity: 1, y: 0 }}
          className={`rounded-lg px-3 py-2 text-sm ${
            feedback.type === 'success'
              ? 'bg-emerald-50 text-emerald-600 dark:bg-emerald-950/50 dark:text-emerald-400'
              : 'bg-red-50 text-red-600 dark:bg-red-950/50 dark:text-red-400'
          }`}
        >
          {feedback.text}
        </motion.p>
      )}

      <motion.div variants={fadeInUp} className="flex flex-wrap items-center gap-2">
        <button
          onClick={() => setTypeId(undefined)}
          className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
            typeId === undefined
              ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30'
              : 'border border-slate-200/70 text-slate-600 hover:border-blue-400 dark:border-white/[0.08] dark:text-slate-300'
          }`}
        >
          🛍️ {t('shop.all')}
        </button>
        {typesQuery.data?.map((type) => (
          <button
            key={type.id}
            onClick={() => setTypeId(type.id)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
              typeId === type.id
                ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30'
                : 'border border-slate-200/70 text-slate-600 hover:border-blue-400 dark:border-white/[0.08] dark:text-slate-300'
            }`}
          >
            {typeEmoji[type.name] ?? '✨'} {type.name}
          </button>
        ))}
      </motion.div>

      {itemsQuery.isLoading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <ShopItemSkeleton key={i} />
          ))}
        </div>
      ) : items.length === 0 ? (
        <EmptyState
          icon={ShoppingBag}
          tint="amber"
          title={t('shop.empty')}
          action={
            typeId !== undefined
              ? {
                  label: t('shop.all'),
                  onClick: () => setTypeId(undefined),
                }
              : undefined
          }
        />
      ) : (
        <motion.div variants={staggerContainer} className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((item) => {
            const Icon = typeIcons[item.itemType] ?? ShoppingBag;
            const isEquipable = EQUIPABLE_ITEM_TYPES.includes(item.itemType);
            const isEquipped = equippedItemIds.has(item.id);
            const canAfford = (user?.coins ?? 0) >= item.price;
            const rarity = getRarity(item.price);
            const rarityStyle = RARITY_STYLES[rarity];
            const isCelebrating = justPurchasedId === item.id;

            return (
              <motion.div key={item.id} variants={fadeInUp}>
                <GlassCard
                  style={{ border: `2px solid ${rarityStyle.borderColor}` }}
                  className={`relative flex flex-col overflow-hidden p-0 ${rarityStyle.glow} ${isCelebrating ? 'animate-card-pulse' : ''}`}
                >
                  {isCelebrating && (
                    <span className="animate-shimmer-sweep pointer-events-none absolute inset-y-0 left-0 z-10 w-1/3 bg-gradient-to-r from-transparent via-white/40 to-transparent" />
                  )}
                  {isCelebrating && (
                    <span className="animate-toast-pop pointer-events-none absolute left-1/2 top-1/2 z-20 -translate-x-1/2 -translate-y-1/2 rounded-full bg-slate-900/90 px-4 py-2 text-sm font-semibold text-white shadow-xl">
                      🎉 {t('shop.purchased')}
                    </span>
                  )}

                  {item.isOwned && (
                    <div
                      className={`flex items-center justify-center gap-1.5 py-2 text-xs font-semibold ${
                        isEquipped
                          ? 'bg-gradient-to-r from-blue-500/20 to-cyan-500/10 text-blue-600 dark:text-cyan-400'
                          : 'bg-gradient-to-r from-emerald-500/20 to-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                      }`}
                    >
                      <Check size={13} />
                      {isEquipped ? t('shop.equipped') : t('shop.owned')}
                    </div>
                  )}

                  <div className="flex flex-1 flex-col p-6">
                    <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-500/10 text-blue-600 dark:text-cyan-400">
                      {item.imageUrl ? (
                        <img src={item.imageUrl} alt="" className="h-full w-full rounded-2xl object-cover" />
                      ) : (
                        <Icon size={26} />
                      )}
                    </div>

                    <h2 className="mt-4 text-lg font-semibold text-slate-900 dark:text-slate-100">{item.name}</h2>
                    <div className="mt-1 flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400">
                      <span>{item.itemType}</span>
                      <span className="text-slate-300 dark:text-slate-700">·</span>
                      <span className={`flex items-center gap-1 font-medium ${rarityStyle.text}`}>
                        <span className={`h-2 w-2 rounded-full ${rarityStyle.dot}`} />
                        {rarityStyle.labelAz}
                      </span>
                    </div>

                    {item.description && (
                      <p className="mt-2 flex-1 text-sm text-slate-600 dark:text-slate-300">{item.description}</p>
                    )}
                    {!item.description && <div className="flex-1" />}

                    <div className="mt-5 flex items-center justify-between gap-2 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
                      <span className="flex items-center gap-1 text-sm font-semibold text-amber-600 dark:text-amber-400">
                        🪙 {item.price}
                      </span>

                      {item.isOwned ? (
                        isEquipable ? (
                          isEquipped ? (
                            <motion.button
                              {...buttonTap}
                              onClick={() => unequipMutation.mutate(item.id)}
                              disabled={unequipMutation.isPending}
                              className="flex items-center gap-1 rounded-full border border-slate-300 px-3.5 py-1.5 text-xs font-medium text-slate-600 transition-colors hover:bg-slate-500/10 disabled:opacity-50 dark:border-white/20 dark:text-slate-300"
                            >
                              <X size={11} />
                              {t('shop.unequip')}
                            </motion.button>
                          ) : (
                            <motion.button
                              {...buttonTap}
                              onClick={() => equipMutation.mutate(item.id)}
                              disabled={equipMutation.isPending}
                              className="rounded-full border border-blue-400 px-3.5 py-1.5 text-xs font-medium text-blue-600 transition-colors hover:bg-blue-500/10 disabled:opacity-50 dark:text-cyan-400"
                            >
                              {t('shop.equip')}
                            </motion.button>
                          )
                        ) : null
                      ) : canAfford ? (
                        <motion.button
                          {...buttonTap}
                          onClick={() => purchaseMutation.mutate(item.id)}
                          disabled={purchaseMutation.isPending}
                          className="rounded-full bg-gradient-to-r from-app-accent to-app-accent-2 px-3.5 py-1.5 text-xs font-medium text-white shadow-sm shadow-blue-500/30"
                        >
                          {t('shop.buy')}
                        </motion.button>
                      ) : (
                        <motion.button
                          {...buttonTap}
                          onClick={() =>
                            showFeedback('error', t('shop.needMoreCoins', { count: item.price - (user?.coins ?? 0) }))
                          }
                          className="flex items-center gap-1.5 rounded-full border border-white/10 bg-white/5 px-3.5 py-1.5 text-xs font-medium text-slate-400 backdrop-blur-sm transition-colors hover:border-white/20 hover:text-slate-300 dark:text-slate-400"
                        >
                          <Lock size={11} />
                          {t('shop.locked')}
                        </motion.button>
                      )}
                    </div>
                  </div>
                </GlassCard>
              </motion.div>
            );
          })}
        </motion.div>
      )}
    </motion.div>
  );
}

// Mirrors the real item card's layout (icon square, title, type/rarity line, description,
// price/action footer) so the page doesn't visually "jump" once real data replaces the skeleton.
function ShopItemSkeleton() {
  return (
    <GlassCard hoverLift={false} className="flex flex-col p-6">
      <Skeleton className="h-14 w-14 rounded-2xl" />
      <Skeleton className="mt-4 h-5 w-2/3" />
      <Skeleton className="mt-2 h-3 w-1/3" />
      <Skeleton className="mt-3 h-3 w-full" />
      <Skeleton className="mt-1.5 h-3 w-4/5" />
      <div className="mt-5 flex items-center justify-between gap-2 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
        <Skeleton className="h-4 w-12" />
        <Skeleton className="h-7 w-16 rounded-full" />
      </div>
    </GlassCard>
  );
}
