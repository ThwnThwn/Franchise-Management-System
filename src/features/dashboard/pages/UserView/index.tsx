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
import { UserTable } from "../../components"
import { CreateUserModal } from "../../components/user/CreateUserModal"
import { RoleSlug } from "@/constants/enums"
import { UserFilterParams } from "../../models/user/request"
import { PaginationParams } from "@/models/common/request"
import { RoleSlugLabels } from "@/constants/labels"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"

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

    return (
        <div>
            <div className="min-h-screen p-4 md:p-6 lg:p-8">
                <div className="max-w-7xl mx-auto">
                    {/* Page Header */}
                    <div className="mb-8">
                        <div className="flex items-center gap-3 mb-2">
                            <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                                <Users className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-3xl lg:text-4xl font-bold text-foreground">
                                    Quản lý người dùng
                                </h1>
                                <p className="text-foreground/60 mt-1">
                                    Quản lý thông tin nhân viên và ca làm việc
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* --- Filter Section --- */}
                    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 bg-white p-4 rounded-xl border border-gray-100 shadow-sm">
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
                                handleSetFilter({ ...filter, roleSlug: e as RoleSlug })
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
                                />
                            </div>
                        </Suspense>
                    )}

                    {(data?.items || []).length > 0 && !isLoading && !isTyping && (
                        <div className="mt-6 flex justify-center">
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
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}
