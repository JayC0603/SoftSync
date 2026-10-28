/* Day Tetris - vẽ canvas (bảng chơi, khối xem trước) */
(function (g) {
  'use strict';
  var DT = g.DT, COLS = DT.COLS, ROWS = DT.ROWS;

  var C = {
    bg: '#0a0e1b', rowA: '#11172c', rowB: '#0e1426', today: 'rgba(255,209,102,0.10)',
    weekend: 'rgba(140,120,255,0.07)', grid: 'rgba(150,165,225,0.10)', label: '#7f8aae',
    labelStrong: '#ffd166', danger: '#ff5d5d', fg: '#e8ecf8'
  };
  var PIPS = { light: 1, mid: 2, heavy: 3 };

  function rr(ctx, x, y, w, h, r) {
    // r = [tl, tr, br, bl]
    ctx.beginPath();
    if (ctx.roundRect) { ctx.roundRect(x, y, w, h, r); return; }
    ctx.rect(x, y, w, h);
  }

  /* Vẽ một ô việc. nb = {u,d,l,r}: có ô cùng việc ở cạnh đó không */
  function drawTile(ctx, x, y, s, t, nb, o) {
    o = o || {};
    nb = nb || {};
    var c = DT.CATS[t.cat] || DT.CATS.study, pad = Math.max(1, Math.round(s / 28)), rad = Math.max(3, s * 0.16);
    var x0 = x + (nb.l ? 0 : pad), x1 = x + s - (nb.r ? 0 : pad);
    var y0 = y + (nb.u ? 0 : pad), y1 = y + s - (nb.d ? 0 : pad);
    var radii = [
      (nb.u || nb.l) ? 0 : rad, (nb.u || nb.r) ? 0 : rad,
      (nb.d || nb.r) ? 0 : rad, (nb.d || nb.l) ? 0 : rad
    ];
    ctx.save();
    ctx.globalAlpha = o.alpha == null ? 1 : o.alpha;
    var kind = t.kind;
    if (kind === 'busy' || kind === 'sick') {
      ctx.fillStyle = c.color; rr(ctx, x0, y0, x1 - x0, y1 - y0, radii); ctx.fill();
      ctx.save(); rr(ctx, x0, y0, x1 - x0, y1 - y0, radii); ctx.clip();
      ctx.strokeStyle = 'rgba(255,255,255,0.14)'; ctx.lineWidth = 2;
      for (var k = -s; k < s * 2; k += 8) { ctx.beginPath(); ctx.moveTo(x + k, y + s); ctx.lineTo(x + k + s, y); ctx.stroke(); }
      ctx.restore();
      if (s >= 26) {
        ctx.fillStyle = 'rgba(255,255,255,0.55)'; ctx.font = '600 ' + Math.round(s * 0.28) + 'px "Be Vietnam Pro", system-ui, sans-serif';
        ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
        ctx.fillText(kind === 'busy' ? 'BẬN' : 'ỐM', x + s / 2, y + s / 2 + 1);
      }
      ctx.restore(); return;
    }
    if (kind === 'buffer') {
      ctx.fillStyle = 'rgba(138,147,166,0.22)'; rr(ctx, x0, y0, x1 - x0, y1 - y0, radii); ctx.fill();
      ctx.strokeStyle = c.light; ctx.lineWidth = 1.5; ctx.setLineDash([4, 3]);
      rr(ctx, x0 + 1, y0 + 1, x1 - x0 - 2, y1 - y0 - 2, radii); ctx.stroke(); ctx.setLineDash([]);
      if (s >= 26) {
        ctx.fillStyle = c.light; ctx.font = '600 ' + Math.round(s * 0.26) + 'px "Be Vietnam Pro", system-ui, sans-serif';
        ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillText('đệm', x + s / 2, y + s / 2 + 1);
      }
      ctx.restore(); return;
    }
    var gr = ctx.createLinearGradient(x, y, x, y + s);
    gr.addColorStop(0, c.light); gr.addColorStop(1, c.color);
    ctx.fillStyle = gr; rr(ctx, x0, y0, x1 - x0, y1 - y0, radii); ctx.fill();
    // viền dưới sẫm cho có khối
    if (!nb.d) { ctx.fillStyle = 'rgba(0,0,0,0.22)'; ctx.fillRect(x0, y1 - Math.max(2, s * 0.09), x1 - x0, Math.max(2, s * 0.09)); }
    if (!nb.u) { ctx.fillStyle = 'rgba(255,255,255,0.28)'; ctx.fillRect(x0 + rad * (nb.l ? 0 : 0.6), y0, x1 - x0 - rad * 1.2, 2); }
    if (kind === 'rest' && s >= 26) {
      ctx.fillStyle = 'rgba(10,30,50,0.55)'; ctx.font = '700 ' + Math.round(s * 0.34) + 'px "Be Vietnam Pro", system-ui, sans-serif';
      ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillText('z', x + s / 2, y + s / 2);
    }
    // năng lượng (chấm)
    if (o.energy && kind === 'work') {
      var n = PIPS[t.e] || 1, pr = Math.max(1.6, s * 0.055), gap = pr * 3;
      ctx.fillStyle = 'rgba(8,12,30,0.62)';
      for (var i = 0; i < n; i++) { ctx.beginPath(); ctx.arc(x + s / 2 + (i - (n - 1) / 2) * gap, y + s - pr * 2.6, pr, 0, 6.2832); ctx.fill(); }
    }
    // trễ hạn
    if (o.late) {
      ctx.save(); rr(ctx, x0, y0, x1 - x0, y1 - y0, radii); ctx.clip();
      ctx.fillStyle = 'rgba(120,10,20,0.38)'; ctx.fillRect(x, y, s, s);
      ctx.strokeStyle = 'rgba(255,120,120,0.85)'; ctx.lineWidth = 2;
      for (var j = -s; j < s * 2; j += 9) { ctx.beginPath(); ctx.moveTo(x + j, y + s); ctx.lineTo(x + j + s, y); ctx.stroke(); }
      ctx.restore();
    }
    // nhãn Eisenhower
    if (o.label && t.q && s >= 28) {
      var q = DT.Q[t.q], fs = Math.max(8, Math.round(s * 0.26));
      ctx.font = '700 ' + fs + 'px "JetBrains Mono", ui-monospace, monospace';
      var tw = ctx.measureText(t.q).width + 6;
      ctx.fillStyle = 'rgba(8,12,30,0.78)'; rr(ctx, x + 3, y + 3, tw, fs + 3, [3, 3, 3, 3]); ctx.fill();
      ctx.fillStyle = q.color; ctx.textAlign = 'left'; ctx.textBaseline = 'top'; ctx.fillText(t.q, x + 6, y + 4);
    }
    ctx.restore();
  }

  function neighbors(set, r, c, same) {
    function has(rr2, cc) { var v = set(rr2, cc); return v && same(v); }
    return { u: has(r - 1, c), d: has(r + 1, c), l: has(r, c - 1), r: has(r, c + 1) };
  }

  /* ================= Renderer ================= */
  function Renderer(canvas) {
    this.canvas = canvas; this.ctx = canvas.getContext('2d');
    this.cell = 36; this.mL = 46; this.mT = 26; this.mR = 18; this.mB = 12;
    this.startHour = 8; this.w = 0; this.h = 0;
  }
  Renderer.prototype.resize = function (availW, availH) {
    var cw = (availW - this.mL - this.mR) / COLS, ch = (availH - this.mT - this.mB) / ROWS;
    this.cell = Math.max(18, Math.min(46, Math.floor(Math.min(cw, ch))));
    this.w = this.mL + COLS * this.cell + this.mR;
    this.h = this.mT + ROWS * this.cell + this.mB;
    var dpr = Math.min(2.5, g.devicePixelRatio || 1);
    this.canvas.width = Math.round(this.w * dpr); this.canvas.height = Math.round(this.h * dpr);
    this.canvas.style.width = this.w + 'px'; this.canvas.style.height = this.h + 'px';
    this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  };

  Renderer.prototype.draw = function (game, now) {
    var ctx = this.ctx, s = this.cell, mL = this.mL, mT = this.mT, lv = game.mode, r, c;
    ctx.clearRect(0, 0, this.w, this.h);
    ctx.fillStyle = C.bg; ctx.fillRect(0, 0, this.w, this.h);

    // ----- nền lưới: mỗi hàng là một ngày -----
    for (r = 0; r < ROWS; r++) {
      var dayN = game.dayOfRow(r), y = mT + r * s;
      ctx.fillStyle = (r % 2) ? C.rowA : C.rowB; ctx.fillRect(mL, y, COLS * s, s);
      if (DT.isWeekend(dayN)) { ctx.fillStyle = C.weekend; ctx.fillRect(mL, y, COLS * s, s); }
      if (r === ROWS - 1) { ctx.fillStyle = C.today; ctx.fillRect(mL, y, COLS * s, s); }
      // nhãn ngày
      var isToday = r === ROWS - 1;
      ctx.textAlign = 'right'; ctx.textBaseline = 'middle';
      ctx.fillStyle = isToday ? C.labelStrong : C.label;
      ctx.font = '600 ' + Math.max(10, Math.round(s * 0.3)) + 'px "Be Vietnam Pro", system-ui, sans-serif';
      ctx.fillText(DT.dayName(dayN), mL - 7, y + s / 2 - (s >= 30 ? 5 : 0));
      if (s >= 30) {
        ctx.font = '500 ' + Math.max(8, Math.round(s * 0.22)) + 'px "JetBrains Mono", ui-monospace, monospace';
        ctx.fillStyle = isToday ? C.labelStrong : 'rgba(127,138,174,0.7)';
        ctx.fillText(isToday ? 'nay' : 'N' + dayN, mL - 7, y + s / 2 + 8);
      }
    }
    ctx.strokeStyle = C.grid; ctx.lineWidth = 1; ctx.beginPath();
    for (c = 0; c <= COLS; c++) { ctx.moveTo(mL + c * s + 0.5, mT); ctx.lineTo(mL + c * s + 0.5, mT + ROWS * s); }
    for (r = 0; r <= ROWS; r++) { ctx.moveTo(mL, mT + r * s + 0.5); ctx.lineTo(mL + COLS * s, mT + r * s + 0.5); }
    ctx.stroke();
    // nhãn giờ
    ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillStyle = C.label;
    ctx.font = '500 ' + Math.max(9, Math.round(s * 0.26)) + 'px "JetBrains Mono", ui-monospace, monospace';
    for (c = 0; c < COLS; c++) ctx.fillText(((this.startHour + c) % 24) + 'h', mL + c * s + s / 2, mT - 12);

    // ----- ô đã khóa -----
    var board = game.board, self = this;
    function cellAt(rr2, cc) { return (rr2 >= 0 && rr2 < ROWS && cc >= 0 && cc < COLS) ? board[rr2][cc] : null; }
    for (r = 0; r < ROWS; r++) {
      for (c = 0; c < COLS; c++) {
        var cell = board[r][c]; if (!cell) continue;
        var t = cell.t;
        var nb = neighbors(cellAt, r, c, function (v) { return v.t === t; });
        var late = t.dl != null && t.kind === 'work' && game.dayOfRow(r) > t.dl;
        drawTile(ctx, mL + c * s, mT + r * s, s, t, nb, { energy: lv.energy, label: lv.labels && !nb.u && !nb.l, late: late && lv.deadlines });
      }
    }

    // ----- vạch hạn chót -----
    if (lv.deadlines) {
      var marks = game.deadlineMarks(), curT = game.piece && game.piece.task;
      marks.forEach(function (m) {
        if (m.row < 0 || m.row > ROWS - 1) return;
        var t = m.task, col = t.q ? DT.Q[t.q].color : DT.CATS[t.cat].color;
        var y = mT + m.row * s, isCur = t === curT;
        ctx.save();
        ctx.strokeStyle = col; ctx.globalAlpha = isCur ? 1 : 0.6; ctx.lineWidth = isCur ? 2 : 1.4;
        ctx.setLineDash(isCur ? [] : [6, 4]);
        ctx.beginPath(); ctx.moveTo(mL, y); ctx.lineTo(mL + COLS * s, y); ctx.stroke();
        ctx.setLineDash([]); ctx.globalAlpha = 1; ctx.fillStyle = col;
        ctx.beginPath(); ctx.moveTo(mL + COLS * s + 2, y); ctx.lineTo(mL + COLS * s + 13, y - 6); ctx.lineTo(mL + COLS * s + 13, y + 6); ctx.closePath(); ctx.fill();
        ctx.restore();
      });
    }

    // ----- khối "ma" và khối đang rơi -----
    var p = game.piece;
    if (p && game.status === 'playing') {
      var gr = game.ghostRow(), t2 = p.task, i;
      var lateGhost = t2.dl != null && p.cells.some(function (q) { return game.dayOfRow(gr + q[0]) > t2.dl; });
      var gc = lateGhost && lv.deadlines ? C.danger : (DT.CATS[t2.cat] || DT.CATS.study).light;
      ctx.save(); ctx.strokeStyle = gc; ctx.lineWidth = 2; ctx.globalAlpha = 0.85; ctx.setLineDash([5, 3]);
      for (i = 0; i < p.cells.length; i++) {
        ctx.strokeRect(mL + (p.c + p.cells[i][1]) * s + 3, mT + (gr + p.cells[i][0]) * s + 3, s - 6, s - 6);
      }
      ctx.restore();
      var set = {}; p.cells.forEach(function (q) { set[q[0] + ',' + q[1]] = 1; });
      var pc = function (a, b) { return set[a + ',' + b] ? { t: t2 } : null; };
      var pulse = 0.5 + 0.5 * Math.sin(now / 160);
      for (i = 0; i < p.cells.length; i++) {
        var q = p.cells[i], nb2 = neighbors(pc, q[0], q[1], function () { return true; });
        var cr = p.r + q[0];
        drawTile(ctx, mL + (p.c + q[1]) * s, mT + cr * s, s, t2, nb2, { energy: lv.energy, label: lv.labels && i === 0 });
      }
      // viền sáng quanh khối đang rơi
      ctx.save(); ctx.strokeStyle = 'rgba(255,255,255,' + (0.25 + 0.2 * pulse) + ')'; ctx.lineWidth = 1.5;
      for (i = 0; i < p.cells.length; i++) {
        var q2 = p.cells[i];
        ctx.strokeRect(mL + (p.c + q2[1]) * s + 0.5, mT + (p.r + q2[0]) * s + 0.5, s - 1, s - 1);
      }
      ctx.restore();
    }

    // ----- đồng hồ ngày -----
    if (lv.dayLen) {
      var frac = Math.min(1, game.dayTimer / (game.dayLength() * 1000));
      ctx.fillStyle = 'rgba(150,165,225,0.14)'; ctx.fillRect(mL, mT + ROWS * s + 4, COLS * s, 4);
      ctx.fillStyle = frac > 0.8 ? C.danger : C.labelStrong; ctx.fillRect(mL, mT + ROWS * s + 4, COLS * s * frac, 4);
    }

    // ----- mệt mỏi: viền đỏ -----
    if (game.energyLow() && game.status === 'playing') {
      var a = 0.12 + 0.1 * Math.sin(now / 220);
      var vg = ctx.createRadialGradient(mL + COLS * s / 2, mT + ROWS * s / 2, s * 3, mL + COLS * s / 2, mT + ROWS * s / 2, s * 9);
      vg.addColorStop(0, 'rgba(255,60,60,0)'); vg.addColorStop(1, 'rgba(255,60,60,' + a + ')');
      ctx.fillStyle = vg; ctx.fillRect(mL, mT, COLS * s, ROWS * s);
    }
    if (game.time - game.stuckAt < 500) {
      ctx.fillStyle = 'rgba(255,209,102,0.95)'; ctx.font = '700 ' + Math.round(s * 0.5) + 'px "Bricolage Grotesque", system-ui, sans-serif';
      ctx.textAlign = 'center'; ctx.fillText('Mất tập trung!', mL + COLS * s / 2, mT + s * 1.5);
    }
  };

  /* ----- khối xem trước / hoãn ----- */
  function drawMini(canvas, desc, opts) {
    opts = opts || {};
    var ctx = canvas.getContext('2d'), dpr = Math.min(2.5, g.devicePixelRatio || 1);
    var cs = opts.cell || 18, W = opts.w || 4, H = opts.h || 3;
    canvas.width = W * cs * dpr; canvas.height = H * cs * dpr;
    canvas.style.width = W * cs + 'px'; canvas.style.height = H * cs + 'px';
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, W * cs, H * cs);
    if (!desc) return;
    var cells = desc.cells, d = DT.util.dims(cells);
    var sc = Math.min(cs, cs * W / Math.max(d.w, W) , cs * H / Math.max(d.h, H));
    if (d.w > W || d.h > H) sc = Math.min(cs * W / d.w, cs * H / d.h);
    var ox = (W * cs - d.w * sc) / 2, oy = (H * cs - d.h * sc) / 2, set = {};
    cells.forEach(function (q) { set[q[0] + ',' + q[1]] = 1; });
    var f = function (a, b) { return set[a + ',' + b] ? { t: desc.task } : null; };
    cells.forEach(function (q) {
      var nb = neighbors(f, q[0], q[1], function () { return true; });
      drawTile(ctx, ox + q[1] * sc, oy + q[0] * sc, sc, desc.task, nb, { energy: false, label: false });
    });
  }

  DT.Renderer = Renderer;
  DT.drawMini = drawMini;
  DT.drawTile = drawTile;
})(typeof window !== 'undefined' ? window : globalThis);
