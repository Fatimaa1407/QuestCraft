import { useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Frame, Palette, Type, ShieldCheck, Zap, Coins as CoinsIcon, Mail, ArrowRight, Sparkles, Trophy, UserCircle, Image, Award, X } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { showToast } from '../../app/toastStore';
import { getMyProfile, updateMyProfile } from '../../api/profile';
import { getMyPurchases, getMyEquippedCosmetics, equipItem, unequipItem } from '../../api/marketplace';
import { getDashboardAnalytics, getMyRank } from '../../api/gamification';
import { getMySubmissions } from '../../api/submissions';
import { getMyQuizAttempts } from '../../api/quizzes';
import { GlassCard } from '../../components/ui/GlassCard';
import { StatCard } from '../../components/ui/StatCard';
import { Textarea } from '../../components/ui/Textarea';
import { EmptyState } from '../../components/ui/EmptyState';
import { Skeleton } from '../../components/ui/Skeleton';
import { FramedAvatar } from '../../components/ui/FramedAvatar';
import { CategoryBreakdown } from '../dashboard/CategoryBreakdown';
import { ActivityFeed } from '../dashboard/ActivityFeed';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { motion } from 'framer-motion';

const equippedTypeIcons: Record<string, typeof Frame> = {
  Avatar: UserCircle,
  ProfileFrame: Frame,
  ProfileBanner: Image,
  Theme: Palette,
  Title: Type,
  Badge: Award,
};

