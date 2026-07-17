import { useTranslation } from 'react-i18next';
import {
  createSeasonalEvent,
  deleteSeasonalEvent,
  getSeasonalEvents,
  updateSeasonalEvent,
  type SeasonalEventPayload,
} from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

export function SeasonalEventsAdminPage() {
  const { t } = useTranslation();

  const fields: CrudFieldConfig[] = [
    { key: 'name', label: t('admin.seasonalEvents.name'), type: 'text', required: true, withEn: true, enLabel: t('admin.seasonalEvents.nameEn') },
    { key: 'description', label: t('admin.seasonalEvents.description'), type: 'textarea', withEn: true, enLabel: t('admin.seasonalEvents.descriptionEn') },
    { key: 'emoji', label: t('admin.seasonalEvents.emoji'), type: 'text' },
    { key: 'startDate', label: t('admin.seasonalEvents.startDate'), type: 'text', required: true },
    { key: 'endDate', label: t('admin.seasonalEvents.endDate'), type: 'text', required: true },
    { key: 'isActive', label: t('admin.seasonalEvents.active'), type: 'checkbox' },
  ];

  const buildPayload = (p: Record<string, unknown>): SeasonalEventPayload => ({
    name: String(p.name ?? ''),
    nameEn: (p.nameEn as string) || null,
    description: (p.description as string) || null,
    descriptionEn: (p.descriptionEn as string) || null,
    startDate: String(p.startDate ?? ''),
    endDate: String(p.endDate ?? ''),
    isActive: Boolean(p.isActive),
    emoji: (p.emoji as string) || null,
  });

  return (
    <SimpleCrudTable
      queryKey="admin-seasonal-events"
      title={t('admin.seasonalEvents.title')}
      addLabel={t('admin.seasonalEvents.add')}
      columns={[
        { key: 'name', label: t('admin.seasonalEvents.name') },
        { key: 'startDate', label: t('admin.seasonalEvents.startDate') },
        { key: 'endDate', label: t('admin.seasonalEvents.endDate') },
      ]}
      fields={fields}
      api={{
        list: getSeasonalEvents,
        create: (p) => createSeasonalEvent(buildPayload(p)),
        update: (id, p) => updateSeasonalEvent(id, buildPayload(p)),
        remove: deleteSeasonalEvent,
      }}
    />
  );
}
