import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteNav } from "../../components/site-nav";
import { fetchPublicCourse, learningUrl } from "../../../lib/public-courses";

export const dynamic = "force-dynamic";

export default async function CoursePage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params;
    if (!/^\d+$/.test(id) || !Number.isSafeInteger(Number(id)) || Number(id) < 1) notFound();
    const { course, error } = await fetchPublicCourse(Number(id));
    if (!error && !course) notFound();
    const learn = learningUrl();
    return <main className="marketing course-experience"><div className="shell"><SiteNav />
        {error ? <section className="course-state" role="status"><span className="course-state-mark" aria-hidden="true">↗</span><div><h3>Chưa thể tải khóa học</h3><p>Kết nối tới máy chủ đang gián đoạn. Vui lòng thử lại.</p><Link className="primary" href={`/courses/${id}`}>Thử lại</Link></div></section>
            : course && <>
                <article className="course-detail-hero">
                    <div className="course-detail-copy"><Link className="text-link" href="/courses">← Tất cả khóa học</Link><p className="eyebrow">SOFTSYNC ACADEMY · LEARNING PATH</p><h1>{course.title}</h1><p>{course.description || "Một khóa học được thiết kế để giúp bạn biến kiến thức thành kỹ năng thực tế."}</p></div>
                    <aside className="course-detail-facts"><span>NỘI DUNG HỌC TẬP</span><strong>{course.totalLessons} bài học</strong>{learn && <a href={learn}>Vào khu vực học tập <span aria-hidden="true">→</span></a>}</aside>
                </article>
                {course.thumbnailUrl && <div className="course-detail-image"><img src={course.thumbnailUrl} alt={course.thumbnailAltText || course.title} /></div>}
                <section className="course-next-step"><span className="course-next-step-icon" aria-hidden="true">✳</span><div><p className="eyebrow">READY WHEN YOU ARE</p><h2>Bắt đầu hành trình học của bạn</h2><p>Đăng nhập vào khu vực học tập để đăng ký, xem bài và theo dõi tiến độ.</p></div>{learn && <a className="secondary" href={learn}>Mở khu vực học tập <span aria-hidden="true">→</span></a>}</section>
            </>}
        <footer className="footer"><Link className="brand" href="/">SoftSync</Link><p>Học tập rõ ràng hơn. Tiến bộ mỗi ngày.</p><Link href="/courses">Khóa học</Link><Link href="/cv">CV thông minh</Link></footer>
    </div></main>;
}
