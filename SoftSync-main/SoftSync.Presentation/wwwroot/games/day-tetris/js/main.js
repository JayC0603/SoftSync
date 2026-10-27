/* Day Tetris - giao diện, điều khiển, vòng lặp, menu và báo cáo */
(function () {
  'use strict';
  var DT = window.DT;
  var $ = function (s, root) { return (root || document).querySelector(s); };
  var $$ = function (s, root) { return Array.prototype.slice.call((root || document).querySelectorAll(s)); };

  var SAVE_KEY = 'daytetris.v1';
  var app = {
    game: null, mode: null, screen: 'menu', returnTo: 'menu', endTimer: 0,
    save: { best: {}, custom: null, startHour: 8 }, last: 0, cache: {}
  };

  /* ---------- Lưu trữ ---------- */
  function loadSave() {
    try {
      var raw = window.localStorage.getItem(SAVE_KEY);
      if (raw) {
        var d = JSON.parse(raw);
        if (d && typeof d === 'object') {
          app.save.best = d.best || {};
          app.save.custom = d.custom || null;
          app.save.startHour = +d.startHour || 8;
        }
      }
    } catch (e) { /* bỏ qua */ }
  }
  function persist() {
    try { window.localStorage.setItem(SAVE_KEY, JSON.stringify(app.save)); } catch (e) { /* bỏ qua */ }
  }

  function esc(s) {
    return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }
  function modeById(id) {
    id = +id;
    if (id >= 1) return DT.LEVELS[id - 1];
    return id === 0 ? DT.FREE_MODE : DT.CUSTOM_MODE;
  }

  var LEVEL_TIPS = {
    1: ['Xếp khối thành hàng kín 10 ô để hoàn thành một ngày. Mỗi hàng là một ngày, mỗi cột là một giờ.',
        'Cứ 30 giây, ngày hôm nay (hàng dưới cùng) được chốt và cả bảng dịch xuống một nấc.',
        'Khối I xoay đứng = học đều 4 ngày, mỗi ngày 1 giờ (+50 điểm). Xoay ngang = cày một mạch 4 giờ trong một ngày.'],
    2: ['Mỗi khối có nhãn Eisenhower Q1–Q4 và hạn chót: một vạch kẻ cùng màu trên bảng. Ô nằm cao hơn vạch là trễ hạn (sọc đỏ).',
        'Q2 xong sớm từ 2 ngày: +150. Q1 đúng hạn: +100. Trễ hạn: −150 điểm và mất uy tín.',
        'Khối Q3 chen trước việc Q2 sẽ bị trừ điểm.'],
    3: ['Năng lượng: ô nặng (3 chấm) tốn nhiều hơn, ô nặng nằm liền nhau tốn thêm. Khối nghỉ màu xanh nhạt hồi năng lượng.',
        'Phím R chèn khối nghỉ (tối đa 2 lần mỗi ngày). Năng lượng dưới 30 làm bạn mất tập trung: khối rơi nhanh, xoay đôi khi bị kẹt.',
        'Hàng đầy có ô nghỉ là Ngày cân bằng (+200). Hàng đầy không nghỉ là Ngày quá tải (+50, −20 năng lượng).',
        'Chơi tốt sẽ mở khóa vật phẩm (phím 1–4).'],
    4: ['Hoãn (C): mỗi ngày một lần. Khối bị hoãn từ 2 ngày sẽ phình to thêm 1 ô mỗi ngày và Q2 biến thành Q1.',
        'Sự kiện ngẫu nhiên mỗi vài ngày: việc đột xuất, bạn rủ đi chơi, ốm nhẹ, thầy cô dời hạn.',
        'Phím B chèn ô đệm màu xám. Khi có việc phát sinh, ô đệm sẽ hấp thụ trước.'],
    5: ['Nhiều môn học, mỗi môn một hạn riêng (tên môn đứng trước tên việc). Hạn của các môn chồng lên nhau.',
        'Lịch bận (ô BẬN) chen vào bảng mỗi ngày, cuối tuần ít hơn. Số giờ khả dụng mỗi ngày khác nhau.'],
    0: ['Chơi không giới hạn. Ngày trôi và khối rơi nhanh dần theo thời gian.', 'Đủ cơ chế: hạn chót, năng lượng, sự kiện, Hoãn, vật phẩm.'],
    '-1': ['Nhập việc thật của bạn thành khối rồi xếp vào lịch tuần này. Không có đồng hồ ngày, bạn xếp thong thả.',
           'Xếp hết việc để nhận nhận xét về lịch của bạn: trễ hạn, ngày quá tải, thiếu nghỉ.']
  };

  /* ---------- Lớp phủ ---------- */
  var overlay = $('#overlay');
  function openOverlay(html) { overlay.innerHTML = html; overlay.hidden = false; overlay.scrollTop = 0; var f = $('[autofocus], .btn.primary', overlay); if (f) { try { f.focus({ preventScroll: true }); } catch (e) { /* */ } } }
  function closeOverlay() { overlay.hidden = true; overlay.innerHTML = ''; }

  function chipsFor(m) {
    var out = [];
    out.push('<span class="chip">' + (m.days ? m.days + ' ngày' : '∞') + '</span>');
    if (m.labels) out.push('<span class="chip">Q1–Q4</span>');
    if (m.deadlines) out.push('<span class="chip">hạn chót</span>');
    if (m.energy) out.push('<span class="chip">năng lượng</span>');
    if (m.events) out.push('<span class="chip">sự kiện</span>');
    if (m.busy) out.push('<span class="chip">lịch bận</span>');
    return out.join('');
  }

  function showMenu() {
    app.screen = 'menu';
    var cards = DT.LEVELS.map(function (m) {
      var b = app.save.best[m.id];
      return '<button class="mode-card" data-act="intro" data-id="' + m.id + '"><span class="num">CẤP ' + m.id + '</span><b>' + esc(m.name.replace(/^Học kỳ \d · /, '')) + '</b>' +
        '<span class="d">' + esc(m.desc) + '</span><div class="chips">' + chipsFor(m) + '</div><span class="best">' + (b ? 'Điểm cao nhất ' + b : 'Chưa chơi') + '</span></button>';
    }).join('');
    var free = DT.FREE_MODE, cus = DT.CUSTOM_MODE;
    cards += '<button class="mode-card" data-act="intro" data-id="0"><span class="num">TỰ DO</span><b>Chơi không giới hạn</b><span class="d">' + esc(free.desc) + '</span><span class="best">' +
      (app.save.best[0] ? 'Điểm cao nhất ' + app.save.best[0] : 'Chưa chơi') + '</span></button>';
    cards += '<button class="mode-card" data-act="custom"><span class="num">TÙY CHỈNH</span><b>Xếp việc thật của bạn</b><span class="d">' + esc(cus.desc) + '</span></button>';
    var hours = '';
    for (var h = 5; h <= 11; h++) hours += '<option value="' + h + '"' + (h === app.save.startHour ? ' selected' : '') + '>' + h + ':00</option>';
    openOverlay('<div class="panel">' +
      '<h1>Day Tetris</h1>' +
      '<p class="tagline">Mỗi khối là một đầu việc. <b>Mỗi ô là một giờ.</b> Mỗi hàng là một ngày. Xếp lịch cho kín nhưng đừng quên chừa chỗ nghỉ.</p>' +
      '<div class="mode-grid">' + cards + '</div>' +
      '<div class="set-row"><label for="startHour">Giờ bắt đầu mỗi ngày</label><select id="startHour">' + hours + '</select>' +
      '<button class="btn small" data-act="howto">Cách chơi</button></div>' +
      '</div>');
  }

  function showIntro(id) {
    var m = modeById(id);
    app.screen = 'intro'; app.introMode = m;
    var tips = (LEVEL_TIPS[id] || []).map(function (t) { return '<li>' + esc(t) + '</li>'; }).join('');
    var goal = m.days
      ? 'Mục tiêu: qua hết ' + m.days + ' ngày với uy tín từ ' + DT.REP_MIN_WIN + '/100 trở lên. Thua nếu chồng khối chạm đỉnh' + (m.energy ? ', uy tín về 0 hoặc kiệt sức 2 lần.' : ' hoặc uy tín về 0.')
      : 'Mục tiêu: sống sót càng lâu càng tốt và ghi điểm cao.';
    openOverlay('<div class="panel narrow">' +
      '<div class="chips" style="margin-bottom:10px">' + chipsFor(m) + '</div>' +
      '<h2>' + esc(m.name) + '</h2><p>' + esc(goal) + '</p>' +
      '<h3>Mới ở cấp này</h3><ul class="tips">' + tips + '</ul>' +
      '<div class="row end"><button class="btn" data-act="menu">Quay lại</button><button class="btn primary" data-act="start" data-id="' + id + '" autofocus>Bắt đầu</button></div></div>');
  }

  function showHowTo(back) {
    app.returnTo = back || app.screen; app.screen = 'howto';
    var cats = ['study', 'personal', 'health', 'social', 'rest', 'buffer', 'busy'].map(function (k) {
      var c = DT.CATS[k];
      return '<div><span class="sw" style="background:' + c.color + '"></span>' + esc(c.name) + '</div>';
    }).join('');
    var qs = Object.keys(DT.Q).map(function (k) {
      return '<div><span class="chip" style="color:' + DT.Q[k].color + '">' + k + '</span>' + esc(DT.Q[k].name) + '</div>';
    }).join('');
    var shapes = ['I', 'O', 'T', 'L', 'S'].map(function (k) { return '<tr><td><b>' + k + '</b></td><td>' + esc(DT.SHAPE_LABELS[k].replace(/^[A-Z0-9]+ · /, '')) + '</td></tr>'; }).join('');
    openOverlay('<div class="panel">' +
      '<h2>Cách chơi</h2>' +
      '<p>Bảng có 10 cột (10 giờ khả dụng) và 14 hàng. Hàng dưới cùng là hôm nay, hàng trên là ngày mai, ngày kia... Khối là các đầu việc rơi từ trên xuống. Xếp đủ 10 ô trong một hàng thì ngày đó hoàn thành và được xóa. Cứ vài chục giây, hôm nay được chốt và cả bảng dịch xuống một nấc.</p>' +
      '<div class="two-col"><div>' +
      '<h3>Điều khiển</h3><table class="t"><tbody>' +
      '<tr><td><kbd>←</kbd> <kbd>→</kbd></td><td>Sang trái, phải</td></tr><tr><td><kbd>↑</kbd> / <kbd>X</kbd></td><td>Xoay (<kbd>Z</kbd> xoay ngược)</td></tr>' +
      '<tr><td><kbd>↓</kbd></td><td>Rơi nhanh</td></tr><tr><td><kbd>Space</kbd></td><td>Thả ngay</td></tr>' +
      '<tr><td><kbd>C</kbd> / <kbd>Shift</kbd></td><td>Hoãn khối (cấp 4+)</td></tr><tr><td><kbd>R</kbd></td><td>Chèn khối nghỉ (cấp 3+)</td></tr>' +
      '<tr><td><kbd>B</kbd></td><td>Chèn ô đệm (cấp 4+)</td></tr><tr><td><kbd>1</kbd>–<kbd>4</kbd></td><td>Pomodoro, Lên danh sách, Ủy quyền, Ngày nghỉ thật</td></tr>' +
      '<tr><td><kbd>P</kbd></td><td>Tạm dừng</td></tr></tbody></table>' +
      '<h3>Hình khối nghĩa là gì</h3><table class="t"><tbody>' + shapes + '</tbody></table>' +
      '</div><div>' +
      '<h3>Đọc bảng</h3><div class="legend">' + cats + '</div>' +
      '<h3>Nhãn Eisenhower</h3><div class="legend">' + qs + '</div>' +
      '<p style="margin-top:10px">Vạch kẻ trên bảng là hạn chót của một việc. Ô nằm cao hơn vạch (xa hơn về tương lai) bị sọc đỏ: trễ hạn. Chấm nhỏ cuối ô là mức năng lượng: 1 nhẹ, 2 vừa, 3 nặng.</p>' +
      '<h3>Điểm số</h3><table class="t"><tbody>' +
      '<tr><td>Ngày cân bằng</td><td>+200</td></tr><tr><td>Q2 xong sớm ≥ 2 ngày</td><td>+150</td></tr><tr><td>Q1 đúng hạn</td><td>+100</td></tr>' +
      '<tr><td>Chia nhỏ khối I theo chiều dọc</td><td>+50</td></tr><tr><td>Ngày quá tải</td><td>+50, −20 năng lượng</td></tr>' +
      '<tr><td>Xếp Q3 trước Q2</td><td>−30</td></tr><tr><td>Trễ hạn</td><td>−150, mất uy tín</td></tr></tbody></table>' +
      '</div></div>' +
      '<div class="row end"><button class="btn primary" data-act="closehelp" autofocus>Đã hiểu</button></div></div>');
  }

  function showPause() {
    app.screen = 'paused';
    openOverlay('<div class="panel narrow"><h2>Tạm dừng</h2><p>Thời gian trong game đang dừng lại.</p>' +
      '<div class="row end" style="justify-content:flex-start"><button class="btn primary" data-act="resume" autofocus>Tiếp tục</button>' +
      '<button class="btn" data-act="howto">Cách chơi</button><button class="btn" data-act="restart">Chơi lại</button><button class="btn" data-act="menu">Menu</button></div></div>');
  }

  function showChoice() {
    var ch = app.game.choice; if (!ch) return;
    app.screen = 'choice';
    openOverlay('<div class="panel narrow"><h2>💬 ' + esc(ch.title) + '</h2><p>' + esc(ch.text) + '</p>' +
      '<p style="color:var(--muted);font-size:13px">Nhận lời: +xã hội, mất ' + ch.size + ' ô (ô đệm sẽ đỡ trước). Từ chối: giữ lịch nhưng chỉ số Xã hội giảm nhẹ.</p>' +
      '<div class="row end"><button class="btn" data-act="decline">Từ chối</button><button class="btn primary" data-act="accept" autofocus>Nhận lời</button></div></div>');
  }

  /* ----- Trình nhập việc (chế độ tùy chỉnh) ----- */
  function taskRow(t, i) {
    function opts(list, sel) { return list.map(function (o) { return '<option value="' + o[0] + '"' + (o[0] === sel ? ' selected' : '') + '>' + esc(o[1]) + '</option>'; }).join(''); }
    var shapes = [['I', 'I · 4 giờ thẳng'], ['O', 'O · 2×2'], ['T', 'T · phụ thuộc'], ['L', 'L'], ['J', 'J'], ['S', 'S'], ['Z', 'Z'], ['I3', '3 giờ thẳng'], ['L3', '3 giờ góc'], ['D2', '2 giờ'], ['M1', '1 giờ']];
    var cats = [['study', 'Học'], ['personal', 'Cá nhân'], ['health', 'Sức khỏe'], ['social', 'Xã hội'], ['rest', 'Nghỉ']];
    var qs = [['Q1', 'Q1 gấp & quan trọng'], ['Q2', 'Q2 quan trọng'], ['Q3', 'Q3 gấp'], ['Q4', 'Q4 thường']];
    var es = [['light', 'Nhẹ'], ['mid', 'Vừa'], ['heavy', 'Nặng']];
    return '<tr data-i="' + i + '"><td><input type="text" class="f-name" value="' + esc(t.name) + '" aria-label="Tên việc" maxlength="40"></td>' +
      '<td><select class="f-shape" aria-label="Hình khối">' + opts(shapes, t.shape) + '</select></td>' +
      '<td><select class="f-cat" aria-label="Loại việc">' + opts(cats, t.cat) + '</select></td>' +
      '<td><select class="f-q" aria-label="Nhãn Eisenhower">' + opts(qs, t.q) + '</select></td>' +
      '<td><select class="f-e" aria-label="Năng lượng">' + opts(es, t.e) + '</select></td>' +
      '<td><input type="number" class="f-dl" min="1" max="14" value="' + esc(t.dl) + '" placeholder="–" aria-label="Hạn chót (ngày thứ mấy)"></td>' +
      '<td><button class="x" data-act="deltask" aria-label="Xóa việc">×</button></td></tr>';
  }
  function readTasks() {
    return $$('#taskBody tr').map(function (tr) {
      return { name: $('.f-name', tr).value.trim() || 'Việc', shape: $('.f-shape', tr).value, cat: $('.f-cat', tr).value, q: $('.f-q', tr).value, e: $('.f-e', tr).value, dl: $('.f-dl', tr).value };
    });
  }
  function showCustom(tasks) {
    app.screen = 'custom';
    tasks = tasks || app.save.custom || DT.SAMPLE_TASKS.map(function (t) { return Object.assign({}, t); });
    openOverlay('<div class="panel"><h2>Xếp việc thật của bạn</h2>' +
      '<p>Mỗi việc là một khối. Chọn hình theo số giờ và cách bạn muốn làm (xoay đứng nghĩa là chia ra nhiều ngày). Hạn chót tính theo ngày thứ mấy trong tuần, 1 là hôm nay.</p>' +
      '<div class="table-scroll"><table class="task-table t"><thead><tr><th>Việc</th><th>Hình</th><th>Loại</th><th>Nhãn</th><th>Năng lượng</th><th>Hạn (ngày)</th><th></th></tr></thead>' +
      '<tbody id="taskBody">' + tasks.map(taskRow).join('') + '</tbody></table></div>' +
      '<div class="row end" style="justify-content:space-between"><div class="row"><button class="btn small" data-act="addtask">+ Thêm việc</button><button class="btn small" data-act="sample">Dùng việc mẫu</button></div>' +
      '<div class="row"><button class="btn" data-act="menu">Quay lại</button><button class="btn primary" data-act="startcustom">Bắt đầu xếp</button></div></div></div>');
  }

  /* ----- Báo cáo cuối ván ----- */
  function showReport() {
    var g = app.game, rp = DT.buildReport(g);
    app.screen = 'report';
    var m = app.mode;
    window.SoftSyncGameHistory?.append('day-tetris', {
      title: 'Day Tetris · ' + (m.name || 'Game'), score: g.score, unit: 'điểm',
      summary: rp.title + (rp.insights && rp.insights.length ? ' · ' + rp.insights[0].text : ''),
      metrics: (rp.summary || []).slice(0, 8).map(function (item) { return { label: item.label, value: item.value }; }),
      details: (rp.insights || []).slice(0, 12).map(function (item) { return item.text; })
    }, 'softsync.day-tetris.history.v1');
    if (m.id >= 0 && g.score > (app.save.best[m.id] || 0)) { app.save.best[m.id] = g.score; persist(); }
    var stats = rp.summary.map(function (s) { return '<div class="stat"><b>' + esc(s.value) + '</b><span>' + esc(s.label) + '</span></div>'; }).join('');
    var ins = rp.insights.map(function (i) { return '<li class="' + i.kind + '">' + esc(i.text) + '</li>'; }).join('');
    var plan = '';
    if (rp.plan && rp.plan.length) {
      plan = '<h3>Lịch của bạn</h3><div class="table-scroll"><table class="t"><thead><tr><th>Ngày</th><th>Giờ</th><th>Nặng</th><th>Nghỉ</th><th>Việc</th></tr></thead><tbody>' +
        rp.plan.map(function (d) { return '<tr><td>' + esc(d.label) + '</td><td>' + d.filled + '/10</td><td>' + d.heavy + '</td><td>' + d.rest + '</td><td>' + esc(d.tasks.join(', ')) + '</td></tr>'; }).join('') + '</tbody></table></div>';
    }
    var next = '';
    if (rp.win && m.id >= 1 && m.id < DT.LEVELS.length) next = '<button class="btn primary" data-act="intro" data-id="' + (m.id + 1) + '">Cấp tiếp theo</button>';
    openOverlay('<div class="panel">' +
      '<div class="score-label">' + esc(m.name) + '</div>' +
      '<h2 class="' + (rp.win ? 'result-win' : 'result-lose') + '">' + esc(rp.title) + '</h2>' +
      '<div class="stat-grid">' + stats + '</div>' + plan +
      '<h3>Nhận xét từ ván chơi của bạn</h3><ul class="insights">' + ins + '</ul>' +
      '<div class="exercise">' + esc(rp.exercise) + '</div>' +
      '<div class="row end"><button class="btn" data-act="menu">Menu</button><button class="btn' + (next ? '' : ' primary') + '" data-act="restart"' + (next ? '' : ' autofocus') + '>Chơi lại</button>' + next + '</div></div>');
  }

  /* ---------- Bắt đầu / kết thúc ---------- */
  var canvas = $('#board'), renderer = new DT.Renderer(canvas), holdCanvas = $('#holdCanvas');
  var toastsEl = $('#toasts'), logEl = $('#log');

  function startGame(mode, opts) {
    closeOverlay();
    opts = opts || {};
    if (mode.custom) opts.customTasks = app.save.custom || DT.SAMPLE_TASKS;
    app.mode = mode; app.game = new DT.Game(mode, opts);
    app.screen = 'playing'; app.cache = {};
    clearTimeout(app.endTimer);
    renderer.startHour = app.save.startHour;
    toastsEl.innerHTML = ''; logEl.innerHTML = '';
    $('#modeChip').textContent = mode.name;
    $('#holdCard').hidden = !mode.hold;
    $('#puCard').hidden = !mode.powerups;
    $('#insertCard').hidden = !mode.rest;
    $('#btnBuffer').hidden = !mode.events;
    $$('[data-key="rest"]').forEach(function (b) { b.hidden = !mode.rest; });
    $$('[data-key="buffer"]').forEach(function (b) { b.hidden = !mode.events; });
    $$('[data-key="hold"]').forEach(function (b) { b.hidden = !mode.hold; });
    $$('[data-meter="energy"]').forEach(function (e) { e.hidden = !mode.energy; });
    $$('[data-meter="balance"]').forEach(function (e) { e.hidden = !mode.energy; });
    if (mode.powerups) buildPowerups();
    fitBoard();
    window.scrollTo(0, 0);
  }
  function restart() {
    if (!app.mode) return showMenu();
    startGame(app.mode);
  }
  function endGame() {
    app.screen = 'ended';
    app.endTimer = setTimeout(showReport, 1100);
  }
  function pauseGame() {
    if (app.screen !== 'playing') return;
    showPause();
  }
  function resume() { closeOverlay(); app.screen = 'playing'; }

  /* ---------- Bố cục ---------- */
  function fitBoard() {
    var wrap = $('#boardWrap').parentElement, vw = window.innerWidth, vh = window.innerHeight;
    var mobile = vw <= 880;
    var availW = Math.min(wrap.clientWidth || vw - 32, mobile ? 520 : 760);
    var top = $('#boardWrap').getBoundingClientRect().top + window.scrollY;
    var availH = mobile ? Math.max(380, Math.min(vh * 0.66, 640)) : Math.max(420, vh - top - 40);
    renderer.resize(availW, availH);
  }
  window.addEventListener('resize', fitBoard);

  /* ---------- Điều khiển ---------- */
  var rep = {};
  function stopRepeat(n) { var r = rep[n]; if (r) { clearTimeout(r.t); clearInterval(r.i); delete rep[n]; } }
  function startRepeat(n, fn) {
    stopRepeat(n); fn();
    var r = rep[n] = {};
    r.t = setTimeout(function () { r.i = setInterval(fn, 48); }, 160);
  }
  function stopAllRepeats() { Object.keys(rep).forEach(stopRepeat); if (app.game) app.game.setSoftDrop(false); }
  function playing() { return app.screen === 'playing' && app.game; }

  function press(name) {
    var g = app.game;
    if (name === 'pause') { pauseGame(); return; }
    if (!playing()) return;
    switch (name) {
      case 'left': startRepeat('left', function () { g.move(-1); }); break;
      case 'right': startRepeat('right', function () { g.move(1); }); break;
      case 'rotate': g.rotate(1); break;
      case 'rotateCCW': g.rotate(-1); break;
      case 'soft': g.setSoftDrop(true); break;
      case 'drop': g.hardDrop(); break;
      case 'hold': g.holdPiece(); break;
      case 'rest': g.insertBlock('rest'); break;
      case 'buffer': g.insertBlock('buffer'); break;
      case 'pu1': g.usePowerup('pomodoro'); break;
      case 'pu2': g.usePowerup('list'); break;
      case 'pu3': g.usePowerup('delegate'); break;
      case 'pu4': g.usePowerup('dayoff'); break;
    }
  }
  function release(name) {
    if (name === 'left' || name === 'right') stopRepeat(name);
    if (name === 'soft' && app.game) app.game.setSoftDrop(false);
  }
  var KEYMAP = {
    ArrowLeft: 'left', a: 'left', A: 'left', ArrowRight: 'right', d: 'right', D: 'right',
    ArrowUp: 'rotate', x: 'rotate', X: 'rotate', z: 'rotateCCW', Z: 'rotateCCW',
    ArrowDown: 'soft', s: 'soft', S: 'soft', ' ': 'drop', c: 'hold', C: 'hold', Shift: 'hold',
    r: 'rest', R: 'rest', b: 'buffer', B: 'buffer', '1': 'pu1', '2': 'pu2', '3': 'pu3', '4': 'pu4',
    p: 'pause', P: 'pause', Escape: 'pause'
  };
  var held = {};
  window.addEventListener('keydown', function (e) {
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    var tag = (e.target && e.target.tagName) || '';
    if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA') return;
    if (app.screen === 'paused' && (e.key === 'p' || e.key === 'P' || e.key === 'Escape')) { e.preventDefault(); resume(); return; }
    var name = KEYMAP[e.key];
    if (!name) return;
    if (playing()) e.preventDefault();
    if (e.repeat || held[e.key]) return;
    held[e.key] = true;
    press(name);
  });
  window.addEventListener('keyup', function (e) {
    held[e.key] = false;
    var name = KEYMAP[e.key]; if (name) release(name);
  });
  window.addEventListener('blur', function () { held = {}; stopAllRepeats(); });
  document.addEventListener('visibilitychange', function () { if (document.hidden) { held = {}; stopAllRepeats(); pauseGame(); } });

  // nút cảm ứng
  $$('#touch button').forEach(function (b) {
    var name = b.getAttribute('data-key');
    b.addEventListener('pointerdown', function (e) { e.preventDefault(); try { b.setPointerCapture(e.pointerId); } catch (x) { /* */ } press(name); });
    ['pointerup', 'pointercancel', 'lostpointercapture'].forEach(function (ev) { b.addEventListener(ev, function () { release(name); }); });
  });

  /* ---------- Bảng điều khiển & HUD ---------- */
  function buildPowerups() {
    $('#puGrid').innerHTML = Object.keys(DT.POWERUPS).map(function (k) {
      var p = DT.POWERUPS[k];
      return '<button class="pu empty" data-pu="' + k + '" title="' + esc(p.desc) + '"><kbd>' + p.key + '</kbd><span>' + esc(p.name) + '</span><b>0</b></button>';
    }).join('');
  }
  var hudNodes = {};
  function hud(name, val) {
    if (app.cache['h' + name] === val) return;
    app.cache['h' + name] = val;
    if (!hudNodes[name]) hudNodes[name] = $$('[data-hud="' + name + '"]');
    hudNodes[name].forEach(function (n) { n.textContent = val; });
  }
  function meter(name, pct, mid, low) {
    var key = 'm' + name + Math.round(pct);
    if (app.cache[key]) return; app.cache['m' + name] = null; app.cache[key] = 1;
    $$('[data-meter="' + name + '"]').forEach(function (m) {
      $('i', m).style.width = Math.max(0, Math.min(100, pct)) + '%';
      m.classList.toggle('low', pct < low); m.classList.toggle('mid', pct >= low && pct < mid);
    });
  }
  function taskMeta(t, g) {
    var c = DT.CATS[t.cat] || DT.CATS.study, parts = [];
    parts.push('<span class="chip"><i class="dot" style="background:' + c.color + '"></i>' + esc(c.name) + '</span>');
    if (t.q && g.mode.labels) parts.push('<span class="chip" style="color:' + DT.Q[t.q].color + '" title="' + esc(DT.Q[t.q].name) + '">' + t.q + '</span>');
    if (g.mode.energy && t.kind === 'work') parts.push('<span class="chip">' + esc(DT.ENERGY[t.e].name) + '</span>');
    return parts.join('');
  }
  function dlText(t, g) {
    if (t.kind !== 'work') return t.kind === 'rest' ? 'Hồi +' + DT.ENERGY_REST_GAIN + ' năng lượng mỗi ô' : '';
    if (t.dl != null) { var left = t.dl - g.day; return 'Hạn ' + DT.dayName(t.dl) + (left > 0 ? ' · còn ' + left + ' ngày' : (left === 0 ? ' · hôm nay' : ' · quá hạn')); }
    if (t.dlOff != null) return 'Hạn sau ~' + t.dlOff + ' ngày';
    return g.mode.deadlines ? 'Không có hạn' : '';
  }
  function updatePanels(g) {
    // điểm / ngày
    hud('score', String(g.score));
    var dayShow = Math.min(g.day, g.mode.days || g.day);
    hud('dayLabel', 'Ngày ' + dayShow + (g.mode.days ? ' / ' + g.mode.days : '') + ' · ' + DT.dayName(g.day));
    hud('dayShort', dayShow + (g.mode.days ? '/' + g.mode.days : ''));
    if (g.mode.dayLen) hud('dayTimer', 'còn ' + Math.max(0, Math.ceil(g.dayLength() - g.dayTimer / 1000)) + 's'); else hud('dayTimer', 'tự xếp');
    hud('energy', String(Math.round(g.energy)));
    hud('gpa', (g.rep / 25).toFixed(1));
    hud('balance', String(g.balance));
    meter('energy', g.energy, 60, 30); meter('rep', g.rep, 70, 45); meter('balance', g.balance, 75, 50);

    // trạng thái
    var st = [];
    if (g.energyLow()) st.push('<span class="chip">Mất tập trung</span>');
    if (g.fatigue > 0) st.push('<span class="chip warn">Mệt mỏi ' + g.fatigue + ' ngày</span>');
    if (g.burnouts > 0) st.push('<span class="chip">Kiệt sức ' + g.burnouts + '/2</span>');
    if (g.previewBoost > 0) st.push('<span class="chip warn">Xem trước 5 (' + g.previewBoost + ')</span>');
    var stKey = st.join('');
    if (app.cache.st !== stKey) { app.cache.st = stKey; $('#statusRow').innerHTML = stKey; }

    // vật phẩm + chèn
    if (g.mode.powerups) {
      $$('#puGrid .pu').forEach(function (b) {
        var n = g.powerups[b.getAttribute('data-pu')];
        b.classList.toggle('empty', !n); $('b', b).textContent = n;
      });
    }
    if (g.mode.rest) {
      var il = g.insertsLeft;
      $('#insertLeft').textContent = 'còn ' + il + ' lượt hôm nay';
      $('#btnRest').disabled = il <= 0; $('#btnBuffer').disabled = il <= 0;
    }

    // đang xếp
    var p = g.piece, curKey = p ? p.task.id + ':' + p.task.dl + ':' + p.task.q + ':' + p.task.e + ':' + g.day : 'none';
    if (app.cache.cur !== curKey) {
      app.cache.cur = curKey;
      if (!p) $('#curCard').innerHTML = '<div class="empty-note">–</div>';
      else {
        var t = p.task, hint = DT.SHAPE_LABELS[t.shape] ? DT.SHAPE_LABELS[t.shape].replace(/^[A-Z0-9]+ · /, '') : '';
        if (t.emergency) hint = 'Việc đột xuất! Xếp xuống thấp để kịp hạn.';
        $('#curCard').innerHTML = '<div class="cur-name">' + esc(t.name) + '</div><div class="cur-meta">' + taskMeta(t, g) + '</div>' +
          '<div class="cur-hint">' + esc(dlText(t, g)) + '</div>' + (hint ? '<div class="cur-hint" style="margin-top:4px">' + esc(hint) + '</div>' : '');
      }
    }

    // kế tiếp
    var pv = g.getPreview(), nk = pv.map(function (d) { return d.task.id + ':' + d.cells.length; }).join('|') + '@' + g.day;
    if (app.cache.next !== nk) {
      app.cache.next = nk;
      var list = $('#nextList');
      list.innerHTML = pv.map(function () { return '<div class="piece-row"><canvas></canvas><div class="piece-info"></div></div>'; }).join('') || '<div class="empty-note">Hết việc trong danh sách.</div>';
      $$('.piece-row', list).forEach(function (row, i) {
        var d = pv[i];
        DT.drawMini($('canvas', row), d, { cell: i === 0 ? 19 : 15, w: 4, h: 3 });
        $('.piece-info', row).innerHTML = '<b>' + esc(d.task.name) + '</b>' + taskMeta(d.task, g) + '<br><span>' + esc(dlText(d.task, g)) + '</span>';
      });
      $('#nextCount').textContent = pv.length ? pv.length + ' khối' : '';
    }
    // hoãn
    if (g.mode.hold) {
      var hk = g.hold ? g.hold.task.id + ':' + g.hold.cells.length + ':' + g.hold.task.q + ':' + g.day : 'none';
      if (app.cache.hold !== hk) {
        app.cache.hold = hk;
        DT.drawMini(holdCanvas, g.hold, { cell: 15, w: 4, h: 3 });
        holdCanvas.style.display = g.hold ? '' : 'none';
        $('#holdInfo').innerHTML = g.hold
          ? '<b>' + esc(g.hold.task.name) + '</b>' + taskMeta(g.hold.task, g) + '<br><span>' + (g.hold.task.bloat ? 'Đã phình thêm ' + g.hold.task.bloat + ' ô' : 'Hoãn càng lâu càng phình') + '</span>'
          : '<span>Trống. Mỗi ngày hoãn tối đa 1 lần.</span>';
      }
    }
  }

  function addToast(ev) {
    if (ev.kind === 'info') return;
    var el = document.createElement('div');
    el.className = 'toast ' + ev.kind; el.textContent = ev.text;
    toastsEl.appendChild(el);
    while (toastsEl.children.length > 4) toastsEl.removeChild(toastsEl.firstChild);
    setTimeout(function () { el.classList.add('out'); }, 2600);
    setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 3100);
  }
  function addLog(ev) {
    var li = document.createElement('li');
    li.className = ev.kind; li.textContent = ev.text;
    logEl.insertBefore(li, logEl.firstChild);
    while (logEl.children.length > 8) logEl.removeChild(logEl.lastChild);
  }
  function drain(g) {
    if (!g.events.length) return;
    var evs = g.events.splice(0, g.events.length);
    evs.forEach(function (e) { addLog(e); addToast(e); });
  }

  /* ---------- Vòng lặp ---------- */
  function loop(ts) {
    window.requestAnimationFrame(loop);
    var dt = ts - app.last; app.last = ts;
    var g = app.game;
    if (!g) return;
    if (app.screen === 'playing') {
      g.update(dt);
      if (g.choice) { stopAllRepeats(); showChoice(); }
      else if (g.status !== 'playing') { stopAllRepeats(); endGame(); }
    }
    drain(g);
    updatePanels(g);
    renderer.draw(g, ts);
  }

  /* ---------- Sự kiện giao diện ---------- */
  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-act], [data-pu]');
    if (!t) return;
    var pu = t.getAttribute('data-pu');
    if (pu && playing()) { app.game.usePowerup(pu); return; }
    var act = t.getAttribute('data-act'), id = t.getAttribute('data-id');
    switch (act) {
      case 'menu': stopAllRepeats(); clearTimeout(app.endTimer); showMenu(); break;
      case 'intro': showIntro(id); break;
      case 'start': startGame(modeById(id)); break;
      case 'custom': showCustom(); break;
      case 'howto': showHowTo(app.screen === 'howto' ? app.returnTo : app.screen); break;
      case 'closehelp':
        if (app.returnTo === 'paused') showPause(); else if (app.returnTo === 'playing') resume(); else showMenu();
        break;
      case 'pause': pauseGame(); break;
      case 'resume': resume(); break;
      case 'restart': startGame(app.mode); break;
      case 'accept': app.game.resolveChoice(true); closeOverlay(); app.screen = 'playing'; break;
      case 'decline': app.game.resolveChoice(false); closeOverlay(); app.screen = 'playing'; break;
      case 'rest': press('rest'); break;
      case 'buffer': press('buffer'); break;
      case 'addtask': {
        var tasks = readTasks(); tasks.push({ name: 'Việc mới', shape: 'D2', cat: 'study', q: 'Q2', e: 'mid', dl: '' });
        showCustom(tasks); break;
      }
      case 'deltask': { var ts2 = readTasks(); ts2.splice(+t.closest('tr').getAttribute('data-i'), 1); showCustom(ts2); break; }
      case 'sample': showCustom(DT.SAMPLE_TASKS.map(function (x) { return Object.assign({}, x); })); break;
      case 'startcustom': {
        var list = readTasks();
        if (!list.length) list = DT.SAMPLE_TASKS.slice();
        app.save.custom = list; persist();
        startGame(DT.CUSTOM_MODE); break;
      }
    }
  });
  document.addEventListener('change', function (e) {
    if (e.target.id === 'startHour') { app.save.startHour = +e.target.value; renderer.startHour = app.save.startHour; persist(); }
  });

  /* ---------- Khởi động ---------- */
  loadSave();
  // Bảng xem trước phía sau menu: một ván nền để trang không trống
  app.mode = DT.LEVELS[0];
  app.game = new DT.Game(app.mode, { seed: 20240901 });
  (function demoFill() {
    var g = app.game, U = DT.util;
    for (var i = 0; i < 7; i++) {
      var p = g.piece, best = null, cells = p.cells;
      for (var rot = 0; rot < 4; rot++) {
        var d = U.dims(cells);
        for (var c = 0; c + d.w <= DT.COLS; c++) {
          if (!g.fits(cells, 0, c)) continue;
          var r = 0; while (g.fits(cells, r + 1, c)) r++;
          var sc = r * 10 - c * 0.1 * (i % 2 ? -1 : 1);
          if (!best || sc > best.sc) best = { sc: sc, cells: cells, c: c, r: r };
        }
        cells = U.rotCW(cells);
      }
      if (best) { p.cells = best.cells; p.c = best.c; p.r = best.r; g.lockPiece(); }
    }
    g.events.length = 0; g.log.length = 0;
  })();
  renderer.startHour = app.save.startHour;
  $('#holdCard').hidden = true; $('#puCard').hidden = true; $('#insertCard').hidden = true;
  fitBoard();
  showMenu();
  window.requestAnimationFrame(function (ts) { app.last = ts; loop(ts); });
  // cho phép tải lại bố cục khi font tải xong
  if (document.fonts && document.fonts.ready) document.fonts.ready.then(function () { app.cache = {}; });
  window.DT_APP = app;
})();
