"use client"

import { SpinnerStyled } from "@/components"
import { useGetMe } from "@/features/profile/hooks"
import { ShiftTemplatePageView } from "@/features/shifts/pages/ShiftTemplatePageView"
import React from "react"

export default function ShiftTemplatePage() {
    const { data: user, isLoading } = useGetMe()
    const storeId = user?.homeStoreId

    if (isLoading)
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <SpinnerStyled />
            </div>
        )

    return <ShiftTemplatePageView storeId={storeId} />
}
