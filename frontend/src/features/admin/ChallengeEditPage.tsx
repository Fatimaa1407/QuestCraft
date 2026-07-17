import { useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { getCategories, getDifficulties } from '../../api/challenges';
import {
  addTestCase,
  createChallenge,
  deleteTestCase,
  getChallengeAdminById,
  updateChallenge,
  type ChallengePayload,
} from '../../api/admin';
import type { HiddenTestCaseDto, TestCaseDto } from '../../types/challenge';
import { GlassCard } from '../../components/ui/GlassCard';
import { TextField } from '../../components/ui/TextField';
import { Textarea } from '../../components/ui/Textarea';
import { Select } from '../../components/ui/Select';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const emptyForm: ChallengePayload = {
  title: '', description: '', categoryId: 0, difficultyId: 0, timeLimitMs: 2000, memoryLimitMb: 256,
  xpReward: 20, coinReward: 5, starterCode: '', constraints: '', inputFormat: '', outputFormat: '',
  sampleInput: '', sampleOutput: '', hint: '', isPublished: true, requiredLevel: 1,
  titleEn: '', descriptionEn: '', constraintsEn: '', inputFormatEn: '', outputFormatEn: '', hintEn: '', starterCodeEn: '',
  tags: '',
};

export function ChallengeEditPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { id } = useParams<{ id: string }>();
  const isNew = id === 'new';
  const challengeId = isNew ? null : Number(id);

  const [form, setForm] = useState<ChallengePayload>(emptyForm);

  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const difficultiesQuery = useQuery({ queryKey: ['difficulties'], queryFn: getDifficulties });
  const challengeQuery = useQuery({
    queryKey: ['admin-challenge', challengeId],
    queryFn: () => getChallengeAdminById(challengeId!),
    enabled: challengeId !== null,
  });

  useEffect(() => {
    if (challengeQuery.data) {
      const c = challengeQuery.data;
      setForm({
        title: c.title, description: c.description, categoryId: c.categoryId, difficultyId: c.difficultyId,
        timeLimitMs: c.timeLimitMs, memoryLimitMb: c.memoryLimitMb, xpReward: c.xpReward, coinReward: c.coinReward,
        starterCode: c.starterCode, constraints: c.constraints ?? '', inputFormat: c.inputFormat ?? '',
        outputFormat: c.outputFormat ?? '', sampleInput: c.sampleInput ?? '', sampleOutput: c.sampleOutput ?? '',
        hint: c.hint ?? '', isPublished: c.isPublished, requiredLevel: c.requiredLevel,
        titleEn: c.titleEn ?? '', descriptionEn: c.descriptionEn ?? '', constraintsEn: c.constraintsEn ?? '',
        inputFormatEn: c.inputFormatEn ?? '', outputFormatEn: c.outputFormatEn ?? '', hintEn: c.hintEn ?? '',
        starterCodeEn: c.starterCodeEn ?? '',
        tags: c.tags ?? '',
      });
    }
  }, [challengeQuery.data]);

  const saveMutation = useMutation({
    mutationFn: () => (isNew ? createChallenge(form) : updateChallenge(challengeId!, form)),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['admin-challenges'] });
      if (isNew && result) {
        navigate(`/admin/challenges/${result.id}`, { replace: true });
      } else {
        queryClient.invalidateQueries({ queryKey: ['admin-challenge', challengeId] });
      }
    },
  });

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    saveMutation.mutate();
  };

  const set = <K extends keyof ChallengePayload>(key: K, value: ChallengePayload[K]) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <Link to="/admin/challenges" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400">
          <ArrowLeft size={14} />
          {t('admin.sections.challenges')}
        </Link>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <TextField id="title" label="Title" required value={form.title} onChange={(e) => set('title', e.target.value)} />
            <TextField id="titleEn" label="Title (EN)" value={form.titleEn ?? ''} onChange={(e) => set('titleEn', e.target.value)} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Textarea id="description" label="Description" required rows={3} value={form.description} onChange={(e) => set('description', e.target.value)} />
            <Textarea id="descriptionEn" label="Description (EN)" rows={3} value={form.descriptionEn ?? ''} onChange={(e) => set('descriptionEn', e.target.value)} />
          </div>

          <div className="grid grid-cols-4 gap-3">
            {categoriesQuery.data && (
              <Select
                id="categoryId"
                label="Category"
                options={categoriesQuery.data.map((c) => ({ value: String(c.id), label: c.name }))}
                value={String(form.categoryId)}
                onChange={(e) => set('categoryId', Number(e.target.value))}
              />
            )}
            {difficultiesQuery.data && (
              <Select
                id="difficultyId"
                label="Difficulty"
                options={difficultiesQuery.data.map((d) => ({ value: String(d.id), label: d.name }))}
                value={String(form.difficultyId)}
                onChange={(e) => set('difficultyId', Number(e.target.value))}
              />
            )}
            <TextField id="requiredLevel" label="Required Level" type="number" min={1} value={form.requiredLevel} onChange={(e) => set('requiredLevel', Number(e.target.value))} />
            <label className="flex items-center gap-2 pt-6 text-sm font-medium text-slate-700 dark:text-slate-300">
              <input type="checkbox" checked={form.isPublished} onChange={(e) => set('isPublished', e.target.checked)} className="h-4 w-4 rounded border-slate-300 text-blue-600 dark:border-slate-700" />
              Published
            </label>
          </div>

          <div className="grid grid-cols-4 gap-3">
            <TextField id="xpReward" label="XP Reward" type="number" value={form.xpReward} onChange={(e) => set('xpReward', Number(e.target.value))} />
            <TextField id="coinReward" label="Coin Reward" type="number" value={form.coinReward} onChange={(e) => set('coinReward', Number(e.target.value))} />
            <TextField id="timeLimitMs" label="Time Limit (ms)" type="number" value={form.timeLimitMs} onChange={(e) => set('timeLimitMs', Number(e.target.value))} />
            <TextField id="memoryLimitMb" label="Memory Limit (MB)" type="number" value={form.memoryLimitMb} onChange={(e) => set('memoryLimitMb', Number(e.target.value))} />
          </div>

          <TextField id="tags" label="Tags (comma-separated)" placeholder="linq, collections, beginner" value={form.tags ?? ''} onChange={(e) => set('tags', e.target.value)} />

          <div className="grid grid-cols-2 gap-3">
            <TextField id="constraints" label="Constraints" value={form.constraints ?? ''} onChange={(e) => set('constraints', e.target.value)} />
            <TextField id="constraintsEn" label="Constraints (EN)" value={form.constraintsEn ?? ''} onChange={(e) => set('constraintsEn', e.target.value)} />
            <TextField id="inputFormat" label="Input Format" value={form.inputFormat ?? ''} onChange={(e) => set('inputFormat', e.target.value)} />
            <TextField id="inputFormatEn" label="Input Format (EN)" value={form.inputFormatEn ?? ''} onChange={(e) => set('inputFormatEn', e.target.value)} />
            <TextField id="outputFormat" label="Output Format" value={form.outputFormat ?? ''} onChange={(e) => set('outputFormat', e.target.value)} />
            <TextField id="outputFormatEn" label="Output Format (EN)" value={form.outputFormatEn ?? ''} onChange={(e) => set('outputFormatEn', e.target.value)} />
            <TextField id="sampleInput" label="Sample Input" value={form.sampleInput ?? ''} onChange={(e) => set('sampleInput', e.target.value)} />
            <TextField id="sampleOutput" label="Sample Output" value={form.sampleOutput ?? ''} onChange={(e) => set('sampleOutput', e.target.value)} />
            <TextField id="hint" label="Hint" value={form.hint ?? ''} onChange={(e) => set('hint', e.target.value)} />
            <TextField id="hintEn" label="Hint (EN)" value={form.hintEn ?? ''} onChange={(e) => set('hintEn', e.target.value)} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Textarea id="starterCode" label="Starter Code" rows={8} required className="font-mono text-xs" value={form.starterCode} onChange={(e) => set('starterCode', e.target.value)} />
            <Textarea id="starterCodeEn" label="Starter Code (EN)" rows={8} className="font-mono text-xs" value={form.starterCodeEn ?? ''} onChange={(e) => set('starterCodeEn', e.target.value)} />
          </div>

          <button
            type="submit"
            disabled={saveMutation.isPending}
            className="rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-6 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-600/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {saveMutation.isPending ? t('admin.saving') : t('admin.save')}
          </button>
        </form>
      </GlassCard>
      </motion.div>

      {challengeId !== null && challengeQuery.data && (
        <motion.div variants={fadeInUp}>
          <TestCaseManager
            challengeId={challengeId}
            testCases={challengeQuery.data.testCases}
            hiddenTestCases={challengeQuery.data.hiddenTestCases ?? []}
          />
        </motion.div>
      )}
    </motion.div>
  );
}

