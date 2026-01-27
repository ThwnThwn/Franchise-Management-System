"use client"

import React, { useCallback } from "react"
import { DropdownTrigger, DropdownMenu, DropdownItem, User } from "@heroui/react"
import { DropdownStyled } from "@/components"
import Link from "next/link"
import { DEFAULT_AVATAR_URL } from "@/constants/constants"
import { RoleName } from "@/constants/enums"
import { STRINGS } from "@/constants/strings"
import { useLogout } from "@/features"
import { useRouter } from "next/navigation"

type DropdownLinkItem = {
    key: string
    href?: string
    label: string
    color?: "danger"
}

export const buildItems = ({
    roleName,
    defaultItems,
    customerItems = [],
    staffItems = [],
    adminItems = [],
    managerItems = [],
    bottomItems = []
}: {
    roleName?: RoleName
    defaultItems: DropdownLinkItem[]
    customerItems?: DropdownLinkItem[]
    staffItems?: DropdownLinkItem[]
    adminItems?: DropdownLinkItem[]
    managerItems?: DropdownLinkItem[]
    bottomItems?: DropdownLinkItem[]
}) => {
    const roleItemsMap: Record<RoleName, DropdownLinkItem[]> = {
        [RoleName.Customer]: customerItems,
        [RoleName.Staff]: staffItems,
        [RoleName.Manager]: managerItems,
        [RoleName.Admin]: adminItems
    }
    const combinedItems = [...defaultItems, ...(roleName ? roleItemsMap[roleName] : [])]

    const uniqueItems: DropdownLinkItem[] = []
    const seenKeys = new Set<string>()

    for (const tab of combinedItems) {
        if (seenKeys.has(tab.key)) continue
        seenKeys.add(tab.key)
        uniqueItems.push(tab)
    }

    uniqueItems.push(...bottomItems)
    return uniqueItems
}

export function ProfileDropdown({ onOpen = undefined }: { onOpen?: () => void }) {
    const router = useRouter()
    const logoutMutation = useLogout({ onSuccess: () => router.replace("/") })

    // const { data: user, isLoading: isGetMeLoading, isError: isGetMeError } = useGetMe()

    const defaultItems: DropdownLinkItem[] = [
        {
            key: "profile",
            href: "/profile",
            label: STRINGS.NAVBAR.PROFILE
        }
    ]

    const bottomItems: DropdownLinkItem[] = [
        { key: "logout", label: STRINGS.NAVBAR.LOGOUT, color: "danger" }
    ]

    const customerItems: DropdownLinkItem[] = []

    const adminItems: DropdownLinkItem[] = []

    const managerItems: DropdownLinkItem[] = []

    const staffItems: DropdownLinkItem[] = []

    const dropdownItems = buildItems({
        // roleName: user?.role?.name as RoleName | undefined,
        roleName: undefined,
        defaultItems,
        managerItems,
        adminItems,
        staffItems,
        customerItems,
        bottomItems
    })

    const handleLogout = useCallback(async () => {
        await logoutMutation.mutateAsync()
    }, [logoutMutation])

    // handle get me error
    // if (isGetMeLoading || isGetMeError) return <Spinner />

    return (
        <div className="gap-4 flex items-center">
            <DropdownStyled
                classNames={{
                    content: "min-w-fit"
                }}
            >
                <DropdownTrigger>
                    <User
                        id="navbar-user"
                        as="button"
                        avatarProps={{
                            isBordered: true,
                            // src: user?.avatarUrl || DEFAULT_AVATAR_URL
                            src: DEFAULT_AVATAR_URL
                        }}
                        className="transition-transform"
                        // name={user?.firstName.trim() || ""}
                        name=""
                        classNames={{
                            name: "hidden sm:block text-[16px] font-bold"
                        }}
                        onClick={() => onOpen?.()}
                    />
                </DropdownTrigger>
                <DropdownMenu className="max-w-fit" variant="flat">
                    {dropdownItems.map((item) =>
                        item.href ? (
                            <DropdownItem
                                id={item.key === "profile" ? "navbar-profile" : ""}
                                key={item.key}
                                as={Link}
                                href={item.href}
                                className="block pr-5"
                            >
                                {item.label}
                            </DropdownItem>
                        ) : (
                            <DropdownItem
                                key={item.key}
                                textValue={item.label}
                                color={item.color}
                                className="block pr-5"
                                onPress={handleLogout}
                            >
                                {item.label}
                            </DropdownItem>
                        )
                    )}
                </DropdownMenu>
            </DropdownStyled>
        </div>
    )
}
