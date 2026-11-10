"use client"

import { Spinner, TableBody, TableCell, TableColumn, TableHeader, TableRow } from "@heroui/react"
import { EyeIcon } from "lucide-react"
import { TableStyled, ButtonIconStyled, AvatarStyled, SpinnerStyled } from "@/components" // Import EnumPicker
import { useTokenStore, useUserHelper } from "@/hooks" // Import hook mutation
import { DEFAULT_AVATAR_URL } from "@/constants/constants"
import { User } from "@/models/user"
import { RoleSlug } from "@/constants/enums"
import { RoleSlugLabels } from "@/constants/labels"
import { useGetStores } from "@/features/stores"

// --- Component con để xử lý ô Role riêng biệt ---
// const RoleCell = ({ user }: { user: User }) => {
//     const { mutate, isPending } = useUpdateUserRole()

//     const handleRoleChange = (key: RoleSlug | undefined) => {
//         if (!key) return

//         mutate({
//             id: user.id,
//             roleSlug: key as RoleSlug
//         })
//     }

//     const roleSlug = user.role?.slug

//     return (
//         <EnumPicker
//             labels={RoleSlugLabels}
//             value={roleSlug}
//             onChange={handleRoleChange}
//             isReadOnly={isPending}
//             isClearable={false}
//             className="min-w-35"
//             aria-label="Chọn vai trò"
//         />
//     )
// }

// --- Component chính ---
type UserUserManagementProps = {
    users: User[]
    isLoading?: boolean
    emptyMessage?: string
    onViewUser?: (user: User) => void
    isRefetching?: boolean
}

export function UserTable({
    users,
    isLoading: isUserLoading = false,
    emptyMessage = "Chưa có nhân viên nào",
    onViewUser,
    isRefetching = false
}: UserUserManagementProps) {
    const userId = useTokenStore((s) => s.userId) || undefined

    const { toFullName } = useUserHelper()
    const { data: stores, isLoading: isStoreLoading } = useGetStores({
        params: {},
        pagination: { page: 1, pageSize: 100 }
    })

    const isLoading = isUserLoading || isStoreLoading

    const renderBody = () => {
        return (users || [])
            .filter((item) => item.role?.slug !== RoleSlug.Admin && item.id !== userId)
            .map((item) => {
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
                                    <span className="font-semibold text-slate-900">
                                        {displayName}
                                    </span>
                                    <span className="text-sm text-slate-500">
                                        {item.email ?? "-"}
                                    </span>
                                </div>
                            </div>
                        </TableCell>

                        {/* Cột 2: Vai trò (MỚI) */}
                        <TableCell className="text-center">
                            {/* Nhúng component RoleCell vào đây */}
                            {/* <RoleCell user={item} /> */}
                            {item.role && RoleSlugLabels[item.role?.slug]}
                        </TableCell>

                        {/* Cột 3: Số điện thoại */}
                        <TableCell className="text-center text-sm text-slate-700">
                            {item.phone ?? "-"}
                        </TableCell>

                        {/* Cột 4: Cửa hàng */}
                        <TableCell className="text-center text-sm text-slate-700">
                            {(stores?.items || []).filter((s) => s.id === item.homeStoreId)[0]
                                ?.name || "-"}
                        </TableCell>

                        {/* Cột 5: Thao tác */}
                        <TableCell className="text-center">
                            <ButtonIconStyled
                                variant="ghost"
                                className="p-0"
                                onPress={() => onViewUser?.(item)}
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
            <TableStyled className="min-w-full text-sm" removeWrapper>
                <TableHeader className="bg-slate-50">
                    <TableColumn className="text-center text-xs font-semibold uppercase tracking-wide text-slate-600 w-[30%]">
                        Người dùng
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
                <TableBody
                    isLoading={isLoading}
                    loadingContent={<Spinner content="Đang tải..." />}
                    emptyContent={emptyMessage}
                >
                    {renderBody()}
                </TableBody>
            </TableStyled>
            {isRefetching ? <SpinnerStyled /> : null}
        </>
    )
}
