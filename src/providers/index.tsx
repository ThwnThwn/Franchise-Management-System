"use client"
import React from "react"
import { HeroUIProvider } from "@heroui/react"
import { ToastProvider } from "@heroui/toast"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { QueryClientProvider } from "@tanstack/react-query"
import { DisclosureProvider } from "./DisclosureProvider"
import { GlobalConfirmDialog } from "@/components"

export function AppProviders({ children }: { children: React.ReactNode }) {
    const queryClient = getQueryClient()

    return (
        <QueryClientProvider client={queryClient}>
            <HeroUIProvider>
                <DisclosureProvider>{children}</DisclosureProvider>
                <GlobalConfirmDialog />
                <ToastProvider
                    placement="bottom-center"
                    toastProps={{
                        variant: "flat",
                        timeout: 1500
                        // hideIcon: true,
                        // classNames: {
                        //     closeButton: "opacity-100 absolute right-4 top-1/2 -translate-y-1/2"
                        // }
                    }}
                />
            </HeroUIProvider>
        </QueryClientProvider>
    )
}
