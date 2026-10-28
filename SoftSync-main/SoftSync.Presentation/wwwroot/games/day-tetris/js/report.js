/* Day Tetris - báo cáo cuối ván (phần "học") */
(function (g) {
  'use strict';
  var DT = g.DT;

  function pct(a, b) { return b ? Math.round(a / b * 100) : 0; }

  DT.buildReport = function (game) {
    var s = game.stats, lv = game.mode, ins = [], i;
    var win = game.status === 'won';
    var reasons = {
      overflow: 'Backlog tràn: công việc chất đến tận đỉnh.',
      rep: 'Uy tín về 0: bạn trượt môn.',
      'rep-low': 'Hết học kỳ nhưng uy tín chưa đủ để qua môn.',
      burnout: 'Burnout: bạn kiệt sức hai lần.'
    };
    var title = win
      ? (lv.custom ? 'Đã xếp xong lịch của bạn' : 'Qua học kỳ!')
      : (reasons[game.endReason] || 'Kết thúc ván');

    var workCells = 0;
    DT.WORK_CATS.forEach(function (c) { workCells += s.catCells[c]; });

    // ---- Trễ hạn ----
    if (s.late.length) {
      var names = s.late.slice(0, 3).map(function (n) { return '«' + n + '»'; }).join(', ');
      ins.push({ kind: 'bad', text: 'Bạn trễ ' + s.late.length + ' hạn chót (' + names + (s.late.length > 3 ? '…' : '') +
        '). Việc có hạn gần nên xếp xuống thấp (ngày gần) trước; việc lớn nên chia nhỏ thay vì dồn vào phút chót.' });
    } else if (lv.deadlines && s.tasksDone >= 3) {
      ins.push({ kind: 'good', text: 'Bạn không trễ hạn nào. Xếp việc có hạn gần xuống các ngày đầu là thói quen tốt.' });
    }
    // ---- Trì hoãn ----
    if (s.bloated > 0) {
      var heldN = s.q2Held || 1;
      ins.push({ kind: 'bad', text: 'Bạn hoãn ' + heldN + ' khối Q2, đến hạn chúng phình to và mất ' + s.bloatLoss +
        ' điểm. Thử xếp việc quan trọng sớm hơn, trước khi nó biến thành việc gấp.' });
    } else if (lv.hold && s.holds === 0) {
      ins.push({ kind: 'tip', text: 'Bạn chưa dùng Hoãn (C). Hoãn chỉ nên là lựa chọn hiếm hoi: khối bị hoãn từ 2 ngày sẽ phình to.' });
    }
    // ---- Chất lượng ngày ----
    if (lv.energy && s.rowsTotal >= 3) {
      var po = pct(s.rows.overload, s.rowsTotal), pb = pct(s.rows.balanced, s.rowsTotal);
      if (po >= 40) ins.push({ kind: 'bad', text: po + '% hàng của bạn là quá tải. Hãy thêm ô nghỉ: xếp kín không có nghĩa là xếp tốt.' });
      else if (pb >= 50) ins.push({ kind: 'good', text: pb + '% số ngày của bạn là ngày cân bằng, có chỗ nghỉ và không dồn quá nhiều việc nặng.' });
      if (s.rows.allnighter > 0) ins.push({ kind: 'warn', text: 'Có ' + s.rows.allnighter + ' ngày cày đêm (≥ 4 giờ nặng liền nhau). Những ngày sau đó bạn bị mệt mỏi: hồi năng lượng chậm lại.' });
    }
    // ---- Chia nhỏ ----
    if (s.verticalI >= 1) {
      ins.push({ kind: 'good', text: 'Bạn chia nhỏ dự án thành các buổi đều đặn (' + s.verticalI + ' lần xếp dọc), rất tốt. Học đều mỗi ngày 1 giờ hiệu quả hơn cày một mạch.' });
    } else if (lv.id >= 1 && s.piecesLocked >= 8) {
      ins.push({ kind: 'tip', text: 'Thử xoay khối I thành thẳng đứng: học 1 giờ mỗi ngày trong 4 ngày thay vì dồn 4 giờ vào một ngày (+50 điểm).' });
    }
    if (s.q2Early >= 1) {
      ins.push({ kind: 'good', text: 'Bạn xong ' + s.q2Early + ' việc quan trọng (Q2) trước hạn từ 2 ngày. Đây là cách phòng việc gấp tốt nhất.' });
    }
    if (s.q3Prime >= 2) {
      ins.push({ kind: 'warn', text: 'Bạn xếp việc Q3 (gấp nhưng ít quan trọng) trước việc Q2 tới ' + s.q3Prime + ' lần. Việc gấp dễ cuốn bạn đi, hãy hỏi mình: việc này có thật sự quan trọng?' });
    }
    // ---- Cân bằng cuộc sống ----
    if (lv.energy && workCells >= 20) {
      var st = pct(s.catCells.study, workCells), he = pct(s.catCells.health, workCells), so = pct(s.catCells.social, workCells);
      if (st > 65) ins.push({ kind: 'warn', text: 'Học chiếm ' + st + '% thời gian của bạn. Sức khỏe chỉ ' + he + '% và xã hội ' + so + '%, cân bằng cuộc sống bị tụt.' });
      else if (he < 8) ins.push({ kind: 'warn', text: 'Sức khỏe chỉ chiếm ' + he + '% lịch của bạn. Cơ thể mệt thì học cũng chậm.' });
      else if (so < 6) ins.push({ kind: 'warn', text: 'Xã hội chỉ chiếm ' + so + '% lịch. Gặp bạn bè cũng là một phần của lịch tốt.' });
    }
    // ---- Sự kiện ----
    if (lv.events) {
      if (s.absorbed > 0) ins.push({ kind: 'good', text: 'Ô đệm đã đỡ giúp bạn ' + s.absorbed + ' lần việc phát sinh. Chừa đệm là cách giữ lịch không bị vỡ.' });
      if (s.emergencyHit + s.sickHit > 0 && s.bufferPlaced === 0) ins.push({ kind: 'tip', text: 'Việc phát sinh rơi thẳng vào lịch kín vì bạn không chừa ô đệm (phím B). Mỗi ngày nên chừa 1 ô trống.' });
      if (s.friendNo >= 2) ins.push({ kind: 'warn', text: 'Bạn từ chối ' + s.friendNo + ' lời rủ đi chơi. Giữ lịch là tốt, nhưng quan hệ cũng cần thời gian.' });
    }
    // ---- Năng lượng ----
    if (s.burnouts > 0) ins.push({ kind: 'bad', text: 'Bạn kiệt sức ' + s.burnouts + ' lần. Hãy chèn ô nghỉ (phím R) trước khi năng lượng xuống thấp.' });
    if (s.pomodoro > 0) ins.push({ kind: 'good', text: 'Bạn dùng Pomodoro ' + s.pomodoro + ' lần: chia việc nặng thành các phiên nhẹ giúp đỡ mệt hơn.' });
    // ---- Giờ trống ----
    if (s.committedCells >= 30 && pct(s.emptyCells, s.committedCells) >= 45) {
      ins.push({ kind: 'tip', text: 'Bạn bỏ trống ' + pct(s.emptyCells, s.committedCells) + '% số giờ khả dụng. Khối rơi nhanh hơn một chút sẽ giúp bạn tận dụng ngày tốt hơn.' });
    }

    // ---- Lịch của bạn (chế độ tùy chỉnh) ----
    var plan = null;
    if (lv.custom) {
      plan = (game.planLog || []).map(function (p) {
        return { label: 'Ngày ' + p.day + ' (' + DT.dayName(p.day) + ')', filled: p.filled, heavy: p.heavy, rest: p.rest, tasks: p.tasks, cleared: true };
      });
      for (var r = DT.ROWS - 1; r >= 0; r--) {
        var filled = 0, heavy = 0, rest = 0, tasks = [], run = 0, maxRun = 0;
        game.board[r].forEach(function (cell) {
          if (!cell) { run = 0; return; }
          filled++;
          if (cell.t.kind === 'work') {
            if (tasks.indexOf(cell.t.name) < 0) tasks.push(cell.t.name);
            if (cell.t.e === 'heavy') { heavy++; run++; if (run > maxRun) maxRun = run; } else run = 0;
          } else run = 0;
          if (cell.t.kind === 'rest') rest++;
        });
        if (filled) plan.push({ label: 'Ngày ' + game.dayOfRow(r) + ' (' + DT.dayName(game.dayOfRow(r)) + ')', filled: filled, heavy: heavy, rest: rest, tasks: tasks, run: maxRun });
      }
      plan.forEach(function (d) {
        if (d.filled >= 8 && d.rest === 0) ins.push({ kind: 'warn', text: d.label + ': ' + d.filled + '/10 giờ mà không có ô nghỉ. Hãy thêm giờ nghỉ hoặc dời bớt việc sang ngày khác.' });
        if ((d.run || 0) >= 4 || d.heavy >= 6) ins.push({ kind: 'warn', text: d.label + ' dồn ' + d.heavy + ' giờ nặng. Thử chia việc nặng ra nhiều ngày.' });
      });
      if (game.hold) ins.push({ kind: 'warn', text: 'Việc «' + game.hold.task.name + '» vẫn đang bị hoãn, chưa có chỗ trong lịch.' });
      if (!s.late.length && !ins.some(function (x) { return x.kind === 'warn' || x.kind === 'bad'; }))
        ins.push({ kind: 'good', text: 'Lịch của bạn kịp mọi hạn chót và không có ngày nào quá tải. Hãy thử làm đúng như vậy với tuần thật!' });
    }

    if (ins.length < 2) ins.push({ kind: 'tip', text: 'Mỗi lượt chơi là một thí nghiệm nhỏ: thử một cách xếp khác và xem điểm thay đổi ra sao.' });

    // sắp xếp: xấu trước, tốt sau
    var order = { bad: 0, warn: 1, tip: 2, good: 3 };
    ins.sort(function (a, b) { return order[a.kind] - order[b.kind]; });

    var summary = [
      { label: 'Điểm', value: game.score },
      { label: 'Ngày đã qua', value: Math.min(game.day - 1, lv.days || 99999) + (lv.days ? '/' + lv.days : '') },
      { label: 'Việc hoàn thành', value: s.tasksDone },
      { label: 'Trễ hạn', value: s.late.length }
    ];
    if (lv.energy) summary.push({ label: 'Ngày cân bằng', value: s.rows.balanced + '/' + s.rowsTotal });
    summary.push({ label: 'Uy tín (GPA)', value: (game.rep / 25).toFixed(1) });

    return {
      win: win, title: title, insights: ins, summary: summary, plan: plan,
      exercise: 'Bài tập nhỏ: hãy mở lịch tuần này của bạn và thử xếp lại theo cách vừa chơi. Việc quan trọng nhưng chưa gấp đặt trước, việc lớn chia thành các buổi nhỏ, và chừa ít nhất một ô nghỉ mỗi ngày.'
    };
  };
})(typeof window !== 'undefined' ? window : globalThis);
