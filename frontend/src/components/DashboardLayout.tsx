"use client";

import { ReactNode, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/Button";
import type { UserRole } from "@/types";
import { LogOut, LucideIcon } from "lucide-react";

interface NavItem {
  label: string;
  href: string;
  icon: LucideIcon;
}

interface DashboardLayoutProps {
  requiredRole: UserRole;
  navItems: NavItem[];
  children: ReactNode;
}

export function DashboardLayout({ requiredRole, navItems, children }: DashboardLayoutProps) {
  const { user, loading, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (loading) return;
    if (!user) {
      router.push("/login");
    } else if (user.role !== requiredRole) {
      router.push("/login");
    }
  }, [user, loading, requiredRole, router]);

  if (loading || !user || user.role !== requiredRole) {
    return (
      <div className="min-h-screen bg-paper flex items-center justify-center">
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-paper flex">
      <aside className="w-64 bg-paper-raised border-r border-slate-light flex flex-col">
        <div className="p-6 border-b border-slate-light">
          <h2 className="text-lg font-semibold text-ink">Assignment System</h2>
          <p className="eyebrow mt-1">{user.role}</p>
        </div>

        <nav className="flex-1 p-3 flex flex-col gap-1">
            {navItems.map((item) => (
            <a
            key={item.href}
            href={item.href}
            className="block px-3 py-2 rounded-md text-sm text-ink-soft hover:bg-paper hover:text-ink transition-colors"
          >
            {item.label}
            </a>
            ))}
        </nav>

        <div className="p-4 border-t border-slate-light">
          <p className="text-sm text-ink truncate">{user.fullName}</p>
          <p className="text-xs text-slate truncate mb-3">{user.email}</p>
          <Button variant="secondary" onClick={logout} className="w-full flex items-center justify-center gap-2">
            <LogOut size={15} />
            Log out
          </Button>
        </div>
      </aside>

      <main className="flex-1 p-8 overflow-auto">{children}</main>
    </div>
  );
}