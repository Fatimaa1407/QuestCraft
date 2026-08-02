import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { BadgeCheck, Loader2, ShieldAlert } from 'lucide-react';
import { verifyCertificate } from '../../api/gamification';
import type { CertificateVerificationDto } from '../../types/gamification';

type LoadState = 'loading' | 'valid' | 'invalid';

export function VerifyCertificatePage() {
  const { t } = useTranslation();
  const { certificateId } = useParams<{ certificateId: string }>();
  const [state, setState] = useState<LoadState>('loading');
  const [result, setResult] = useState<CertificateVerificationDto | null>(null);

  useEffect(() => {
    if (!certificateId) {
      setState('invalid');
      return;
    }
    let cancelled = false;
    verifyCertificate(certificateId)
      .then((data) => {
        if (cancelled) return;
        if (data) {
          setResult(data);
          setState('valid');
        } else {
          setState('invalid');
        }
      })
      .catch(() => {
        if (!cancelled) setState('invalid');
      });
    return () => {
      cancelled = true;
    };
  }, [certificateId]);

  const completionPercent =
    result && result.maxLevel > 0 ? Math.round((result.level / result.maxLevel) * 100) : 100;

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#0D1117] px-4 py-10 text-slate-100">
      <div className="w-full max-w-md rounded-2xl border border-[#2A3352] bg-[#141A30] p-8 text-center shadow-2xl">
        <p className="text-xs font-bold tracking-[0.3em] text-violet-400">QUESTCRAFT</p>

        {state === 'loading' && (
          <div className="mt-8 flex flex-col items-center gap-3 text-slate-400">
            <Loader2 size={32} className="animate-spin" />
            <p className="text-sm">{t('certificateVerify.loading')}</p>
          </div>
        )}

        {state === 'valid' && result && (
          <div className="mt-6">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/15 text-emerald-400">
              <BadgeCheck size={34} />
            </div>
            <h1 className="mt-4 text-xl font-bold text-white">{t('certificateVerify.validTitle')}</h1>
            <p className="mt-1 text-sm text-slate-400">{t('certificateVerify.validSubtitle')}</p>

            <p className="mt-5 text-2xl font-extrabold text-white">{result.fullName}</p>

            <div className="mt-6 grid grid-cols-2 gap-3 text-left">
              <StatRow label={t('certificateVerify.level')} value={`${result.level} / ${result.maxLevel}`} />
              <StatRow label={t('certificateVerify.xp')} value={result.totalXp.toLocaleString()} />
              <StatRow label={t('certificateVerify.challenges')} value={String(result.totalChallengesSolved)} />
              <StatRow label={t('certificateVerify.completion')} value={`${completionPercent}%`} />
            </div>

            <div className="mt-5 border-t border-[#2A3352] pt-4 text-left text-xs text-slate-400">
              <p>
                {t('certificateVerify.issuedOn')}: {new Date(result.issuedAt).toLocaleDateString()}
              </p>
              <p className="mt-1 font-mono text-slate-300">
                {t('certificateVerify.certificateId')}: {result.certificateId}
              </p>
            </div>
          </div>
        )}

        {state === 'invalid' && (
          <div className="mt-6">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-red-500/15 text-red-400">
              <ShieldAlert size={34} />
            </div>
            <h1 className="mt-4 text-xl font-bold text-white">{t('certificateVerify.invalidTitle')}</h1>
            <p className="mt-1 text-sm text-slate-400">{t('certificateVerify.invalidSubtitle')}</p>
          </div>
        )}

        <Link
          to="/"
          className="mt-8 inline-block text-sm font-medium text-violet-400 hover:text-violet-300 hover:underline"
        >
          {t('certificateVerify.backHome')}
        </Link>
      </div>
    </div>
  );
}

function StatRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-[#2A3352] bg-[#0D1117] px-3 py-2">
      <p className="text-[10px] uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-0.5 text-sm font-bold text-white">{value}</p>
    </div>
  );
}
