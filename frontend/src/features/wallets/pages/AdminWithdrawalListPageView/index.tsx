"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Spinner,
    Tabs,
    Tab
} from "@heroui/react"
import { DollarSign } from "lucide-react"
import dayjs from "dayjs"

import { DashboardHeader } from "@/features/dashboard"
import { PaginationStyled } from "@/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"

import { useGetAllWithdrawals } from "@/features/wallets/hooks"
import {
    WithdrawalStatus,
    WithdrawalStatusLabels,
    WithdrawalStatusColors
} from "@/features/wallets/constants"
import { PaginationParams } from "@/models/common/request"
import { useHydrateUsers, UserCell } from "@/features/users"

export default function AdminWithdrawalListPageView() {
    const router = useRouter()

    // State Filter & Phân trang
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 20 })
    const [statusFilter, setStatusFilter] = useState<string>("ALL")

    // Gọi hook GetAll (có truyền filter vào API nếu BE hỗ trợ)
    const { data, isLoading } = useGetAllWithdrawals({
        ...pagination,
        status: statusFilter !== "ALL" ? Number(statusFilter) : undefined
    })

    const { getUserInfo } = useHydrateUsers(data?.items, (item) => item.userId)

    const handleRowClick = (id: string) => {
        router.push(`/dashboard/wallets/withdrawals/${id}`)
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 space-y-6">
            <DashboardHeader
                icon={<DollarSign className="text-white" />}
                title="Quản lý Lệnh Rút Tiền"
                description="Danh sách yêu cầu rút tiền từ người dùng toàn hệ thống"
            />

            <div className="bg-white rounded-2xl p-4 shadow-sm border border-default-100 flex flex-col gap-4">
                {/* BỘ LỌC TRẠNG THÁI */}
                <Tabs
                    aria-label="Lọc trạng thái"
                    color="primary"
                    variant="underlined"
                    selectedKey={statusFilter}
                    onSelectionChange={(key) => {
                        setStatusFilter(key.toString())
                        setPagination((p) => ({ ...p, pageIndex: 1 })) // Reset về trang 1 khi đổi filter
                    }}
                >
                    <Tab key="ALL" title="Tất cả" />
                    <Tab key={WithdrawalStatus.Pending.toString()} title="Chờ duyệt" />
                    <Tab key={WithdrawalStatus.Approved.toString()} title="Đã duyệt (Chờ CK)" />
                    <Tab key={WithdrawalStatus.Completed.toString()} title="Hoàn tất" />
                    <Tab key={WithdrawalStatus.Cancelled.toString()} title="Đã hủy" />
                </Tabs>

                {/* BẢNG DỮ LIỆU */}
                <Table
                    aria-label="Danh sách lệnh rút"
                    selectionMode="single"
                    onRowAction={(key) => handleRowClick(key.toString())}
                    classNames={{ wrapper: "shadow-none border border-default-200" }}
                >
                    <TableHeader>
                        <TableColumn>MÃ LỆNH</TableColumn>
                        <TableColumn>NGƯỜI DÙNG</TableColumn>
                        <TableColumn>THỜI GIAN</TableColumn>
                        <TableColumn>NGÂN HÀNG</TableColumn>
                        <TableColumn align="end">SỐ TIỀN</TableColumn>
                        <TableColumn>TRẠNG THÁI</TableColumn>
                    </TableHeader>
                    <TableBody
                        items={data?.items || []}
                        isLoading={isLoading}
                        loadingContent={<Spinner color="primary" />}
                        emptyContent="Không có yêu cầu rút tiền nào."
                    >
                        {(item) => {
                            return (
                                <TableRow
                                    key={item.id}
                                    className="cursor-pointer hover:bg-default-50 transition-colors"
                                >
                                    <TableCell className="font-bold text-primary">
                                        {item.requestCode}
                                    </TableCell>
                                    <TableCell className="text-sm">
                                        <UserCell
                                            user={getUserInfo(item.userId)}
                                            userId={item.userId}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        {dayjs(item.createdAt).format("HH:mm DD/MM/YYYY")}
                                    </TableCell>
                                    <TableCell>
                                        <p className="font-semibold text-default-900">
                                            {item.bankName}
                                        </p>
                                        <p className="text-xs text-default-500 font-mono">
                                            {item.maskedAccountNumber}
                                        </p>
                                    </TableCell>
                                    <TableCell className="font-black text-danger">
                                        {formatCurrencyWithSymbol(item.amount)}
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            size="sm"
                                            color={WithdrawalStatusColors[item.status]}
                                            variant="flat"
                                            className="font-bold"
                                        >
                                            {WithdrawalStatusLabels[item.status]}
                                        </Chip>
                                    </TableCell>
                                </TableRow>
                            )
                        }}
                    </TableBody>
                </Table>

                {/* PHÂN TRANG */}
                {(data?.totalPages || 0) > 1 && !isLoading && (
                    <PaginationStyled
                        page={pagination.page ?? 1}
                        total={data?.totalPages ?? 1}
                        onChange={(page: number) =>
                            setPagination((prev) => ({ ...prev, pageIndex: page }))
                        }
                    />
                )}
            </div>
        </div>
    )
}
