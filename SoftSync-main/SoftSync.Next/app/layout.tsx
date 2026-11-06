import type { Metadata } from "next";
import Script from "next/script";
import "./styles.css";
import "./glass-buttons.css";
import "./softsync-motion.css";
import "./course-experience.css";
import { PageMotion } from "./components/page-motion";

export const metadata: Metadata = {
  title: "SoftSync",
  description: "Learning and collaboration platform",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="vi"><body>{children}<PageMotion /><Script src="/softsync-motion.js" strategy="afterInteractive" /><Script src="/website-activity.js" strategy="afterInteractive" /></body></html>;
}
