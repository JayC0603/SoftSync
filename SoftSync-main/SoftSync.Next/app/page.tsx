const features = [
  ["Lộ trình học", "Theo dõi kỹ năng và tiến độ trong một nơi."],
  ["Khóa học", "Học theo bài, lưu tiến độ và luyện tập."],
  ["Cộng đồng", "Chia sẻ kiến thức và kết nối người học."],
];

export default function HomePage() {
  return (
    <main className="shell">
      <nav className="nav"><strong>SoftSync</strong><a href="#features">Tính năng</a><button>Đăng nhập</button></nav>
      <section className="hero"><p className="eyebrow">LEARN · BUILD · SHARE</p><h1>Học tập rõ ràng hơn.<br /><span>Tiến bộ mỗi ngày.</span></h1><p className="intro">Nền tảng học tập và phát triển kỹ năng cho cộng đồng SoftSync.</p><div className="actions"><button className="primary">Bắt đầu học</button><a href="#features">Khám phá tính năng →</a></div></section>
      <section id="features" className="features">{features.map(([title, text]) => <article key={title}><div className="icon">✦</div><h2>{title}</h2><p>{text}</p></article>)}</section>
    </main>
  );
}
