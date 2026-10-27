/* Day Tetris - lõi game (không phụ thuộc DOM, chạy được cả trong Node để test) */
(function (g) {
  'use strict';
  var DT = g.DT;
  var COLS = DT.COLS, ROWS = DT.ROWS;

  /* ================= Tiện ích ================= */
  function mulberry32(a) {
    return function () {
      a |= 0; a = (a + 0x6D2B79F5) | 0;
      var t = Math.imul(a ^ (a >>> 15), 1 | a);
      t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
      return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
  }
  function clamp(v, a, b) { return v < a ? a : (v > b ? b : v); }
  function norm(cells) {
    var mr = Infinity, mc = Infinity;
    cells.forEach(function (p) { mr = Math.min(mr, p[0]); mc = Math.min(mc, p[1]); });
    return cells.map(function (p) { return [p[0] - mr, p[1] - mc]; })
      .sort(function (a, b) { return a[0] - b[0] || a[1] - b[1]; });
  }
  function rotCW(cells) { return norm(cells.map(function (p) { return [p[1], -p[0]]; })); }
  function rotCCW(cells) { return norm(cells.map(function (p) { return [-p[1], p[0]]; })); }
  function dims(cells) {
    var h = 0, w = 0;
    cells.forEach(function (p) { h = Math.max(h, p[0] + 1); w = Math.max(w, p[1] + 1); });
    return { h: h, w: w };
  }
  function emptyRow() { var r = []; for (var i = 0; i < COLS; i++) r.push(null); return r; }
  function key(p) { return p[0] + ',' + p[1]; }
  function connected(cells) {
    if (cells.length <= 1) return true;
    var set = {}; cells.forEach(function (p) { set[key(p)] = 1; });
    var seen = {}, stack = [cells[0]]; seen[key(cells[0])] = 1;
    var n = 1;
    while (stack.length) {
      var p = stack.pop();
      [[1, 0], [-1, 0], [0, 1], [0, -1]].forEach(function (d) {
        var q = [p[0] + d[0], p[1] + d[1]], k = key(q);
        if (set[k] && !seen[k]) { seen[k] = 1; n++; stack.push(q); }
      });
    }
    return n === cells.length;
  }
  // Thêm 1 ô vào khối (mô phỏng việc "phình to" do trì hoãn)
  function growShape(cells, rng) {
    var set = {}; cells.forEach(function (p) { set[key(p)] = 1; });
    var cand = [], seen = {};
    cells.forEach(function (p) {
      [[1, 0], [-1, 0], [0, 1], [0, -1]].forEach(function (d) {
        var q = [p[0] + d[0], p[1] + d[1]], k = key(q);
        if (!set[k] && !seen[k]) { seen[k] = 1; cand.push(q); }
      });
    });
    var best = [], bestScore = Infinity;
    cand.forEach(function (q) {
      var n = norm(cells.concat([q])), d = dims(n);
      var s = Math.max(d.w, d.h) * 10 + d.w * d.h;
      if (d.w > 6 || d.h > 6) s += 100;
      if (s < bestScore) { bestScore = s; best = [n]; } else if (s === bestScore) best.push(n);
    });
    return best[Math.floor(rng() * best.length)];
  }

  DT.util = { norm: norm, rotCW: rotCW, rotCCW: rotCCW, dims: dims, mulberry32: mulberry32, clamp: clamp };

  var ROW_KINDS = {
    balanced:   { name: 'Ngày cân bằng', score: DT.SCORE.rowBalanced },
    overload:   { name: 'Ngày quá tải',  score: DT.SCORE.rowOverload },
    allnighter: { name: 'Ngày cày đêm',  score: DT.SCORE.rowAllNighter },
    idle:       { name: 'Ngày thư giãn', score: DT.SCORE.rowIdle },
    plain:      { name: 'Ngày hoàn thành', score: DT.SCORE.rowPlain }
  };

  /* ================= Game ================= */
  function Game(mode, opts) {
    opts = opts || {};
    this.mode = mode;
    this.rng = opts.rng || mulberry32(opts.seed != null ? opts.seed : ((Math.random() * 4294967296) >>> 0));
    this.autoChoice = !!opts.autoChoice;
    this.nextTaskId = 1;

    this.board = [];
    for (var r = 0; r < ROWS; r++) this.board.push(emptyRow());

    this.day = 1;
    this.dayTimer = 0;
    this.time = 0;
    this.score = 0;
    this.rep = DT.REP_START;
    this.energy = DT.ENERGY_START;
    this.burnouts = 0;
    this.fatigue = 0;
    this.balance = 100;
    this.socialDebt = 0;
    this.daysSinceRest = 0;
    this.recent = [];
    this.powerups = { pomodoro: 0, list: 0, delegate: 0, dayoff: 0 };
    if (mode.powerups) { this.powerups.pomodoro = 1; this.powerups.list = 1; }
    this.goodPlays = 0;
    this.previewBoost = 0;
    this.hold = null;
    this.holdUsed = false;
    this.insertsLeft = DT.INSERTS_PER_DAY;
    this.queue = [];
    this.bag = [];
    this.piece = null;
    this.gravityTimer = 0;
    this.lockTimer = 0;
    this.lockResets = 0;
    this.softDrop = false;
    this.pieceAge = 0;
    this.stuckAt = -9999;
    this.active = [];
    this.status = 'playing';
    this.endReason = '';
    this.choice = null;
    this.events = [];
    this.log = [];
    this.planLog = [];
    this.nextEventIn = 3 + (this.rng() < 0.5 ? 1 : 0);
    this.projects = [];
    this.busyTask = this.mkTask({ kind: 'busy', cat: 'busy', name: 'Bận', size: 1 });
    this.sickTask = this.mkTask({ kind: 'sick', cat: 'sick', name: 'Ốm', size: 1 });
    this.stats = {
      tasksDone: 0, late: [], q2Early: 0, q1Ok: 0, rowsTotal: 0,
      rows: { balanced: 0, overload: 0, allnighter: 0, plain: 0, idle: 0 },
      verticalI: 0, q3Prime: 0, holds: 0, bloatLoss: 0, bloated: 0, q2Held: 0,
      absorbed: 0, emergencyHit: 0, sickHit: 0, friendYes: 0, friendNo: 0,
      catCells: { study: 0, personal: 0, health: 0, social: 0, rest: 0 },
      emptyCells: 0, committedCells: 0, committedRows: 0, burnouts: 0,
      pomodoro: 0, delegate: 0, dayoff: 0, piecesLocked: 0, bufferPlaced: 0, maxCombo: 0
    };
    this.combo = 0;

    if (mode.custom) {
      var self = this;
      (opts.customTasks || []).forEach(function (o) { self.queue.push(self.customPiece(o)); });
    }
    if (mode.clusters) this.initProjects();
    if (mode.busy) for (var rr = 0; rr < 3; rr++) this.seedBusy(1 + rr);
    this.refill();
    this.spawn();
  }
  DT.Game = Game;
  var P = Game.prototype;

  /* ---------- RNG helpers ---------- */
  P.rand = function () { return this.rng(); };
  P.randInt = function (a, b) { return a + Math.floor(this.rng() * (b - a + 1)); };
  P.pick = function (arr) { return arr[Math.floor(this.rng() * arr.length)]; };
  P.weighted = function (obj) {
    var sum = 0, k;
    for (k in obj) sum += obj[k];
    var r = this.rng() * sum;
    for (k in obj) { r -= obj[k]; if (r <= 0) return k; }
    return k;
  };
  P.shuffle = function (arr) {
    for (var i = arr.length - 1; i > 0; i--) {
      var j = Math.floor(this.rng() * (i + 1)), t = arr[i]; arr[i] = arr[j]; arr[j] = t;
    }
    return arr;
  };

  /* ---------- Thông báo ---------- */
  P.emit = function (kind, text) {
    var ev = { kind: kind, text: text, at: this.time, day: this.day };
    this.events.push(ev);
    if (this.events.length > 200) this.events.shift();
    this.log.push(ev);
    if (this.log.length > 80) this.log.shift();
  };
  P.addScore = function (n, label, silent) {
    this.score += n;
    if (!silent && label) this.emit(n >= 0 ? 'good' : 'bad', (n >= 0 ? '+' : '') + n + ' ' + label);
  };

  /* ---------- Tạo việc ---------- */
  P.mkTask = function (o) {
    return {
      id: this.nextTaskId++, name: o.name || '', shape: o.shape || 'M1', cat: o.cat || 'study',
      kind: o.kind || 'work', q: o.q || null, e: o.e || 'light',
      dl: o.dl == null ? null : o.dl, dlOff: o.dlOff == null ? null : o.dlOff,
      size: o.size || 1, cellsLeft: o.size || 1, late: false, done: false, lastDay: 0,
      heldSince: null, emergency: !!o.emergency, project: o.project || null,
      splitDone: false, bloat: 0, spawned: false
    };
  };
  P.customPiece = function (o) {
    var shape = DT.SHAPES[o.shape] ? o.shape : 'D2';
    var cells = norm(DT.SHAPES[shape]);
    var isRest = o.cat === 'rest';
    var dl = (o.dl === '' || o.dl == null || isNaN(+o.dl)) ? null : Math.max(1, Math.round(+o.dl));
    var t = this.mkTask({
      name: o.name || 'Việc', shape: shape, cat: isRest ? 'rest' : (o.cat || 'study'),
      kind: isRest ? 'rest' : 'work', q: isRest ? null : (o.q || 'Q2'), e: isRest ? 'light' : (o.e || 'mid'),
      dl: dl, size: cells.length
    });
    return { task: t, cells: cells };
  };
  P.initProjects = function () {
    var names = this.shuffle(['Giải tích', 'Vật lý', 'Kinh tế', 'Tiếng Anh', 'Lập trình']).slice(0, 3);
    var offs = [5, 8, 11], self = this;
    this.projects = names.map(function (n, i) { return { name: n, deadline: 1 + offs[i] }; });
    this.projects.forEach(function (p) { p.base = p.deadline; });
  };
  P.nextShape = function () {
    if (!this.bag.length) {
      var s = this.mode.shapes.slice();
      if (s.length < 5) s = s.concat(s);
      this.bag = this.shuffle(s);
    }
    return this.bag.pop();
  };
  P.genPiece = function () {
    var lv = this.mode, cells, shape, t;
    var restP = DT.REST_CHANCE;
    if (lv.energy && this.energy < 45) restP = 0.24;
    if (lv.rest && this.rand() < restP) {
      shape = this.rand() < 0.55 ? 'D2' : 'M1';
      cells = norm(DT.SHAPES[shape]);
      t = this.mkTask({ kind: 'rest', cat: 'rest', name: this.pick(DT.REST_NAMES), shape: shape, e: 'light', size: cells.length });
      return { task: t, cells: cells };
    }
    shape = this.nextShape();
    cells = norm(DT.SHAPES[shape]);
    var cat = this.weighted(DT.SHAPE_CATS[shape]);
    var names = (DT.SHAPE_NAMES[shape] && DT.SHAPE_NAMES[shape][cat]) || DT.NAMES[cat];
    var name = this.pick(names);
    var q = null, dlOff = null, dl = null, e = 'mid', project = null;
    if (lv.labels) q = this.weighted(DT.Q_WEIGHTS);
    if (lv.energy) e = this.weighted(DT.ENERGY_BY_CAT[cat]);
    if (lv.deadlines && q && DT.DEADLINE_RANGE[q]) dlOff = this.randInt(DT.DEADLINE_RANGE[q][0], DT.DEADLINE_RANGE[q][1]);
    if (lv.clusters && cat === 'study') {
      var pr = this.pick(this.projects);
      if (pr.deadline < this.day + 1) pr.deadline = this.day + this.randInt(7, 10);
      project = pr.name; dl = pr.deadline; dlOff = null;
      q = (pr.deadline - this.day <= 3) ? 'Q1' : 'Q2';
      name = pr.name + ': ' + name;
    }
    t = this.mkTask({ name: name, shape: shape, cat: cat, q: q, e: e, dl: dl, dlOff: dlOff, size: cells.length, project: project });
    return { task: t, cells: cells };
  };
  P.refill = function () {
    if (this.mode.custom) return;
    while (this.queue.length < 8) this.queue.push(this.genPiece());
  };
  P.getPreview = function () { return this.queue.slice(0, this.previewBoost > 0 ? 5 : 3); };

  P.makeEmergency = function (size) {
    var shape = size === 1 ? 'M1' : (size === 2 ? 'D2' : this.pick(['I3', 'L3']));
    var cells = norm(DT.SHAPES[shape]);
    var t = this.mkTask({
      name: this.pick(DT.EMERGENCY_NAMES), shape: shape, cat: this.pick(['study', 'personal']),
      q: 'Q1', e: 'mid', dl: this.day + 2, size: cells.length, emergency: true
    });
    return { task: t, cells: cells };
  };

  /* ---------- Bàn chơi & khối đang rơi ---------- */
  P.fits = function (cells, r, c) {
    for (var i = 0; i < cells.length; i++) {
      var rr = r + cells[i][0], cc = c + cells[i][1];
      if (cc < 0 || cc >= COLS || rr < 0 || rr >= ROWS) return false;
      if (this.board[rr][cc]) return false;
    }
    return true;
  };
  P.dayOfRow = function (r) { return this.day + (ROWS - 1 - r); };
  P.ghostRow = function () {
    var p = this.piece; if (!p) return 0;
    var r = p.r;
    while (this.fits(p.cells, r + 1, p.c)) r++;
    return r;
  };
  P.canAct = function () { return this.status === 'playing' && !this.choice && !!this.piece; };

  P.spawn = function (desc) {
    if (this.status !== 'playing') return;
    if (!desc) {
      if (this.mode.custom && !this.queue.length) {
        if (this.hold) { desc = this.hold; this.hold = null; desc.task.heldSince = null; }
        else { this.finish(true, 'plan'); return; }
      }
      if (!desc) {
        desc = this.queue.shift();
        this.refill();
        if (this.previewBoost > 0) this.previewBoost--;
      }
    }
    var t = desc.task;
    if (!t.spawned) {
      t.spawned = true;
      if (t.dl == null && t.dlOff != null) t.dl = this.day + t.dlOff;
      if (t.kind === 'work') this.active.push(t);
    }
    var cells = norm(desc.cells);
    var d = dims(cells);
    this.piece = { task: t, cells: cells, r: 0, c: Math.floor((COLS - d.w) / 2) };
    this.gravityTimer = 0; this.lockTimer = 0; this.lockResets = 0; this.pieceAge = 0;
    if (!this.fits(cells, 0, this.piece.c)) { this.lose('overflow'); }
  };
  P.interrupt = function (desc) {
    if (this.piece) {
      this.queue.unshift({ task: this.piece.task, cells: norm(this.piece.cells) });
      this.piece = null;
    }
    this.spawn(desc);
  };

  /* ---------- Điều khiển ---------- */
  P.touch = function () {
    var p = this.piece;
    if (p && !this.fits(p.cells, p.r + 1, p.c) && this.lockResets < 12) { this.lockTimer = 0; this.lockResets++; }
  };
  P.move = function (dc) {
    if (!this.canAct()) return false;
    var p = this.piece;
    if (this.fits(p.cells, p.r, p.c + dc)) { p.c += dc; this.touch(); return true; }
    return false;
  };
  P.rotate = function (dir) {
    if (!this.canAct()) return false;
    var p = this.piece;
    if (this.mode.energy && this.energy < DT.ENERGY_LOW && this.rand() < 0.2) { this.stuckAt = this.time; return false; }
    var o = dims(p.cells);
    var cells = dir > 0 ? rotCW(p.cells) : rotCCW(p.cells);
    var n = dims(cells);
    var baseC = p.c + Math.trunc((o.w - n.w) / 2);
    var kicks = [[0, 0], [0, -1], [0, 1], [0, -2], [0, 2], [-1, 0], [-1, -1], [-1, 1]];
    for (var i = 0; i < kicks.length; i++) {
      var rr = p.r + kicks[i][0], cc = baseC + kicks[i][1];
      if (this.fits(cells, rr, cc)) { p.cells = cells; p.r = rr; p.c = cc; this.touch(); return true; }
    }
    return false;
  };
  P.setSoftDrop = function (on) { this.softDrop = !!on; };
  P.hardDrop = function () {
    if (!this.canAct()) return false;
    var p = this.piece;
    p.r = this.ghostRow();
    this.lockPiece();
    return true;
  };
  P.holdPiece = function () {
    if (!this.canAct() || !this.mode.hold) return false;
    if (this.holdUsed) { this.emit('warn', 'Mỗi ngày chỉ được hoãn 1 lần.'); return false; }
    this.holdUsed = true; this.stats.holds++;
    var cur = { task: this.piece.task, cells: norm(this.piece.cells) };
    cur.task.heldSince = this.day;
    this.piece = null;
    var old = this.hold;
    this.hold = cur;
    if (old) { old.task.heldSince = null; this.spawn(old); } else this.spawn();
    return true;
  };
  P.insertBlock = function (kind) {
    if (!this.canAct()) return false;
    var lv = this.mode;
    if (kind === 'rest' && !lv.rest) return false;
    if (kind === 'buffer' && !lv.events) { this.emit('warn', 'Ô đệm mở khóa từ Học kỳ 4.'); return false; }
    if (this.insertsLeft <= 0) { this.emit('warn', 'Hôm nay hết lượt chèn nghỉ/đệm.'); return false; }
    this.insertsLeft--;
    var shape = kind === 'rest' ? 'D2' : 'M1';
    var cells = norm(DT.SHAPES[shape]);
    var t = this.mkTask({
      kind: kind, cat: kind, name: kind === 'rest' ? this.pick(DT.REST_NAMES) : 'Ô đệm', shape: shape, e: 'light', size: cells.length
    });
    this.interrupt({ task: t, cells: cells });
    return true;
  };

  /* ---------- Vòng cập nhật ---------- */
  P.dayLength = function () {
    var lv = this.mode;
    return lv.speedUp ? Math.max(14, lv.dayLen - 0.6 * (this.day - 1)) : lv.dayLen;
  };
  P.gravityMs = function () {
    var lv = this.mode, base = lv.gravity;
    if (lv.speedUp) base = Math.max(180, base - 22 * (this.day - 1));
    if (lv.energy && this.energy < DT.ENERGY_LOW) base *= 0.72;
    return base;
  };
  P.update = function (dt) {
    if (this.status !== 'playing' || this.choice) return;
    dt = Math.min(dt, 250);
    this.time += dt;
    if (this.mode.dayLen) {
      this.dayTimer += dt;
      var len = this.dayLength() * 1000;
      while (this.dayTimer >= len && this.status === 'playing' && !this.choice) {
        this.dayTimer -= len;
        this.dayTick();
        len = this.dayLength() * 1000;
      }
    }
    if (this.status !== 'playing' || this.choice) return;
    if (this.piece) this.stepPiece(dt);
  };
  P.stepPiece = function (dt) {
    var p = this.piece;
    this.pieceAge += dt;
    if (this.fits(p.cells, p.r + 1, p.c)) {
      this.lockTimer = 0;
      this.gravityTimer += dt;
      var gm = this.softDrop ? Math.min(45, this.gravityMs()) : this.gravityMs();
      while (this.gravityTimer >= gm && this.fits(p.cells, p.r + 1, p.c)) { p.r++; this.gravityTimer -= gm; }
      if (!this.fits(p.cells, p.r + 1, p.c)) this.gravityTimer = 0;
    } else {
      this.lockTimer += dt;
      if (this.lockTimer >= (this.softDrop ? 120 : DT.LOCK_DELAY)) this.lockPiece();
    }
  };

  /* ---------- Khóa khối ---------- */
  P.lockPiece = function () {
    var p = this.piece;
    if (!p || this.status !== 'playing') return;
    var abs = [], i;
    for (i = 0; i < p.cells.length; i++) {
      var rr = p.r + p.cells[i][0], cc = p.c + p.cells[i][1];
      this.board[rr][cc] = { t: p.task };
      abs.push([rr, cc]);
    }
    this.piece = null;
    this.stats.piecesLocked++;
    this.onLocked(p, abs);
    if (this.status !== 'playing') return;
    this.clearRows();
    if (this.status !== 'playing') return;
    this.spawn();
  };
  P.pushRecent = function (cat) {
    this.recent.push(cat);
    if (this.recent.length > 40) this.recent.shift();
  };
  P.onLocked = function (p, abs) {
    var t = p.task, lv = this.mode, n = abs.length, i, self = this;
    if (t.kind === 'rest') {
      this.changeEnergy(DT.ENERGY_REST_GAIN * n);
      this.daysSinceRest = 0;
      for (i = 0; i < n; i++) this.pushRecent('rest');
      this.stats.catCells.rest += n;
    } else if (t.kind === 'buffer') {
      this.stats.bufferPlaced += n;
    } else if (t.kind === 'work') {
      this.score += DT.SCORE.cellBase * n;
      for (i = 0; i < n; i++) this.pushRecent(t.cat);
      this.stats.catCells[t.cat] += n;
      if (lv.energy) {
        var cost = DT.ENERGY[t.e].cost * n, adj = 0;
        if (t.e === 'heavy') {
          abs.forEach(function (a) {
            [-1, 1].forEach(function (d) {
              var cc = a[1] + d; if (cc < 0 || cc >= COLS) return;
              var nb = self.board[a[0]][cc];
              if (nb && nb.t !== t && nb.t.kind === 'work' && nb.t.e === 'heavy') adj++;
            });
          });
          cost += adj * DT.ENERGY_HEAVY_ADJ;
        }
        if (this.fatigue > 0) cost *= 1.5;
        if (adj > 0) this.emit('warn', 'Ô nặng liền nhau: mệt hơn (−' + Math.round(cost) + ' năng lượng)');
        if (cost > 0) this.changeEnergy(-cost);
        if (this.status !== 'playing') return;
      }
      // chia nhỏ khối I theo chiều dọc
      if ((t.shape === 'I' || t.shape === 'I3') && !t.splitDone && n >= 3) {
        var rows = {}, cols = {};
        abs.forEach(function (a) { rows[a[0]] = 1; cols[a[1]] = 1; });
        if (Object.keys(rows).length === n && Object.keys(cols).length === 1) {
          t.splitDone = true; this.stats.verticalI++;
          this.addScore(DT.SCORE.splitI, 'Chia nhỏ dự án theo chiều dọc');
        }
      }
      // Q3 chen trước Q2
      if (lv.labels && t.q === 'Q3') {
        var maxR = Math.max.apply(null, abs.map(function (a) { return a[0]; })), prime = false;
        if (this.hold && this.hold.task.q === 'Q2') prime = true;
        else {
          var seen = {};
          for (var r = ROWS - 1; r >= 0 && !prime; r--) {
            for (var c = 0; c < COLS; c++) {
              var cell = this.board[r][c];
              if (cell && cell.t.kind === 'work' && cell.t.q === 'Q2' && cell.t !== t && !seen[cell.t.id]) {
                seen[cell.t.id] = 1;
                if (r < maxR) { prime = true; break; }
              }
            }
          }
        }
        if (prime) { this.stats.q3Prime++; this.addScore(DT.SCORE.q3Prime, 'Xếp việc Q3 trước việc quan trọng Q2'); }
      }
      // cảnh báo trễ hạn khi xếp
      if (t.dl != null) {
        var late = abs.some(function (a) { return self.dayOfRow(a[0]) > t.dl; });
        if (late) this.emit('warn', '«' + t.name + '» đang nằm sau hạn chót!');
      }
    }
    this.recalcBalance();
  };

  /* ---------- Năng lượng, cân bằng ---------- */
  P.changeEnergy = function (d) {
    if (!this.mode.energy) return;
    this.energy = clamp(this.energy + d, 0, 100);
    if (this.energy <= 0 && this.status === 'playing') this.burnout();
  };
  P.burnout = function () {
    this.burnouts++; this.stats.burnouts++; this.fatigue = 2;
    if (this.burnouts >= 2 && !this.mode.soft) {
      this.emit('bad', '🔥 Kiệt sức lần 2!');
      this.energy = 0;
      this.lose('burnout');
      return;
    }
    this.emit('bad', '🔥 KIỆT SỨC! Bạn phải nghỉ ngơi (lần ' + this.burnouts + '/2).');
    this.energy = DT.BURNOUT_RESET;
    this.addScore(-100, 'Kiệt sức');
  };
  P.changeRep = function (d) {
    this.rep = clamp(this.rep + d, 0, 100);
    if (this.rep <= 0 && !this.mode.soft && this.status === 'playing') this.lose('rep');
  };
  P.recalcBalance = function () {
    if (!this.mode.energy) { this.balance = 100; return; }
    var cnt = { study: 0, personal: 0, health: 0, social: 0, rest: 0 }, work = 0, tot = this.recent.length;
    this.recent.forEach(function (c) { cnt[c]++; if (c !== 'rest') work++; });
    var bal = 100;
    if (work >= 12) {
      DT.WORK_CATS.forEach(function (cat) {
        var pct = cnt[cat] / work * 100, rg = DT.BALANCE_TARGET[cat];
        if (pct < rg[0]) bal -= (rg[0] - pct) * 1.4; else if (pct > rg[1]) bal -= (pct - rg[1]) * 1.4;
      });
      var rp = tot ? cnt.rest / tot * 100 : 0;
      if (rp < 6) bal -= (6 - rp) * 2;
    }
    bal -= this.socialDebt * 3;
    this.balance = clamp(Math.round(bal), 0, 100);
  };

  /* ---------- Hoàn thành / trễ hạn ---------- */
  P.removeCell = function (cell, dayOfCell) {
    var t = cell.t;
    if (t.kind !== 'work') return;
    t.cellsLeft--;
    if (dayOfCell > t.lastDay) t.lastDay = dayOfCell;
    if (t.cellsLeft <= 0 && !t.done) this.completeTask(t);
  };
  P.applyLate = function (t) {
    if (t.late) return;
    t.late = true;
    this.stats.late.push(t.name);
    this.addScore(DT.SCORE.late, 'Trễ hạn «' + t.name + '»');
    this.changeRep(DT.SCORE.lateRep);
  };
  P.completeTask = function (t) {
    t.done = true;
    this.stats.tasksDone++;
    var lv = this.mode;
    if (!lv.labels) { this.addScore(20, null, true); return; }
    if (t.late) return;
    if (t.dl != null && t.lastDay > t.dl) { this.applyLate(t); return; }
    var S = DT.SCORE;
    if (t.q === 'Q1') {
      this.stats.q1Ok++;
      this.addScore(S.q1Ok, 'Q1 đúng hạn «' + t.name + '»'); this.changeRep(S.onTimeRep); this.earnGood();
    } else if (t.q === 'Q2') {
      if (t.dl != null && t.dl - t.lastDay >= 2) {
        this.stats.q2Early++;
        this.addScore(S.q2Early, 'Q2 xong sớm «' + t.name + '»'); this.changeRep(S.q2EarlyRep); this.earnGood();
      } else { this.addScore(S.q2Ok, 'Q2 đúng hạn «' + t.name + '»'); this.changeRep(S.onTimeRep); }
    } else if (t.q === 'Q3') {
      this.addScore(S.q3Ok, null, true);
    } else {
      this.addScore(S.q4Ok, null, true);
    }
  };
  P.activeTasks = function () {
    this.active = this.active.filter(function (t) { return !t.done; });
    return this.active;
  };
  P.earnGood = function () {
    this.goodPlays++;
    if (!this.mode.powerups || this.goodPlays % 2) return;
    var pool = { pomodoro: 3, list: 2, delegate: 2 };
    if (this.mode.events) pool.dayoff = 1;
    var k = this.weighted(pool);
    if (this.powerups[k] >= 3) return;
    this.powerups[k]++;
    this.emit('good', '🎁 Mở khóa vật phẩm: ' + DT.POWERUPS[k].name);
  };

  /* ---------- Xóa hàng ---------- */
  P.classifyRow = function (row) {
    if (!this.mode.energy) return 'plain';
    var slack = 0, heavy = 0, run = 0, maxRun = 0, relax = 0;
    row.forEach(function (cell) {
      var t = cell.t;
      if (t.kind === 'rest' || t.kind === 'buffer') { slack++; relax++; }
      if (t.kind === 'work' && t.e === 'heavy') { heavy++; run++; if (run > maxRun) maxRun = run; } else run = 0;
    });
    if (maxRun >= 4) return 'allnighter';
    if (relax >= 6) return 'idle';
    if (slack === 0) return 'overload';
    return heavy <= 6 ? 'balanced' : 'plain';
  };
  P.clearRows = function () {
    var full = [], r;
    for (r = 0; r < ROWS; r++) {
      var ok = true;
      for (var c = 0; c < COLS; c++) if (!this.board[r][c]) { ok = false; break; }
      if (ok) full.push(r);
    }
    if (!full.length) { this.combo = 0; return 0; }
    this.removeRows(full, false);
    return full.length;
  };
  // rows: danh sách chỉ số hàng; dayoff = true thì không chấm điểm hàng
  P.removeRows = function (rows, dayoff) {
    var self = this, i, kindCount = 0;
    rows.forEach(function (r) {
      var row = self.board[r], kind = null;
      var dayN = self.dayOfRow(r);
      if (!dayoff) {
        kind = self.classifyRow(row);
        var K = ROW_KINDS[kind];
        self.stats.rows[kind]++; self.stats.rowsTotal++;
        self.addScore(K.score, K.name + ' (' + DT.dayName(dayN) + ')');
        if (kind === 'balanced') { self.changeEnergy(10); self.earnGood(); }
        else if (kind === 'overload') { self.changeEnergy(-20); self.emit('warn', 'Quá tải: không có ô nghỉ (−20 năng lượng)'); }
        else if (kind === 'allnighter') {
          self.fatigue = Math.max(self.fatigue, 2); self.changeEnergy(-8);
          self.emit('warn', 'Cày đêm! Những ngày tới bạn sẽ mệt mỏi.');
        }
        kindCount++;
      }
      if (self.mode.custom) {
        var names = [], heavy = 0, rest = 0;
        row.forEach(function (cell) {
          if (!cell) return;
          if (cell.t.kind === 'work') { if (names.indexOf(cell.t.name) < 0) names.push(cell.t.name); if (cell.t.e === 'heavy') heavy++; }
          if (cell.t.kind === 'rest') rest++;
        });
        self.planLog.push({ day: dayN, filled: COLS, heavy: heavy, rest: rest, tasks: names, kind: kind });
      }
      if (self.status !== 'playing') return;
      row.forEach(function (cell) { if (cell) self.removeCell(cell, dayN); });
    });
    if (this.status !== 'playing') return;
    if (!dayoff && rows.length > 1) this.addScore((rows.length - 1) * DT.SCORE.combo, 'Xóa ' + rows.length + ' ngày cùng lúc');
    this.combo += rows.length;
    this.stats.maxCombo = Math.max(this.stats.maxCombo, rows.length);
    var nb = [], n = rows.length;
    for (i = 0; i < ROWS; i++) if (rows.indexOf(i) < 0) nb.push(this.board[i]);
    while (nb.length < ROWS) nb.unshift(emptyRow());
    this.board = nb;
    this.fixPieceAfterShift();
  };
  P.fixPieceAfterShift = function () {
    var p = this.piece; if (!p) return;
    while (!this.fits(p.cells, p.r, p.c) && p.r > 0) p.r--;
    if (!this.fits(p.cells, p.r, p.c)) this.lose('overflow');
  };

  /* ---------- Ngày mới ---------- */
  // Lịch bận (họp, ca làm...) chen vào ngay: các ô rơi xuống chạm chồng khối trong cột của chúng.
  P.seedBusy = function (dayN) {
    var n = DT.isWeekend(dayN) ? this.randInt(0, 1) : this.randInt(1, 3);
    var cols = this.shuffle([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]), placed = 0, pc = {};
    if (this.piece) for (var k = 0; k < this.piece.cells.length; k++) pc[this.piece.c + this.piece.cells[k][1]] = 1;
    for (var i = 0; i < cols.length && placed < n; i++) {
      var c = cols[i], r = ROWS - 1;
      while (r >= 0 && this.board[r][c]) r--;
      if (r < 3 || pc[c]) continue;
      this.board[r][c] = { t: this.busyTask };
      placed++;
    }
    return placed;
  };
  P.dayTick = function () {
    var lv = this.mode, tries = 0, i;
    // khối đang rơi mà không "sống sót" sau khi bảng dịch xuống thì chốt luôn
    while (this.piece) {
      var p = this.piece, ok = true;
      for (i = 0; i < p.cells.length; i++) {
        var rr = p.r + p.cells[i][0] - 1, cc = p.c + p.cells[i][1];
        if (rr >= 0 && this.board[rr][cc]) { ok = false; break; }
      }
      if (ok) break;
      if (tries++ >= 4) { this.lose('overflow'); return; }
      this.lockPiece();
      if (this.status !== 'playing') return;
    }
    // chốt hàng hôm nay
    var row = this.board[ROWS - 1], dayN = this.day, filled = 0, nonNull = 0, self = this;
    row.forEach(function (cell) {
      if (!cell) return;
      nonNull++;
      if (cell.t.kind === 'work') filled++;
      self.removeCell(cell, dayN);
    });
    if (this.status !== 'playing') return;
    this.stats.committedRows++;
    this.stats.committedCells += COLS;
    this.stats.emptyCells += COLS - nonNull;
    var pts = filled * DT.SCORE.dayCell;
    if (pts) { this.score += pts; this.emit('info', DT.dayName(dayN) + ' chốt: ' + filled + ' giờ làm việc (+' + pts + ')'); }
    if (lv.energy && dayN >= 3 && nonNull === 0) this.addScore(DT.SCORE.emptyDay, 'Ngày bỏ trống');
    this.board.pop();
    this.board.unshift(emptyRow());
    this.day++;
    if (lv.busy) { var nb = this.seedBusy(this.day); if (nb) this.emit('info', '📌 Lịch bận ' + DT.dayName(this.day) + ': ' + nb + ' giờ bị chiếm sẵn.'); }
    this.holdUsed = false;
    this.insertsLeft = DT.INSERTS_PER_DAY;
    if (this.socialDebt > 0 && this.day % 2 === 0) { this.socialDebt--; this.recalcBalance(); }

    // quá hạn
    this.activeTasks().forEach(function (t) {
      if (t.dl != null && t.dl < self.day && !t.late) self.applyLate(t);
    });
    if (this.status !== 'playing') return;

    // năng lượng theo ngày
    if (lv.energy) {
      var regen = Math.round(DT.ENERGY_DAILY_REGEN * (0.4 + 0.6 * this.balance / 100));
      if (this.fatigue > 0) { regen = 0; this.fatigue--; }
      this.daysSinceRest++;
      if (this.daysSinceRest >= 3) { regen -= 2; this.emit('warn', 'Nhiều ngày không nghỉ: −2 năng lượng'); }
      if (this.balance < 50) this.emit('warn', 'Cuộc sống mất cân bằng: hồi năng lượng chậm lại');
      this.changeEnergy(regen);
      if (this.status !== 'playing') return;
    }
    if (this.projects.length) this.projects.forEach(function (pr) {
      if (pr.deadline < self.day) pr.deadline = self.day + self.randInt(7, 10);
    });

    // hoãn -> phình to
    if (this.hold && lv.hold) {
      var ht = this.hold.task, held = this.day - (ht.heldSince == null ? this.day : ht.heldSince);
      if (held >= 2) {
        if (ht.q === 'Q2') { ht.q = 'Q1'; this.stats.q2Held++; this.emit('bad', '«' + ht.name + '» bị hoãn quá lâu: Q2 → Q1!'); }
        if (this.hold.cells.length < DT.MAX_BLOAT_CELLS) {
          this.hold.cells = growShape(this.hold.cells, this.rng);
          ht.size++; ht.cellsLeft++; ht.bloat++;
          this.stats.bloated++; this.stats.bloatLoss += DT.SCORE.bloatPerCell;
          this.addScore(-DT.SCORE.bloatPerCell, '«' + ht.name + '» phình to thêm 1 ô vì trì hoãn');
        }
      }
    }

    // sự kiện ngẫu nhiên
    if (lv.events) {
      this.nextEventIn--;
      if (this.nextEventIn <= 0) {
        this.nextEventIn = 3 + (this.rng() < 0.5 ? 1 : 0);
        this.triggerEvent();
        if (this.status !== 'playing') return;
      }
    }
    if (lv.days && this.day > lv.days) this.finish(true, 'semester');
  };

  /* ---------- Sự kiện ---------- */
  P.consumeBuffer = function (n) {
    var got = 0;
    for (var r = ROWS - 1; r >= 0 && got < n; r--)
      for (var c = 0; c < COLS && got < n; c++) {
        var cell = this.board[r][c];
        if (cell && cell.t.kind === 'buffer') { this.board[r][c] = null; got++; }
      }
    return got;
  };
  P.absorbed = function (n) {
    if (n > 0) { this.stats.absorbed++; this.addScore(DT.SCORE.absorb, 'Ô đệm hấp thụ ' + n + ' ô việc phát sinh'); }
  };
  P.triggerEvent = function () {
    var pool = { emergency: 3, friend: 2, sick: 2, deadline: 2 };
    if (!this.activeTasks().some(function (t) { return t.dl != null && !t.late; })) delete pool.deadline;
    var k = this.weighted(pool), self = this;
    if (k === 'emergency') {
      var size = this.pick([1, 2, 2, 3]);
      var ab = this.consumeBuffer(size);
      if (ab > 0) this.absorbed(ab);
      var remain = size - ab;
      if (remain <= 0) { this.emit('good', '⚡ Việc đột xuất xuất hiện nhưng ô đệm đã đỡ hết!'); return; }
      this.stats.emergencyHit++;
      var desc = this.makeEmergency(remain);
      this.emit('bad', '⚡ Việc đột xuất: «' + desc.task.name + '» (Q1, hạn ' + DT.dayName(this.day + 2) + ') rơi xuống ngay!');
      this.interrupt(desc);
    } else if (k === 'friend') {
      var sz = this.pick([2, 3]);
      this.choice = { type: 'friend', size: sz, title: 'Bạn rủ đi chơi', text: 'Một người bạn rủ bạn đi chơi (mất ' + sz + ' ô). Nhận lời hay từ chối?' };
      this.emit('info', '💬 Bạn rủ đi chơi!');
      if (this.autoChoice) this.resolveChoice(this.rand() < 0.5);
    } else if (k === 'sick') {
      var ab2 = this.consumeBuffer(2);
      if (ab2 > 0) this.absorbed(ab2);
      var need = 2 - ab2;
      if (need <= 0) { this.emit('good', '🤒 Ốm nhẹ nhưng nhờ ô đệm bạn vẫn kịp lịch.'); return; }
      var rr = ROWS - 2, used = {};
      if (this.piece) this.piece.cells.forEach(function (cl) { if (self.piece.r + cl[0] === rr) used[self.piece.c + cl[1]] = 1; });
      var cols = [];
      for (var c = 0; c < COLS; c++) if (!this.board[rr][c] && !used[c]) cols.push(c);
      var sup = cols.filter(function (c) { return self.board[ROWS - 1][c]; });
      var order = this.shuffle(sup).concat(this.shuffle(cols.filter(function (c) { return sup.indexOf(c) < 0; })));
      var placed = 0;
      for (var i = 0; i < order.length && placed < need; i++) { this.board[rr][order[i]] = { t: this.sickTask }; placed++; }
      this.stats.sickHit++;
      this.changeEnergy(-5);
      this.emit('bad', '🤒 Ốm nhẹ: ngày mai mất ' + placed + ' giờ khả dụng.');
    } else if (k === 'deadline') {
      var list = this.activeTasks().filter(function (t) { return t.dl != null && !t.late; });
      var t = this.pick(list);
      var delta = this.pick([-2, -1, 1, 1, 2]);
      var nd = Math.max(this.day + 1, t.dl + delta);
      if (nd === t.dl) nd = t.dl + 1;
      this.emit(nd > t.dl ? 'good' : 'bad', '📅 Thầy cô dời hạn «' + t.name + '»: ' + DT.dayName(t.dl) + ' → ' + DT.dayName(nd));
      t.dl = nd;
    }
  };
  P.resolveChoice = function (accept) {
    var ch = this.choice; if (!ch) return;
    this.choice = null;
    if (ch.type === 'friend') {
      if (accept) {
        var ab = this.consumeBuffer(ch.size);
        if (ab > 0) this.absorbed(ab);
        var remain = ch.size - ab;
        if (remain > 0) {
          var shape = remain === 1 ? 'M1' : (remain === 2 ? 'D2' : 'I3');
          var cells = norm(DT.SHAPES[shape]);
          var t = this.mkTask({ name: this.pick(DT.NAMES.social), shape: shape, cat: 'social', q: 'Q4', e: 'light', size: cells.length });
          this.queue.unshift({ task: t, cells: cells });
        }
        this.stats.friendYes++;
        this.addScore(DT.SCORE.friend, 'Đi chơi cùng bạn');
      } else {
        this.stats.friendNo++;
        this.socialDebt = Math.min(5, this.socialDebt + 1);
        this.recalcBalance();
        this.emit('warn', 'Từ chối lời rủ: chỉ số Xã hội giảm nhẹ.');
      }
    }
    if (this.mode.days && this.day > this.mode.days && this.status === 'playing') this.finish(true, 'semester');
  };

  /* ---------- Vật phẩm ---------- */
  P.partition = function (cells) {
    var n = cells.length, k = Math.ceil(n / 2), best = null, idx = [], i;
    function rec(start, chosen) {
      if (best) return;
      if (chosen.length === k) {
        var a = chosen.map(function (j) { return cells[j]; });
        var b = cells.filter(function (_, j) { return chosen.indexOf(j) < 0; });
        if (connected(a) && connected(b)) best = [a, b];
        return;
      }
      for (var j = start; j < n; j++) rec(j + 1, chosen.concat([j]));
    }
    rec(0, []);
    if (!best) best = [cells.slice(0, k), cells.slice(k)];
    return best;
  };
  P.usePowerup = function (name) {
    if (!this.canAct() || !this.mode.powerups) return false;
    if (!this.powerups[name]) { this.emit('warn', 'Bạn chưa có vật phẩm «' + DT.POWERUPS[name].name + '».'); return false; }
    var p = this.piece, t = p.task, self = this;
    if (name === 'pomodoro') {
      if (t.kind !== 'work' || t.e !== 'heavy' || p.cells.length < 2) { this.emit('warn', 'Pomodoro chỉ dùng cho khối NẶNG đang rơi.'); return false; }
      var parts = this.partition(p.cells);
      t.e = 'light';
      this.piece = null;
      this.queue.unshift({ task: t, cells: norm(parts[1]) });
      this.spawn({ task: t, cells: norm(parts[0]) });
      this.emit('good', '🍅 Pomodoro: «' + t.name + '» được chia thành 2 phiên nhẹ.');
      this.stats.pomodoro++;
    } else if (name === 'list') {
      this.previewBoost = DT.LIST_PIECES;
      this.emit('good', '📝 Lên danh sách: xem trước 5 khối trong ' + DT.LIST_PIECES + ' khối tới.');
    } else if (name === 'delegate') {
      if (t.q === 'Q3' && t.kind === 'work') {
        t.cellsLeft -= p.cells.length; if (t.cellsLeft <= 0) t.done = true;
        this.piece = null;
        this.emit('good', '🤝 Ủy quyền «' + t.name + '» cho người khác.');
        this.spawn();
      } else {
        var found = {}, order = [];
        for (var r = ROWS - 1; r >= 0; r--) for (var c = 0; c < COLS; c++) {
          var cell = this.board[r][c];
          if (cell && cell.t.kind === 'work' && cell.t.q === 'Q3' && !cell.t.done) {
            if (!found[cell.t.id]) { found[cell.t.id] = { t: cell.t, n: 0 }; order.push(found[cell.t.id]); }
            found[cell.t.id].n++;
          }
        }
        var cand = order.filter(function (o) { return o.n === o.t.cellsLeft; });
        if (!cand.length) { this.emit('warn', 'Không có việc Q3 nào để ủy quyền.'); return false; }
        cand.sort(function (a, b) { return b.n - a.n; });
        var target = cand[0].t;
        for (r = 0; r < ROWS; r++) for (c = 0; c < COLS; c++) {
          if (this.board[r][c] && this.board[r][c].t === target) this.board[r][c] = null;
        }
        target.cellsLeft = 0; target.done = true;
        this.emit('good', '🤝 Ủy quyền «' + target.name + '»: đã xóa khỏi lịch.');
      }
      this.stats.delegate++;
    } else if (name === 'dayoff') {
      this.removeRows([ROWS - 1], true);
      if (this.status !== 'playing') return true;
      this.energy = 100;
      this.emit('good', '🏖️ Ngày nghỉ thật: xóa hôm nay, năng lượng đầy!');
      this.stats.dayoff++;
    }
    this.powerups[name]--;
    return true;
  };

  /* ---------- Kết thúc ---------- */
  P.evaluateRemaining = function () {
    var self = this, maxDay = {};
    for (var r = 0; r < ROWS; r++) for (var c = 0; c < COLS; c++) {
      var cell = this.board[r][c];
      if (cell && cell.t.kind === 'work') {
        var d = this.dayOfRow(r);
        if (!maxDay[cell.t.id] || d > maxDay[cell.t.id].d) maxDay[cell.t.id] = { t: cell.t, d: d };
      }
    }
    for (var id in maxDay) {
      var m = maxDay[id];
      if (m.t.dl != null && m.d > m.t.dl && !m.t.late) this.applyLate(m.t);
    }
  };
  P.finish = function (win, reason) {
    if (this.status !== 'playing') return;
    this.evaluateRemaining();
    if (this.status !== 'playing') return;
    if (win && !this.mode.soft && this.rep < DT.REP_MIN_WIN) {
      this.lose('rep-low');
      return;
    }
    this.status = 'won';
    this.endReason = reason || 'semester';
    if (this.mode.days) {
      var bonus = 300 + Math.round(this.rep * 5);
      this.score += bonus;
      this.emit('good', '🎓 Hoàn thành học kỳ! +' + bonus);
    }
  };
  P.lose = function (reason) {
    if (this.status !== 'playing') return;
    this.status = 'lost';
    this.endReason = reason;
    var msg = {
      overflow: 'Backlog tràn! Công việc chất đến tận đỉnh.',
      rep: 'Uy tín về 0: trượt môn!',
      'rep-low': 'Hết học kỳ nhưng uy tín quá thấp.',
      burnout: 'Burnout! Bạn kiệt sức 2 lần.'
    }[reason] || 'Kết thúc';
    this.emit('bad', '💥 ' + msg);
  };

  /* ---------- Dữ liệu cho giao diện ---------- */
  P.deadlineMarks = function () {
    var seen = {}, out = [], self = this;
    function add(t) {
      if (!t || t.dl == null || t.done || seen[t.id]) return;
      seen[t.id] = 1;
      out.push({ task: t, row: ROWS - 1 - (t.dl - self.day) });
    }
    for (var r = 0; r < ROWS; r++) for (var c = 0; c < COLS; c++) if (this.board[r][c]) add(this.board[r][c].t);
    if (this.piece) add(this.piece.task);
    if (this.hold) add(this.hold.task);
    return out;
  };
  P.energyLow = function () { return this.mode.energy && this.energy < DT.ENERGY_LOW; };
})(typeof window !== 'undefined' ? window : globalThis);
