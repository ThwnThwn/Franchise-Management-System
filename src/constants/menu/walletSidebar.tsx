import { DollarSign, Wallet } from "lucide-react"
import { RoleSlug } from "../enums"
import { MenuItem } from "./types"

export const WALLET_MENU_ITEMS: MenuItem[] = [
    {
        label: "Ví của tôi",
        href: "/profile/wallet",
        icon: <Wallet className="w-5 h-5" />,
        allowedRoles: [RoleSlug.Customer]
    },
    {
        label: "Lịch sử rút tiền",
        href: "/profile/wallet/withdrawals",
        icon: <DollarSign className="w-5 h-5" />,
        allowedRoles: [RoleSlug.Customer]
    }
]
