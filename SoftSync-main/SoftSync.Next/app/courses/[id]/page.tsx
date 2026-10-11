import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteNav } from "../../components/site-nav";
import { fetchPublicCourse, learningUrl } from "../../../lib/public-courses";
export const dynamic = "force-dynamic";
export default async function CoursePage({ params }: {
    params: Promise<{
        id: string;
    }>;
}) {
    const { id } = await params;
    if (!/^\d+$/.test(id) || !Number.isSafeInteger(Number(id)) || Number(id) < 1)
        notFound();
    const { course, error } = await fetchPublicCourse(Number(id));
    if (!error && !course)
        notFound();
    const learn = learningUrl();
    return <main className="marketing"><div className="shell"><SiteNav /><section className="page-heading"><Link className="text-link" href="/courses">← Tất cả khóa học</Link><p className="eyebrow">SOFTSYNC LEARNING</p><h1>{course?.title || "Chưa thể tải khóa học"}</h1></section>{error ? <div className="notice" role="status"><p>Kết nối tới máy chủ đang gián đoạn. Vui lòng thử lại.</p><Link href={`/courses/${id}`}>Thử lại</Link></div> : course && <article className="course-detail"><p className="course-status">{course.totalLessons} bài học</p><p className="description">{course.description || "Khóa học phát triển kỹ năng cùng SoftSync."}</p><div className="notice"><p>Đăng nhập vào khu vực học tập để đăng ký khóa học, xem bài học và theo dõi tiến độ.</p>{learn && <a className="primary" href={learn}>Mở khu vực học tập →</a>}</div></article>}</div></main>;
}
