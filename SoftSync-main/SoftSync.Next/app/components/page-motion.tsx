"use client";

import { useEffect } from "react";
import { usePathname } from "next/navigation";

export function PageMotion() {
  const pathname = usePathname();
  useEffect(() => { window.dispatchEvent(new Event("softsync:navigate")); }, [pathname]);
  return null;
}
