"use client"
import React from "react"
import { HeroUIProvider } from "@heroui/react"
import { ToastProvider } from "@heroui/toast"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { QueryClientProvider } from "@tanstack/react-query"
import { DisclosureProvider } from "./DisclosureProvider"
import { GlobalConfirmDialog } from "@/components"
import { StaffAttendanceFloatingBar } from "@/features/shifts/components"
import { I18nProvider } from "@react-aria/i18n"

export function AppProviders({ children }: { children: React.ReactNode }) {
    const queryClient = getQueryClient()

    return (
        <QueryClientProvider client={queryClient}>
            <I18nProvider locale="vi">
                <HeroUIProvider>
                    <DisclosureProvider>{children}</DisclosureProvider>
                    <GlobalConfirmDialog />
                    <ToastProvider
                        placement="top-center"
                        toastProps={{
                            variant: "flat",
                            timeout: 2000,
                            classNames: {
                                closeButton: "opacity-100 absolute right-4 top-1/2 -translate-y-1/2"
                            },
                            closeIcon: (
                                <svg
                                    fill="none"
                                    height="32"
                                    stroke="currentColor"
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth="2"
                                    viewBox="0 0 24 24"
                                    width="32"
                                >
                                    <path d="M18 6 6 18" />
                                    <path d="m6 6 12 12" />
                                </svg>
                            )
                            // hideIcon: true,
                        }}
                    />
                    <StaffAttendanceFloatingBar />
                </HeroUIProvider>
            </I18nProvider>
        </QueryClientProvider>
    )
}
