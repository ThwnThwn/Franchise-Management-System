"use client"
import React from "react"
import { HeroUIProvider, useDisclosure } from "@heroui/react"
import { ToastProvider } from "@heroui/toast"
import { getQueryClient } from "@/utils/helpers/getQueryClient"
import { QueryClientProvider } from "@tanstack/react-query"
import { DisclosureProvider } from "./DisclosureProvider"
import { CartButton, CartDrawer } from "@/features/cart/components"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

export function AppProviders({ children }: { children: React.ReactNode }) {
    const queryClient = getQueryClient()
    const { isOpen: isCartOpen, onOpen: onCartOpen, onClose: onCartClose } = useDisclosure()
    const isLogined = useTokenStore((s) => !!s.accessToken)
    const roleSlug = useTokenStore((s) => s.roleSlug)

    return (
        <QueryClientProvider client={queryClient}>
            <HeroUIProvider>
                <DisclosureProvider>
                    {children}
                    {isLogined && roleSlug === RoleSlug.Customer && (
                        <div className="fixed bottom-6 right-6 z-30">
                            <CartButton onPress={onCartOpen} />
                        </div>
                    )}
                    <CartDrawer isOpen={isCartOpen} onClose={onCartClose} />
                </DisclosureProvider>
                <ToastProvider />
            </HeroUIProvider>
        </QueryClientProvider>
    )
}
