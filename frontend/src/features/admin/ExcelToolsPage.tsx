import { useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Download, Upload } from 'lucide-react';
import { downloadExcelExport, importChallenges, importQuizQuestions, importTestCases, type ExcelExportKind } from '../../api/admin';
import type { ExcelImportResultDto } from '../../types/admin';
import type { LeaderboardPeriod } from '../../types/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { TextField } from '../../components/ui/TextField';
import { fadeInUp, staggerContainer } from '../../utils/motion';

const exportKinds: ExcelExportKind[] = ['users', 'challenges', 'quiz-results', 'leaderboard', 'marketplace'];
const periods: LeaderboardPeriod[] = ['Daily', 'Weekly', 'Monthly', 'AllTime'];

function ImportResultView({ result }: { result: ExcelImportResultDto }) {
  return (
    <div className="mt-3 rounded-lg border border-slate-200/70 bg-slate-50 p-3 text-sm dark:border-white/[0.08] dark:bg-white/5">
      <p className="text-slate-700 dark:text-slate-200">
        {result.successRows} / {result.totalRows} uğurlu, {result.failedRows} uğursuz
      </p>
      {result.errors.length > 0 && (
        <ul className="mt-2 list-disc pl-4 text-xs text-red-500">
          {result.errors.slice(0, 10).map((err, i) => (
            <li key={i}>{err}</li>
          ))}
        </ul>
      )}
    </div>
  );
}

interface FilePickerButtonProps {
  id: string;
  disabled?: boolean;
  onFileSelected: (file: File) => void;
}

function FilePickerButton({ id, disabled, onFileSelected }: FilePickerButtonProps) {
  const { t } = useTranslation();
  const inputRef = useRef<HTMLInputElement>(null);
  const [fileName, setFileName] = useState<string | null>(null);

  return (
    <div className="flex flex-wrap items-center gap-3">
      <input
        ref={inputRef}
        id={id}
        type="file"
        accept=".xlsx"
        disabled={disabled}
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) {
            setFileName(file.name);
            onFileSelected(file);
          }
        }}
        className="hidden"
      />
      <button
        type="button"
        disabled={disabled}
        onClick={() => inputRef.current?.click()}
        className="flex items-center gap-1.5 rounded-lg border border-dashed border-slate-300 bg-slate-50 px-3 py-2 text-xs font-medium text-slate-600 transition hover:border-blue-400 hover:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/15 dark:bg-white/5 dark:text-slate-300 dark:hover:border-cyan-400/50 dark:hover:bg-white/10"
      >
        <Upload size={14} />
        {t('admin.excel.chooseFile')}
      </button>
      <span className="truncate text-xs text-slate-500 dark:text-slate-400">
        {fileName ?? t('admin.excel.noFileChosen')}
      </span>
    </div>
  );
}

export function ExcelToolsPage() {
  const { t } = useTranslation();
  const [period, setPeriod] = useState<LeaderboardPeriod>('AllTime');
  const [challengeIdForTestCases, setChallengeIdForTestCases] = useState('');
  const [quizIdForQuestions, setQuizIdForQuestions] = useState('');

  const importChallengesMutation = useMutation({ mutationFn: importChallenges });
  const importTestCasesMutation = useMutation({
    mutationFn: (file: File) => importTestCases(Number(challengeIdForTestCases), file),
  });
  const importQuestionsMutation = useMutation({
    mutationFn: (file: File) => importQuizQuestions(Number(quizIdForQuestions), file),
  });

  return (
    <motion.div variants={staggerContainer} initial="hidden" animate="show" className="space-y-6">
      <motion.div variants={fadeInUp}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-3xl">{t('admin.sections.excel')}</h1>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.excel.importChallenges')}</h2>
        <div className="mt-3">
          <FilePickerButton
            id="challenges-file"
            onFileSelected={(file) => importChallengesMutation.mutate(file)}
          />
        </div>
        {importChallengesMutation.data && <ImportResultView result={importChallengesMutation.data} />}
      </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.excel.importTestCases')}</h2>
        <div className="mt-3 flex flex-wrap items-center gap-3">
          <TextField id="challenge-id" label={t('admin.excel.challengeId')} type="number" value={challengeIdForTestCases} onChange={(e) => setChallengeIdForTestCases(e.target.value)} />
          <FilePickerButton
            id="test-cases-file"
            disabled={!challengeIdForTestCases}
            onFileSelected={(file) => importTestCasesMutation.mutate(file)}
          />
        </div>
        {importTestCasesMutation.data && <ImportResultView result={importTestCasesMutation.data} />}
      </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.excel.importQuestions')}</h2>
        <div className="mt-3 flex flex-wrap items-center gap-3">
          <TextField id="quiz-id" label={t('admin.excel.quizId')} type="number" value={quizIdForQuestions} onChange={(e) => setQuizIdForQuestions(e.target.value)} />
          <FilePickerButton
            id="questions-file"
            disabled={!quizIdForQuestions}
            onFileSelected={(file) => importQuestionsMutation.mutate(file)}
          />
        </div>
        {importQuestionsMutation.data && <ImportResultView result={importQuestionsMutation.data} />}
      </GlassCard>
      </motion.div>

      <motion.div variants={fadeInUp}>
      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t('admin.excel.exportTitle')}</h2>
        <div className="mt-3 flex flex-wrap items-center gap-3">
          {exportKinds.map((kind) => (
            <button
              key={kind}
              type="button"
              onClick={() => downloadExcelExport(kind, kind === 'leaderboard' ? { period } : undefined)}
              className="flex items-center gap-1.5 rounded-lg bg-slate-100 px-3 py-1.5 text-xs font-medium text-slate-700 transition hover:bg-slate-200 dark:bg-white/5 dark:text-slate-200 dark:hover:bg-white/10"
            >
              <Download size={13} />
              {t(`admin.excel.exportKinds.${kind}`)}
            </button>
          ))}
          <select
            value={period}
            onChange={(e) => setPeriod(e.target.value as LeaderboardPeriod)}
            className="rounded-full border border-slate-200/70 bg-white/80 px-4 py-2 text-xs text-slate-900 outline-none transition focus:border-blue-400 dark:border-white/[0.08] dark:bg-white/5 dark:text-slate-100"
          >
            {periods.map((p) => (
              <option key={p} value={p}>
                {t(`leaderboard.period.${p}`)}
              </option>
            ))}
          </select>
        </div>
      </GlassCard>
      </motion.div>
    </motion.div>
  );
}
