/* Mô phỏng bot chơi Day Tetris bằng Node: node tests/sim.js */
require('../js/config.js'); require('../js/game.js');
const DT = globalThis.DT, U = DT.util;

function evalBoard(board, cells, r, c) {
  const b = board.map(row => row.map(x => !!x));
  cells.forEach(p => { b[r + p[0]][c + p[1]] = true; });
  let lines = 0;
  for (let i = 0; i < DT.ROWS; i++) if (b[i].every(Boolean)) lines++;
  let agg = 0, holes = 0, bump = 0, prev = null;
  for (let j = 0; j < DT.COLS; j++) {
    let h = 0, seen = false;
    for (let i = 0; i < DT.ROWS; i++) {
      if (b[i][j]) { if (!seen) { h = DT.ROWS - i; seen = true; } } else if (seen) holes++;
    }
    agg += h; if (prev !== null) bump += Math.abs(h - prev); prev = h;
  }
  return lines * 9 - holes * 6 - agg * 0.5 - bump * 0.35;
}
function botMove(g) {
  const p = g.piece; let best = null, cells = p.cells;
  for (let rot = 0; rot < 4; rot++) {
    const d = U.dims(cells);
    for (let c = 0; c + d.w <= DT.COLS; c++) {
      if (!g.fits(cells, 0, c)) continue;
      let r = 0; while (g.fits(cells, r + 1, c)) r++;
      let s = evalBoard(g.board, cells, r, c);
      // ưu tiên Q1 xuống thấp
      if (p.task.q === 'Q1') s += r * 0.6;
      if (!best || s > best.s) best = { s, cells, c, r };
    }
    cells = U.rotCW(cells);
  }
  if (best) { p.cells = best.cells; p.c = best.c; p.r = best.r; g.lockPiece(); }
  else g.lockPiece();
}
function play(mode, seed, think, opts) {
  const g = new DT.Game(mode, Object.assign({ seed, autoChoice: true }, opts || {}));
  let guard = 0;
  while (g.status === 'playing' && guard++ < 400000) {
    g.update(100);
    if (g.status !== 'playing') break;
    if (g.piece && g.pieceAge >= think) {
      // dùng vật phẩm đơn giản
      if (g.powerups.pomodoro && g.piece.task.e === 'heavy' && g.piece.cells.length > 1) g.usePowerup('pomodoro');
      if (g.mode.rest && g.energy < 35 && g.insertsLeft > 0 && g.piece.task.kind !== 'rest') g.insertBlock('rest');
      if (g.mode.powerups && g.powerups.dayoff && g.energy < 40) g.usePowerup('dayoff');
      if (g.piece) botMove(g);
    }
  }
  return g;
}
const modes = DT.LEVELS.concat([DT.FREE_MODE]);
const think = +process.argv[2] || 2500;
const N = +process.argv[3] || 10;
modes.forEach(m => {
  const res = { won: 0, lost: {}, score: 0, rep: 0, late: 0, rows: 0, days: 0 };
  for (let s = 1; s <= N; s++) {
    let g;
    try { g = play(m, s * 7919, think); } catch (e) { console.log('CRASH', m.name, 'seed', s, e.stack); process.exit(1); }
    if (g.status === 'won') res.won++; else res.lost[g.endReason] = (res.lost[g.endReason] || 0) + 1;
    res.score += g.score; res.rep += g.rep; res.late += g.stats.late.length; res.rows += g.stats.rowsTotal; res.days += g.day;
  }
  console.log(m.name.padEnd(28), 'thắng', res.won + '/' + N, 'thua', JSON.stringify(res.lost),
    'điểm TB', Math.round(res.score / N), 'uy tín TB', Math.round(res.rep / N), 'trễ TB', (res.late / N).toFixed(1), 'hàng TB', (res.rows / N).toFixed(1), 'ngày TB', (res.days / N).toFixed(1));
});
// custom
const tasks = DT.SAMPLE_TASKS;
const gc = play(DT.CUSTOM_MODE, 5, 800, { customTasks: tasks });
console.log('custom:', gc.status, gc.endReason, 'score', gc.score, 'late', gc.stats.late);
