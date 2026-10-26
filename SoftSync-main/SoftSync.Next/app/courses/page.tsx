import Link from "next/link";
import { SiteNav } from "../components/site-nav";
import { fetchPublicCourses } from "../../lib/public-courses";

export const dynamic = "force-dynamic";

function CourseArtwork({ title, lessons, image, alt, index }: { title: string; lessons: number; image?: string; alt?: string; index: number }) {
    return <div className={`course-artwork course-artwork-${index % 4}`}>
        {image ? <img src={image} alt={alt || title} /> : <img src="/images/backgrounds/softsync-course-library.webp" alt="" />}
        <span className="course-artwork-label">{String(index + 1).padStart(2, "0")} <i>·</i> {lessons} bài học</span>
    </div>;
}

export default async function CoursesPage() {
    const result = await fetchPublicCourses();
    return <main className="marketing course-experience"><div className="shell"><SiteNav />
        <section className="course-library-hero" aria-labelledby="course-library-title">
            <div className="course-library-copy"><p className="eyebrow">SOFTSYNC ACADEMY <span>· LEARN WITH INTENTION</span></p>
                <h1 id="course-library-title">Bước tiếp theo <em>bắt đầu từ đây.</em></h1>
                <p className="course-library-intro">Một kỹ năng mới có thể mở ra hướng đi mới. Chọn khóa học phù hợp và tiến bộ theo nhịp của riêng bạn.</p>
                <div className="course-library-actions"><a className="primary" href="#course-catalog">Khám phá khóa học <span aria-hidden="true">↓</span></a><Link className="secondary" href="/cv">Khám phá điểm mạnh</Link></div>
                <div className="course-library-meta"><span><i aria-hidden="true">↗</i> Lộ trình rõ ràng</span><span><i aria-hidden="true">✳</i> Học theo nhịp riêng</span></div>
            </div>
            <div className="course-library-art" role="img" aria-label="Sách mở ra một lộ trình học tập mới" />
            <div className="course-library-stamp"><span>YOUR NEXT</span><strong>chapter</strong><i aria-hidden="true">↗</i></div>
        </section>

        <section className="course-catalog" id="course-catalog" aria-labelledby="catalog-title">
            <div className="course-catalog-heading"><div><p className="eyebrow">THE SOFTSYNC COLLECTION</p><h2 id="catalog-title">Khóa học dành cho hành trình của bạn</h2><p>Học điều có ý nghĩa. Áp dụng vào công việc và cuộc sống.</p></div>
                {!result.error && <span className="course-count"><strong>{result.courses.length.toString().padStart(2, "0")}</strong> khóa học</span>}
            </div>
            {result.error ? <div className="course-state"><span className="course-state-mark" aria-hidden="true">↗</span><div><h3>Chưa thể tải khóa học</h3><p>Kết nối tới máy chủ đang gián đoạn. Bạn có thể thử lại hoặc bắt đầu với công cụ CV.</p><Link className="primary" href="/courses">Thử lại</Link> <Link className="text-link" href="/cv">Phân tích CV →</Link></div></div>
                : result.courses.length === 0 ? <div className="course-state"><span className="course-state-mark" aria-hidden="true">✳</span><div><h3>Khóa học đang được chuẩn bị</h3><p>Các khóa học mới sẽ sớm có mặt tại đây. Trong lúc chờ, hãy khám phá điểm mạnh và định hướng tiếp theo của bạn.</p><Link className="primary" href="/cv">Khám phá CV thông minh <span aria-hidden="true">→</span></Link></div><div className="course-state-art" aria-hidden="true" /></div>
                    : <div className="course-grid">{result.courses.map((course, index) => <article className="course-card" key={course.id}>
                        <CourseArtwork title={course.title} lessons={course.totalLessons} image={course.thumbnailUrl} alt={course.thumbnailAltText} index={index} />
                        <div className="course-card-body"><p className="course-status"><span className="course-status-dot" /> LEARNING PATH <span>·</span> {course.totalLessons} BÀI HỌC</p>
                            <h3>{course.title}</h3><p className="course-card-description">{course.description || "Một khóa học được thiết kế để giúp bạn biến kiến thức thành kỹ năng thực tế."}</p>
                            <Link className="course-card-link" href={`/courses/${course.id}`}><span>Khám phá khóa học</span><b aria-hidden="true">↗</b></Link>
                        </div>
                    </article>)}</div>}
        </section>
        <section className="course-next-step"><span className="course-next-step-icon" aria-hidden="true">✳</span><div><p className="eyebrow">A GOOD PLACE TO BEGIN</p><h2>Chưa biết bắt đầu từ đâu?</h2><p>Khám phá thế mạnh của bạn và tìm hướng phát triển phù hợp.</p></div><Link className="secondary" href="/cv">Phân tích CV <span aria-hidden="true">→</span></Link></section>
        <footer className="footer"><Link className="brand" href="/">SoftSync</Link><p>Học tập rõ ràng hơn. Tiến bộ mỗi ngày.</p><Link href="/courses">Khóa học</Link><Link href="/cv">CV thông minh</Link></footer>
    </div></main>;
}
