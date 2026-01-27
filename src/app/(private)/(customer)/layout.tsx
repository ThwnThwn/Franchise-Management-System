"use client"
import React, { useEffect } from "react"
import { useRouter } from "next/navigation"
import { SpinnerStyled } from "@/components/styled"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

export default function CustomerLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter()

    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isCustomer = roleSlug === RoleSlug.Customer

    useEffect(() => {
        if (!isCustomer) {
            router.replace("/")
        }
    }, [isCustomer, router])

    if (!isCustomer) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <SpinnerStyled />
            </div>
        )
    }

    return <>{children}</>
}
