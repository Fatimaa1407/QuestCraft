import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { getItemTypes, getMarketplaceItems } from '../../api/marketplace';
import { createMarketplaceItem, deleteMarketplaceItem, updateMarketplaceItem, type MarketplaceItemPayload } from '../../api/admin';
import { SimpleCrudTable, type CrudFieldConfig } from '../../components/admin/SimpleCrudTable';

export function MarketplaceAdminPage() {
  const { t } = useTranslation();
  const itemTypesQuery = useQuery({ queryKey: ['marketplace', 'item-types'], queryFn: getItemTypes });

  if (!itemTypesQuery.data) return null;

  const fields: CrudFieldConfig[] = [
    { key: 'name', label: t('admin.marketplace.name'), type: 'text', required: true, withEn: true, enLabel: t('admin.marketplace.nameEn') },
    { key: 'description', label: t('admin.marketplace.description'), type: 'textarea' },
    {
      key: 'itemTypeId',
      label: t('admin.marketplace.itemType'),
      type: 'select',
      options: itemTypesQuery.data.map((type) => ({ value: String(type.id), label: type.name })),
      required: true,
    },
    { key: 'price', label: t('admin.marketplace.price'), type: 'number', required: true },
    { key: 'imageUrl', label: t('admin.marketplace.imageUrl'), type: 'text' },
    { key: 'isActive', label: t('admin.marketplace.active'), type: 'checkbox' },
  ];

  const buildPayload = (p: Record<string, unknown>): MarketplaceItemPayload => ({
    name: String(p.name ?? ''),
    description: (p.description as string) || null,
    itemTypeId: Number(p.itemTypeId),
    price: Number(p.price),
    imageUrl: (p.imageUrl as string) || null,
    isActive: Boolean(p.isActive),
    nameEn: (p.nameEn as string) || null,
    descriptionEn: (p.descriptionEn as string) || null,
  });

  return (
    <SimpleCrudTable
      queryKey="admin-marketplace"
      title={t('admin.marketplace.title')}
      addLabel={t('admin.marketplace.add')}
      columns={[
        { key: 'name', label: t('admin.marketplace.name') },
        { key: 'itemType', label: t('admin.marketplace.itemType') },
        { key: 'price', label: t('admin.marketplace.price') },
      ]}
      fields={fields}
      api={{
        list: () => getMarketplaceItems(),
        create: (p) => createMarketplaceItem(buildPayload(p)),
        update: (id, p) => updateMarketplaceItem(id, buildPayload(p)),
        remove: deleteMarketplaceItem,
      }}
    />
  );
}
