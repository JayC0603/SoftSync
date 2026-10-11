import { useCallback, useEffect, useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useAuth } from '../context/AuthContext';
import GoogleSignInButton from './GoogleSignInButton';
import './Login.css';
import './NeoLogin.css';

interface LoginProps {
  onSwitchToSignup: () => void;
  onForgotPassword: () => void;
}

export default function Login({ onSwitchToSignup, onForgotPassword }: LoginProps) {
  const { login } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [googleError, setGoogleError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [rememberUsername, setRememberUsername] = useState(false);
  const [theme, setTheme] = useState('light');
  const startGoogle = useCallback(() => { setError(''); setGoogleError(''); setIsSubmitting(true); }, []);
  const finishGoogle = useCallback(() => setIsSubmitting(false), []);

  useEffect(() => {
    try {
      const saved = localStorage.getItem('softsync-login-username');
      if (saved) { setUsername(saved); setRememberUsername(true); }
      setTheme(localStorage.getItem('softsync-login-theme') === 'dark' ? 'dark' : 'light');
    } catch { /* Login also works when browser storage is unavailable. */ }
  }, []);

  const toggleTheme = () => {
    const next = theme === 'light' ? 'dark' : 'light';
    setTheme(next);
    try { localStorage.setItem('softsync-login-theme', next); } catch {}
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    if (!username.trim() || !password) {
      setError('Vui lòng nhập tên đăng nhập và mật khẩu.');
      return;
    }
    setIsSubmitting(true);
    try {
      await login(username.trim(), password);
      try {
        if (rememberUsername) localStorage.setItem('softsync-login-username', username.trim());
        else localStorage.removeItem('softsync-login-username');
      } catch {}
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Không thể đăng nhập. Vui lòng thử lại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-container neo-login" data-login-theme={theme}>
      <div className="neo-decoration neo-decoration-one" aria-hidden="true" />
      <div className="neo-decoration neo-decoration-two" aria-hidden="true" />
      <div className="neo-decoration neo-decoration-three" aria-hidden="true" />
      <Link href="/" className="neo-home">← Về trang chủ</Link>
      <button type="button" className="neo-theme" onClick={toggleTheme} aria-label={theme === 'light' ? 'Chuyển sang giao diện tối' : 'Chuyển sang giao diện sáng'}>{theme === 'light' ? '☾' : '☀'}</button>
      <div className="auth-form-side">
        <div className="auth-card neo-content">
          <Link href="/" className="neo-logo" aria-label="SoftSync — Trang chủ">
            <Image src="/images/logo.png" width={112} height={112} alt="Logo nhóm SoftSync" priority />
          </Link>
          <div className="auth-header">
            <span className="neo-brand-name">SOFTSYNC</span>
            <h1>Chào mừng trở lại</h1>
            <p>Đăng nhập để tiếp tục hành trình của bạn</p>
          </div>
          <form onSubmit={handleSubmit} className="auth-form" aria-busy={isSubmitting}>
            {error && <div className="error-message" role="alert"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>{error}</div>}
            <div className="input-group">
              <label htmlFor="username">Tên đăng nhập</label>
              <div className="input-wrapper">
                <input type="text" id="username" value={username} onChange={event => setUsername(event.target.value)} placeholder="Tên đăng nhập của bạn" autoComplete="username" required disabled={isSubmitting} />
                <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="7" r="4" /><path d="M4 21v-2a8 8 0 0 1 16 0v2H4Z" /></svg>
              </div>
            </div>
            <div className="input-group">
              <label htmlFor="password">Mật khẩu</label>
              <div className="input-wrapper">
                <input type={showPassword ? 'text' : 'password'} id="password" value={password} onChange={event => setPassword(event.target.value)} placeholder="Mật khẩu của bạn" autoComplete="current-password" required disabled={isSubmitting} />
                <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" /></svg>
                <button type="button" className="neo-password-toggle" onClick={() => setShowPassword(value => !value)} aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'} aria-pressed={showPassword} aria-controls="password" disabled={isSubmitting}>
                  <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />{showPassword && <path d="m3 3 18 18" />}</svg>
                </button>
              </div>
            </div>
            <div className="form-options">
              <label className="remember-me"><input type="checkbox" checked={rememberUsername} onChange={event => setRememberUsername(event.target.checked)} disabled={isSubmitting} /><span>Nhớ tên đăng nhập</span></label>
              <button type="button" className="text-button forgot-password" onClick={onForgotPassword} disabled={isSubmitting}>Quên mật khẩu?</button>
            </div>
            <button type="submit" className="submit-button" disabled={isSubmitting}><span>{isSubmitting ? 'Đang đăng nhập…' : 'Đăng nhập'}</span><span className="neo-submit-arrow" aria-hidden="true">→</span></button>
            <div className="auth-divider"><span>hoặc tiếp tục với</span></div>
            <GoogleSignInButton disabled={isSubmitting} onError={setGoogleError} onStart={startGoogle} onFinish={finishGoogle} />
            {googleError && <p className="neo-google-status" role="status">Google hiện chưa khả dụng. Bạn có thể đăng nhập bằng tài khoản SoftSync.</p>}
          </form>
          <div className="auth-footer"><p>Bạn chưa có tài khoản? <button type="button" className="text-button" onClick={onSwitchToSignup} disabled={isSubmitting}>Đăng ký</button></p></div>
          <p className="neo-tagline">LEARN · BUILD · GROW TOGETHER</p>
        </div>
      </div>
    </main>
  );
}
