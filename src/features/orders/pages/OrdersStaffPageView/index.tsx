"use client"

import { useCallback, useEffect, useMemo, useState } from "react"
import { useRouter } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { PaginationParams } from "@/models/common/request"
import { RoleSlug } from "@/constants/enums"
import { debouncedWrapper } from "@/utils/helpers/axiosHelper"
import { PaginationStyled, ButtonStyled } from "@/components"
import { ShoppingBag, BarChart2 } from "lucide-react"
import { useInView } from "react-intersection-observer"

import { DashboardHeader } from "@/features/dashboard"
import { useGetInfiniteStores } from "@/features/stores"
import { GetOrdersQuery } from "@/features/orders/models/request"
import { useGetOrders } from "@/features/orders/hooks"
import { OrdersTable, OrdersToolBar } from "@/features/orders/components"

export default function OrdersStaffPageView() {
    const router = useRouter()
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const storeIdFromToken = useTokenStore((s) => s.storeId)

    const [filter, setFilter] = useState<GetOrdersQuery>({
        storeId: storeIdFromToken || undefined
    })
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading } = useGetOrders({
        params: filter,
        pagination: pagination,
        enabled: roleSlug !== RoleSlug.Customer
    })

    // Search & Filter State
    const { ref, inView } = useInView()
    const {
        data: storesData,
        fetchNextPage: fetchStoresNextPage,
        hasNextPage: hasStoresNextPage,
        isFetchingNextPage: isFetchingStoresNextPage
    } = useGetInfiniteStores({
        params: {},
        pageSize: 10,
        enabled: roleSlug === RoleSlug.Admin
    })
    // Tự động load trang mới khi scroll đến cuối
    useEffect(() => {
        if (inView && hasStoresNextPage && !isFetchingStoresNextPage) {
            fetchStoresNextPage()
        }
    }, [inView, hasStoresNextPage, isFetchingStoresNextPage, fetchStoresNextPage])

    const stores = useMemo(
        () => storesData?.pages.flatMap((page) => page.items) ?? [],
        [storesData]
    )

    const handleSetFilter = useCallback((newFilter: GetOrdersQuery) => {
        setPagination({ page: 1, pageSize: 10 })
        setFilter(newFilter)
    }, [])

    const [isTyping, setIsTyping] = useState(false)
    const debouncedSearch = useMemo(
        () =>
            debouncedWrapper(
                async (params: GetOrdersQuery) => {
                    handleSetFilter(params)
                },
                1000,
                () => setIsTyping(true),
                () => setIsTyping(false)
            ),
        [handleSetFilter]
    )

    return (
        <div className="max-w-7xl mb-8 mx-auto min-h-screen p-4 md:p-6 lg:p-8 bg-slate-50/30">
            {/* --- HEADER --- */}
            <div className="flex items-start justify-between mb-0">
                <DashboardHeader
                    icon={<ShoppingBag className="w-6 h-6 text-white" />}
                    title="Quản lý đơn hàng"
                    description="Quản lý lịch sử, trạng thái đơn hàng..."
                />
                {roleSlug === RoleSlug.Admin && (
                    <ButtonStyled
                        variant="bordered"
                        startContent={<BarChart2 size={17} />}
                        className="shrink-0 mt-1"
                        onPress={() => router.push("/dashboard/orders/analytics")}
                    >
                        Phân tích đơn hàng
                    </ButtonStyled>
                )}
            </div>

            <div className="space-y-3">
                <OrdersToolBar
                    filter={filter}
                    onFilterChange={handleSetFilter}
                    debouncedSearch={debouncedSearch}
                    stores={stores}
                    scrollRef={ref}
                />

                <OrdersTable orders={data?.items || []} isLoading={isLoading} />
            </div>

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
