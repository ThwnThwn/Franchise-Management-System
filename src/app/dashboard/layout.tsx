/**
 * Dashboard Layout
 * Main layout wrapper with sidebar
 */

"use client";

import { Sidebar } from "@/components/dashboard/Sidebar";

interface DashboardLayoutProps {
    children: React.ReactNode;
}

export default function DashboardLayout({ children }: DashboardLayoutProps) {
    return (
        <div className="flex min-h-screen bg-gradient-to-br from-background via-primary-50/30 to-background">
            {/* Sidebar */}
            <Sidebar />

            {/* Main Content */}
            <main className="flex-1 lg:ml-0">
                {children}
            </main>
        </div>
    );
}
