"use client"

import React from "react"
import { Skeleton, TableBody, TableCell, TableColumn, TableHeader, TableRow } from "@heroui/react"
import { EyeIcon } from "lucide-react"
import {
    TableStyled,
    ButtonIconStyled,
    AvatarStyled,
    EnumPicker,
    SpinnerStyled
} from "@/components" // Import EnumPicker
import { useUserHelper } from "@/hooks" // Import hook mutation
import { DEFAULT_AVATAR_URL } from "@/constants/constants"
import { User } from "@/models/user"
import { Key } from "@react-types/shared"
import { useUpdateUserRole } from "@/features/dashboard/hooks"
import { RoleSlug } from "@/constants/enums"
import { RoleSlugLabels } from "@/constants/labels"

// --- Component con để xử lý ô Role riêng biệt ---
const RoleCell = ({ user }: { user: User }) => {
    const { mutate, isPending } = useUpdateUserRole()

    const handleRoleChange = (key: Key | null) => {
        if (!key) return

        mutate({
            id: user.id,
            roleSlug: key as RoleSlug
        })
    }

    const roleSlug = user.role?.name.toLowerCase()

    return (
        <EnumPicker
            labels={RoleSlugLabels}
            value={roleSlug}
            onChange={handleRoleChange}
            isReadOnly={isPending}
            isClearable={false}
            className="min-w-35"
            aria-label="Chọn vai trò"
        />
    )
}

// --- Component chính ---
type UserUserManagementProps = {
    users: User[]
    isLoading?: boolean
    emptyMessage?: string
    onEditUser?: (user: User) => void
    isRefetching?: boolean
}

export function UserTable({
    users,
    isLoading = false,
    emptyMessage,
    onEditUser,
    isRefetching = false
}: UserUserManagementProps) {
    const { toFullName } = useUserHelper()

    const renderBody = () => {
        if (isLoading) {
            return Array.from({ length: 4 }).map((_, index) => (
                <TableRow key={`skeleton-${index}`} className="h-20">
                    {/* Tăng colSpan lên 5 vì thêm cột Role */}
                    <TableCell className="text-center" colSpan={5}>
                        <Skeleton className="h-12 w-full rounded-xl" />
                    </TableCell>
                </TableRow>
            ))
        }

        if (users.length === 0) {
            return (
                <TableRow>
                    <TableCell
                        colSpan={5} // Tăng colSpan lên 5
                        className="px-6 py-10 text-center text-sm text-slate-500"
                    >
                        {emptyMessage ?? "Chưa có nhân viên nào"}
                    </TableCell>
                </TableRow>
            )
        }

        return (users || []).map((item) => {
            const displayName = toFullName({
                firstName: item.firstName,
                lastName: item.lastName
            })

            return (
                <TableRow
                    key={item.id}
                    className="border-b border-gray-100 last:border-0 transition-colors hover:bg-gray-50"
                >
                    {/* Cột 1: Thông tin nhân viên */}
                    <TableCell className="px-6 py-4 text-left align-middle">
                        <div className="flex items-center gap-3 ml-5">
                            <AvatarStyled
                                src={DEFAULT_AVATAR_URL}
                                name={displayName || item.email || ""}
                                className="h-10 w-10 shrink-0 text-sm"
                                radius="full"
                            />
                            <div className="flex flex-col items-start text-left">
                                <span className="font-semibold text-slate-900">{displayName}</span>
                                <span className="text-sm text-slate-500">{item.email ?? "-"}</span>
                            </div>
                        </div>
                    </TableCell>

                    {/* Cột 2: Vai trò (MỚI) */}
                    <TableCell className="text-center">
                        {/* Nhúng component RoleCell vào đây */}
                        <RoleCell user={item} />
                    </TableCell>

                    {/* Cột 3: Số điện thoại */}
                    <TableCell className="text-center text-sm text-slate-700">
                        {item.phone ?? "-"}
                    </TableCell>

                    {/* Cột 4: Cửa hàng */}
                    <TableCell className="text-center text-sm text-slate-700">
                        {item.homeStoreId || "-"}
                    </TableCell>

                    {/* Cột 5: Thao tác */}
                    <TableCell className="text-center">
                        <ButtonIconStyled
                            variant="ghost"
                            className="p-0"
                            onPress={() => onEditUser?.(item)}
                        >
                            <EyeIcon size={20} />
                        </ButtonIconStyled>
                    </TableCell>
                </TableRow>
            )
        })
    }

    return (
        <>
            <TableStyled className="min-w-full table-fixed text-sm" removeWrapper>
                <TableHeader className="bg-slate-50">
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[30%]">
                        Nhân viên
                    </TableColumn>
                    {/* Thêm cột Vai trò */}
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[20%]">
                        Vai trò
                    </TableColumn>
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[15%]">
                        Số điện thoại
                    </TableColumn>
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[20%]">
                        Cửa hàng
                    </TableColumn>
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[15%]">
                        Thao tác
                    </TableColumn>
                </TableHeader>
                <TableBody>{renderBody()}</TableBody>
            </TableStyled>
            {isRefetching ? <SpinnerStyled /> : null}
        </>
    )
}
