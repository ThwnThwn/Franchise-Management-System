import { Handshake, ClipboardList } from "lucide-react"
import { RoleSlug } from "../enums"
import { MenuItem } from "./types"

export const FRANCHISE_MENU_ITEMS: MenuItem[] = [
    {
        label: "Đơn đăng kí",
        href: "/profile/franchise/applications",
        icon: <ClipboardList className="w-5 h-5" />,
        allowedRoles: [RoleSlug.Customer, RoleSlug.Manager]
    },
    {
        label: "Hợp đồng",
        href: "/profile/franchise/contracts",
        icon: <Handshake className="w-5 h-5" />,
        allowedRoles: [RoleSlug.Customer, RoleSlug.Manager]
    }
]
