"use client";
import { useCallback, useEffect, useState } from 'react';
import CvApp from '../../cv-app/App';
import type { StoredAuth } from '../../cv-app/context/AuthContext';
import '../../cv-app/index.css';
import './workspace.css';

type SessionMessage = { accessToken: string; user: StoredAuth['user']; expiresAt: number };
export default function IntegratedCv({ hostOrigin }: { hostOrigin: string }) {
  const [session, setSession] = useState<SessionMessage | null>(null);
  const [error, setError] = useState('');
  const requestSession = useCallback(() => {
    setError('');
    window.parent.postMessage({ type: 'softsync-cv-ready' }, hostOrigin);
  }, [hostOrigin]);

  useEffect(() => {
    if (window.parent === window) { setError('Hãy mở CV thông minh từ menu SoftSync.'); return; }
    let timeout: ReturnType<typeof setTimeout>;
    let refresh: ReturnType<typeof setTimeout>;
    const listener = (event: MessageEvent) => {
      if (event.source !== window.parent || event.origin !== hostOrigin) return;
      if (event.data?.type === 'softsync-cv-theme') {
        const value = event.data;
        if (!['light', 'dark'].includes(value.theme)) return;
        document.documentElement.dataset.theme = value.theme;
        document.documentElement.dataset.reduceMotion = value.reduceMotion === true ? '1' : '0';
        const names = ['--ss-canvas', '--ss-surface', '--ss-surface-soft', '--ss-surface-accent', '--ss-text', '--ss-text-muted', '--ss-border', '--ss-primary', '--ss-primary-hover', '--ss-success', '--ss-warning', '--ss-danger', '--ss-info'];
        for (const name of names) {
          const color = value.tokens?.[name];
          if (typeof color === 'string' && CSS.supports('color', color)) document.documentElement.style.setProperty(name, color);
        }
        return;
      }
      if (event.data?.type === 'softsync-cv-host-ready') requestSession();
      if (event.data?.type === 'softsync-cv-error') {
        clearTimeout(timeout); clearTimeout(refresh); setSession(null);
        setError(typeof event.data.message === 'string' ? event.data.message : 'Không thể kết nối tài khoản SoftSync.');
      }
      if (event.data?.type !== 'softsync-cv-session') return;
      const value = event.data.session as SessionMessage;
      if (!value || typeof value.accessToken !== 'string' || !value.accessToken || !value.user
          || typeof value.user.name !== 'string' || typeof value.user.email !== 'string'
          || typeof value.expiresAt !== 'number' || value.expiresAt * 1000 <= Date.now()) return;
      clearTimeout(timeout); clearTimeout(refresh); setError(''); setSession(value);
      // Renew through the current SoftSync cookie, never a separate CV login.
      refresh = setTimeout(requestSession, Math.max(1000, value.expiresAt * 1000 - Date.now() - 60000));
    };
    window.addEventListener('message', listener);
    timeout = setTimeout(() => setError('Kết nối tài khoản chưa hoàn tất. Vui lòng thử lại.'), 30000);
    requestSession();
    const focused = () => requestSession();
    window.addEventListener('focus', focused);
    return () => { clearTimeout(timeout); clearTimeout(refresh); window.removeEventListener('message', listener); window.removeEventListener('focus', focused); };
  }, [hostOrigin, requestSession]);

  if (!session) return <main className="cv-connect"><img src="/images/logo.png" width="64" height="64" alt="SoftSync"/><h1>CV thông minh</h1><p role="status">{error || 'Đang kết nối tài khoản SoftSync…'}</p>{error && <button type="button" onClick={requestSession}>Thử lại</button>}<a href={`${hostOrigin}/dashboard`} target="_top">Quay lại SoftSync</a></main>;
  return <div className="softsync-cv-module"><CvApp integrated hostOrigin={hostOrigin} initialAuth={{ accessToken: session.accessToken, refreshToken: '', user: session.user }}/></div>;
}
