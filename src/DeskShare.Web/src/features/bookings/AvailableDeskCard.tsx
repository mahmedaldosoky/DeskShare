import { FeatureTags } from '../../components/FeatureTags';
import type { Desk } from '../../types/models';
import styles from './BookDeskPage.module.scss';

interface AvailableDeskCardProps {
  desk: Desk;
  isBooking: boolean;
  onBook: (desk: Desk) => void;
}

export function AvailableDeskCard({ desk, isBooking, onBook }: AvailableDeskCardProps) {
  return (
    <article className={styles.card}>
      <div>
        <h2 className={styles.code}>{desk.code}</h2>
        <p className={styles.floor}>Floor {desk.floor}</p>
      </div>
      <FeatureTags features={desk.features} />
      <button type="button" className="button button--primary" disabled={isBooking} onClick={() => onBook(desk)}>
        Book this desk
      </button>
    </article>
  );
}
