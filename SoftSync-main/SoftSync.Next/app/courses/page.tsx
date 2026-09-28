import Link from "next/link";

export const dynamic = "force-dynamic";

type PublicCourse = {
  id: number;
  title: string;
  description: string;
  thumbnailUrl: string;
  thumbnailAltText: string;
  totalLessons: number;
};

export default async function CoursesPage() {
  let courses: PublicCourse[] = [];
  let error = "";

  try {
    const backendUrl = process.env.BACKEND_URL;
    if (!backendUrl) throw new Error("BACKEND_URL is not configured");

    const response = await fetch(`${backendUrl.replace(/\/$/, "")}/api/courses`, {
      cache: "no-store",
      headers: { Accept: "application/json" },
    });
    if (!response.ok) throw new Error(`Backend returned ${response.status}`);
    courses = (await response.json()) as PublicCourse[];
  } catch {
    error = "Không thể tải khóa học từ máy chủ. Vui lòng thử lại sau.";
  }

  return (
    <main className="shell">
      <nav className="nav"><Link href="/" className="brand">SoftSync</Link><Link href="/">Trang chủ</Link></nav>
      <section className="page-heading"><p className="eyebrow">SOFTSYNC LEARNING</p><h1>Khóa học</h1><p className="intro">Chọn khóa học phù hợp để bắt đầu lộ trình phát triển kỹ năng.</p></section>
      {error ? <div className="notice">{error}</div> : courses.length === 0 ? <div className="empty">Chưa có khóa học được xuất bản.</div> : <section className="course-grid">{courses.map((course) => <article className="course-card" key={course.id}><p className="course-status">{course.totalLessons} bài học</p><h2>{course.title}</h2><p>{course.description || "Khóa học phát triển kỹ năng cùng SoftSync."}</p><button className="primary">Xem khóa học</button></article>)}</section>}
    </main>
  );
}
