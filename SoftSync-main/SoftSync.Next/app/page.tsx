import Link from "next/link";

const features = [
  ["Lộ trình học", "Theo dõi kỹ năng và tiến độ trong một nơi.", "/courses"],
  ["Khóa học", "Học theo bài, lưu tiến độ và luyện tập.", "/courses"],
  ["Cộng đồng", "Chia sẻ kiến thức và kết nối người học.", "#features"],
] as const;

export default function HomePage() {
  return (
    <main className="shell">
      <nav className="nav"><strong>SoftSync</strong><a href="#features">Tính năng</a><Link className="primary" href="/courses">Đăng nhập</Link></nav>
      <section className="hero"><p className="eyebrow">LEARN · BUILD · SHARE</p><h1>Học tập rõ ràng hơn.<br /><span>Tiến bộ mỗi ngày.</span></h1><p className="intro">Nền tảng học tập và phát triển kỹ năng cho cộng đồng SoftSync.</p><div className="actions"><Link className="primary" href="/courses">Bắt đầu học</Link><a href="#features">Khám phá tính năng →</a></div></section>
      <section id="features" className="features">{features.map(([title, text, href]) => <Link className="feature-link" href={href} key={title}><article><div className="icon">✦</div><h2>{title}</h2><p>{text}</p><span className="feature-cta">Mở tính năng →</span></article></Link>)}</section>
    </main>
  );
}
