"use client"

import { Suspense, useCallback, useMemo, useState } from "react"
import { Spinner, useDisclosure } from "@heroui/react"
import { Plus, Search, Users } from "lucide-react"
import {
    ButtonIconStyled,
    EnumPicker,
    InputStyled,
    PaginationStyled,
    SpinnerStyled
} from "@/components"

import { useGetAllUsers } from "@/features/users/hooks"
import { UserModal, UserTable } from "@/features/users/components"
import { UserFilterParams } from "@/features/users/models/request"

import { RoleSlug } from "@/constants/enums"
import { PaginationParams } from "@/models/common/request"
import { RoleSlugLabels } from "@/constants/labels"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { DashboardHeader } from "@/features/dashboard"
import { omitRecord } from "@/utils/helpers/uiHelper"
import { useTokenStore } from "@/hooks"
import { User } from "@/models/user"

export default function UserManagementPageView() {
    const currStoreId = useTokenStore((s) => s.storeId) || undefined
    const roleSlug = useTokenStore((s) => s.roleSlug)

    const isAdmin = roleSlug === RoleSlug.Admin

    const { defaultManageRole, manageRoleLabels } = useMemo(() => {
        switch (roleSlug) {
            case RoleSlug.Admin:
                return {
                    defaultManageRole: RoleSlug.Manager,
                    manageRoleLabels: omitRecord(RoleSlugLabels, [RoleSlug.Admin])
                }
            case RoleSlug.Manager:
                return {
                    defaultManageRole: RoleSlug.Staff,
                    manageRoleLabels: omitRecord(RoleSlugLabels, [RoleSlug.Admin, RoleSlug.Manager])
                }
            case RoleSlug.Staff:
                return {
                    defaultManageRole: RoleSlug.Customer,
                    manageRoleLabels: omitRecord(RoleSlugLabels, [RoleSlug.Admin, RoleSlug.Manager])
                }
            default:
                return {
                    defaultManageRole: undefined,
                    manageRoleLabels: undefined
                }
        }
    }, [roleSlug])

    // --- Data Fetching ---
    // Assuming useGetAllUsers has 'params' and 'pagination' in its dependency array
    const [filter, setFilter] = useState<UserFilterParams>({
        roleSlug: defaultManageRole,
        homeStoreId: defaultManageRole === RoleSlug.Customer ? undefined : currStoreId
    })
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading } = useGetAllUsers({
        params: filter,
        pagination,
        enabled: true
    })

    const handleSetFilter = useCallback(
        (newFilter: UserFilterParams) => {
            setPagination({ page: 1 })

            const resolvedRoleSlug =
                !isAdmin && newFilter.roleSlug === undefined
                    ? defaultManageRole
                    : newFilter.roleSlug

            setFilter({
                ...newFilter,
                roleSlug: resolvedRoleSlug,
                homeStoreId: resolvedRoleSlug === RoleSlug.Customer ? undefined : currStoreId
            })
        },
        [currStoreId, defaultManageRole, isAdmin]
    )

    const [isTyping, setIsTyping] = useState(false)
    const debouncedSearch = useMemo(
        () =>
            debouncedWrapper(
                async (params: UserFilterParams) => {
                    handleSetFilter(params)
                },
                1000,
                () => setIsTyping(true),
                () => setIsTyping(false)
            ),
        [handleSetFilter]
    )

    // --- Mutations ---
    const { isOpen, onOpen, onClose } = useDisclosure()
    const [selectedUser, setSelectedUser] = useState<User | undefined>(undefined)
    const handleClose = () => {
        setSelectedUser(undefined)
        onClose()
    }

    if (!manageRoleLabels) return null

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            {/* Page Header */}
            <DashboardHeader
                icon={<Users className="w-6 h-6 text-white" />}
                title="Quản lý người dùng"
                description="Quản lý thông tin người dùng"
            />

            {/* --- Filter Section --- */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-3 bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
                <InputStyled
                    placeholder="Tìm kiếm theo tên, email, số điện thoại..."
                    startContent={<Search size={18} className="text-slate-400" />}
                    variant="bordered"
                    className="w-full"
                    onChange={async (e) => {
                        await debouncedSearch({
                            ...filter,
                            keyword: e.target.value.trim()
                        })
                    }}
                />
                <EnumPicker
                    placeholder="Chọn vai trò"
                    labels={manageRoleLabels}
                    value={filter.roleSlug}
                    onChange={(e) => {
                        handleSetFilter({ ...filter, roleSlug: e })
                    }}
                    isReadOnly={isLoading}
                    className="w-70"
                    aria-label="Chọn vai trò"
                    isClearable={isAdmin}
                    // hidden={!isAdmin}
                />

                <ButtonIconStyled
                    variant="bordered"
                    className="btn-gradient rounded-lg"
                    onPress={onOpen}
                >
                    <Plus />
                </ButtonIconStyled>

                {/* Create Modal */}
                <UserModal
                    isOpen={isOpen}
                    onClose={handleClose}
                    initialData={selectedUser}
                    manageRoleLabels={manageRoleLabels}
                    defaultManageRole={defaultManageRole}
                />

                {/* {selectedUser && (
                    <ViewUserModal
                        isOpen={isViewOpen}
                        onOpenChange={onViewOpenChange}
                        userId={selectedUser.id}
                    />
                )} */}
            </div>

            {isTyping ? (
                <div className="text-center my-20">
                    <Spinner className="mx-auto" />
                </div>
            ) : (
                <Suspense fallback={<SpinnerStyled />}>
                    <div className="bg-white rounded-xl overflow-hidden">
                        <UserTable
                            users={data?.items || []}
                            isLoading={isLoading}
                            emptyMessage="Không tìm thấy nhân viên nào"
                            onViewUser={(user: User) => {
                                setSelectedUser(user)
                                onOpen()
                            }}
                        />
                    </div>
                </Suspense>
            )}

            {(data?.items || []).length > 0 && !isLoading && !isTyping && (
                <PaginationStyled
                    page={pagination.page ?? 1}
                    total={data?.totalPages ?? 10}
                    onChange={(page: number) =>
                        setPagination((prev) => {
                            return {
                                ...prev,
                                page: page
                            }
                        })
                    }
                />
            )}
        </div>
    )
}
