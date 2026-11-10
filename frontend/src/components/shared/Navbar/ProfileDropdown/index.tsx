"use client"

import React, { useMemo } from "react"
import { DropdownTrigger, DropdownMenu, DropdownItem, User, cn } from "@heroui/react"
import Link from "next/link"
import { useRouter } from "next/navigation"

import { DEFAULT_AVATAR_URL } from "@/constants/constants"
import { useLogout } from "@/features"
import { useTokenStore, useUserHelper } from "@/hooks"
// Giả sử đường dẫn này, nếu bạn để enum chỗ khác thì sửa lại nhé
import { DropdownStyled } from "@/components/styled"
import { useGetMe } from "@/features/profile/hooks"
import { PROFILE_DROPDOWN_MENU } from "@/constants/menu/profileDropdown"

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

    // Cấu hình Menu Items
    const menuItems = useMemo(() => {
        return PROFILE_DROPDOWN_MENU.map((item) => {
            if (item.key === "logout") {
                return { ...item, action: () => logoutMutation.mutate() }
            }
            return item
        })
    }, [logoutMutation])

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
                                "text-left w-full",
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
