import { useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Frame, Palette, Type, ShieldCheck, Zap, Coins as CoinsIcon, Mail, ArrowRight } from 'lucide-react';
import { useAuthStore } from '../../app/authStore';
import { getMyProfile, updateMyProfile } from '../../api/profile';
import { getMyPurchases } from '../../api/marketplace';
import { GlassCard } from '../../components/ui/GlassCard';
import { StatCard } from '../../components/ui/StatCard';
import { Textarea } from '../../components/ui/Textarea';
import { fadeInUp, staggerContainer } from '../../utils/motion';
import { motion } from 'framer-motion';

const equippedTypeIcons: Record<string, typeof Frame> = {
  ProfileFrame: Frame,
  Theme: Palette,
  Title: Type,
};

export function ProfilePage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [bio, setBio] = useState('');
  const [isDirty, setIsDirty] = useState(false);

  const profileQuery = useQuery({ queryKey: ['profile', 'me'], queryFn: getMyProfile });
  const purchasesQuery = useQuery({ queryKey: ['marketplace', 'my-purchases'], queryFn: getMyPurchases });

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

  const equippedItems = (purchasesQuery.data ?? []).filter((p) => p.isEquipped);

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <GlassCard className="flex flex-col items-center gap-4 p-8 text-center sm:flex-row sm:text-left">
          {user?.avatarUrl ? (
            <img src={user.avatarUrl} alt="" className="h-20 w-20 rounded-full object-cover" />
          ) : (
            <span className="flex h-20 w-20 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 text-2xl font-semibold text-white">
              {(user?.firstName ?? user?.username ?? '?').charAt(0).toUpperCase()}
            </span>
          )}
          <div className="min-w-0">
            <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
              {user?.firstName} {user?.lastName}
            </h1>
            <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">@{user?.username}</p>
            <div className="mt-3 flex flex-wrap items-center justify-center gap-2 sm:justify-start">
              <span className="rounded-full bg-indigo-500/10 px-3 py-1 text-xs font-semibold text-indigo-600 dark:text-cyan-400">
                {user?.role}
              </span>
              <span className="flex items-center gap-1.5 text-xs text-slate-500 dark:text-slate-400">
                <Mail size={13} />
                {user?.email}
              </span>
            </div>
          </div>
        </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard icon={ShieldCheck} label={t('dashboard.level')} value={user?.level ?? 1} tint="blue" />
        <StatCard icon={Zap} label={t('dashboard.xp')} value={user?.xp ?? 0} tint="cyan" />
        <StatCard icon={CoinsIcon} label={t('dashboard.coins')} value={user?.coins ?? 0} tint="amber" />
      </motion.div>

      <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('profile.equippedTitle')}</h2>
            <Link
              to="/shop"
              className="flex items-center gap-1 text-sm font-medium text-indigo-600 hover:underline dark:text-cyan-400"
            >
              {t('profile.changeInShop')}
              <ArrowRight size={14} />
            </Link>
          </div>
          {equippedItems.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500 dark:text-slate-400">{t('profile.noEquipped')}</p>
          ) : (
            <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
              {equippedItems.map((item) => {
                const Icon = equippedTypeIcons[item.itemType] ?? Frame;
                return (
                  <div
                    key={item.id}
                    className="flex items-center gap-3 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5"
                  >
                    <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-500 to-cyan-500 text-white">
                      <Icon size={16} />
                    </span>
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-slate-800 dark:text-slate-100">{item.itemName}</p>
                      <p className="text-xs text-slate-500 dark:text-slate-400">{item.itemType}</p>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </GlassCard>
      </motion.div>

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
                className="rounded-lg bg-gradient-to-r from-indigo-600 to-cyan-500 px-5 py-2 text-sm font-medium text-white shadow-lg shadow-indigo-600/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
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
