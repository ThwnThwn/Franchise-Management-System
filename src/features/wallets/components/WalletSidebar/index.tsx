"use client"

import { usePathname } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { useGetMyWallet } from "@/features/wallets/hooks"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { BaseSidebar } from "@/components"
import { WALLET_MENU_ITEMS } from "@/constants/menu/walletSidebar"

interface WalletSidebarProps {
    isMobile?: boolean
    onClose?: () => void
}

export const WalletSidebar = ({ isMobile = false, onClose = () => {} }: WalletSidebarProps) => {
    const pathname = usePathname()
    const roleSlug = useTokenStore((s) => s.roleSlug) || undefined

    const { data: wallet } = useGetMyWallet()

    return (
        <aside className="min-w-60 bg-background h-full border-r border-divider flex-col">
            <BaseSidebar
                isMobile={isMobile}
                pathname={pathname}
                items={WALLET_MENU_ITEMS}
                onClose={onClose}
                userRole={roleSlug}
                header={
                    <div className="bg-primary-50 p-4 rounded-2xl border border-primary-100 mb-2">
                        <p className="text-[10px] uppercase font-bold text-primary-600 tracking-wider">
                            Số dư khả dụng
                        </p>
                        <p className="text-xl font-black text-primary-700 mt-1">
                            {formatCurrencyWithSymbol(wallet?.balance || 0)}
                        </p>
                    </div>
                }
                // footer={
                //     <div className="text-center">
                //         <p className="text-[10px] text-default-400">Hỗ trợ: 1900 xxxx</p>
                //     </div>
                // }
            />
        </aside>
    )
}
