import { useEffect, useState, type FormEvent, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Pencil, Plus, RotateCcw, Trash2 } from 'lucide-react';
import { GlassCard } from '../ui/GlassCard';
import { Modal } from '../ui/Modal';
import { TextField } from '../ui/TextField';
import { Textarea } from '../ui/Textarea';
import { Select } from '../ui/Select';

// Deliberately loosely-typed: this table drives 5 structurally different admin
// entities (Category/Difficulty/MarketplaceItem/Achievement/DailyQuestTemplate),
// so a shared generic here would fight TypeScript more than it would help.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type Row = Record<string, any>;

export interface CrudFieldConfig {
  key: string;
  label: string;
  type: 'text' | 'textarea' | 'number' | 'select' | 'checkbox';
  options?: { value: string; label: string }[];
  withEn?: boolean;
  enLabel?: string;
  required?: boolean;
}

export interface CrudColumnConfig {
  key: string;
  label: string;
  render?: (item: Row) => ReactNode;
}

export interface SimpleCrudApi {
  list: () => Promise<Row[]>;
  // Not every entity has a soft-delete/restore workflow (e.g. Marketplace items are
  // just deactivated) — omit these to hide the "show deleted" toggle entirely.
  listDeleted?: () => Promise<Row[]>;
  create: (payload: Row) => Promise<Row | null>;
  update: (id: number, payload: Row) => Promise<Row | null>;
  remove: (id: number) => Promise<void>;
  restore?: (id: number) => Promise<Row | null>;
}

interface SimpleCrudTableProps {
  queryKey: string;
  title: string;
  addLabel: string;
  columns: CrudColumnConfig[];
  fields: CrudFieldConfig[];
  api: SimpleCrudApi;
}

function buildDefaults(fields: CrudFieldConfig[]): Row {
  const defaults: Row = {};
  for (const field of fields) {
    defaults[field.key] = field.type === 'checkbox' ? true : field.type === 'number' ? 0 : '';
    if (field.withEn) defaults[`${field.key}En`] = '';
  }
  return defaults;
}

