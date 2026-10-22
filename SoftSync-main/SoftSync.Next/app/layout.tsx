import type { Metadata } from "next";
import "./styles.css";
import "../../SoftSync.Presentation/wwwroot/css/glass-buttons.css";

export const metadata: Metadata = {
  title: "SoftSync",
  description: "Learning and collaboration platform",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="vi"><body>{children}</body></html>;
}
