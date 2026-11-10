"use client"

import React from "react"
import { useDisclosure } from "@heroui/react"
import { useSecurityMenu } from "@/features/auth/hooks/useSecurityMenu"
import { ChangePasswordModal, SecurityRow } from "@/features/auth/components"

export default function ProfileSecurityPageView() {
    const {
        isOpen: isPassOpen,
        onOpen: onPassOpen,
        onOpenChange: onPassOpenChange
    } = useDisclosure()

    const securityItems = useSecurityMenu({
        onOpenChangePassword: onPassOpen
    })

    return (
        <div className="w-full min-h-[calc(100vh-80px)] p-4 md:p-8">
            {/* Tiêu đề */}
            <div className="mb-8 px-2">
                <h1 className="text-3xl font-black text-default-900 uppercase tracking-tighter">
                    Mật khẩu và bảo mật
                </h1>
            </div>

            {/* Khung chứa các Row: Nền trắng, bo viền, shadow nhẹ */}
            <div className="bg-white border border-default-100 rounded-4xl p-2 shadow-sm">
                <div className="flex flex-col gap-1">
                    {securityItems.map((item, index) => (
                        <React.Fragment key={index}>
                            <SecurityRow
                                icon={item.icon}
                                title={item.title}
                                description={item.description}
                                onClick={item.onClick}
                            />
                            {/* Đường kẻ phân cách giữa các dòng (trừ dòng cuối) */}
                            {index !== securityItems.length - 1 && (
                                <div className="mx-6 border-b border-default-50" />
                            )}
                        </React.Fragment>
                    ))}
                </div>
            </div>

            {/* Modals */}
            <ChangePasswordModal isOpen={isPassOpen} onOpenChange={onPassOpenChange} />
        </div>
    )
}
