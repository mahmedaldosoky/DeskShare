import { useState, type FormEvent } from 'react';
import { authApi } from '../../api/authApi';
import { Alert } from '../../components/Alert';
import { useAction } from '../../hooks/useAction';
import type { DevelopmentSignInRequest } from '../../types/models';
import styles from './LoginPage.module.scss';

const samplePeople: DevelopmentSignInRequest[] = [
  { displayName: 'Sara Employee', email: 'sara@contoso.com', isOfficeManager: false },
  { displayName: 'Omar Manager', email: 'omar@contoso.com', isOfficeManager: true },
];

export function DevelopmentSignInForm({ onSignedIn }: { onSignedIn: () => Promise<void> }) {
  const [person, setPerson] = useState<DevelopmentSignInRequest>(samplePeople[0]);
  const signIn = useAction();

  async function submit(event: FormEvent) {
    event.preventDefault();
    const succeeded = await signIn.run(() => authApi.developmentSignIn(person));
    if (succeeded) await onSignedIn();
  }

  return (
    <form className={styles.form} onSubmit={submit}>
      <p className={styles.notice}>
        Development sign-in: no identity provider is configured, so pick who you want to be.
      </p>

      <div className={styles.presets}>
        {samplePeople.map((sample) => (
          <button key={sample.email} type="button" className="button button--small" onClick={() => setPerson(sample)}>
            {sample.displayName}
          </button>
        ))}
      </div>

      <label className="field">
        <span className="field__label">Name</span>
        <input
          className="field__input"
          value={person.displayName}
          required
          onChange={(event) => setPerson({ ...person, displayName: event.target.value })}
        />
      </label>

      <label className="field">
        <span className="field__label">Email</span>
        <input
          className="field__input"
          type="email"
          value={person.email}
          required
          onChange={(event) => setPerson({ ...person, email: event.target.value })}
        />
      </label>

      <label className="checkbox">
        <input
          type="checkbox"
          checked={person.isOfficeManager}
          onChange={(event) => setPerson({ ...person, isOfficeManager: event.target.checked })}
        />
        Member of the office managers group
      </label>

      <Alert kind="error" message={signIn.error} />

      <button type="submit" className="button button--primary" disabled={signIn.isPending}>
        Sign in
      </button>
    </form>
  );
}
