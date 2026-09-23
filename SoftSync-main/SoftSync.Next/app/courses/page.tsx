import Link from "next/link";
import { prisma } from "@/lib/prisma";

export const dynamic = "force-dynamic";

export default async function CoursesPage() {
  let courses: { id: number; title: string; description: string; status: string }[] = [];
  let error = "";

  try {
    // The existing ASP.NET/EF database uses quoted PascalCase table and column
    // names. Read it directly while the full Prisma schema is being mapped.
    courses = await prisma.$queryRaw<typeof courses>`
      SELECT "Id" AS id, "Title" AS title, "Description" AS description,
             "Status"::text AS status
      FROM "Courses"
      WHERE "Status" = 1
      ORDER BY "CreatedAtUtc" DESC
    `;
  } catch {
    error = "Đã kết nối database nhưng chưa đọc được bảng Courses cũ. Kiểm tra log Vercel để map schema EF Core.";
  }

  return (
    <main className="shell">
      <nav className="nav"><Link href="/" className="brand">SoftSync</Link><Link href="/">Trang chủ</Link></nav>
      <section className="page-heading"><p className="eyebrow">SOFTSYNC LEARNING</p><h1>Khóa học</h1><p className="intro">Chọn khóa học phù hợp để bắt đầu lộ trình phát triển kỹ năng.</p></section>
      {error ? <div className="notice">{error}</div> : courses.length === 0 ? <div className="empty">Chưa có khóa học được xuất bản.</div> : <section className="course-grid">{courses.map((course) => <article className="course-card" key={course.id}><p className="course-status">{course.status}</p><h2>{course.title}</h2><p>{course.description || "Khóa học phát triển kỹ năng cùng SoftSync."}</p><button className="primary">Xem khóa học</button></article>)}</section>}
    </main>
  );
}
