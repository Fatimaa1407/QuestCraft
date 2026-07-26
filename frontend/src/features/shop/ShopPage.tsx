import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { useSearchParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { UserCircle, Frame, Image, Palette, Award, Type, ShoppingBag, Wallet, Snowflake, Receipt, Heart, SearchX } from 'lucide-react';
import {
  getMarketplaceItems, getItemTypes, purchaseItem, equipItem, unequipItem, getMyPurchases,
  getBundles, purchaseBundle, toggleWishlist, getMyWishlist, getMyEquippedCosmetics,
} from '../../api/marketplace';
import { EQUIPABLE_ITEM_TYPES } from '../../types/marketplace';
import type { PurchaseResultDto, MarketplaceItemDto } from '../../types/marketplace';
import { useAnimatedNumber } from '../../utils/useAnimatedNumber';
import { getRarity, RARITY_ORDER } from '../../utils/rarity';
import { useAuthStore } from '../../app/authStore';
import { showToast } from '../../app/toastStore';
import { Skeleton } from '../../components/ui/Skeleton';
import { GlassCard } from '../../components/ui/GlassCard';
import { EmptyState } from '../../components/ui/EmptyState';
import { QueryErrorState } from '../../components/ui/QueryErrorState';
import { PurchaseSuccessModal } from '../../components/ui/PurchaseSuccessModal';
import { getApiErrorMessage } from '../../utils/apiError';
import { playSuccessSound, playErrorSound } from '../../utils/sounds';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { ShopItemCard } from './ShopItemCard';
import { FeaturedItem } from './FeaturedItem';
import { BundleCard } from './BundleCard';
import { MysteryBox } from './MysteryBox';
import { MiniProfilePreview } from './MiniProfilePreview';
import { ShopFilters, type ShopOwnership, type ShopSort } from './ShopFilters';

// staggerContainer's default 0.12s-per-child delay is fine for short lists, but the catalogue
// grid can hold well over a dozen items — at the default rate the last cards wouldn't start
// entering for 2+ seconds, reading as "missing" rather than "still animating in".
const itemGridStagger = { hidden: {}, show: { transition: { staggerChildren: 0.025, delayChildren: 0.05 } } };

const typeIcons: Record<string, typeof UserCircle> = {
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
  const [searchParams, setSearchParams] = useSearchParams();
  const typeId = searchParams.get('type') ? Number(searchParams.get('type')) : undefined;
  const setTypeId = (value: number | undefined) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (value === undefined) next.delete('type');
        else next.set('type', String(value));
        return next;
      },
      { replace: true },
    );
  };
  const [tab, setTab] = useState<'shop' | 'wishlist'>('shop');
  const [search, setSearch] = useState('');
  const [sort, setSort] = useState<ShopSort>('default');
  const [ownership, setOwnership] = useState<ShopOwnership>('all');
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [celebratingId, setCelebratingId] = useState<number | null>(null);
  const [purchaseModalItem, setPurchaseModalItem] = useState<PurchaseResultDto | null>(null);
  const [hoveredItem, setHoveredItem] = useState<MarketplaceItemDto | null>(null);

  const animatedCoins = useAnimatedNumber(user?.coins ?? 0);

  const typesQuery = useQuery({ queryKey: ['marketplace', 'item-types'], queryFn: getItemTypes });
  // Full catalogue fetched once — category/search/sort/ownership are all applied client-side so the
  // Featured pick and filters compose without extra round-trips.
  const itemsQuery = useQuery({ queryKey: ['marketplace', 'items'], queryFn: () => getMarketplaceItems() });
  const purchasesQuery = useQuery({ queryKey: ['marketplace', 'my-purchases'], queryFn: getMyPurchases });
  const bundlesQuery = useQuery({ queryKey: ['marketplace', 'bundles'], queryFn: getBundles });
  const wishlistQuery = useQuery({ queryKey: ['marketplace', 'wishlist'], queryFn: getMyWishlist, enabled: tab === 'wishlist' });
  // Same queryKey AppLayout uses for the navbar cosmetics chip — shares its cache instead of refetching.
  const equippedQuery = useQuery({ queryKey: ['profile', 'equipped'], queryFn: getMyEquippedCosmetics });
  const equippedItemIds = new Set((purchasesQuery.data ?? []).filter((p) => p.isEquipped).map((p) => p.marketplaceItemId));

  const invalidateEquipState = () => {
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'my-purchases'] });
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'bundles'] });
    queryClient.invalidateQueries({ queryKey: ['profile', 'equipped'] });
  };

  const showFeedback = (type: 'success' | 'error', text: string) => {
    setFeedback({ type, text });
    setTimeout(() => setFeedback(null), 3000);
  };

  const items = itemsQuery.data ?? [];

  const purchaseMutation = useMutation({
    mutationFn: purchaseItem,
    onSuccess: (result, itemId) => {
      if (!result) return;
      updateUser({ coins: result.remainingCoins });
      invalidateEquipState();
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'wishlist'] });
      playSuccessSound();
      setCelebratingId(itemId);
      setTimeout(() => setCelebratingId(null), 1300);
      showFeedback('success', t('shop.purchaseSuccess', { name: result.itemName }));
      if (result.autoEquipped) {
        showToast({ title: t('shop.toastEquipped'), message: result.itemName, imageUrl: result.imageUrl, emoji: '✨' });
      }
      setPurchaseModalItem(result);
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const equipMutation = useMutation({
    mutationFn: equipItem,
    onSuccess: (_data, itemId) => {
      invalidateEquipState();
      playSuccessSound();
      setCelebratingId(itemId);
      setTimeout(() => setCelebratingId(null), 1300);
      const item = items.find((i) => i.id === itemId);
      showToast({ title: t('shop.toastEquipped'), message: item?.name, imageUrl: item?.imageUrl, emoji: '✨' });
      setPurchaseModalItem((current) => (current?.marketplaceItemId === itemId ? null : current));
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const unequipMutation = useMutation({
    mutationFn: unequipItem,
    onSuccess: (_data, itemId) => {
      invalidateEquipState();
      playSuccessSound();
      const item = items.find((i) => i.id === itemId);
      showToast({ title: t('shop.toastUnequipped'), message: item?.name, emoji: '👋' });
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const purchaseBundleMutation = useMutation({
    mutationFn: purchaseBundle,
    onSuccess: (result) => {
      if (!result) return;
      updateUser({ coins: result.remainingCoins });
      invalidateEquipState();
      playSuccessSound();
      showToast({ title: t('shop.purchaseSuccess', { name: result.bundleName }), emoji: '🎁' });
    },
    onError: (err) => {
      playErrorSound();
      showFeedback('error', getApiErrorMessage(err, t('shop.actionError')));
    },
  });

  const wishlistMutation = useMutation({
    mutationFn: toggleWishlist,
    onSuccess: (isWishlisted, itemId) => {
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
      queryClient.invalidateQueries({ queryKey: ['marketplace', 'wishlist'] });
      const item = items.find((i) => i.id === itemId) ?? wishlistQuery.data?.find((i) => i.id === itemId);
      showToast({
        title: isWishlisted ? t('shop.wishlistAdded') : t('shop.wishlistRemoved'),
        message: item?.name,
        emoji: isWishlisted ? '❤️' : '💔',
      });
    },
    onError: (err) => showFeedback('error', getApiErrorMessage(err, t('shop.actionError'))),
  });

  const featuredItem = useMemo(() => {
    if (items.length === 0) return null;
    return items.find((i) => i.isFeatured) ?? items.reduce((a, b) => (b.price > a.price ? b : a));
  }, [items]);

  const filteredItems = useMemo(() => {
    let list = items;
    if (typeId !== undefined) list = list.filter((i) => i.itemTypeId === typeId);
    if (search.trim()) {
      const term = search.trim().toLowerCase();
      list = list.filter((i) => i.name.toLowerCase().includes(term) || i.description?.toLowerCase().includes(term));
    }
    if (ownership === 'owned') list = list.filter((i) => i.isOwned);
    else if (ownership === 'notOwned') list = list.filter((i) => !i.isOwned);
    else if (ownership === 'equipped') list = list.filter((i) => equippedItemIds.has(i.id));
    else if (ownership === 'wishlisted') list = list.filter((i) => i.isWishlisted);

    const sorted = [...list];
    if (sort === 'priceAsc') sorted.sort((a, b) => a.price - b.price);
    else if (sort === 'priceDesc') sorted.sort((a, b) => b.price - a.price);
    else if (sort === 'rarityAsc') sorted.sort((a, b) => RARITY_ORDER.indexOf(getRarity(a.price)) - RARITY_ORDER.indexOf(getRarity(b.price)));
    else if (sort === 'rarityDesc') sorted.sort((a, b) => RARITY_ORDER.indexOf(getRarity(b.price)) - RARITY_ORDER.indexOf(getRarity(a.price)));
    return sorted;
  }, [items, typeId, search, ownership, sort, equippedItemIds]);

  const hasActiveFilters = typeId !== undefined || search.trim() !== '' || ownership !== 'all' || sort !== 'default';
  const clearFilters = () => {
    setTypeId(undefined);
    setSearch('');
    setOwnership('all');
    setSort('default');
  };

  const wishlistItems = wishlistQuery.data ?? [];

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-8">
      <motion.div variants={fadeInUp} className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-start sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{t('shop.title')}</h1>
          <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t('shop.subtitle')}</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Link
            to="/shop/history"
            className="flex items-center gap-1.5 rounded-full border border-slate-200/70 px-4 py-2.5 text-sm font-medium text-slate-600 transition-colors hover:border-app-accent dark:border-white/[0.08] dark:text-slate-300"
          >
            <Receipt size={15} />
            {t('shop.history')}
          </Link>
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

      <motion.div variants={fadeInUp} className="flex items-center gap-2">
        <button
          onClick={() => setTab('shop')}
          className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
            tab === 'shop'
              ? 'bg-gradient-to-r from-app-accent to-app-accent-2 text-white shadow-sm shadow-app-accent/30'
              : 'border border-slate-200/70 text-slate-600 dark:border-white/[0.08] dark:text-slate-300'
          }`}
        >
          🛍️ {t('shop.title')}
        </button>
        <button
          onClick={() => setTab('wishlist')}
          className={`flex items-center gap-1.5 rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
            tab === 'wishlist'
              ? 'bg-gradient-to-r from-rose-500 to-red-500 text-white shadow-sm shadow-rose-500/30'
              : 'border border-slate-200/70 text-slate-600 dark:border-white/[0.08] dark:text-slate-300'
          }`}
        >
          <Heart size={13} className={tab === 'wishlist' ? 'fill-white' : ''} />
          {t('shop.wishlist')}
        </button>
      </motion.div>

      {tab === 'shop' ? (
        <>
          {featuredItem && (
            <motion.div variants={fadeInUp}>
              <FeaturedItem
                item={featuredItem}
                icon={typeIcons[featuredItem.itemType] ?? ShoppingBag}
                canAfford={(user?.coins ?? 0) >= featuredItem.price}
                isPurchasing={purchaseMutation.isPending && purchaseMutation.variables === featuredItem.id}
                isTogglingWishlist={wishlistMutation.isPending && wishlistMutation.variables === featuredItem.id}
                onPurchase={() => purchaseMutation.mutate(featuredItem.id)}
                onLockedClick={() => showFeedback('error', t('shop.needMoreCoins', { count: featuredItem.price - (user?.coins ?? 0) }))}
                onToggleWishlist={() => wishlistMutation.mutate(featuredItem.id)}
              />
            </motion.div>
          )}

          {(bundlesQuery.data?.length ?? 0) > 0 && (
            <motion.div variants={fadeInUp} className="space-y-3">
              <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">{t('shop.bundles')}</h2>
              <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
                {bundlesQuery.data!.map((bundle) => (
                  <BundleCard
                    key={bundle.id}
                    bundle={bundle}
                    canAfford={(user?.coins ?? 0) >= bundle.bundlePrice}
                    isPurchasing={purchaseBundleMutation.isPending && purchaseBundleMutation.variables === bundle.id}
                    onPurchase={() => purchaseBundleMutation.mutate(bundle.id)}
                  />
                ))}
                <MysteryBox />
              </div>
            </motion.div>
          )}
          {(bundlesQuery.data?.length ?? 0) === 0 && (
            <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
              <MysteryBox />
            </motion.div>
          )}

          <motion.div variants={fadeInUp} className="space-y-3">
            <div className="flex flex-wrap items-center gap-2">
              <button
                onClick={() => setTypeId(undefined)}
                className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
                  typeId === undefined
                    ? 'bg-gradient-to-r from-app-accent to-app-accent-2 text-white shadow-sm shadow-app-accent/30'
                    : 'border border-slate-200/70 text-slate-600 hover:border-app-accent dark:border-white/[0.08] dark:text-slate-300'
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
            </div>
            <ShopFilters search={search} onSearchChange={setSearch} sort={sort} onSortChange={setSort} ownership={ownership} onOwnershipChange={setOwnership} />
          </motion.div>

          {itemsQuery.isLoading ? (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {Array.from({ length: 6 }).map((_, i) => (
                <ShopItemSkeleton key={i} />
              ))}
            </div>
          ) : itemsQuery.isError ? (
            <QueryErrorState onRetry={() => itemsQuery.refetch()} />
          ) : filteredItems.length === 0 ? (
            <EmptyState
              icon={items.length === 0 ? ShoppingBag : ownership === 'wishlisted' ? Heart : SearchX}
              tint="amber"
              title={
                items.length === 0
                  ? t('shop.empty')
                  : ownership === 'owned'
                    ? t('shop.noOwnedItems')
                    : ownership === 'equipped'
                      ? t('shop.noEquippedItems')
                      : ownership === 'wishlisted'
                        ? t('shop.wishlistEmpty')
                        : t('shop.noSearchResults')
              }
              action={hasActiveFilters ? { label: t('shop.clearFilters'), onClick: clearFilters } : undefined}
            />
          ) : (
            <motion.div variants={itemGridStagger} initial="hidden" animate="show" className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {filteredItems.map((item) => {
                const Icon = typeIcons[item.itemType] ?? ShoppingBag;
                const isEquipable = EQUIPABLE_ITEM_TYPES.includes(item.itemType);
                const isEquipped = equippedItemIds.has(item.id);
                const canAfford = (user?.coins ?? 0) >= item.price;

                return (
                  <ShopItemCard
                    key={item.id}
                    item={item}
                    icon={Icon}
                    isEquipable={isEquipable}
                    isEquipped={isEquipped}
                    canAfford={canAfford}
                    isCelebrating={celebratingId === item.id}
                    isPurchasing={purchaseMutation.isPending && purchaseMutation.variables === item.id}
                    isEquipping={equipMutation.isPending && equipMutation.variables === item.id}
                    isUnequipping={unequipMutation.isPending && unequipMutation.variables === item.id}
                    isTogglingWishlist={wishlistMutation.isPending && wishlistMutation.variables === item.id}
                    onPurchase={() => purchaseMutation.mutate(item.id)}
                    onEquip={() => equipMutation.mutate(item.id)}
                    onUnequip={() => unequipMutation.mutate(item.id)}
                    onLockedClick={() => showFeedback('error', t('shop.needMoreCoins', { count: item.price - (user?.coins ?? 0) }))}
                    onToggleWishlist={() => wishlistMutation.mutate(item.id)}
                    onHoverStart={() => setHoveredItem(item)}
                    onHoverEnd={() => setHoveredItem((current) => (current?.id === item.id ? null : current))}
                  />
                );
              })}
            </motion.div>
          )}
        </>
      ) : wishlistQuery.isLoading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <ShopItemSkeleton key={i} />
          ))}
        </div>
      ) : wishlistItems.length === 0 ? (
        <EmptyState icon={Heart} tint="amber" title={t('shop.wishlistEmpty')} description={t('shop.wishlistEmptyHint')} />
      ) : (
        <motion.div variants={itemGridStagger} initial="hidden" animate="show" className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {wishlistItems.map((item) => {
            const Icon = typeIcons[item.itemType] ?? ShoppingBag;
            const isEquipable = EQUIPABLE_ITEM_TYPES.includes(item.itemType);
            const isEquipped = equippedItemIds.has(item.id);
            const canAfford = (user?.coins ?? 0) >= item.price;

            return (
              <ShopItemCard
                key={item.id}
                item={item}
                icon={Icon}
                isEquipable={isEquipable}
                isEquipped={isEquipped}
                canAfford={canAfford}
                isCelebrating={celebratingId === item.id}
                isPurchasing={purchaseMutation.isPending && purchaseMutation.variables === item.id}
                isEquipping={equipMutation.isPending && equipMutation.variables === item.id}
                isUnequipping={unequipMutation.isPending && unequipMutation.variables === item.id}
                isTogglingWishlist={wishlistMutation.isPending && wishlistMutation.variables === item.id}
                onPurchase={() => purchaseMutation.mutate(item.id)}
                onEquip={() => equipMutation.mutate(item.id)}
                onUnequip={() => unequipMutation.mutate(item.id)}
                onLockedClick={() => showFeedback('error', t('shop.needMoreCoins', { count: item.price - (user?.coins ?? 0) }))}
                onToggleWishlist={() => wishlistMutation.mutate(item.id)}
                onHoverStart={() => setHoveredItem(item)}
                onHoverEnd={() => setHoveredItem((current) => (current?.id === item.id ? null : current))}
              />
            );
          })}
        </motion.div>
      )}

      <MiniProfilePreview item={hoveredItem} equipped={equippedQuery.data} username={user?.username ?? ''} level={user?.level ?? 1} />

      <PurchaseSuccessModal
        isOpen={!!purchaseModalItem}
        itemName={purchaseModalItem?.itemName ?? ''}
        itemType={purchaseModalItem?.itemType ?? ''}
        imageUrl={purchaseModalItem?.imageUrl ?? null}
        pricePaid={purchaseModalItem?.pricePaid ?? 0}
        autoEquipped={!!purchaseModalItem?.autoEquipped}
        onClose={() => setPurchaseModalItem(null)}
      />
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
