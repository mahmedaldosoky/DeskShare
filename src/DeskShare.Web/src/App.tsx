import { BrowserRouter, Navigate, Route, Routes } from 'react-router';
import { AuthProvider } from './auth/AuthProvider';
import { RequireAuth } from './auth/RequireAuth';
import { Layout } from './components/Layout';
import { LoginPage } from './features/auth/LoginPage';
import { AllBookingsPage } from './features/bookings/AllBookingsPage';
import { BookDeskPage } from './features/bookings/BookDeskPage';
import { MyBookingsPage } from './features/bookings/MyBookingsPage';
import { ManageDesksPage } from './features/desks/ManageDesksPage';

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route
            element={
              <RequireAuth>
                <Layout />
              </RequireAuth>
            }
          >
            <Route index element={<BookDeskPage />} />
            <Route path="my-bookings" element={<MyBookingsPage />} />
            <Route
              path="desks"
              element={
                <RequireAuth role="OfficeManager">
                  <ManageDesksPage />
                </RequireAuth>
              }
            />
            <Route
              path="bookings"
              element={
                <RequireAuth role="OfficeManager">
                  <AllBookingsPage />
                </RequireAuth>
              }
            />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
