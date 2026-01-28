"use client"

import { SpinnerStyled } from "@/components"
import { useTokenStore } from "@/hooks"
import { usePathname, useRouter } from "next/navigation"
import React, { useEffect } from "react"

export default function PrivateLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter()
    const pathName = usePathname()
    const isLogined = useTokenStore((s) => !!s.accessToken)

    useEffect(() => {
        if (!isLogined && pathName !== "/") {
            router.replace("/")
        }
    }, [isLogined, pathName, router])

    if (!isLogined && pathName !== "/") {
        return <SpinnerStyled />
    }

    return <>{children}</>
}
