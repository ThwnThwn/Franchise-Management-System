import { RoleSlug } from "@/constants/enums"
import {
    LayoutDashboard,
    CalendarRange,
    Coffee,
    Users,
    Settings,
    ShoppingBag,
    FlaskConical,
    Ticket,
    Receipt
} from "lucide-react"
import { MenuItem } from "./types"

export const DASHBOARD_SIDEBAR_MENU: MenuItem[] = [
    {
        icon: <LayoutDashboard className="w-5 h-5" />,
        label: "Tổng quan",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager],
        subItems: [
            {
                label: "Tổng quan",
                href: "/dashboard",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                label: "Thống kê",
                href: "/dashboard/orders/analytics",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            }
        ]
    },
    {
        icon: <ShoppingBag className="w-5 h-5" />,
        label: "Đơn hàng",
        href: "/dashboard/orders",
        allowedRoles: [RoleSlug.Manager, RoleSlug.Staff]
    },
    {
        icon: <CalendarRange className="w-5 h-5" />,
        label: "Ca làm",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager, RoleSlug.Staff],
        subItems: [
            {
                label: "Ca làm",
                href: "/dashboard/shifts",
                allowedRoles: [RoleSlug.Manager, RoleSlug.Staff]
            },
            {
                label: "Chợ ca",
                href: "/dashboard/shifts/pools",
                allowedRoles: [RoleSlug.Staff]
            },
            {
                label: "Chấm công",
                href: "/dashboard/shifts/time-logs",
                allowedRoles: [RoleSlug.Manager, RoleSlug.Staff]
            },
            {
                label: "Lịch cá nhân",
                href: "/dashboard/shifts/my-schedule",
                allowedRoles: [RoleSlug.Staff]
            },
            {
                label: "Đổi ca",
                href: "/dashboard/shifts/swaps",
                allowedRoles: [RoleSlug.Manager, RoleSlug.Staff]
            },
            {
                label: "Cấu hình",
                href: "/dashboard/shifts/settings",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            }
        ]
    },
    {
        icon: <Coffee className="w-5 h-5" />,
        label: "Sản phẩm",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager],
        subItems: [
            {
                label: "Sản phẩm",
                href: "/dashboard/products",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                label: "Danh mục",
                href: "/dashboard/categories",
                allowedRoles: [RoleSlug.Admin]
            }
        ]
    },
    {
        icon: <Ticket className="w-5 h-5" />,
        label: "Voucher",
        href: "/dashboard/vouchers",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
    },
    {
        icon: <FlaskConical className="w-5 h-5" />,
        label: "Nguyên liệu",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager],
        subItems: [
            {
                label: "Nguyên liệu",
                href: "/dashboard/ingredients",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                label: "Danh mục",
                href: "/dashboard/ingredients/categories",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                label: "Kho",
                href: "/dashboard/inventory",
                allowedRoles: [RoleSlug.Manager]
            }
        ]
    },
    {
        icon: <Users className="w-5 h-5" />,
        label: "Người dùng",
        allowedRoles: [RoleSlug.Admin, RoleSlug.Manager, RoleSlug.Staff],
        subItems: [
            {
                label: "Người dùng",
                href: "/dashboard/users",
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager, RoleSlug.Staff]
            },
            {
                label: "Vai trò",
                href: "/dashboard/roles",
                allowedRoles: [RoleSlug.Admin]
            }
        ]
    },
    {
        icon: <Receipt className="w-5 h-5" />,
        label: "Ví",
        allowedRoles: [RoleSlug.Admin],
        subItems: [
            {
                label: "Giao dịch",
                href: "/dashboard/wallets/transactions",
                allowedRoles: [RoleSlug.Admin]
            },
            {
                label: "Đơn rút tiền",
                href: "/dashboard/wallets/withdrawals",
                allowedRoles: [RoleSlug.Admin]
            }
        ]
    },
    {
        icon: <Settings className="w-5 h-5" />,
        label: "Cài đặt",
        href: "/dashboard/settings",
        allowedRoles: [RoleSlug.Admin]
    }
]
