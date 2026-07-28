import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { ShieldCheck, Zap, Award, Calendar } from 'lucide-react';
import { Modal } from './Modal';
import { FramedAvatar } from './FramedAvatar';
import { Skeleton } from './Skeleton';
import { getUserProfile } from '../../api/profile';

interface UserProfileModalProps {
  userId: number | null;
  onClose: () => void;
}

// Read-only "view another user's profile" popup — reused everywhere a username/avatar appears for
// someone other than the logged-in user (friends, leaderboard, chat, discussion, battles), so Bio
// and equipped cosmetics are finally visible to someone other than their owner.
export function UserProfileModal({ userId, onClose }: UserProfileModalProps) {
  const { t, i18n } = useTranslation();
  const profileQuery = useQuery({
    queryKey: ['profile', 'public', userId],
    queryFn: () => getUserProfile(userId!),
    enabled: userId !== null,
  });

  const profile = profileQuery.data;

  return (
    <Modal isOpen={userId !== null} onClose={onClose} title={t('profile.viewProfile')}>
      {profileQuery.isLoading ? (
        <div className="flex items-center gap-4">
          <Skeleton className="h-16 w-16 shrink-0 rounded-full" />
          <div className="min-w-0 flex-1 space-y-2">
            <Skeleton className="h-4 w-1/2" />
            <Skeleton className="h-3 w-1/3" />
          </div>
        </div>
      ) : profileQuery.isError || !profile ? (
        <p className="text-sm text-slate-500 dark:text-slate-400">{t('profile.loadError')}</p>
      ) : (
        <div className="space-y-5">
          {profile.bannerImageUrl && (
            <div
              className="-mx-6 -mt-6 h-24 w-[calc(100%+3rem)] bg-cover bg-center"
              style={{ backgroundImage: `url(${profile.bannerImageUrl})` }}
            />
          )}
          <div className={`flex items-center gap-4 ${profile.bannerImageUrl ? '-mt-10' : ''}`}>
            <FramedAvatar
              username={profile.username}
              avatarUrl={profile.avatarUrl}
              frameImageUrl={profile.frameImageUrl}
              size={72}
              className="shrink-0 ring-4 ring-white dark:ring-slate-900"
            />
            <div className="min-w-0">
              <h3 className="truncate text-lg font-bold text-slate-900 dark:text-white">@{profile.username}</h3>
              {profile.titleText && (
                <p className="text-xs font-semibold text-app-accent dark:text-app-accent-2">{profile.titleText}</p>
              )}
              {profile.badgeName && (
                <span className="mt-1 flex w-fit items-center gap-1 rounded-full bg-amber-500/10 px-2.5 py-0.5 text-[11px] font-semibold text-amber-600 dark:text-amber-400">
                  {profile.badgeImageUrl ? (
                    <img src={profile.badgeImageUrl} alt="" className="h-3 w-3 rounded-full" />
                  ) : (
                    <Award size={11} />
                  )}
                  {profile.badgeName}
                </span>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="flex items-center gap-2 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5">
              <ShieldCheck size={16} className="text-app-accent dark:text-app-accent-2" />
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400">{t('dashboard.level')}</p>
                <p className="font-semibold text-slate-900 dark:text-white">{profile.level}</p>
              </div>
            </div>
            <div className="flex items-center gap-2 rounded-xl border border-slate-200/70 bg-white/60 px-4 py-3 dark:border-white/[0.08] dark:bg-white/5">
              <Zap size={16} className="text-app-accent dark:text-app-accent-2" />
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400">{t('dashboard.xp')}</p>
                <p className="font-semibold text-slate-900 dark:text-white">{profile.xp}</p>
              </div>
            </div>
          </div>

          <div>
            <p className="text-sm text-slate-700 dark:text-slate-200">
              {profile.bio || <span className="italic text-slate-400 dark:text-slate-500">{t('profile.noBio')}</span>}
            </p>
          </div>

          <div className="flex items-center gap-1.5 text-xs text-slate-400 dark:text-slate-500">
            <Calendar size={12} />
            {t('profile.memberSince', { date: new Date(profile.joinedAt).toLocaleDateString(i18n.language === 'en' ? 'en-US' : 'az-AZ') })}
          </div>
        </div>
      )}
    </Modal>
  );
}
