"use client"

import React from "react"
import { useRouter, useSearchParams, usePathname } from "next/navigation"
import { Tabs, Tab } from "@heroui/react"
import { FileClock, ShieldCheck } from "lucide-react"
import ShiftTemplatesPageView from "../ShiftTemplatesPageView"
import ShiftRulesManagerPageView from "../ShiftRulesManagerPageView"

export default function ShiftSettingsPageView() {
    const router = useRouter()
    const pathname = usePathname()
    const searchParams = useSearchParams()

    // 1. Lấy tab hiện tại từ URL, mặc định là 'templates'
    const activeTab = searchParams.get("tab") || "templates"

    // 2. Hàm xử lý khi thay đổi Tab
    const handleTabChange = (key: React.Key) => {
        const params = new URLSearchParams(searchParams.toString())
        params.set("tab", key.toString())

        // Cập nhật URL mà không làm scroll trang
        router.push(`${pathname}?${params.toString()}`, { scroll: false })
    }

    return (
        <div>
            <Tabs
                aria-label="Shift Settings"
                color="primary"
                variant="underlined"
                selectedKey={activeTab}
                onSelectionChange={handleTabChange}
                className="w-full"
                classNames={{
                    tabList: "gap-6 min-w-full relative rounded-none p-0 border-b border-divider",
                    cursor: "w-full bg-primary",
                    tab: "px-0 h-12",
                    tabContent: "text-primary-400 group-data-[selected=true]:text-primary font-bold"
                }}
            >
                <Tab
                    key="templates"
                    title={
                        <div className="flex items-center space-x-2">
                            <FileClock size={18} />
                            <span>Mẫu ca làm việc</span>
                        </div>
                    }
                >
                    <div>
                        <ShiftTemplatesPageView />
                    </div>
                </Tab>

                <Tab
                    key="rules"
                    title={
                        <div className="flex items-center space-x-2">
                            <ShieldCheck size={18} />
                            <span>Quy tắc & Chính sách</span>
                        </div>
                    }
                >
                    <div>
                        <ShiftRulesManagerPageView />
                    </div>
                </Tab>
            </Tabs>
        </div>
    )
}
