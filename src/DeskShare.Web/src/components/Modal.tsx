import { useEffect, useRef, type ReactNode } from 'react';
import styles from './Modal.module.scss';

interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
}

/** Built on the native <dialog> element, which gives focus trapping and Escape-to-close for free. */
export function Modal({ title, onClose, children }: ModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    dialogRef.current?.showModal();
  }, []);

  return (
    <dialog ref={dialogRef} className={styles.dialog} onClose={onClose}>
      <header className={styles.header}>
        <h2>{title}</h2>
        <button type="button" className={styles.close} onClick={onClose} aria-label="Close">
          ×
        </button>
      </header>
      {children}
    </dialog>
  );
}