export function SimpleCrudTable({ queryKey, title, addLabel, columns, fields, api }: SimpleCrudTableProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showDeleted, setShowDeleted] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formValues, setFormValues] = useState<Row>(buildDefaults(fields));

  const supportsSoftDelete = Boolean(api.listDeleted && api.restore);

  const listQuery = useQuery({
    queryKey: [queryKey, showDeleted ? 'deleted' : 'active'],
    queryFn: () => (showDeleted && api.listDeleted ? api.listDeleted() : api.list()),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: [queryKey] });

  const createMutation = useMutation({ mutationFn: (payload: Row) => api.create(payload), onSuccess: invalidate });
  const updateMutation = useMutation({
    mutationFn: (payload: Row) => api.update(editingId!, payload),
    onSuccess: invalidate,
  });
  const deleteMutation = useMutation({ mutationFn: (id: number) => api.remove(id), onSuccess: invalidate });
  const restoreMutation = useMutation({
    mutationFn: (id: number) => api.restore!(id),
    onSuccess: invalidate,
  });

  useEffect(() => {
    if (!modalOpen) setFormValues(buildDefaults(fields));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [modalOpen]);

  const openCreate = () => {
    setEditingId(null);
    setFormValues(buildDefaults(fields));
    setModalOpen(true);
  };

  const openEdit = (item: Row) => {
    setEditingId(item.id);
    setFormValues(item);
    setModalOpen(true);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (editingId === null) {
      await createMutation.mutateAsync(formValues);
    } else {
      await updateMutation.mutateAsync(formValues);
    }
    setModalOpen(false);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm(t('admin.confirmDelete'))) return;
    await deleteMutation.mutateAsync(id);
  };

  const items = listQuery.data ?? [];
  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <GlassCard className="p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{title}</h2>
        <div className="flex items-center gap-2">
          {supportsSoftDelete && (
            <button
              type="button"
              onClick={() => setShowDeleted((v) => !v)}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition ${
                showDeleted
                  ? 'bg-indigo-600 text-white'
                  : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-white/5 dark:text-slate-300 dark:hover:bg-white/10'
              }`}
            >
              {showDeleted ? t('admin.showingDeleted') : t('admin.showDeleted')}
            </button>
          )}
          {!showDeleted && (
            <button
              type="button"
              onClick={openCreate}
              className="flex items-center gap-1.5 rounded-lg bg-gradient-to-r from-indigo-600 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white shadow-lg shadow-indigo-600/25 transition hover:brightness-110"
            >
              <Plus size={14} />
              {addLabel}
            </button>
          )}
        </div>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead>
            <tr className="border-b border-slate-200/70 text-xs uppercase tracking-wide text-slate-400 dark:border-white/[0.06] dark:text-slate-500">
              {columns.map((col) => (
                <th key={col.key} className="px-3 py-2 font-medium">
                  {col.label}
                </th>
              ))}
              <th className="px-3 py-2 font-medium">{t('admin.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 ? (
              <tr>
                <td colSpan={columns.length + 1} className="px-3 py-8 text-center text-slate-500 dark:text-slate-400">
                  {t('admin.empty')}
                </td>
              </tr>
            ) : (
              items.map((item) => (
                <tr key={item.id} className="border-b border-slate-100 last:border-0 dark:border-white/[0.04]">
                  {columns.map((col) => (
                    <td key={col.key} className="px-3 py-2.5 text-slate-700 dark:text-slate-200">
                      {col.render ? col.render(item) : String(item[col.key] ?? '')}
                    </td>
                  ))}
                  <td className="px-3 py-2.5">
                    {showDeleted ? (
                      <button
                        type="button"
                        onClick={() => restoreMutation.mutate(item.id)}
                        className="flex items-center gap-1 text-xs font-medium text-indigo-600 hover:underline dark:text-cyan-400"
                      >
                        <RotateCcw size={13} />
                        {t('admin.restore')}
                      </button>
                    ) : (
                      <div className="flex items-center gap-3">
                        <button
                          type="button"
                          onClick={() => openEdit(item)}
                          className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-indigo-600 dark:text-slate-300 dark:hover:text-cyan-400"
                        >
                          <Pencil size={13} />
                          {t('admin.edit')}
                        </button>
                        <button
                          type="button"
                          onClick={() => handleDelete(item.id)}
                          className="flex items-center gap-1 text-xs font-medium text-slate-600 hover:text-red-600 dark:text-slate-300 dark:hover:text-red-400"
                        >
                          <Trash2 size={13} />
                          {t('admin.delete')}
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)} title={editingId === null ? addLabel : t('admin.edit')}>
        <form onSubmit={handleSubmit} className="space-y-3">
          {fields.map((field) => {
            const value = formValues[field.key] ?? (field.type === 'checkbox' ? false : '');
            const enValue = field.withEn ? (formValues[`${field.key}En`] ?? '') : undefined;

            return (
              <div key={field.key} className={field.withEn ? 'grid grid-cols-2 gap-3' : ''}>
                {field.type === 'textarea' ? (
                  <Textarea
                    id={field.key}
                    label={field.label}
                    required={field.required}
                    rows={3}
                    value={value}
                    onChange={(e) => setFormValues((prev) => ({ ...prev, [field.key]: e.target.value }))}
                  />
                ) : field.type === 'select' ? (
                  <Select
                    id={field.key}
                    label={field.label}
                    required={field.required}
                    options={field.options ?? []}
                    value={value}
                    onChange={(e) => setFormValues((prev) => ({ ...prev, [field.key]: e.target.value }))}
                  />
                ) : field.type === 'checkbox' ? (
                  <label className="flex items-center gap-2 text-sm font-medium text-slate-700 dark:text-slate-300">
                    <input
                      type="checkbox"
                      checked={Boolean(value)}
                      onChange={(e) => setFormValues((prev) => ({ ...prev, [field.key]: e.target.checked }))}
                      className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-700"
                    />
                    {field.label}
                  </label>
                ) : (
                  <TextField
                    id={field.key}
                    label={field.label}
                    type={field.type === 'number' ? 'number' : 'text'}
                    required={field.required}
                    value={value}
                    onChange={(e) =>
                      setFormValues((prev) => ({
                        ...prev,
                        [field.key]: field.type === 'number' ? Number(e.target.value) : e.target.value,
                      }))
                    }
                  />
                )}
                {field.withEn && (
                  <TextField
                    id={`${field.key}En`}
                    label={field.enLabel ?? `${field.label} (EN)`}
                    value={enValue}
                    onChange={(e) => setFormValues((prev) => ({ ...prev, [`${field.key}En`]: e.target.value }))}
                  />
                )}
              </div>
            );
          })}

          <button
            type="submit"
            disabled={isSaving}
            className="w-full rounded-lg bg-gradient-to-r from-indigo-600 to-cyan-500 px-4 py-2.5 text-sm font-medium text-white shadow-lg shadow-indigo-600/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSaving ? t('admin.saving') : t('admin.save')}
          </button>
        </form>
      </Modal>
    </GlassCard>
  );
}
