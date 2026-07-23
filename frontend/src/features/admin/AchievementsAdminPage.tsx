import { useTranslation } from 'react-i18next';
import {
  createAchievement,
  deleteAchievement,
  getAchievementsAdmin,
  getDeletedAchievements,
  restoreAchievement,
  updateAchievement,
  type AchievementPayload,
} from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

const conditionTypeOptions = [
  { value: 'SubmissionCount', label: 'SubmissionCount' },
  { value: 'AcceptedCount', label: 'AcceptedCount' },
  { value: 'XpTotal', label: 'XpTotal' },
  { value: 'StreakDays', label: 'StreakDays' },
  { value: 'SpeedSolve', label: 'SpeedSolve' },
  { value: 'QuizzesCompleted', label: 'QuizzesCompleted' },
  { value: 'DailyPuzzleDaysSolved', label: 'DailyPuzzleDaysSolved' },
];

export function AchievementsAdminPage() {
  const { t } = useTranslation();

  const fields: CrudFieldConfig[] = [
    { key: 'name', label: t('admin.achievements.name'), type: 'text', required: true, withEn: true, enLabel: t('admin.achievements.nameEn') },
    { key: 'description', label: t('admin.achievements.description'), type: 'textarea', required: true },
    { key: 'iconUrl', label: t('admin.achievements.iconUrl'), type: 'text' },
    { key: 'conditionType', label: t('admin.achievements.conditionType'), type: 'select', options: conditionTypeOptions, required: true },
    { key: 'conditionValue', label: t('admin.achievements.conditionValue'), type: 'number', required: true },
    { key: 'xpReward', label: t('admin.achievements.xpReward'), type: 'number' },
    { key: 'coinReward', label: t('admin.achievements.coinReward'), type: 'number' },
    { key: 'isActive', label: t('admin.achievements.active'), type: 'checkbox' },
  ];

  const buildPayload = (p: Record<string, unknown>): AchievementPayload => ({
    name: String(p.name ?? ''),
    nameEn: (p.nameEn as string) || null,
    description: String(p.description ?? ''),
    descriptionEn: (p.descriptionEn as string) || null,
    iconUrl: (p.iconUrl as string) || null,
    conditionType: String(p.conditionType ?? 'XpTotal'),
    conditionValue: Number(p.conditionValue),
    xpReward: Number(p.xpReward ?? 0),
    coinReward: Number(p.coinReward ?? 0),
    isActive: Boolean(p.isActive),
  });

  return (
    <SimpleCrudTable
      queryKey="admin-achievements"
      title={t('admin.achievements.title')}
      addLabel={t('admin.achievements.add')}
      columns={[
        { key: 'name', label: t('admin.achievements.name') },
        { key: 'conditionType', label: t('admin.achievements.conditionType') },
        { key: 'xpReward', label: t('admin.achievements.xpReward') },
      ]}
      fields={fields}
      api={{
        list: getAchievementsAdmin,
        listDeleted: getDeletedAchievements,
        create: (p) => createAchievement(buildPayload(p)),
        update: (id, p) => updateAchievement(id, buildPayload(p)),
        remove: deleteAchievement,
        restore: restoreAchievement,
      }}
    />
  );
}
