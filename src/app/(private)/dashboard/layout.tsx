/**
 * Dashboard Layout
 * Main layout wrapper with sidebar
 */

"use client"

import { SpinnerStyled } from "@/components"
import { RoleSlug } from "@/constants/enums"
import { Sidebar } from "@/features/dashboard"
import { useTokenStore } from "@/hooks"
import { useRouter } from "next/navigation"
import React, { useEffect } from "react"

interface DashboardLayoutProps {
    children: React.ReactNode
}

export default function DashboardLayout({ children }: DashboardLayoutProps) {
    const router = useRouter()

    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isAdmin = roleSlug === RoleSlug.Admin

    useEffect(() => {
        if (!isAdmin) {
            router.replace("/")
        }
    }, [isAdmin, router])

    if (!isAdmin) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <SpinnerStyled />
            </div>
        )
    }

    return (
        <div className="flex justify-between rounded-2xl w-7xl mx-7 bg-linear-to-b from-white/55 to-white/50 backdrop-blur-xl border-r border-primary-100 shadow-2xl">
            <Sidebar />
            <div className="w-full">{children}</div>
        </div>
    )
}
