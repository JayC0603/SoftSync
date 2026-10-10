import Link from "next/link";

const features = [
  ["Lộ trình học", "Theo dõi kỹ năng và tiến độ trong một nơi.", "/courses"],
  ["Khóa học", "Học theo bài, lưu tiến độ và luyện tập.", "/courses"],
  ["CV thông minh", "Lưu trữ, so khớp, xếp hạng, so sánh và tìm kiếm CV.", "/cv"],
] as const;

export default function HomePage() {
  return (
    <main className="shell">
      <nav className="nav">
        <strong>SoftSync</strong>
        <a href="#features">Tính năng</a>
        <Link className="primary" href="/cv">Phân tích CV</Link>
      </nav>
      <section className="hero">
        <p className="eyebrow">LEARN · BUILD · SHARE</p>
        <h1>Học tập rõ ràng hơn.<br /><span>Tiến bộ mỗi ngày.</span></h1>
        <p className="intro">Nền tảng học tập, phát triển kỹ năng và hỗ trợ nghề nghiệp cho cộng đồng SoftSync.</p>
        <div className="actions">
          <Link className="primary" href="/cv">Bắt đầu với CV</Link>
          <a href="#features">Khám phá tính năng →</a>
        </div>
      </section>
      <section id="features" className="features">
        {features.map(([title, text, href]) => (
          <Link className="feature-link" href={href} key={title}>
            <article>
              <div className="icon">✦</div>
              <h2>{title}</h2>
              <p>{text}</p>
              <span className="feature-cta">Mở tính năng →</span>
            </article>
          </Link>
        ))}
      </section>
    </main>
  );
}
