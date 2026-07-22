import { useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { User as UserIcon } from 'lucide-react';
import { login } from '../../api/auth';
import { useAuthStore } from '../../app/authStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { AuthLayout } from '../../components/layout/AuthLayout';
import { TextField } from '../../components/ui/TextField';
import { PasswordField } from '../../components/ui/PasswordField';
import { Button } from '../../components/ui/Button';

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const setAuth = useAuthStore((s) => s.setAuth);
  const registered = Boolean((location.state as { registered?: boolean } | null)?.registered);
  const [emailOrUsername, setEmailOrUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const response = await login({ emailOrUsername, password });
      if (response.data) {
        setAuth(response.data.user, response.data.accessToken);
        navigate('/', { replace: true });
      }
    } catch (err) {
      setError(getApiErrorMessage(err, t('auth.login.errorFallback')));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout>
      <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">{t('auth.login.title')}</h1>
      <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('auth.login.subtitle')}</p>

      <form onSubmit={handleSubmit} className="mt-5 space-y-3">
        {registered && !error && (
          <p className="rounded-lg bg-green-50 px-3 py-2 text-sm text-green-600 dark:bg-green-950/50 dark:text-green-400">
            {t('auth.login.registeredSuccess')}
          </p>
        )}
        {error && (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/50 dark:text-red-400">{error}</p>
        )}

        <TextField
          id="emailOrUsername"
          type="text"
          label={t('auth.login.emailLabel')}
          placeholder={t('auth.login.emailPlaceholder')}
          icon={<UserIcon size={16} />}
          required
          value={emailOrUsername}
          onChange={(e) => setEmailOrUsername(e.target.value)}
        />

        <PasswordField
          id="password"
          label={t('auth.login.passwordLabel')}
          placeholder={t('auth.login.passwordPlaceholder')}
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <Button type="submit" loading={isSubmitting}>
          {isSubmitting ? t('auth.login.submitting') : t('auth.login.submit')}
        </Button>

        <p className="text-center text-sm text-slate-500 dark:text-slate-400">
          {t('auth.login.noAccount')}{' '}
          <Link to="/register" className="font-medium text-indigo-600 hover:underline dark:text-cyan-400">
            {t('auth.login.registerLink')}
          </Link>
        </p>
      </form>
    </AuthLayout>
  );
}
