import Link from "next/link";
import { SiteNav } from "../components/site-nav";
import { fetchPublicCourses } from "../../lib/public-courses";
export const dynamic = "force-dynamic";
export default async function CoursesPage() {
    const result = await fetchPublicCourses();
    return <main className="marketing"><div className="shell"><SiteNav /><section className="page-heading"><p className="eyebrow">SOFTSYNC LEARNING</p><h1>Bước tiếp theo của bạn.</h1><p className="intro">Khám phá các khóa học và chọn hướng phát triển kỹ năng phù hợp.</p></section>{result.error ? <div className="notice" role="status"><h2>Chưa thể tải khóa học</h2><p>Kết nối tới máy chủ đang gián đoạn. Bạn có thể thử lại hoặc khám phá công cụ CV.</p><Link href="/courses">Thử lại</Link> · <Link href="/cv">Phân tích CV</Link></div> : result.courses.length === 0 ? <div className="empty"><h2>Khóa học đang được chuẩn bị</h2><p>Chưa có khóa học được xuất bản. Trong lúc chờ, bạn có thể phân tích hồ sơ nghề nghiệp.</p><Link href="/cv">Khám phá CV thông minh →</Link></div> : <section className="course-grid" aria-label="Danh sách khóa học">{result.courses.map(course => <article className="course-card" key={course.id}><p className="course-status">{course.totalLessons} bài học</p><h2>{course.title}</h2><p>{course.description || "Khóa học phát triển kỹ năng cùng SoftSync."}</p><Link className="primary" href={`/courses/${course.id}`}>Xem khóa học →</Link></article>)}</section>}</div></main>;
}
