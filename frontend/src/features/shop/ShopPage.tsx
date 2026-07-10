import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { Lightbulb, UserCircle, Frame, Palette, Award, Type, ShoppingBag, Check, Lock, Wallet } from 'lucide-react';
import { getMarketplaceItems, getItemTypes, purchaseItem, equipItem } from '../../api/marketplace';
import { EQUIPABLE_ITEM_TYPES } from '../../types/marketplace';
import { getRarity, RARITY_STYLES } from '../../utils/rarity';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { useAuthStore } from '../../app/authStore';
import { GlassCard } from '../../components/ui/GlassCard';
import { getApiErrorMessage } from '../../utils/apiError';
import { playSuccessSound, playErrorSound } from '../../utils/sounds';

const typeIcons: Record<string, typeof Lightbulb> = {
  Hint: Lightbulb,
  Avatar: UserCircle,
  ProfileFrame: Frame,
  Theme: Palette,
  Badge: Award,
  Title: Type,
};

const typeEmoji: Record<string, string> = {
  Avatar: '😀',
  Badge: '🏅',
  Hint: '💡',
  ProfileFrame: '🖼️',
  Theme: '🎨',
  Title: '👑',
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

  const showFeedback = (type: 'success' | 'error', text: string) => {
    setFeedback({ type, text });
    setTimeout(() => setFeedback(null), 3000);
  };

  const purchaseMutation = useMutation({
    mutationFn: purchaseItem,
    onSuccess: (result, itemId) => {
      if (!result) return;
      updateUser({ coins: result.remainingCoins });
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
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
      playSuccessSound();
      showFeedback('success', t('shop.equipSuccess'));
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const items = itemsQuery.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t('shop.title')}</h1>
          <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400">{t('shop.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2.5 rounded-2xl border border-amber-400/40 bg-amber-500/10 px-5 py-3">
          <Wallet size={20} className="text-amber-600 dark:text-amber-400" />
          <div>
            <p className="text-[11px] font-medium uppercase tracking-wide text-amber-600/80 dark:text-amber-400/80">
              {t('shop.coinsLabel')}
            </p>
            <p className="flex items-center gap-1 text-xl font-bold text-amber-600 dark:text-amber-400">
              🪙 {animatedCoins}
            </p>
          </div>
        </div>
      </div>

      {feedback && (
        <p
          className={`rounded-lg px-3 py-2 text-sm ${
            feedback.type === 'success'
              ? 'bg-emerald-50 text-emerald-600 dark:bg-emerald-950/50 dark:text-emerald-400'
              : 'bg-red-50 text-red-600 dark:bg-red-950/50 dark:text-red-400'
          }`}
        >
          {feedback.text}
        </p>
      )}

      <div className="flex flex-wrap items-center gap-2">
        <button
          onClick={() => setTypeId(undefined)}
          className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
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
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
              typeId === type.id
                ? 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-sm shadow-blue-500/30'
                : 'border border-slate-200/70 text-slate-600 hover:border-blue-400 dark:border-white/[0.08] dark:text-slate-300'
            }`}
          >
            {typeEmoji[type.name] ?? '✨'} {type.name}
          </button>
        ))}
      </div>

      {itemsQuery.isLoading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">{t('common.loading')}</p>
      ) : items.length === 0 ? (
        <p className="text-sm text-slate-500 dark:text-slate-500">{t('shop.empty')}</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((item) => {
            const Icon = typeIcons[item.itemType] ?? ShoppingBag;
            const isEquipable = EQUIPABLE_ITEM_TYPES.includes(item.itemType);
            const canAfford = (user?.coins ?? 0) >= item.price;
            const rarity = getRarity(item.price);
            const rarityStyle = RARITY_STYLES[rarity];
            const isCelebrating = justPurchasedId === item.id;

            return (
              <GlassCard
                key={item.id}
                style={{ border: `2px solid ${rarityStyle.borderColor}` }}
                className={`relative flex flex-col overflow-hidden p-0 transition-all duration-200 hover:-translate-y-1 hover:scale-[1.02] hover:shadow-2xl ${rarityStyle.glow} ${
                  isCelebrating ? 'animate-card-pulse' : ''
                }`}
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
                  <div className="flex items-center justify-center gap-1.5 bg-emerald-500/15 py-1.5 text-xs font-semibold text-emerald-600 dark:text-emerald-400">
                    <Check size={13} />
                    {t('shop.owned')}
                  </div>
                )}

                <div className="flex flex-1 flex-col p-5">
                  <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-500/10 text-blue-600 dark:text-cyan-400">
                    {item.imageUrl ? (
                      <img src={item.imageUrl} alt="" className="h-full w-full rounded-2xl object-cover" />
                    ) : (
                      <Icon size={26} />
                    )}
                  </div>

                  <h2 className="mt-3 text-lg font-semibold text-slate-900 dark:text-slate-100">{item.name}</h2>
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

                  <div className="mt-4 flex items-center justify-between gap-2">
                    <span className="flex items-center gap-1 text-sm font-semibold text-amber-600">
                      🪙 {item.price}
                    </span>

                    {item.isOwned ? (
                      isEquipable ? (
                        <button
                          onClick={() => equipMutation.mutate(item.id)}
                          disabled={equipMutation.isPending}
                          className="rounded-full border border-blue-400 px-3.5 py-1.5 text-xs font-medium text-blue-600 transition hover:bg-blue-500/10 disabled:opacity-50 dark:text-cyan-400"
                        >
                          {t('shop.equip')}
                        </button>
                      ) : null
                    ) : canAfford ? (
                      <button
                        onClick={() => purchaseMutation.mutate(item.id)}
                        disabled={purchaseMutation.isPending}
                        className="rounded-full bg-gradient-to-r from-blue-500 to-cyan-500 px-3.5 py-1.5 text-xs font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-110 disabled:opacity-50"
                      >
                        {t('shop.buy')}
                      </button>
                    ) : (
                      <button
                        onClick={() =>
                          showFeedback('error', t('shop.needMoreCoins', { count: item.price - (user?.coins ?? 0) }))
                        }
                        className="flex items-center gap-1 rounded-full bg-slate-200/70 px-3.5 py-1.5 text-xs font-medium text-slate-500 transition hover:bg-slate-300/70 dark:bg-white/[0.06] dark:text-slate-400 dark:hover:bg-white/10"
                      >
                        <Lock size={11} />
                        {t('shop.locked')}
                      </button>
                    )}
                  </div>
                </div>
              </GlassCard>
            );
          })}
        </div>
      )}
    </div>
  );
}
