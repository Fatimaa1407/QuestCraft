import { useTranslation } from 'react-i18next';
import { getDifficulties } from '../../api/challenges';
import { createDifficulty, deleteDifficulty, getDeletedDifficulties, restoreDifficulty, updateDifficulty } from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

export function DifficultiesAdminPage() {
  const { t } = useTranslation();

  const fields: CrudFieldConfig[] = [
    { key: 'name', label: t('admin.difficulties.name'), type: 'text', required: true },
    { key: 'color', label: t('admin.difficulties.color'), type: 'text' },
    { key: 'xpMultiplier', label: t('admin.difficulties.xpMultiplier'), type: 'number' },
  ];

  return (
    <SimpleCrudTable
      queryKey="admin-difficulties"
      title={t('admin.difficulties.title')}
      addLabel={t('admin.difficulties.add')}
      columns={[
        { key: 'name', label: t('admin.difficulties.name') },
        { key: 'xpMultiplier', label: t('admin.difficulties.xpMultiplier') },
      ]}
      fields={fields}
      api={{
        list: getDifficulties,
        listDeleted: getDeletedDifficulties,
        create: (p) => createDifficulty({ name: p.name, color: p.color || null, xpMultiplier: Number(p.xpMultiplier) }),
        update: (id, p) => updateDifficulty(id, { name: p.name, color: p.color || null, xpMultiplier: Number(p.xpMultiplier) }),
        remove: deleteDifficulty,
        restore: restoreDifficulty,
      }}
    />
  );
}
