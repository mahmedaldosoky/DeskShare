import { Navigate, useLocation } from 'react-router';
import { authApi } from '../../api/authApi';
import { useAuth } from '../../auth/AuthContext';
import { DevelopmentSignInForm } from './DevelopmentSignInForm';
import styles from './LoginPage.module.scss';

export function LoginPage() {
  const { user, signInMode, refreshSession } = useAuth();
  const location = useLocation();
  const returnUrl = (location.state as { from?: string } | null)?.from ?? '/';

  if (user) return <Navigate to={returnUrl} replace />;

  return (
    <div className={styles.page}>
      <section className={styles.card}>
        <h1 className={styles.brand}>
          Desk<span>Share</span>
        </h1>
        <p className={styles.tagline}>Book your desk at the office in a few clicks.</p>

        {signInMode === 'Oidc' ? (
          <a className="button button--primary" href={authApi.singleSignOnUrl(returnUrl)}>
            Sign in with your company account
          </a>
        ) : (
          <DevelopmentSignInForm onSignedIn={refreshSession} />
        )}
      </section>
    </div>
  );
}
