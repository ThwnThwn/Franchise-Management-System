"use client"

import React, { useCallback, useMemo } from "react"
import { DropdownTrigger, DropdownMenu, DropdownItem, User, cn } from "@heroui/react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { LogOut, User as UserIcon, LayoutDashboard, ShoppingBag, History } from "lucide-react"

import { DEFAULT_AVATAR_URL } from "@/constants/constants"
import { STRINGS } from "@/constants/strings"
import { useLogout } from "@/features"
import { useTokenStore, useUserHelper } from "@/hooks"
// Giả sử đường dẫn này, nếu bạn để enum chỗ khác thì sửa lại nhé
import { RoleSlug } from "@/constants/enums"
import { DropdownStyled } from "@/components/styled"
import { useGetMe } from "@/features/profile/hooks"

// Định nghĩa kiểu dữ liệu cho Menu Item
type ProfileMenuItem = {
    key: string
    label: string
    href?: string
    icon?: React.ReactNode
    color?: "default" | "danger"
    allowedRoles?: RoleSlug[] // Nếu không có field này nghĩa là hiện cho all
    action?: () => void // Dành cho nút Logout
}

export function ProfileDropdown({ isTextChangeColor = false }: { isTextChangeColor?: boolean }) {
    const router = useRouter()
    const { toFullName } = useUserHelper()
    const { data: user } = useGetMe()

    // Global State
    const roleSlug = useTokenStore((s) => s.roleSlug)

    // Logout Mutation
    const logoutMutation = useLogout({
        onSuccess: () => router.replace("/")
    })

    const handleLogout = useCallback(async () => {
        await logoutMutation.mutateAsync()
    }, [logoutMutation])

    // Cấu hình Menu Items
    const menuItems: ProfileMenuItem[] = useMemo(
        () => [
            {
                key: "profile",
                label: STRINGS.NAVBAR.PROFILE || "Hồ sơ cá nhân",
                href: "/profile",
                icon: <UserIcon size={18} />
            },
            {
                key: "dashboard",
                label: STRINGS.NAVBAR.DASHBOARD?.DASHBOARD || "Trang quản trị",
                href: "/dashboard",
                icon: <LayoutDashboard size={18} />,
                // Chỉ hiện cho Admin và Manager
                allowedRoles: [RoleSlug.Admin, RoleSlug.Manager]
            },
            {
                key: "orders",
                label: STRINGS.NAVBAR.ORDER_HISTORY || "Lịch sử mua hàng",
                href: "/orders",
                icon: <ShoppingBag size={18} />,
                // Chỉ hiện cho Customer
                allowedRoles: [RoleSlug.Customer]
            },
            // Ví dụ thêm item cho Staff
            {
                key: "schedule",
                label: "Lịch làm việc",
                href: "/staff/schedule",
                icon: <History size={18} />,
                allowedRoles: [RoleSlug.Staff]
            },
            {
                key: "logout",
                label: STRINGS.NAVBAR.LOGOUT || "Đăng xuất",
                color: "danger",
                icon: <LogOut size={18} />,
                action: handleLogout // Action riêng
            }
        ],
        [handleLogout]
    )

    // Filter Items dựa trên Role
    const visibleItems = useMemo(() => {
        return menuItems.filter((item) => {
            // 1. Nếu không quy định role -> Hiện all
            if (!item.allowedRoles) return true
            // 2. Nếu có role -> Check role hiện tại có trong list không
            return roleSlug && item.allowedRoles.includes(roleSlug)
        })
    }, [menuItems, roleSlug])

    return (
        <div className="flex items-center gap-4">
            <DropdownStyled
                placement="bottom-end"
                classNames={{
                    content: "min-w-fit w-fit"
                }}
            >
                <DropdownTrigger>
                    <User
                        as="button"
                        avatarProps={{
                            isBordered: true,
                            src: DEFAULT_AVATAR_URL, // Thay bằng user?.avatarUrl
                            color: "primary",
                            size: "sm"
                        }}
                        className="transition-transform"
                        description={roleSlug || "Guest"} // Hiển thị role nhỏ ở dưới
                        name={toFullName({
                            firstName: user?.firstName,
                            lastName: user?.lastName
                        })}
                        classNames={{
                            name: cn(
                                "text-sm font-semibold ",
                                isTextChangeColor ? "text-white" : "text-default-700"
                            ),
                            description: cn(
                                "text-xs capitalize",
                                isTextChangeColor ? "text-white-500" : "text-default-500"
                            )
                        }}
                    />
                </DropdownTrigger>

                <DropdownMenu
                    aria-label="Profile Actions"
                    variant="flat"
                    className="w-fit"
                    items={visibleItems} // Dùng items prop của HeroUI để tối ưu
                >
                    {(item) => (
                        <DropdownItem
                            key={item.key}
                            color={item.color || "default"}
                            className={cn(
                                "text-left w-fit",
                                item.color === "danger" && "text-danger"
                            )}
                            startContent={item.icon}
                            // Logic xử lý Link hoặc Button
                            href={item.href}
                            as={item.href ? Link : "button"}
                            onPress={item.action} // Nếu có action (logout) thì gọi onPress
                        >
                            {item.label}
                        </DropdownItem>
                    )}
                </DropdownMenu>
            </DropdownStyled>
        </div>
    )
}
