import { useTranslation } from 'react-i18next';
import {
  createDailyQuestTemplate,
  deleteDailyQuestTemplate,
  getDailyQuestTemplatesAdmin,
  getDeletedDailyQuestTemplates,
  restoreDailyQuestTemplate,
  updateDailyQuestTemplate,
  type DailyQuestTemplatePayload,
} from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

const targetTypeOptions = [
  { value: 'SolveChallenge', label: 'SolveChallenge' },
  { value: 'CompleteQuiz', label: 'CompleteQuiz' },
  { value: 'EarnXp', label: 'EarnXp' },
];

export function DailyQuestsAdminPage() {
  const { t } = useTranslation();

  const fields: CrudFieldConfig[] = [
    { key: 'title', label: t('admin.dailyQuests.questTitle'), type: 'text', required: true, withEn: true, enLabel: t('admin.dailyQuests.questTitleEn') },
    { key: 'description', label: t('admin.dailyQuests.description'), type: 'textarea' },
    { key: 'targetType', label: t('admin.dailyQuests.targetType'), type: 'select', options: targetTypeOptions, required: true },
    { key: 'targetValue', label: t('admin.dailyQuests.targetValue'), type: 'number', required: true },
    { key: 'xpReward', label: t('admin.dailyQuests.xpReward'), type: 'number' },
    { key: 'coinReward', label: t('admin.dailyQuests.coinReward'), type: 'number' },
    { key: 'isActive', label: t('admin.dailyQuests.active'), type: 'checkbox' },
  ];

  const buildPayload = (p: Record<string, unknown>): DailyQuestTemplatePayload => ({
    title: String(p.title ?? ''),
    titleEn: (p.titleEn as string) || null,
    description: (p.description as string) || null,
    descriptionEn: (p.descriptionEn as string) || null,
    targetType: String(p.targetType ?? 'EarnXp'),
    targetValue: Number(p.targetValue),
    xpReward: Number(p.xpReward ?? 0),
    coinReward: Number(p.coinReward ?? 0),
    isActive: Boolean(p.isActive),
  });

  return (
    <SimpleCrudTable
      queryKey="admin-daily-quests"
      title={t('admin.dailyQuests.title')}
      addLabel={t('admin.dailyQuests.add')}
      columns={[
        { key: 'title', label: t('admin.dailyQuests.questTitle') },
        { key: 'targetType', label: t('admin.dailyQuests.targetType') },
        { key: 'xpReward', label: t('admin.dailyQuests.xpReward') },
      ]}
      fields={fields}
      api={{
        list: getDailyQuestTemplatesAdmin,
        listDeleted: getDeletedDailyQuestTemplates,
        create: (p) => createDailyQuestTemplate(buildPayload(p)),
        update: (id, p) => updateDailyQuestTemplate(id, buildPayload(p)),
        remove: deleteDailyQuestTemplate,
        restore: restoreDailyQuestTemplate,
      }}
    />
  );
}
