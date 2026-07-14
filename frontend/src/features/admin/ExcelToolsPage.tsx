import { useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation } from '@tanstack/react-query';
import { Download, Upload } from 'lucide-react';
import { downloadExcelExport, importChallenges, importQuizQuestions, importTestCases, type ExcelExportKind } from '../../api/admin';
import type { ExcelImportResultDto } from '../../types/admin';
import type { LeaderboardPeriod } from '../../types/gamification';
import { GlassCard } from '../../components/ui/GlassCard';
import { TextField } from '../../components/ui/TextField';

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

export function ExcelToolsPage() {
  const { t } = useTranslation();
  const [period, setPeriod] = useState<LeaderboardPeriod>('AllTime');
  const [challengeIdForTestCases, setChallengeIdForTestCases] = useState('');
  const [quizIdForQuestions, setQuizIdForQuestions] = useState('');

  const challengesFileRef = useRef<HTMLInputElement>(null);
  const testCasesFileRef = useRef<HTMLInputElement>(null);
  const questionsFileRef = useRef<HTMLInputElement>(null);

  const importChallengesMutation = useMutation({ mutationFn: importChallenges });
  const importTestCasesMutation = useMutation({
    mutationFn: (file: File) => importTestCases(Number(challengeIdForTestCases), file),
  });
  const importQuestionsMutation = useMutation({
    mutationFn: (file: File) => importQuizQuestions(Number(quizIdForQuestions), file),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('admin.sections.excel')}</h1>

      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Import — Challenges</h2>
        <div className="mt-3 flex items-center gap-3">
          <input
            ref={challengesFileRef}
            type="file"
            accept=".xlsx"
            onChange={(e) => e.target.files?.[0] && importChallengesMutation.mutate(e.target.files[0])}
            className="text-sm text-slate-600 dark:text-slate-300"
          />
          <Upload size={16} className="text-slate-400" />
        </div>
        {importChallengesMutation.data && <ImportResultView result={importChallengesMutation.data} />}
      </GlassCard>

      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Import — Test Cases</h2>
        <div className="mt-3 flex items-center gap-3">
          <TextField id="challenge-id" label="Challenge ID" type="number" value={challengeIdForTestCases} onChange={(e) => setChallengeIdForTestCases(e.target.value)} />
          <input
            ref={testCasesFileRef}
            type="file"
            accept=".xlsx"
            disabled={!challengeIdForTestCases}
            onChange={(e) => e.target.files?.[0] && importTestCasesMutation.mutate(e.target.files[0])}
            className="text-sm text-slate-600 disabled:opacity-40 dark:text-slate-300"
          />
        </div>
        {importTestCasesMutation.data && <ImportResultView result={importTestCasesMutation.data} />}
      </GlassCard>

      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Import — Quiz Questions</h2>
        <div className="mt-3 flex items-center gap-3">
          <TextField id="quiz-id" label="Quiz ID" type="number" value={quizIdForQuestions} onChange={(e) => setQuizIdForQuestions(e.target.value)} />
          <input
            ref={questionsFileRef}
            type="file"
            accept=".xlsx"
            disabled={!quizIdForQuestions}
            onChange={(e) => e.target.files?.[0] && importQuestionsMutation.mutate(e.target.files[0])}
            className="text-sm text-slate-600 disabled:opacity-40 dark:text-slate-300"
          />
        </div>
        {importQuestionsMutation.data && <ImportResultView result={importQuestionsMutation.data} />}
      </GlassCard>

      <GlassCard className="p-6">
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Export</h2>
        <div className="mt-3 flex flex-wrap items-center gap-3">
          {exportKinds.map((kind) => (
            <button
              key={kind}
              type="button"
              onClick={() => downloadExcelExport(kind, kind === 'leaderboard' ? { period } : undefined)}
              className="flex items-center gap-1.5 rounded-lg bg-slate-100 px-3 py-1.5 text-xs font-medium text-slate-700 transition hover:bg-slate-200 dark:bg-white/5 dark:text-slate-200 dark:hover:bg-white/10"
            >
              <Download size={13} />
              {kind}
            </button>
          ))}
          <select
            value={period}
            onChange={(e) => setPeriod(e.target.value as LeaderboardPeriod)}
            className="rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-xs dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-100"
          >
            {periods.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </div>
      </GlassCard>
    </div>
  );
}