export function ProfilePage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [bio, setBio] = useState('');
  const [isDirty, setIsDirty] = useState(false);
  const [justEquippedFlash, setJustEquippedFlash] = useState(false);

  const profileQuery = useQuery({ queryKey: ['profile', 'me'], queryFn: getMyProfile });
  const equippedQuery = useQuery({ queryKey: ['profile', 'equipped'], queryFn: getMyEquippedCosmetics });
  const purchasesQuery = useQuery({ queryKey: ['marketplace', 'my-purchases'], queryFn: getMyPurchases });
  const rankQuery = useQuery({ queryKey: ['gamification', 'my-rank'], queryFn: () => getMyRank('AllTime') });
  const analyticsQuery = useQuery({ queryKey: ['dashboard-analytics'], queryFn: getDashboardAnalytics });
  const historySubmissionsQuery = useQuery({
    queryKey: ['submissions', 'my', 1, 15],
    queryFn: () => getMySubmissions(1, 15),
  });
  const historyQuizzesQuery = useQuery({
    queryKey: ['quizzes', 'attempts', 'my', 1, 15],
    queryFn: () => getMyQuizAttempts(1, 15),
  });

  useEffect(() => {
    if (profileQuery.data && !isDirty) {
      setBio(profileQuery.data.bio ?? '');
    }
  }, [profileQuery.data, isDirty]);

  const saveMutation = useMutation({
    mutationFn: () => updateMyProfile({ bio: bio.trim() || null, avatarUrl: profileQuery.data?.avatarUrl ?? null }),
    onSuccess: () => {
      setIsDirty(false);
      queryClient.invalidateQueries({ queryKey: ['profile', 'me'] });
    },
  });

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    saveMutation.mutate();
  };

  const invalidateEquipState = () => {
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'items'] });
    queryClient.invalidateQueries({ queryKey: ['marketplace', 'my-purchases'] });
    queryClient.invalidateQueries({ queryKey: ['profile', 'equipped'] });
  };
  const equipMutation = useMutation({
    mutationFn: equipItem,
    onSuccess: (_data, itemId) => {
      invalidateEquipState();
      const item = (purchasesQuery.data ?? []).find((p) => p.marketplaceItemId === itemId);
      showToast({ title: t('shop.toastEquipped'), message: item?.itemName, imageUrl: item?.imageUrl, emoji: '✨' });
      setJustEquippedFlash(true);
      setTimeout(() => setJustEquippedFlash(false), 700);
    },
  });
  const unequipMutation = useMutation({
    mutationFn: unequipItem,
    onSuccess: (_data, itemId) => {
      invalidateEquipState();
      const item = (purchasesQuery.data ?? []).find((p) => p.marketplaceItemId === itemId);
      showToast({ title: t('shop.toastUnequipped'), message: item?.itemName, emoji: '👋' });
    },
  });

  const equippedItems = (purchasesQuery.data ?? []).filter((p) => p.isEquipped);
  const equipped = equippedQuery.data;

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <GlassCard className="overflow-hidden p-0">
          <div
            className="relative h-40 w-full bg-gradient-to-r from-app-accent/20 to-app-accent-2/20 sm:h-56"
            style={equipped?.bannerImageUrl ? { backgroundImage: `url(${equipped.bannerImageUrl})`, backgroundSize: 'cover', backgroundPosition: 'center' } : undefined}
          >
            {equipped?.bannerImageUrl && (
              <div className="absolute inset-0 bg-gradient-to-t from-white/90 via-white/10 to-transparent dark:from-slate-900/90 dark:via-slate-900/10" />
            )}
          </div>
          <div className="flex flex-col items-center gap-4 px-8 pb-8 pt-0 text-center sm:flex-row sm:text-left">
            <FramedAvatar
              username={user?.firstName ?? user?.username ?? '?'}
              avatarUrl={equipped?.avatarUrl ?? user?.avatarUrl}
              frameImageUrl={equipped?.frameImageUrl}
              size={88}
              className={`-mt-12 shrink-0 rounded-full ring-4 ring-white transition-transform duration-300 dark:ring-slate-900 ${justEquippedFlash ? 'animate-pop-in' : ''}`}
            />
            <div className="min-w-0">
              <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
                {user?.firstName} {user?.lastName}
              </h1>
              <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">@{user?.username}</p>
              {equipped?.titleText && (
                <p className="mt-0.5 text-xs font-semibold text-app-accent dark:text-app-accent-2">{equipped.titleText}</p>
              )}
              <div className="mt-3 flex flex-wrap items-center justify-center gap-2 sm:justify-start">
                <span className="rounded-full bg-app-accent/10 px-3 py-1 text-xs font-semibold text-app-accent dark:text-app-accent-2">
                  {user?.role}
                </span>
                {equipped?.badgeName && (
                  <span className="flex items-center gap-1 rounded-full bg-amber-500/10 px-3 py-1 text-xs font-semibold text-amber-600 dark:text-amber-400">
                    {equipped.badgeImageUrl ? (
                      <img src={equipped.badgeImageUrl} alt="" className="h-3.5 w-3.5 rounded-full" />
                    ) : (
                      <Award size={13} />
                    )}
                    {equipped.badgeName}
                  </span>
                )}
                <span className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
                  <Mail size={13} />
                  {user?.email}
                </span>
              </div>
            </div>
          </div>
        </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <StatCard icon={ShieldCheck} label={t('dashboard.level')} value={user?.level ?? 1} tint="blue" />
        <StatCard icon={Zap} label={t('dashboard.xp')} value={user?.xp ?? 0} tint="cyan" />
        <StatCard icon={CoinsIcon} label={t('dashboard.coins')} value={user?.coins ?? 0} tint="amber" />
        {rankQuery.isLoading ? (
          <GlassCard className="p-6">
            <Skeleton className="mb-4 h-11 w-11 rounded-xl" />
            <Skeleton className="h-3.5 w-16" />
            <Skeleton className="mt-2 h-7 w-20" />
          </GlassCard>
        ) : (
          <StatCard
            icon={Trophy}
            label={t('profile.rank')}
            value={`#${rankQuery.data?.rank ?? '-'} / ${rankQuery.data?.totalUsers ?? '-'}`}
            tint="violet"
          />
        )}
      </motion.div>

      <motion.div variants={fadeInUp}>
        <CategoryBreakdown data={analyticsQuery.data?.categoryProgress} isLoading={analyticsQuery.isLoading} />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <ActivityFeed
          submissions={historySubmissionsQuery.data?.items}
          quizzes={historyQuizzesQuery.data?.items}
          isLoading={historySubmissionsQuery.isLoading || historyQuizzesQuery.isLoading}
        />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('profile.equippedTitle')}</h2>
            <Link
              to="/shop"
              className="flex items-center gap-1 text-sm font-medium text-app-accent hover:underline dark:text-app-accent-2"
            >
              {t('profile.changeInShop')}
              <ArrowRight size={14} />
            </Link>
          </div>
          {purchasesQuery.isLoading ? (
            <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <div
                  key={i}
                  className="flex items-center gap-3 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5"
                >
                  <Skeleton className="h-9 w-9 shrink-0 rounded-lg" />
                  <div className="min-w-0 flex-1 space-y-1.5">
                    <Skeleton className="h-3.5 w-2/3" />
                    <Skeleton className="h-3 w-1/3" />
                  </div>
                </div>
              ))}
            </div>
          ) : equippedItems.length === 0 ? (
            <EmptyState
              bare
              icon={Sparkles}
              tint="blue"
              title={t('profile.noEquipped')}
              action={{ label: t('profile.browseShop'), to: '/shop' }}
              className="mt-2"
            />
          ) : (
            <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
              {equippedItems.map((item) => {
                const Icon = equippedTypeIcons[item.itemType] ?? Frame;
                return (
                  <div
                    key={item.id}
                    className="flex items-center gap-3 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5"
                  >
                    <span className="flex h-9 w-9 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-gradient-to-br from-app-accent to-app-accent-2 text-white">
                      {item.imageUrl ? <img src={item.imageUrl} alt="" className="h-full w-full object-cover" /> : <Icon size={16} />}
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-slate-800 dark:text-slate-100">{item.itemName}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">{item.itemType}</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => unequipMutation.mutate(item.marketplaceItemId)}
                      disabled={unequipMutation.isPending}
                      className="flex shrink-0 items-center gap-1 rounded-full border border-slate-300 px-2.5 py-1 text-[11px] font-medium text-slate-600 transition-colors hover:bg-slate-500/10 disabled:opacity-50 dark:border-white/20 dark:text-slate-300"
                    >
                      <X size={10} />
                      {t('shop.unequip')}
                    </button>
                  </div>
                );
              })}
            </div>
          )}
        </GlassCard>
      </motion.div>

      {(purchasesQuery.data ?? []).some((p) => !p.isEquipped) && (
        <motion.div variants={fadeInUp}>
          <GlassCard className="p-6">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('profile.inventoryTitle')}</h2>
            <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
              {(purchasesQuery.data ?? [])
                .filter((p) => !p.isEquipped)
                .map((item) => {
                  const Icon = equippedTypeIcons[item.itemType] ?? Frame;
                  const isEquipableType = Object.prototype.hasOwnProperty.call(equippedTypeIcons, item.itemType);
                  return (
                    <div
                      key={item.id}
                      className="flex items-center gap-3 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5"
                    >
                      <span className="flex h-9 w-9 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-slate-500/10 text-slate-500 dark:text-slate-400">
                        {item.imageUrl ? <img src={item.imageUrl} alt="" className="h-full w-full object-cover" /> : <Icon size={16} />}
                      </span>
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm font-medium text-slate-800 dark:text-slate-100">{item.itemName}</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400">{item.itemType}</p>
                      </div>
                      {isEquipableType && (
                        <button
                          type="button"
                          onClick={() => equipMutation.mutate(item.marketplaceItemId)}
                          disabled={equipMutation.isPending}
                          className="shrink-0 rounded-full border border-app-accent px-2.5 py-1 text-[11px] font-medium text-app-accent transition-colors hover:bg-app-accent/10 disabled:opacity-50 dark:text-app-accent-2"
                        >
                          {t('shop.equip')}
                        </button>
                      )}
                    </div>
                  );
                })}
            </div>
          </GlassCard>
        </motion.div>
      )}

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('profile.bioTitle')}</h2>
          <form onSubmit={handleSubmit} className="mt-4 space-y-3">
            <Textarea
              id="bio"
              label={t('profile.bioLabel')}
              placeholder={t('profile.bioPlaceholder')}
              rows={3}
              maxLength={200}
              value={bio}
              onChange={(e) => {
                setBio(e.target.value);
                setIsDirty(true);
              }}
            />
            <div className="flex items-center gap-3">
              <button
                type="submit"
                disabled={!isDirty || saveMutation.isPending}
                className="rounded-lg bg-gradient-to-r from-app-accent to-app-accent-2 px-5 py-2 text-sm font-medium text-white shadow-lg shadow-app-accent/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {saveMutation.isPending ? t('profile.saving') : t('profile.save')}
              </button>
              {saveMutation.isSuccess && !isDirty && (
                <span className="text-xs text-emerald-600 dark:text-emerald-400">{t('profile.saved')}</span>
              )}
            </div>
          </form>
        </GlassCard>
      </motion.div>
    </motion.div>
  );
}
