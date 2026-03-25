"use client"

import {
    Button,
    Chip,
    Table,
    TableBody,
    TableCell,
    TableColumn,
    TableHeader,
    TableRow
} from "@heroui/react"
import { Eye, FileText, Landmark } from "lucide-react"
import React, { useState } from "react"

import { DashboardHeader } from "@/features/dashboard"
import {
    CONTRACT_STATUS_COLORS,
    CONTRACT_STATUS_LABELS,
    ContractStatus,
    DepositStatus
} from "@/features/franchise/constants"
import {
    useConfirmPayment,
    useGetAllContracts,
    useGetMyContracts
} from "@/features/franchise/hooks/useFranchise"
import { FranchiseContractResponse } from "@/features/franchise/models/response"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { PaginationStyled } from "@/components"
import { PaginationParams } from "@/models/common/request"
import { useRouter } from "next/navigation"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"

export default function FranchiseContractsListView() {
    const router = useRouter()
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)

    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })

    const { data, isLoading: isAllLoading } = useGetAllContracts(
        {
            page: pagination.page,
            pageSize: pagination.pageSize
        },
        isAdmin
    )

    const { data: myCons, isLoading: isConsLoading } = useGetMyContracts({ enabled: !isAdmin })

    const cons = (isAdmin ? data?.items : myCons) || []
    const isLoading = isAllLoading || isConsLoading

    const { confirmWithDialog, isPending: isConfirming } = useConfirmPayment()

    const handleViewDetail = (id: string) => {
        const url = isAdmin
            ? `/dashboard/franchise/contracts/${id}`
            : `/profile/franchise/contracts/${id}`

        router.push(url)
    }

    const renderCell = (item: FranchiseContractResponse, columnKey: React.Key) => {
        switch (columnKey) {
            case "id":
                return <span className="font-mono text-tiny uppercase">#{item.id.slice(-6)}</span>
            case "financials":
                return (
                    <div className="flex flex-col">
                        <p className="text-sm font-bold">
                            Phí: {formatCurrencyWithSymbol(item.franchiseFee)}
                        </p>
                        <p className="text-tiny text-default-400">
                            Cọc: {formatCurrencyWithSymbol(item.depositAmount)}
                        </p>
                    </div>
                )
            case "depositStatus":
                return (
                    <Chip
                        size="sm"
                        variant="flat"
                        color={item.depositStatus === DepositStatus.Paid ? "success" : "warning"}
                    >
                        {item.depositStatus === DepositStatus.Paid ? "Đã đóng cọc" : "Chờ tiền"}
                    </Chip>
                )
            case "status":
                return (
                    <Chip size="sm" variant="dot" color={CONTRACT_STATUS_COLORS[item.status]}>
                        {CONTRACT_STATUS_LABELS[item.status]}
                    </Chip>
                )
            case "actions":
                return (
                    <div className="flex items-center justify-center gap-2">
                        <Button
                            isIconOnly
                            size="sm"
                            variant="light"
                            onPress={() => handleViewDetail(item.id)}
                        >
                            <Eye size={18} />
                        </Button>

                        {/* Hiện nút xác nhận cọc nếu đang ở trạng thái chờ tiền */}
                        {isAdmin && item.status === ContractStatus.AwaitingPayment && (
                            <Button
                                isIconOnly
                                size="sm"
                                color="warning"
                                variant="flat"
                                isLoading={isConfirming}
                                onPress={() => confirmWithDialog(item.id)}
                            >
                                <Landmark size={18} />
                            </Button>
                        )}
                    </div>
                )
            default:
                return <span>{String(item[columnKey as keyof FranchiseContractResponse])}</span>
        }
    }

    return (
        <div className="p-6">
            <DashboardHeader
                icon={<FileText className="w-6 h-6 text-white" />}
                title="Hợp đồng & Tài chính"
                description="Quản lý dòng tiền nhượng quyền và trạng thái pháp lý cửa hàng."
            />

            <div className="flex flex-col gap-4">
                <Table aria-label="Contract Table">
                    <TableHeader>
                        <TableColumn key="id">MÃ HĐ</TableColumn>
                        <TableColumn key="financials">TÀI CHÍNH</TableColumn>
                        <TableColumn key="depositStatus">TIỀN CỌC</TableColumn>
                        <TableColumn key="status">TRẠNG THÁI HĐ</TableColumn>
                        <TableColumn key="actions" align="center">
                            THAO TÁC
                        </TableColumn>
                    </TableHeader>
                    <TableBody
                        items={cons}
                        isLoading={isLoading}
                        emptyContent="Không có hợp đồng nào."
                    >
                        {(item) => (
                            <TableRow key={item.id}>
                                {(columnKey) => (
                                    <TableCell>{renderCell(item, columnKey)}</TableCell>
                                )}
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
                {(data?.items || []).length > 0 && !isLoading && (
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
        </div>
    )
}
