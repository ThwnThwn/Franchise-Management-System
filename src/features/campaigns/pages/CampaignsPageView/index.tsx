"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Button } from "@heroui/react"
import { Megaphone, Plus, Search } from "lucide-react"

import { InputStyled } from "@/components"
import {
    useGetAllCampaigns,
    useUpdateCampaignActiveStatus
} from "@/features/campaigns/hooks/useCampaigns"
import { CampaignsTable } from "@/features/campaigns/components"
import { DashboardHeader } from "@/features/dashboard"

export default function CampaignsPageView() {
    const router = useRouter()

    // Hooks gọi API
    const { data: campaigns, isLoading } = useGetAllCampaigns()
    const { updateStatusWithConfirm } = useUpdateCampaignActiveStatus()

    const [searchQuery, setSearchQuery] = useState("")

    // Chuyển hướng sang trang Detail với tham số "new"
    const handleCreate = () => {
        router.push("/dashboard/campaigns/new")
    }

    // Chuyển hướng sang trang Detail với ID cụ thể
    const handleRowClick = (id: string) => {
        router.push(`/dashboard/campaigns/${id}`)
    }

    const handleToggleStatus = (id: string, currentIsActive: boolean) => {
        updateStatusWithConfirm(id, currentIsActive)
    }

    // Lọc theo tên Campaign
    const filteredCampaigns =
        campaigns?.filter((c) => c.name.toLowerCase().includes(searchQuery.toLowerCase())) || []

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            {/* Header */}
            <div className="flex flex-col md:flex-row justify-between">
                <DashboardHeader
                    icon={<Megaphone className="w-6 h-6 text-white" />}
                    title="Chiến dịch"
                    description="Quản lý Chiến dịch / Sự kiện"
                />
                <Button color="primary" onPress={handleCreate} startContent={<Plus size={18} />}>
                    Tạo Sự Kiện Mới
                </Button>
            </div>

            <div className="bg-default-50/50 p-4 rounded-2xl border border-default-100 flex items-center gap-4">
                <InputStyled
                    placeholder="Tìm kiếm theo tên sự kiện..."
                    value={searchQuery}
                    onValueChange={setSearchQuery}
                    startContent={<Search className="text-default-400" size={18} />}
                    className="max-w-xs"
                />
            </div>

            <div className="bg-background rounded-2xl border border-default-100 shadow-sm overflow-hidden p-0">
                <CampaignsTable
                    campaigns={filteredCampaigns}
                    isLoading={isLoading}
                    onToggleStatus={handleToggleStatus}
                    onRowAction={handleRowClick}
                />
            </div>
        </div>
    )
}
