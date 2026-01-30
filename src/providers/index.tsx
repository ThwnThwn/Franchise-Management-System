"use client"
import React from "react"
import { HeroUIProvider } from "@heroui/react"
import { ToastProvider } from "@heroui/toast"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { QueryClientProvider } from "@tanstack/react-query"
import { DisclosureProvider } from "./DisclosureProvider"

export function AppProviders({ children }: { children: React.ReactNode }) {
    const queryClient = getQueryClient()

    return (
        <QueryClientProvider client={queryClient}>
            <HeroUIProvider>
                <DisclosureProvider>{children}</DisclosureProvider>
                <ToastProvider />
            </HeroUIProvider>
        </QueryClientProvider>
    )
}