function TestCaseManager({
  challengeId,
  testCases,
  hiddenTestCases,
}: {
  challengeId: number;
  testCases: TestCaseDto[];
  hiddenTestCases: HiddenTestCaseDto[];
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [newCase, setNewCase] = useState({ input: '', expectedOutput: '', isHidden: false, weight: 1 });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin-challenge', challengeId] });

  const addMutation = useMutation({
    mutationFn: () =>
      addTestCase(challengeId, {
        input: newCase.input,
        expectedOutput: newCase.expectedOutput,
        orderIndex: testCases.length + hiddenTestCases.length + 1,
        isHidden: newCase.isHidden,
        weight: newCase.weight,
      }),
    onSuccess: () => {
      invalidate();
      setNewCase({ input: '', expectedOutput: '', isHidden: false, weight: 1 });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: ({ id, isHidden }: { id: number; isHidden: boolean }) => deleteTestCase(id, isHidden),
    onSuccess: invalidate,
  });

  const allCases = [
    ...testCases.map((tc) => ({ ...tc, isHidden: false, weight: 1 })),
    ...hiddenTestCases.map((tc) => ({ ...tc, isHidden: true })),
  ].sort((a, b) => a.orderIndex - b.orderIndex);

  return (
    <GlassCard className="p-6">
      <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Test Cases</h2>

      <div className="mt-4 space-y-2">
        {allCases.map((tc) => (
          <div key={`${tc.isHidden}-${tc.id}`} className="flex items-center gap-3 rounded-lg border border-slate-200/70 px-3 py-2 text-sm dark:border-white/[0.08]">
            <span className={`rounded px-1.5 py-0.5 text-[10px] font-semibold ${tc.isHidden ? 'bg-slate-200 text-slate-600 dark:bg-white/10 dark:text-slate-300' : 'bg-blue-500/10 text-blue-600 dark:text-cyan-400'}`}>
              {tc.isHidden ? 'HIDDEN' : 'VISIBLE'}
            </span>
            <code className="min-w-0 flex-1 truncate text-slate-700 dark:text-slate-200">
              in: {tc.input} → out: {tc.expectedOutput}
            </code>
            <button
              type="button"
              onClick={() => deleteMutation.mutate({ id: tc.id, isHidden: tc.isHidden })}
              className="text-slate-400 transition hover:text-red-600 dark:hover:text-red-400"
            >
              <Trash2 size={14} />
            </button>
          </div>
        ))}
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3 border-t border-slate-200/70 pt-4 dark:border-white/[0.06]">
        <TextField id="tc-input" label="Input" value={newCase.input} onChange={(e) => setNewCase((p) => ({ ...p, input: e.target.value }))} />
        <TextField id="tc-output" label="Expected Output" value={newCase.expectedOutput} onChange={(e) => setNewCase((p) => ({ ...p, expectedOutput: e.target.value }))} />
        <label className="flex items-center gap-2 text-sm font-medium text-slate-700 dark:text-slate-300">
          <input type="checkbox" checked={newCase.isHidden} onChange={(e) => setNewCase((p) => ({ ...p, isHidden: e.target.checked }))} className="h-4 w-4 rounded border-slate-300 dark:border-slate-700" />
          Hidden
        </label>
        <TextField id="tc-weight" label="Weight" type="number" value={newCase.weight} onChange={(e) => setNewCase((p) => ({ ...p, weight: Number(e.target.value) }))} />
        <button
          type="button"
          onClick={() => addMutation.mutate()}
          disabled={!newCase.input || !newCase.expectedOutput || addMutation.isPending}
          className="col-span-2 flex items-center justify-center gap-1.5 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-blue-600/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Plus size={14} />
          {t('admin.add')}
        </button>
      </div>
    </GlassCard>
  );
}
