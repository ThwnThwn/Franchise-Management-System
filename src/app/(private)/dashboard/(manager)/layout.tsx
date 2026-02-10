"use client"
import React, { useEffect } from "react"
import { useRouter } from "next/navigation"
import { SpinnerStyled } from "@/components/styled"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

export default function ManagerLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter()

    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isRoleCheck = roleSlug === RoleSlug.Manager

    useEffect(() => {
        if (!isRoleCheck) {
            router.replace("/")
        }
    }, [isRoleCheck, router])

    if (!isRoleCheck) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <SpinnerStyled />
            </div>
        )
    }

    return <>{children}</>
}
