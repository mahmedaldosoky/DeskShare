import { NavLink, Outlet } from 'react-router';
import { authApi } from '../api/authApi';
import { useAuth } from '../auth/AuthContext';
import styles from './Layout.module.scss';

interface NavigationItem {
  to: string;
  label: string;
  managersOnly?: boolean;
}

const navigationItems: NavigationItem[] = [
  { to: '/', label: 'Book a desk' },
  { to: '/my-bookings', label: 'My bookings' },
  { to: '/desks', label: 'Manage desks', managersOnly: true },
  { to: '/bookings', label: 'All bookings', managersOnly: true },
];

export function Layout() {
  const { user, isOfficeManager } = useAuth();
  const visibleItems = navigationItems.filter((item) => !item.managersOnly || isOfficeManager);

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <span className={styles.brand}>
            Desk<span className={styles.brandAccent}>Share</span>
          </span>

          <nav className={styles.navigation} aria-label="Main">
            {visibleItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end
                className={({ isActive }) => (isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink)}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className={styles.user}>
            <span className={styles.userName}>
              {user?.displayName}
              {isOfficeManager && <span className="tag tag--highlight">Office manager</span>}
            </span>
            <form method="post" action={authApi.signOutUrl}>
              <button type="submit" className="button button--small">
                Sign out
              </button>
            </form>
          </div>
        </div>
      </header>

      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  );
}
