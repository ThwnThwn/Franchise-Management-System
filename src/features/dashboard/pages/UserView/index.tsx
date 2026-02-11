"use client"

import React, { Suspense, useCallback, useMemo, useState } from "react"
import { Spinner, useDisclosure } from "@heroui/react"
import { Plus, Search, Users } from "lucide-react"
import { useCreateUser, useGetAllUsers } from "../../hooks"
import {
    ButtonIconStyled,
    EnumPicker,
    InputStyled,
    PaginationStyled,
    SpinnerStyled
} from "@/components"
import { UserTable, ViewUserModal } from "../../components"
import { CreateUserModal } from "../../components/user/CreateUserModal"
import { RoleSlug } from "@/constants/enums"
import { UserFilterParams } from "../../models/user/request"
import { PaginationParams } from "@/models/common/request"
import { RoleSlugLabels } from "@/constants/labels"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { DashboardHeader } from "../../components/general/DashboardHeader"

export default function UserView() {
    // --- Data Fetching ---
    // Assuming useGetAllUsers has 'params' and 'pagination' in its dependency array
    const [filter, setFilter] = useState<UserFilterParams>({})
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading } = useGetAllUsers({
        params: filter,
        pagination,
        enabled: true
    })

    const handleSetFilter = useCallback((newFilter: UserFilterParams) => {
        setPagination({ page: 1, pageSize: 10 })
        setFilter(newFilter)
    }, [])

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
    const {
        isOpen: isCreateOpen,
        onOpen: onCreateOpen,
        onOpenChange: onCreateOpenChange,
        onClose: onCreateClose
    } = useDisclosure()

    const createMutation = useCreateUser({
        onSuccess: onCreateClose
    })

    // --- View detail ---
    const {
        isOpen: isViewOpen,
        onOpen: onViewOpen,
        onOpenChange: onViewOpenChange
        // onClose: onViewClose
    } = useDisclosure()
    const [selectedUserId, setSelectedUserId] = useState<string | undefined>(undefined)

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
                    placeholder="Tìm kiếm sản phẩm..."
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
                    labels={RoleSlugLabels}
                    value={filter.roleSlug}
                    onChange={(e) => {
                        handleSetFilter({ ...filter, roleSlug: e })
                    }}
                    isReadOnly={isLoading}
                    className="w-70"
                    aria-label="Chọn vai trò"
                />

                <ButtonIconStyled
                    variant="bordered"
                    className="btn-gradient rounded-lg"
                    onPress={onCreateOpen}
                >
                    <Plus />
                </ButtonIconStyled>

                {/* Create Modal */}
                <CreateUserModal
                    isOpen={isCreateOpen}
                    onOpenChange={onCreateOpenChange}
                    createMutation={createMutation}
                    isCreateCustomer={false}
                    createRoleSlug={RoleSlug.Staff}
                />

                {selectedUserId && (
                    <ViewUserModal
                        isOpen={isViewOpen}
                        onOpenChange={onViewOpenChange}
                        userId={selectedUserId}
                    />
                )}
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
                            onViewUser={(userId: string) => {
                                setSelectedUserId(userId)
                                onViewOpen()
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
