"use client"

import { ArrowRightLeft } from "lucide-react"
import { DashboardHeader } from "@/features/dashboard"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { ManagerSwapView, StaffSwapView } from "@/features/shifts/components"

export default function ShiftSwapPageView() {
    const roleSlug = useTokenStore((s) => s.roleSlug)

    // Nếu là Admin hoặc Manager thì xài Manager View
    const isManager = roleSlug === RoleSlug.Admin || roleSlug === RoleSlug.Manager

    return (
        <div className="max-w-6xl mx-auto p-4 md:p-6 lg:p-8 min-h-screen">
            <DashboardHeader
                icon={<ArrowRightLeft className="w-6 h-6 text-white" />}
                title={isManager ? "Quản lý Yêu Cầu Đổi Ca" : "Yêu Cầu Đổi Ca Của Tôi"}
                description={
                    isManager
                        ? "Xét duyệt các kèo đổi ca của nhân viên"
                        : "Quản lý các lời mời đi làm thay và yêu cầu bạn đã gửi"
                }
            />

            <div className="mt-6 bg-white rounded-xl shadow-sm border border-default-100 p-4 min-h-125">
                {isManager ? <ManagerSwapView /> : <StaffSwapView />}
            </div>
        </div>
    )
}
