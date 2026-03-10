"use client"

import { useState, useMemo } from "react"
import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Spinner,
    Button
} from "@heroui/react"
import { Activity, Search, X } from "lucide-react"
import dayjs from "dayjs"

import { DashboardHeader } from "@/features/dashboard"
import { PaginationStyled, InputStyled } from "@/components"
import {
    formatCurrencyWithSymbol,
    formatCurrency,
    normalizeCurrency
} from "@/utils/helpers/currency"

import { useGetAllTransactions, useGetTransactionByCode } from "@/features/wallets/hooks"
import {
    TransactionStatusLabels,
    TransactionStatusColors,
    TransactionTypeLabels,
    TransactionTypeColors
} from "@/features/wallets/constants"
import { WalletTransactionResponse } from "@/features/wallets/models/response"
import { useHydrateUsers, UserCell } from "@/features/users"

export default function AdminTransactionListPageView() {
    const [pagination, setPagination] = useState({ pageIndex: 1, pageSize: 20 })

    const [inputValue, setInputValue] = useState("")
    const [searchQuery, setSearchQuery] = useState("")

    const isSearching = searchQuery.length > 0

    // Hành động kích hoạt tìm kiếm
    const triggerSearch = () => {
        setSearchQuery(inputValue.trim())
        if (!isSearching) setPagination((p) => ({ ...p, pageIndex: 1 }))
    }

    const clearSearch = () => {
        setInputValue("")
        setSearchQuery("")
    }

    // 2. Fetch API Danh sách (Tắt khi đang search)
    const { data: allData, isLoading: isLoadingAll } = useGetAllTransactions(
        pagination,
        !isSearching
    )

    // 3. Fetch API Exact Match (Bật khi có searchQuery)
    const { data: singleData, isLoading: isLoadingSingle } = useGetTransactionByCode(
        searchQuery,
        isSearching
    )

    // 4. Logic Gộp Data cho Bảng
    const tableItems = useMemo(() => {
        if (isSearching) {
            return singleData ? [singleData] : []
        }
        return allData?.items || []
    }, [isSearching, singleData, allData])

    const { getUserInfo } = useHydrateUsers(tableItems, (item) => item.userId)

    const isLoading = isSearching ? isLoadingSingle : isLoadingAll

    // Render Số tiền
    const renderAmount = (item: WalletTransactionResponse) => {
        const amount = normalizeCurrency(item.amount)
        const balanceBefore = normalizeCurrency(item.balanceBefore)
        const balanceAfter = normalizeCurrency(item.balanceAfter)

        const isCredit = balanceAfter > balanceBefore

        if (amount === 0 || balanceAfter === balanceBefore) {
            return <span className="text-default-500 font-bold">{formatCurrencyWithSymbol(0)}</span>
        }

        if (isCredit) {
            return (
                <span className="text-success font-black">+{formatCurrencyWithSymbol(amount)}</span>
            )
        }
        return <span className="text-danger font-black">-{formatCurrencyWithSymbol(amount)}</span>
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 space-y-6">
            <DashboardHeader
                icon={<Activity className="text-white" />}
                title="Sổ Giao Dịch Hệ Thống"
                description="Đối soát và theo dõi toàn bộ biến động số dư của người dùng"
            />

            <div className="bg-white rounded-2xl p-4 shadow-sm border border-default-100 flex flex-col gap-4">
                {/* TOOLBAR TÌM KIẾM CHUẨN ADMIN */}
                <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
                    <div className="flex items-center gap-2 w-full max-w-md">
                        <InputStyled
                            placeholder="Nhập chính xác Mã Giao Dịch..."
                            value={inputValue}
                            onValueChange={setInputValue}
                            // Bấm Enter để search luôn cho pro
                            onKeyDown={(e) => {
                                if (e.key === "Enter") triggerSearch()
                            }}
                            className="flex-1"
                        />
                        <Button
                            color="primary"
                            isIconOnly
                            onPress={triggerSearch}
                            isDisabled={!inputValue.trim() && !searchQuery}
                        >
                            <Search size={18} />
                        </Button>
                        {isSearching && (
                            <Button variant="flat" color="danger" isIconOnly onPress={clearSearch}>
                                <X size={18} />
                            </Button>
                        )}
                    </div>
                </div>

                {/* BẢNG DỮ LIỆU */}
                <Table
                    aria-label="Sổ giao dịch toàn sàn"
                    classNames={{ wrapper: "shadow-none border border-default-200" }}
                >
                    <TableHeader>
                        <TableColumn>MÃ GIAO DỊCH</TableColumn>
                        <TableColumn>THỜI GIAN</TableColumn>
                        <TableColumn>NGƯỜI DÙNG</TableColumn>
                        <TableColumn>LOẠI GIAO DỊCH</TableColumn>
                        <TableColumn align="end">BIẾN ĐỘNG</TableColumn>
                        <TableColumn align="end">DƯ TRƯỚC {`->`} SAU</TableColumn>
                        <TableColumn>TRẠNG THÁI</TableColumn>
                    </TableHeader>
                    <TableBody
                        items={tableItems}
                        isLoading={isLoading}
                        loadingContent={<Spinner color="primary" />}
                        emptyContent={
                            isSearching
                                ? `Không tìm thấy mã "${searchQuery}".`
                                : "Chưa có giao dịch nào."
                        }
                    >
                        {(item) => {
                            return (
                                <TableRow key={item.id} className="hover:bg-default-50">
                                    <TableCell>
                                        <span className="font-bold text-primary">
                                            {item.transactionCode}
                                        </span>
                                        {item.referenceId && (
                                            <p className="text-xs text-default-400 mt-1">
                                                Ref: {item.referenceId.substring(0, 8)}
                                            </p>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {dayjs(item.createdAt).format("HH:mm DD/MM/YYYY")}
                                    </TableCell>
                                    <TableCell className="font-mono text-xs text-default-500">
                                        <UserCell
                                            user={getUserInfo(item.userId)}
                                            userId={item.userId}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            size="sm"
                                            variant="flat"
                                            color={TransactionTypeColors[item.transactionType]}
                                        >
                                            {TransactionTypeLabels[item.transactionType]}
                                        </Chip>
                                        <p
                                            className="text-xs text-default-500 mt-1 max-w-37.5 truncate"
                                            title={item.description}
                                        >
                                            {item.description}
                                        </p>
                                    </TableCell>
                                    <TableCell>{renderAmount(item)}</TableCell>
                                    <TableCell className="text-xs font-mono text-default-600">
                                        {formatCurrency(item.balanceBefore)}
                                        <span className="mx-1 text-default-400">{`->`}</span>
                                        <span className="font-bold">
                                            {formatCurrency(item.balanceAfter)}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            size="sm"
                                            variant="dot"
                                            color={TransactionStatusColors[item.status]}
                                        >
                                            {TransactionStatusLabels[item.status]}
                                        </Chip>
                                    </TableCell>
                                </TableRow>
                            )
                        }}
                    </TableBody>
                </Table>

                {/* PHÂN TRANG (Chỉ hiện khi KHÔNG TÌM KIẾM) */}
                {!isSearching && (allData?.totalPages || 0) > 1 && !isLoading && (
                    <PaginationStyled
                        page={pagination.pageIndex ?? 1}
                        total={allData?.totalPages ?? 1}
                        onChange={(page: number) =>
                            setPagination((prev) => ({ ...prev, pageIndex: page }))
                        }
                    />
                )}
            </div>
        </div>
    )
}
