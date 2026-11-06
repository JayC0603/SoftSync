(() => {
  'use strict';
  const API = '/api/game-history';
  const MAX = 50;
  const configs = new Map();

  function read(config) {
    try {
      const rows = JSON.parse(localStorage.getItem(config.storageKey) || '[]');
      const clearAt = Math.max(config.clearedAt || 0, Number(localStorage.getItem(`${config.storageKey}.clearedAt`)) || 0);
      return Array.isArray(rows) ? rows.filter(x => x && x.id && x.data && Date.parse(x.playedAt) > clearAt).slice(0, MAX).map(row => {
        if (config.gameKey !== 'inbox-rush' || row.data.title) return row;
        const data = { ...row.data };
        data.title = `Inbox Rush · Bộ ${data.set || '—'}`;
        data.score = data.total;
        data.unit = '/100';
        data.summary = `Seed ${data.seed ?? '—'} · Đã xếp ${data.placed || 0} việc, trễ hoặc bỏ lỡ ${data.failed || 0}.`;
        data.metrics = [
          { label: 'Ưu tiên hóa', value: `${data.PS ?? '—'}%` },
          { label: 'Ước lượng', value: data.CS == null ? '—' : `${data.CS}%` },
          { label: 'Thích ứng', value: data.AS == null ? '—' : `${data.AS}%` }
        ];
        if (!Array.isArray(data.details)) data.details = Array.isArray(data.tasks) ? data.tasks.map(x => `${x.name} — ${x.outcomeLabel || x.outcome || '—'}`) : [];
        return { ...row, data };
      }) : [];
    } catch { return []; }
  }
  function write(config, rows) {
    try { localStorage.setItem(config.storageKey, JSON.stringify(rows.slice(0, MAX))); return true; }
    catch { return false; }
  }
  function saveClearAt(config, value) {
    const time = Date.parse(value || '') || 0;
    config.clearedAt = Math.max(config.clearedAt || 0, time);
    try { localStorage.setItem(`${config.storageKey}.clearedAt`, String(config.clearedAt)); } catch { }
  }
  function makeId() {
    return crypto.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}-${Math.random().toString(36).slice(2)}`;
  }
  function node(tag, className, text) {
    const el = document.createElement(tag);
    if (className) el.className = className;
    if (text !== undefined) el.textContent = text;
    return el;
  }
  function mount(config) {
    const root = document.getElementById('game-history');
    if (!root) return;
    configs.set(config.gameKey, config);
    root.replaceChildren();
    const bar = node('div', 'ss-gh-bar');
    const status = node('span', 'ss-gh-status', 'Đang kiểm tra lịch sử…');
    const login = node('a', 'ss-gh-login', 'Đăng nhập để đồng bộ');
    login.href = `/Account/Login?ReturnUrl=${encodeURIComponent(location.pathname + location.search)}`;
    login.hidden = true;
    const open = node('button', 'ss-gh-open', 'Lịch sử lượt chơi');
    open.type = 'button';
    const panel = node('section', 'ss-gh-panel');
    panel.hidden = true;
    panel.setAttribute('aria-label', `Lịch sử ${config.title}`);
    const heading = node('div', 'ss-gh-heading');
    heading.append(node('h2', '', `Lịch sử ${config.title}`));
    const close = node('button', 'ss-gh-close', 'Đóng');
    close.type = 'button';
    const clear = node('button', 'ss-gh-clear', 'Xóa lịch sử');
    clear.type = 'button';
    close.onclick = () => { panel.hidden = true; open.focus(); };
    clear.onclick = () => clearHistory(config);
    heading.append(node('div', 'ss-gh-heading-actions', ''), close);
    heading.querySelector('.ss-gh-heading-actions').append(clear);
    const list = node('div', 'ss-gh-list');
    panel.append(heading, list);
    open.onclick = () => { panel.hidden = !panel.hidden; if (!panel.hidden) render(config); };
    bar.append(status, login, open);
    root.append(bar, panel);
    config.ui = { status, login, open, panel, list };
    try { config.clearedAt = Number(localStorage.getItem(`${config.storageKey}.clearedAt`)) || 0; }
    catch { config.clearedAt = 0; }
    render(config);
    sync(config);
  }
  function render(config) {
    const ui = config.ui;
    if (!ui) return;
    const rows = read(config);
    ui.list.replaceChildren();
    if (!rows.length) {
      ui.list.append(node('p', 'ss-gh-empty', 'Chưa có lượt chơi nào. Hoàn thành một ván để lưu kết quả vào lịch sử.'));
      return;
    }
    const dateFormat = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' });
    rows.forEach((entry, index) => {
      const card = node('article', 'ss-gh-entry');
      const data = entry.data;
      const top = node('div', 'ss-gh-entry-top');
      const title = data.title || `Lượt ${rows.length - index}`;
      const score = data.score === undefined ? '' : ` · ${data.score}${data.unit ? ` ${data.unit}` : ''}`;
      top.append(node('h3', '', `${title}${score}`));
      const date = new Date(entry.playedAt || data.playedAt || 0);
      top.append(node('time', 'ss-gh-date', Number.isNaN(date.valueOf()) || !date.valueOf() ? 'Ngày chơi không xác định' : dateFormat.format(date)));
      card.append(top);
      if (data.summary) card.append(node('p', 'ss-gh-summary', data.summary));
      if (Array.isArray(data.metrics) && data.metrics.length) {
        const metrics = node('div', 'ss-gh-metrics');
        data.metrics.forEach(item => metrics.append(node('span', '', `${item.label}: ${item.value}`)));
        card.append(metrics);
      }
      if (Array.isArray(data.details) && data.details.length) {
        const details = node('details', 'ss-gh-details');
        details.append(node('summary', '', 'Xem chi tiết'));
        const listDetails = node('ul', '');
        data.details.forEach(item => listDetails.append(node('li', '', item)));
        details.append(listDetails);
        card.append(details);
      }
      const exportButton = node('button', 'ss-gh-export', 'Tải kết quả (.json)');
      exportButton.type = 'button';
      exportButton.onclick = () => {
        const anchor = document.createElement('a');
        anchor.href = URL.createObjectURL(new Blob([JSON.stringify({ playedAt: entry.playedAt, ...data }, null, 2)], { type: 'application/json' }));
        anchor.download = `${config.gameKey}-history.json`;
        anchor.click();
        setTimeout(() => URL.revokeObjectURL(anchor.href), 1000);
      };
      card.append(exportButton);
      ui.list.append(card);
    });
  }
  async function sync(config) {
    if (config.pending) { config.needsSync = true; return config.pending; }
    config.pending = (async () => {
      const ui = config.ui;
      if (ui) { ui.status.textContent = 'Đang đồng bộ lịch sử…'; ui.login.hidden = true; }
      try {
        const response = await fetch(`${API}/${encodeURIComponent(config.gameKey)}`, {
          credentials: 'same-origin', headers: { Accept: 'application/json' }, cache: 'no-store'
        });
        if (response.status === 401 || response.redirected || !(response.headers.get('content-type') || '').includes('application/json')) {
          if (ui) { ui.status.textContent = 'Lịch sử đang lưu trên thiết bị này.'; ui.login.hidden = false; }
          render(config);
          return false;
        }
        if (!response.ok) throw new Error(`History request failed (${response.status})`);
        const remoteState = await response.json();
        saveClearAt(config, remoteState.clearedAtUtc);
        const localRows = read(config);
        const csrfResponse = await fetch(`${API}/csrf`, { credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json' } });
        if (!csrfResponse.ok || !(csrfResponse.headers.get('content-type') || '').includes('application/json')) throw new Error('Sign-in or CSRF token unavailable');
        const csrf = await csrfResponse.json();
        const entries = localRows.map(row => ({ clientEntryId: row.id, playedAtUtc: row.playedAt, data: row.data }));
        const pushed = await fetch(`${API}/${encodeURIComponent(config.gameKey)}/sync`, {
          method: 'POST', credentials: 'same-origin', cache: 'no-store',
          headers: { 'Content-Type': 'application/json', [csrf.header || 'RequestVerificationToken']: csrf.token },
          body: JSON.stringify({ entries })
        });
        if (!pushed.ok) throw new Error(`History sync failed (${pushed.status})`);
        const result = await pushed.json();
        saveClearAt(config, result.clearedAtUtc);
        const serverRows = Array.isArray(result.entries) ? result.entries.map(row => ({
          id: row.clientEntryId, playedAt: row.playedAtUtc, data: row.data
        })) : [];
        const merged = new Map();
        [...read(config), ...serverRows].filter(row => Date.parse(row.playedAt) > (config.clearedAt || 0))
          .forEach(row => { if (row && row.id && row.data) merged.set(row.id, row); });
        const rows = [...merged.values()].sort((a, b) => Date.parse(b.playedAt) - Date.parse(a.playedAt)).slice(0, MAX);
        write(config, rows);
        if (ui) ui.status.textContent = `Đã đồng bộ ${serverRows.length} lượt với tài khoản SoftSync.`;
        render(config);
        return true;
      } catch {
        if (ui) { ui.status.textContent = 'Chưa thể đồng bộ; lịch sử vẫn được giữ trên thiết bị và sẽ thử lại khi kết nối được khôi phục.'; ui.login.hidden = true; }
        render(config);
        return false;
      } finally {
        config.pending = null;
        if (config.needsSync) { config.needsSync = false; sync(config); }
      }
    })();
    return config.pending;
  }
  async function clearHistory(config) {
    if (!confirm('Xóa lịch sử game này trên tài khoản SoftSync và các thiết bị đã đồng bộ?')) return;
    if (config.ui) config.ui.status.textContent = 'Đang xóa lịch sử đã đồng bộ…';
    try {
      const csrfResponse = await fetch(`${API}/csrf`, { credentials: 'same-origin', cache: 'no-store', headers: { Accept: 'application/json' } });
      if (!csrfResponse.ok || !(csrfResponse.headers.get('content-type') || '').includes('application/json')) throw new Error('Sign-in required');
      const csrf = await csrfResponse.json();
      const response = await fetch(`${API}/${encodeURIComponent(config.gameKey)}`, {
        method: 'DELETE', credentials: 'same-origin', cache: 'no-store',
        headers: { [csrf.header || 'RequestVerificationToken']: csrf.token }
      });
      if (!response.ok) throw new Error('Unable to clear synchronized history');
      const result = await response.json();
      saveClearAt(config, result.clearedAtUtc);
      write(config, []);
      if (config.ui) config.ui.status.textContent = 'Đã xóa lịch sử khỏi tài khoản SoftSync và thiết bị này.';
      render(config);
    } catch {
      if (config.ui) { config.ui.status.textContent = 'Đăng nhập và kết nối mạng để xóa lịch sử đã đồng bộ.'; config.ui.login.hidden = false; }
    }
  }
  function append(gameKey, data, storageKey) {
    const config = configs.get(gameKey) || { gameKey, storageKey: storageKey || `softsync.${gameKey}.history.v1`, title: gameKey };
    if (storageKey) config.storageKey = storageKey;
    const rows = read(config);
    const entry = { id: makeId(), playedAt: new Date().toISOString(), data };
    rows.unshift(entry);
    write(config, rows);
    render(config);
    if (config.ui) config.ui.status.textContent = 'Đã lưu trên thiết bị; đang đồng bộ…';
    sync(config);
    return entry;
  }
  window.addEventListener('online', () => configs.forEach(config => sync(config)));
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') configs.forEach(config => sync(config));
  });
  window.SoftSyncGameHistory = { mount, append, sync };
})();
