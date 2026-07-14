import { useTranslation } from 'react-i18next';
import { getCategories } from '../../api/challenges';
import { createCategory, deleteCategory, getDeletedCategories, restoreCategory, updateCategory } from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

export function CategoriesAdminPage() {
  const { t } = useTranslation();

  const fields: CrudFieldConfig[] = [
    { key: 'name', label: t('admin.categories.name'), type: 'text', required: true },
    { key: 'description', label: t('admin.categories.description'), type: 'textarea' },
    { key: 'iconUrl', label: t('admin.categories.iconUrl'), type: 'text' },
  ];

  return (
    <SimpleCrudTable
      queryKey="admin-categories"
      title={t('admin.categories.title')}
      addLabel={t('admin.categories.add')}
      columns={[
        { key: 'name', label: t('admin.categories.name') },
        { key: 'description', label: t('admin.categories.description') },
      ]}
      fields={fields}
      api={{
        list: getCategories,
        listDeleted: getDeletedCategories,
        create: (p) => createCategory({ name: p.name, description: p.description || null, iconUrl: p.iconUrl || null }),
        update: (id, p) => updateCategory(id, { name: p.name, description: p.description || null, iconUrl: p.iconUrl || null }),
        remove: deleteCategory,
        restore: restoreCategory,
      }}
    />
  );
}
