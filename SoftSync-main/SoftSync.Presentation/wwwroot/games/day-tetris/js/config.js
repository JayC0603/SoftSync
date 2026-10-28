/* Day Tetris - cấu hình, hằng số, dữ liệu tĩnh */
(function (g) {
  'use strict';
  var DT = (g.DT = g.DT || {});

  DT.COLS = 10;
  DT.ROWS = 14;

  /* ---------- Ngày trong tuần (ngày 1 = Thứ Hai) ---------- */
  DT.WEEKDAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  DT.dayName = function (absDay) {
    return DT.WEEKDAYS[(((absDay - 1) % 7) + 7) % 7];
  };
  DT.isWeekend = function (absDay) {
    var w = (((absDay - 1) % 7) + 7) % 7;
    return w >= 5;
  };

  /* ---------- Loại việc ---------- */
  DT.CATS = {
    study:    { name: 'Học',      color: '#4f8cff', light: '#8fb6ff', dark: '#2a5bc0' },
    personal: { name: 'Cá nhân',  color: '#a77bff', light: '#c9b0ff', dark: '#6b45c7' },
    health:   { name: 'Sức khỏe', color: '#2fcf8a', light: '#7ae8b9', dark: '#17935f' },
    social:   { name: 'Xã hội',   color: '#ff6fa5', light: '#ff9fc3', dark: '#c73b74' },
    rest:     { name: 'Nghỉ',     color: '#9fe3ff', light: '#d2f3ff', dark: '#5bb4d6' },
    buffer:   { name: 'Đệm',      color: '#8a93a6', light: '#aab2c4', dark: '#5d6578' },
    busy:     { name: 'Bận',      color: '#3a4060', light: '#4a5176', dark: '#262b45' },
    sick:     { name: 'Ốm',       color: '#8a3544', light: '#b04c5d', dark: '#5c1f2b' }
  };
  DT.WORK_CATS = ['study', 'personal', 'health', 'social'];
  DT.NON_WORK = { rest: 1, buffer: 1, busy: 1, sick: 1 };

  /* ---------- Ma trận Eisenhower ---------- */
  DT.Q = {
    Q1: { name: 'Gấp & quan trọng',      color: '#ff5d5d' },
    Q2: { name: 'Quan trọng, chưa gấp',  color: '#ffcf4a' },
    Q3: { name: 'Gấp, ít quan trọng',    color: '#ff9a3c' },
    Q4: { name: 'Không gấp, không quan trọng', color: '#9aa3b5' }
  };
  DT.Q_WEIGHTS = { Q1: 25, Q2: 40, Q3: 20, Q4: 15 };
  // Hạn chót (số ngày kể từ lúc khối xuất hiện)
  DT.DEADLINE_RANGE = { Q1: [3, 6], Q2: [6, 12], Q3: [2, 5], Q4: null };

  /* ---------- Năng lượng ---------- */
  DT.ENERGY = {
    light: { name: 'Nhẹ',  cost: 0 },
    mid:   { name: 'Vừa',  cost: 0.5 },
    heavy: { name: 'Nặng', cost: 1.5 }
  };
  DT.ENERGY_BY_CAT = {
    study:    { light: 15, mid: 45, heavy: 40 },
    personal: { light: 50, mid: 40, heavy: 10 },
    health:   { light: 30, mid: 50, heavy: 20 },
    social:   { light: 60, mid: 35, heavy: 5 }
  };
  DT.ENERGY_START = 100;
  DT.ENERGY_REST_GAIN = 3;        // mỗi ô nghỉ
  DT.ENERGY_HEAVY_ADJ = 1.2;      // phạt khối nặng nằm sát khối nặng khác
  DT.ENERGY_DAILY_REGEN = 8;
  DT.ENERGY_LOW = 30;             // dưới mức này -> mất tập trung
  DT.BURNOUT_RESET = 35;

  /* ---------- Hình khối ---------- */
  DT.SHAPES = {
    I:  [[0, 0], [0, 1], [0, 2], [0, 3]],
    O:  [[0, 0], [0, 1], [1, 0], [1, 1]],
    T:  [[0, 1], [1, 0], [1, 1], [1, 2]],
    L:  [[0, 2], [1, 0], [1, 1], [1, 2]],
    J:  [[0, 0], [1, 0], [1, 1], [1, 2]],
    S:  [[0, 1], [0, 2], [1, 0], [1, 1]],
    Z:  [[0, 0], [0, 1], [1, 1], [1, 2]],
    I3: [[0, 0], [0, 1], [0, 2]],
    L3: [[0, 0], [1, 0], [1, 1]],
    D2: [[0, 0], [0, 1]],
    M1: [[0, 0]]
  };
  DT.SHAPE_LABELS = {
    I:  'I · Dự án dài / ôn thi (4 ô thẳng)',
    O:  'O · Bài tập nhóm / họp (2×2)',
    T:  'T · Việc có phần phụ thuộc',
    L:  'L · Việc nhà / đi lại / tập luyện',
    J:  'J · Việc nhà / đi lại / tập luyện',
    S:  'S · Việc lệch lịch',
    Z:  'Z · Việc lệch lịch',
    I3: 'I3 · Việc 3 giờ (thẳng)',
    L3: 'L3 · Việc 3 giờ (góc)',
    D2: 'D2 · Việc 2 giờ',
    M1: 'M1 · Việc 1 giờ'
  };
  // Trọng số loại việc theo hình khối
  DT.SHAPE_CATS = {
    I: { study: 6, personal: 1, health: 1 },
    O: { study: 4, social: 3, personal: 1 },
    T: { study: 5, personal: 2, health: 1, social: 1 },
    L: { personal: 4, health: 3, study: 1, social: 1 },
    J: { personal: 4, health: 3, study: 1, social: 1 },
    S: { social: 3, personal: 3, study: 2, health: 1 },
    Z: { social: 3, personal: 3, study: 2, health: 1 }
  };

  /* ---------- Tên việc ---------- */
  DT.NAMES = {
    study:    ['Ôn thi Giải tích', 'Đồ án Lập trình', 'Bài tập Vật lý', 'Đọc tài liệu Kinh tế', 'Làm slide thuyết trình', 'Luyện đề Tiếng Anh', 'Báo cáo thí nghiệm', 'Học từ vựng'],
    personal: ['Dọn phòng', 'Giặt giũ & nấu ăn', 'Đi chợ', 'Làm giấy tờ', 'Di chuyển / đi lại', 'Tập đàn guitar', 'Đọc sách'],
    health:   ['Chạy bộ', 'Tập gym', 'Đạp xe', 'Yoga', 'Bơi lội', 'Đi bộ buổi tối'],
    social:   ['Họp nhóm', 'Cà phê với bạn', 'Sinh hoạt CLB', 'Gọi điện về nhà', 'Sinh nhật bạn', 'Tình nguyện']
  };
  DT.SHAPE_NAMES = {
    I: { study: ['Ôn thi cuối kỳ', 'Đồ án cuối kỳ', 'Đọc giáo trình', 'Luyện đề Tiếng Anh'] },
    O: { study: ['Bài tập nhóm', 'Họp nhóm đồ án', 'Buổi thực hành'], social: ['Họp CLB', 'Buổi gặp nhóm'] },
    T: { study: ['Học bài → làm bài tập', 'Đọc tài liệu → viết tóm tắt'] }
  };
  DT.REST_NAMES = ['Ngủ đủ giấc', 'Ăn trưa thong thả', 'Nghỉ ngơi', 'Đi dạo thư giãn', 'Xem phim'];
  DT.EMERGENCY_NAMES = ['Sếp nhờ gấp', 'Sửa lỗi nộp ngay', 'Báo cáo đột xuất', 'Kiểm tra bất ngờ'];
  DT.REST_CHANCE = 0.13;

  /* ---------- Điểm số ---------- */
  DT.SCORE = {
    cellBase: 3,
    q2Early: 150,
    q2Ok: 80,
    q1Ok: 100,
    q3Ok: 40,
    q4Ok: 20,
    late: -150,
    lateRep: -12,
    onTimeRep: 2,
    q2EarlyRep: 3,
    q3Prime: -30,
    splitI: 50,
    rowPlain: 100,
    rowBalanced: 200,
    rowOverload: 50,
    rowIdle: 40,
    rowAllNighter: 80,
    combo: 100,
    dayCell: 5,
    emptyDay: -20,
    absorb: 30,
    friend: 40,
    bloatPerCell: 50
  };
  DT.REP_START = 100;
  DT.REP_MIN_WIN = 40;
  DT.LOCK_DELAY = 420;
  DT.MAX_BLOAT_CELLS = 8;
  DT.INSERTS_PER_DAY = 2;
  DT.LIST_PIECES = 15;
  // Khoảng lý tưởng (%) cho mỗi loại việc trong 40 ô gần nhất
  DT.BALANCE_TARGET = {
    study: [30, 60],
    personal: [10, 30],
    health: [10, 30],
    social: [8, 28]
  };

  /* ---------- Vật phẩm ---------- */
  DT.POWERUPS = {
    pomodoro: { name: 'Pomodoro',       key: '1', desc: 'Khối nặng đang rơi → chia thành các phiên nhẹ.' },
    list:     { name: 'Lên danh sách',  key: '2', desc: 'Xem trước 5 khối trong 15 khối tới.' },
    delegate: { name: 'Ủy quyền',       key: '3', desc: 'Bỏ khối Q3 đang rơi, hoặc xóa 1 việc Q3 trên bảng.' },
    dayoff:   { name: 'Ngày nghỉ thật', key: '4', desc: 'Xóa sạch hàng hôm nay, hồi đầy năng lượng.' }
  };

  /* ---------- Cấp độ (học kỳ) ---------- */
  var ALL = ['I', 'O', 'T', 'L', 'J', 'S', 'Z'];
  DT.LEVELS = [
    { id: 1, name: 'Học kỳ 1 · Làm quen', desc: 'Chỉ khối I, O, T. Xếp kín các ngày.',
      days: 8, shapes: ['I', 'O', 'T'], labels: false, deadlines: false, energy: false, rest: false,
      events: false, hold: false, powerups: false, busy: false, clusters: false, dayLen: 30, gravity: 800 },
    { id: 2, name: 'Học kỳ 2 · Hạn chót', desc: 'Thêm hạn chót và nhãn Eisenhower (Q1–Q4).',
      days: 10, shapes: ['I', 'O', 'T', 'L', 'J'], labels: true, deadlines: true, energy: false, rest: false,
      events: false, hold: false, powerups: false, busy: false, clusters: false, dayLen: 28, gravity: 720 },
    { id: 3, name: 'Học kỳ 3 · Năng lượng', desc: 'Thêm năng lượng, ô nghỉ và vật phẩm.',
      days: 12, shapes: ALL, labels: true, deadlines: true, energy: true, rest: true,
      events: false, hold: false, powerups: true, busy: false, clusters: false, dayLen: 26, gravity: 650 },
    { id: 4, name: 'Học kỳ 4 · Bất ngờ', desc: 'Thêm sự kiện ngẫu nhiên và Hoãn (trì hoãn có giá).',
      days: 14, shapes: ALL, labels: true, deadlines: true, energy: true, rest: true,
      events: true, hold: true, powerups: true, busy: false, clusters: false, dayLen: 24, gravity: 580 },
    { id: 5, name: 'Học kỳ 5 · Nhiều môn', desc: 'Nhiều dự án chồng hạn, lịch bận mỗi ngày khác nhau.',
      days: 18, shapes: ALL, labels: true, deadlines: true, energy: true, rest: true,
      events: true, hold: true, powerups: true, busy: true, clusters: true, dayLen: 22, gravity: 520 }
  ];
  DT.FREE_MODE = {
    id: 0, name: 'Tự do', desc: 'Chơi không giới hạn, tốc độ tăng dần.',
    days: null, shapes: ALL, labels: true, deadlines: true, energy: true, rest: true,
    events: true, hold: true, powerups: true, busy: false, clusters: true, dayLen: 24, gravity: 700, speedUp: true
  };
  DT.CUSTOM_MODE = {
    id: -1, name: 'Tùy chỉnh', desc: 'Nhập việc thật của bạn thành khối để tập xếp lịch.',
    days: null, shapes: ALL, labels: true, deadlines: true, energy: true, rest: true,
    events: false, hold: true, powerups: false, busy: false, clusters: false, dayLen: 0, gravity: 900, custom: true, soft: true
  };

  /* ---------- Việc mẫu cho chế độ tùy chỉnh ---------- */
  DT.SAMPLE_TASKS = [
    { name: 'Ôn thi giữa kỳ Giải tích', shape: 'I',  cat: 'study',    q: 'Q2', e: 'heavy', dl: 6 },
    { name: 'Bài tập nhóm Kinh tế',     shape: 'O',  cat: 'study',    q: 'Q1', e: 'mid',   dl: 3 },
    { name: 'Chạy bộ',                  shape: 'L',  cat: 'health',   q: 'Q2', e: 'mid',   dl: '' },
    { name: 'Dọn phòng, giặt đồ',       shape: 'J',  cat: 'personal', q: 'Q3', e: 'light', dl: 4 },
    { name: 'Làm slide thuyết trình',   shape: 'T',  cat: 'study',    q: 'Q1', e: 'mid',   dl: 4 },
    { name: 'Cà phê với bạn',           shape: 'D2', cat: 'social',   q: 'Q4', e: 'light', dl: '' },
    { name: 'Đọc sách',                 shape: 'I3', cat: 'personal', q: 'Q4', e: 'light', dl: '' }
  ];
})(typeof window !== 'undefined' ? window : globalThis);
