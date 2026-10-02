interface AlertProps {
  kind: 'error' | 'success';
  message?: string;
}

export function Alert({ kind, message }: AlertProps) {
  if (!message) return null;

  return (
    <p className={`alert alert--${kind}`} role={kind === 'error' ? 'alert' : 'status'}>
      {message}
    </p>
  );
}
