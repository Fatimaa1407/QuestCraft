import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AtSign, Mail, User as UserIcon } from 'lucide-react';
import { register } from '../../api/auth';
import { useAuthStore } from '../../app/authStore';
import { getApiErrorMessage } from '../../utils/apiError';
import { AuthLayout } from '../../components/layout/AuthLayout';
import { TextField } from '../../components/ui/TextField';
import { PasswordField } from '../../components/ui/PasswordField';
import { PasswordStrengthChecklist } from '../../components/ui/PasswordStrengthChecklist';
import { Button } from '../../components/ui/Button';

export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);

  const [username, setUsername] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError(t('auth.register.passwordMismatch'));
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await register({ username, firstName, lastName, email, password });
      if (response.data) {
        setAuth(response.data.user, response.data.accessToken);
        navigate('/', { replace: true });
      }
    } catch (err) {
      setError(getApiErrorMessage(err, t('auth.register.errorFallback')));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout>
      <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">{t('auth.register.title')}</h1>
      <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('auth.register.subtitle')}</p>

      <form onSubmit={handleSubmit} className="mt-5 space-y-3">
        {error && (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/50 dark:text-red-400">{error}</p>
        )}

        <div className="grid grid-cols-2 gap-3">
          <TextField
            id="firstName"
            label={t('auth.register.firstNameLabel')}
            placeholder={t('auth.register.firstNamePlaceholder')}
            icon={<UserIcon size={16} />}
            required
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
          />
          <TextField
            id="lastName"
            label={t('auth.register.lastNameLabel')}
            placeholder={t('auth.register.lastNamePlaceholder')}
            required
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
          />
        </div>

        <TextField
          id="username"
          label={t('auth.register.usernameLabel')}
          placeholder={t('auth.register.usernamePlaceholder')}
          icon={<AtSign size={16} />}
          required
          minLength={3}
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />

        <TextField
          id="email"
          type="email"
          label={t('auth.register.emailLabel')}
          placeholder={t('auth.register.emailPlaceholder')}
          icon={<Mail size={16} />}
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <div className="space-y-2">
          <PasswordField
            id="password"
            label={t('auth.register.passwordLabel')}
            required
            minLength={8}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <PasswordStrengthChecklist password={password} />
        </div>

        <PasswordField
          id="confirmPassword"
          label={t('auth.register.confirmPasswordLabel')}
          required
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
        />

        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? t('auth.register.submitting') : t('auth.register.submit')}
        </Button>

        <p className="text-center text-sm text-slate-500 dark:text-slate-400">
          {t('auth.register.haveAccount')}{' '}
          <Link to="/login" className="font-medium text-indigo-600 hover:underline dark:text-cyan-400">
            {t('auth.register.loginLink')}
          </Link>
        </p>
      </form>
    </AuthLayout>
  );
}
