import { useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { ArrowLeft, Pencil, Plus, Trash2 } from 'lucide-react';
import { getCategories } from '../../api/challenges';
import {
  addQuestion,
  createQuiz,
  deleteQuestion,
  getQuizAdminById,
  updateQuestion,
  updateQuiz,
  type QuestionOptionInputPayload,
  type QuestionPayload,
  type QuizPayload,
} from '../../api/admin';
import type { QuestionAdminDto } from '../../types/admin';
import { GlassCard } from '../../components/ui/GlassCard';
import { TextField } from '../../components/ui/TextField';
import { Textarea } from '../../components/ui/Textarea';
import { Select } from '../../components/ui/Select';
import { Modal } from '../../components/ui/Modal';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const emptyQuiz: QuizPayload = { title: '', categoryId: null, xpReward: 45, isPublished: true, requiredLevel: 1, titleEn: '', tags: '' };

// The exactly-one-correct-option rule (enforced server-side) means the form always
// needs a default. Randomizing which slot starts checked stops admins who forget to
// double-check it from silently piling every correct answer onto option 1.
function emptyOptions(): QuestionOptionInputPayload[] {
  const correctIndex = Math.floor(Math.random() * 4);
  return Array.from({ length: 4 }, (_, i) => ({ text: '', isCorrect: i === correctIndex, textEn: '' }));
}

function emptyQuestion(): QuestionPayload {
  return { text: '', explanation: '', options: emptyOptions(), textEn: '', explanationEn: '' };
}

export function QuizEditPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { id } = useParams<{ id: string }>();
  const isNew = id === 'new';
  const quizId = isNew ? null : Number(id);

  const [form, setForm] = useState<QuizPayload>(emptyQuiz);
  const [questionModalOpen, setQuestionModalOpen] = useState(false);
  const [editingQuestionId, setEditingQuestionId] = useState<number | null>(null);
  const [questionForm, setQuestionForm] = useState<QuestionPayload>(emptyQuestion);

  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const quizQuery = useQuery({
    queryKey: ['admin-quiz', quizId],
    queryFn: () => getQuizAdminById(quizId!),
    enabled: quizId !== null,
  });

  useEffect(() => {
    if (quizQuery.data) {
      const q = quizQuery.data;
      setForm({
        title: q.title, categoryId: q.categoryId, xpReward: q.xpReward,
        isPublished: q.isPublished, requiredLevel: q.requiredLevel, titleEn: q.titleEn ?? '',
        tags: q.tags ?? '',
      });
    }
  }, [quizQuery.data]);

  const saveMutation = useMutation({
    mutationFn: () => (isNew ? createQuiz(form) : updateQuiz(quizId!, form)),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['admin-quizzes'] });
      if (isNew && result) {
        navigate(`/admin/quizzes/${result.id}`, { replace: true });
      } else {
        queryClient.invalidateQueries({ queryKey: ['admin-quiz', quizId] });
      }
    },
  });

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    saveMutation.mutate();
  };

  const set = <K extends keyof QuizPayload>(key: K, value: QuizPayload[K]) => setForm((prev) => ({ ...prev, [key]: value }));

  const invalidateQuiz = () => queryClient.invalidateQueries({ queryKey: ['admin-quiz', quizId] });

  const addQuestionMutation = useMutation({
    mutationFn: () => addQuestion(quizId!, questionForm),
    onSuccess: () => {
      invalidateQuiz();
      setQuestionModalOpen(false);
    },
  });
  const updateQuestionMutation = useMutation({
    mutationFn: () => updateQuestion(editingQuestionId!, questionForm),
    onSuccess: () => {
      invalidateQuiz();
      setQuestionModalOpen(false);
    },
  });
  const deleteQuestionMutation = useMutation({ mutationFn: deleteQuestion, onSuccess: invalidateQuiz });

  const openAddQuestion = () => {
    setEditingQuestionId(null);
    setQuestionForm(emptyQuestion());
    setQuestionModalOpen(true);
  };

  const openEditQuestion = (question: QuestionAdminDto) => {
    setEditingQuestionId(question.id);
    setQuestionForm({
      text: question.text,
      textEn: question.textEn ?? '',
      explanation: question.explanation ?? '',
      explanationEn: question.explanationEn ?? '',
      options: question.options.map((o) => ({ text: o.text, isCorrect: o.isCorrect, textEn: o.textEn ?? '' })),
    });
    setQuestionModalOpen(true);
  };

  const handleQuestionSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (editingQuestionId === null) addQuestionMutation.mutate();
    else updateQuestionMutation.mutate();
  };

  const setOption = (index: number, patch: Partial<QuestionOptionInputPayload>) => {
    setQuestionForm((prev) => ({
      ...prev,
      options: prev.options.map((o, i) => (i === index ? { ...o, ...patch } : o)),
    }));
  };

  const setCorrectOption = (index: number) => {
    setQuestionForm((prev) => ({
      ...prev,
      options: prev.options.map((o, i) => ({ ...o, isCorrect: i === index })),
    }));
  };

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <Link to="/admin/quizzes" className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-blue-600 dark:text-slate-400 dark:hover:text-cyan-400">
          <ArrowLeft size={14} />
          {t('admin.sections.quizzes')}
        </Link>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <TextField id="title" label="Title" required value={form.title} onChange={(e) => set('title', e.target.value)} />
            <TextField id="titleEn" label="Title (EN)" value={form.titleEn ?? ''} onChange={(e) => set('titleEn', e.target.value)} />
          </div>
          <div className="grid grid-cols-4 gap-3">
            {categoriesQuery.data && (
              <Select
                id="categoryId"
                label="Category"
                options={[{ value: '', label: '—' }, ...categoriesQuery.data.map((c) => ({ value: String(c.id), label: c.name }))]}
                value={form.categoryId === null ? '' : String(form.categoryId)}
                onChange={(e) => set('categoryId', e.target.value === '' ? null : Number(e.target.value))}
              />
            )}
            <TextField id="xpReward" label="XP Reward" type="number" value={form.xpReward} onChange={(e) => set('xpReward', Number(e.target.value))} />
            <TextField id="requiredLevel" label="Required Level" type="number" min={1} value={form.requiredLevel} onChange={(e) => set('requiredLevel', Number(e.target.value))} />
            <label className="flex items-center gap-2 pt-6 text-sm font-medium text-slate-700 dark:text-slate-300">
              <input type="checkbox" checked={form.isPublished} onChange={(e) => set('isPublished', e.target.checked)} className="h-4 w-4 rounded border-slate-300 text-blue-600 dark:border-slate-700" />
              Published
            </label>
          </div>
          <TextField id="tags" label="Tags (comma-separated)" placeholder="linq, collections, beginner" value={form.tags ?? ''} onChange={(e) => set('tags', e.target.value)} />
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

      {quizId !== null && quizQuery.data && (
        <motion.div variants={fadeInUp}>
        <GlassCard className="p-6">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Questions ({quizQuery.data.questions.length})</h2>
            <button
              type="button"
              onClick={openAddQuestion}
              className="flex items-center gap-1.5 rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-3 py-1.5 text-xs font-medium text-white shadow-lg shadow-blue-600/25 transition hover:brightness-110"
            >
              <Plus size={14} />
              {t('admin.add')}
            </button>
          </div>

          <div className="mt-4 space-y-2">
            {quizQuery.data.questions.map((q) => (
              <div key={q.id} className="rounded-lg border border-slate-200/70 px-3 py-2.5 text-sm dark:border-white/[0.08]">
                <div className="flex items-start justify-between gap-3">
                  <p className="text-slate-800 dark:text-slate-100">{q.text}</p>
                  <div className="flex shrink-0 items-center gap-2">
                    <button type="button" onClick={() => openEditQuestion(q)} className="text-slate-400 transition hover:text-blue-600 dark:hover:text-cyan-400">
                      <Pencil size={14} />
                    </button>
                    <button type="button" onClick={() => deleteQuestionMutation.mutate(q.id)} className="text-slate-400 transition hover:text-red-600 dark:hover:text-red-400">
                      <Trash2 size={14} />
                    </button>
                  </div>
                </div>
                <ul className="mt-2 space-y-0.5 text-xs text-slate-500 dark:text-slate-400">
                  {q.options.map((o) => (
                    <li key={o.id} className={o.isCorrect ? 'font-medium text-emerald-600 dark:text-emerald-400' : ''}>
                      {o.isCorrect ? '✓ ' : '— '}
                      {o.text}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </GlassCard>
        </motion.div>
      )}

      <Modal isOpen={questionModalOpen} onClose={() => setQuestionModalOpen(false)} title={editingQuestionId === null ? t('admin.add') : t('admin.edit')}>
        <form onSubmit={handleQuestionSubmit} className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <Textarea id="q-text" label="Text" required rows={2} value={questionForm.text} onChange={(e) => setQuestionForm((p) => ({ ...p, text: e.target.value }))} />
            <Textarea id="q-textEn" label="Text (EN)" rows={2} value={questionForm.textEn ?? ''} onChange={(e) => setQuestionForm((p) => ({ ...p, textEn: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Textarea id="q-explanation" label="Explanation" rows={2} value={questionForm.explanation ?? ''} onChange={(e) => setQuestionForm((p) => ({ ...p, explanation: e.target.value }))} />
            <Textarea id="q-explanationEn" label="Explanation (EN)" rows={2} value={questionForm.explanationEn ?? ''} onChange={(e) => setQuestionForm((p) => ({ ...p, explanationEn: e.target.value }))} />
          </div>

          <div className="space-y-2">
            {questionForm.options.map((option, index) => (
              <div key={index} className="grid grid-cols-[auto_1fr_1fr] items-center gap-2">
                <input
                  type="radio"
                  name="correct-option"
                  checked={option.isCorrect}
                  onChange={() => setCorrectOption(index)}
                  className="h-4 w-4 text-blue-600"
                  title="Correct answer"
                />
                <TextField id={`opt-${index}`} label={`Option ${index + 1}`} required value={option.text} onChange={(e) => setOption(index, { text: e.target.value })} />
                <TextField id={`opt-${index}-en`} label={`Option ${index + 1} (EN)`} value={option.textEn ?? ''} onChange={(e) => setOption(index, { textEn: e.target.value })} />
              </div>
            ))}
          </div>

          <button
            type="submit"
            disabled={addQuestionMutation.isPending || updateQuestionMutation.isPending}
            className="w-full rounded-lg bg-gradient-to-r from-blue-600 to-cyan-500 px-4 py-2.5 text-sm font-medium text-white shadow-lg shadow-blue-600/25 transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {t('admin.save')}
          </button>
        </form>
      </Modal>
    </motion.div>
  );
}
