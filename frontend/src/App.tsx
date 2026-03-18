import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import { initializeApi, setAuthToken } from './services/api';
import { LoginPage } from './pages/LoginPage';
import { ProfilePage } from './pages/ProfilePage';
import { InventoryPage } from './pages/InventoryPage';
import { ProductsPage } from './pages/ProductsPage';
import { ReportsPage } from './pages/ReportsPage';
import { NotFound } from './pages/NotFound';
import { RoleProtectedRoute } from './components/RoleProtectedRoute';
import { NotificationCenter } from './components/NotificationCenter';
import { useInventoryNotifications } from './hooks/useInventoryNotifications';
import './styles/global.css';

function ProtectedRoute({
  children,
  isAuthenticated,
}: {
  children: React.ReactNode;
  isAuthenticated: boolean;
}) {
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function App() {
  const { isAuthenticated, token, isAdmin } = useAuth();
  const { notifications, dismissNotification } = useInventoryNotifications(isAuthenticated && isAdmin);

  useEffect(() => {
    if (token) {
      initializeApi(token);
      setAuthToken(token);
    } else {
      initializeApi();
    }
  }, [token]);

  return (
    <BrowserRouter>
      {isAuthenticated && isAdmin && (
        <NotificationCenter 
          notifications={notifications} 
          onDismiss={dismissNotification}
          maxVisible={3}
        />
      )}
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/profile"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              <ProfilePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/inventory"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              <InventoryPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/products"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              <ProductsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/reports"
          element={
            <RoleProtectedRoute requiredRole="Admin">
              <ReportsPage />
            </RoleProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to={isAuthenticated ? "/inventory" : "/login"} replace />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
