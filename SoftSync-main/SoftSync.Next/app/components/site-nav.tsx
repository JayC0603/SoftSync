import Link from "next/link";
export function Icon({ name }: {
    name: string;
}) {
    const paths: Record<string, string> = { spark: "m13 2-3 8H3l8 5-2 7 12-13h-8l3-7", file: "M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6ZM14 2v6h6M8 12h8M8 16h6", compass: "m16 8-3 5-5 3 3-5 5-3M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0", layers: "m12 3 10 5-10 5L2 8l10-5ZM2 12l10 5 10-5M2 16l10 5 10-5", chart: "M5 20v-7M12 20V8M19 20V3", grid: "M3 3h7v7H3ZM14 3h7v7h-7ZM3 14h7v7H3ZM14 14h7v7h-7Z" };
    return <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d={paths[name] || paths.grid}/></svg>;
}
export function SiteNav() {
    return <header className="nav"><Link href="/" className="brand" aria-label="SoftSync — Trang chủ"><span className="brand-mark"><Icon name="spark"/></span>SoftSync</Link><nav className="desktop-links" aria-label="Điều hướng chính"><Link href="/#features">Khám phá</Link><Link href="/courses">Lộ trình học</Link><Link href="/cv">CV thông minh</Link></nav><div className="nav-actions"><Link href="/cv" className="login-link">Đăng nhập CV</Link><Link href="/#start" className="primary">Bắt đầu →</Link></div><details className="mobile-menu"><summary aria-label="Mở menu điều hướng">☰</summary><nav aria-label="Điều hướng trên điện thoại"><Link href="/#features">Khám phá</Link><Link href="/courses">Lộ trình & khóa học</Link><Link href="/cv">CV thông minh / Đăng nhập</Link><Link href="/#start">Bắt đầu</Link></nav></details></header>;
}
