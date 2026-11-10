import { RoleSlug } from "@/constants/enums"
import { STRINGS } from "@/constants/strings"
import {
    LayoutDashboard,
    LogOut,
    ShoppingBag,
    UserIcon,
    Ticket,
    Wallet,
    Handshake
} from "lucide-react"
import { ProfileMenuItem } from "./types"

export const PROFILE_DROPDOWN_MENU: ProfileMenuItem[] = [
    {
        key: "profile",
        label: STRINGS.NAVBAR.PROFILE || "Hồ sơ cá nhân",
        href: "/profile",
        icon: <UserIcon size={18} />
    },
    {
        key: "dashboard",
        label: STRINGS.NAVBAR.DASHBOARD.DASHBOARD || "Trang quản trị",
        href: "/dashboard",
        icon: <LayoutDashboard size={18} />,
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager, RoleSlug.Staff]
    },
    {
        key: "orders",
        label: STRINGS.NAVBAR.ORDER_HISTORY || "Lịch sử mua hàng",
        href: "/orders",
        icon: <ShoppingBag size={18} />,
        allowedRoles: [RoleSlug.Customer]
    },
    {
        key: "wallet",
        label: "Ví của tôi",
        href: "/profile/wallet",
        icon: <Wallet size={18} />,
        allowedRoles: [RoleSlug.Customer]
    },
    {
        key: "my-vouchers",
        label: "Voucher của tôi",
        href: "/my-vouchers",
        icon: <Ticket size={18} />,
        allowedRoles: [RoleSlug.Customer]
    },
    {
        key: "profile/franchise/applications",
        label: "Chuyển nhượng",
        href: "/profile/franchise/applications",
        icon: <Handshake className="w-5 h-5" />,
        allowedRoles: [RoleSlug.Customer, RoleSlug.Manager]
    },
    {
        key: "logout",
        label: STRINGS.NAVBAR.LOGOUT || "Đăng xuất",
        color: "danger",
        icon: <LogOut size={18} />
    }
]
