import Link from 'next/link';
import { redirect } from 'next/navigation';
import IntegratedCv from './integrated-cv';
export const dynamic = 'force-dynamic';
export default async function CvPage({ searchParams }: { searchParams: Promise<{ integrated?: string; host?: string }> }) {
  const params = await searchParams;
  const configured = process.env.SOFTSYNC_APP_URL || process.env.BACKEND_URL;
  let origin: string | null = null;
  try { if (configured) { const url = new URL(configured); if (['http:', 'https:'].includes(url.protocol) && !url.username && !url.password) origin = url.origin; } } catch {}
  if (origin && params.integrated === '1' && params.host === origin) return <IntegratedCv hostOrigin={origin}/>;
  if (origin) redirect(`${origin}/cv`);
  return <main className="marketing"><div className="shell"><section className="page-heading"><h1>CV thông minh · SoftSync</h1><p className="intro">Khu vực CV dùng chung tài khoản SoftSync. Dịch vụ đang chờ được kết nối.</p><Link className="primary" href="/">Về trang chủ</Link></section></div></main>;
}
