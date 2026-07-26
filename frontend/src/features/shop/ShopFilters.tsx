import { useTranslation } from 'react-i18next';
import { Search } from 'lucide-react';

export type ShopSort = 'default' | 'priceAsc' | 'priceDesc' | 'rarityAsc' | 'rarityDesc';
export type ShopOwnership = 'all' | 'owned' | 'notOwned' | 'equipped' | 'wishlisted';

interface ShopFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  sort: ShopSort;
  onSortChange: (value: ShopSort) => void;
  ownership: ShopOwnership;
  onOwnershipChange: (value: ShopOwnership) => void;
}

const selectClass =
  'rounded-full border border-slate-200/70 bg-white/80 px-3.5 py-2 text-xs font-medium text-slate-600 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-300';

export function ShopFilters({ search, onSearchChange, sort, onSortChange, ownership, onOwnershipChange }: ShopFiltersProps) {
  const { t } = useTranslation();

  return (
    <div className="flex flex-wrap items-center gap-2">
      <div className="relative">
        <Search size={15} className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder={t('shop.searchPlaceholder')}
          className="w-56 rounded-full border border-slate-200/70 bg-white/80 py-2 pl-9 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
        />
      </div>

      <select value={sort} onChange={(e) => onSortChange(e.target.value as ShopSort)} className={selectClass}>
        <option value="default">{t('shop.sortLabel')}</option>
        <option value="priceAsc">{t('shop.sortPriceAsc')}</option>
        <option value="priceDesc">{t('shop.sortPriceDesc')}</option>
        <option value="rarityAsc">{t('shop.sortRarityAsc')}</option>
        <option value="rarityDesc">{t('shop.sortRarityDesc')}</option>
      </select>

      <select value={ownership} onChange={(e) => onOwnershipChange(e.target.value as ShopOwnership)} className={selectClass}>
        <option value="all">{t('shop.filterAll')}</option>
        <option value="owned">{t('shop.filterOwned')}</option>
        <option value="notOwned">{t('shop.filterNotOwned')}</option>
        <option value="equipped">{t('shop.filterEquipped')}</option>
        <option value="wishlisted">{t('shop.filterWishlisted')}</option>
      </select>
    </div>
  );
}
