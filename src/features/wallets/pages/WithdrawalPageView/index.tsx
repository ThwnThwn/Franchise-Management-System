"use client"

import { useState, useMemo } from "react"
import {
    useDisclosure,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Spinner,
    addToast
} from "@heroui/react"
import { DollarSign } from "lucide-react"
import dayjs from "dayjs"

import { DashboardHeader } from "@/features/dashboard"
import { ButtonStyled, PaginationStyled, TableStyled } from "@/components"
import { useGetMyWithdrawals } from "@/features/wallets/hooks"
import {
    WithdrawalStatus,
    WithdrawalStatusColors,
    WithdrawalStatusLabels
} from "@/features/wallets/constants"

import { CreateWithdrawalModal, WithdrawalDetailModal } from "@/features/wallets/components"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { PaginationParams } from "@/models/common/request"

export default function WithdrawalPageView() {
    // Hooks Modals
    const { isOpen: isCreateOpen, onOpen: onCreateOpen, onClose: onCreateClose } = useDisclosure()
    const { isOpen: isDetailOpen, onOpen: onDetailOpen, onClose: onDetailClose } = useDisclosure()
    const [selectedId, setSelectedId] = useState<string | null>(null)

    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })

    // Fetch data
    const { data, isLoading } = useGetMyWithdrawals(pagination)

    // Logic chặn tạo lệnh (Quét trong danh sách trang hiện tại)
    // Mặc định API trả về sort theo mới nhất, nên nếu có đơn treo nó sẽ luôn nằm ở page 1
    const hasActiveWithdrawal = useMemo(() => {
        const pendingGroup = [
            WithdrawalStatus.Pending,
            WithdrawalStatus.Approved,
            WithdrawalStatus.Processing
        ]

        return data?.items.some((x) => pendingGroup.includes(x.status))
    }, [data])

    const handleOpenCreate = () => {
        if (hasActiveWithdrawal) {
            addToast({
                title: "Không thể tạo lệnh",
                description:
                    "Bạn đang có một lệnh rút tiền chờ xử lý. Vui lòng đợi hoàn tất hoặc hủy lệnh hiện tại.",
                color: "warning"
            })
            return
        }
        onCreateOpen()
    }

    const handleRowClick = (id: string) => {
        setSelectedId(id)
        onDetailOpen()
    }

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8 space-y-6">
            <DashboardHeader
                icon={<DollarSign className="text-white" />}
                title="Rút Tiền"
                description="Quản lý yêu cầu rút tiền về tài khoản ngân hàng của bạn"
            />

            <div className="flex justify-end">
                <ButtonStyled
                    color="primary"
                    onPress={handleOpenCreate}
                    className="font-bold shadow-md"
                >
                    + Tạo lệnh rút tiền
                </ButtonStyled>
            </div>

            <div className="bg-white rounded-2xl p-6 shadow-sm border border-default-100">
                <TableStyled
                    aria-label="Lịch sử rút tiền"
                    selectionMode="single"
                    onRowAction={(key) => handleRowClick(key.toString())}
                >
                    <TableHeader>
                        <TableColumn>MÃ LỆNH</TableColumn>
                        <TableColumn>THỜI GIAN</TableColumn>
                        <TableColumn>NGÂN HÀNG</TableColumn>
                        <TableColumn align="end">SỐ TIỀN</TableColumn>
                        <TableColumn>TRẠNG THÁI</TableColumn>
                    </TableHeader>
                    <TableBody
                        items={data?.items || []}
                        isLoading={isLoading}
                        loadingContent={<Spinner color="primary" />}
                        emptyContent="Chưa có lịch sử rút tiền nào."
                    >
                        {(item) => (
                            <TableRow
                                key={item.id}
                                className="cursor-pointer hover:bg-default-100 transition-colors"
                            >
                                <TableCell className="font-medium text-default-600">
                                    {item.requestCode}
                                </TableCell>
                                <TableCell>
                                    {dayjs(item.createdAt).format("HH:mm - DD/MM/YYYY")}
                                </TableCell>
                                <TableCell>
                                    <p className="font-semibold text-default-900">
                                        {item.bankName}
                                    </p>
                                    <p className="text-xs text-default-500">
                                        {item.maskedAccountNumber}
                                    </p>
                                </TableCell>
                                <TableCell className="font-bold">
                                    {formatCurrencyWithSymbol(item.amount)}
                                </TableCell>
                                <TableCell>
                                    <Chip
                                        color={WithdrawalStatusColors[item.status]}
                                        variant="flat"
                                    >
                                        {WithdrawalStatusLabels[item.status]}
                                    </Chip>
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </TableStyled>

                {/* Khối Phân Trang chuẩn bài của ní */}
                {(data?.items || []).length > 0 && (data?.totalPages || 0) > 1 && !isLoading && (
                    <PaginationStyled
                        page={pagination.page ?? 1}
                        total={data?.totalPages ?? 1}
                        onChange={(page: number) =>
                            setPagination((prev) => ({
                                ...prev,
                                pageIndex: page
                            }))
                        }
                    />
                )}
            </div>

            <CreateWithdrawalModal isOpen={isCreateOpen} onClose={onCreateClose} />
            <WithdrawalDetailModal id={selectedId} isOpen={isDetailOpen} onClose={onDetailClose} />
        </div>
    )
}
