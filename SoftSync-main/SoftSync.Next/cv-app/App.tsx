import { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import type { StoredAuth } from './context/AuthContext';
import Login from './components/Login';
import Signup from './components/Signup';
import Dashboard from './components/Dashboard';
import ForgotPassword from './components/ForgotPassword';

type AuthView = 'login' | 'signup' | 'forgot-password';

function AppContent() {
  const { isAuthenticated, integrated } = useAuth();
  const [authView, setAuthView] = useState<AuthView>('login');

  if (isAuthenticated) {
    return <Dashboard />;
  }
  if (integrated) return <p role="status">Đang kết nối tài khoản SoftSync…</p>;

  if (authView === 'signup') {
    return <Signup onSwitchToLogin={() => setAuthView('login')} />;
  }

  if (authView === 'forgot-password') {
    return <ForgotPassword onBackToLogin={() => setAuthView('login')} />;
  }

  return (
    <Login
      onSwitchToSignup={() => setAuthView('signup')}
      onForgotPassword={() => setAuthView('forgot-password')}
    />
  );
}

function App({ initialAuth, integrated, hostOrigin }: { initialAuth?: StoredAuth; integrated?: boolean; hostOrigin?: string }) {
  return (
    <AuthProvider initialAuth={initialAuth} integrated={integrated} hostOrigin={hostOrigin}>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
