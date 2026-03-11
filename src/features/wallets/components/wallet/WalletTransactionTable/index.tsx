"use client"

import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Spinner,
    Chip
} from "@heroui/react"
import dayjs from "dayjs"
import { useGetMyTransactions } from "@/features/wallets/hooks"
import { PaginationStyled } from "@/components"
import { PaginationParams } from "@/models/common/request"
import { useState } from "react"
import { formatCurrency, normalizeCurrency } from "@/utils/helpers/currency"
import { TransactionStatusLabels, TransactionTypeColors } from "@/features/wallets/constants"

export const WalletTransactionTable = () => {
    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const { data, isLoading } = useGetMyTransactions({ ...pagination })

    const renderAmount = (amountStr: string) => {
        const amount = normalizeCurrency(amountStr)
        if (amount > 0)
            return <span className="text-success font-bold">+{formatCurrency(amount)}</span>
        if (amount < 0)
            return <span className="text-danger font-bold">{formatCurrency(amount)}</span>
        return <span>0</span>
    }

    if (isLoading) return <Spinner className="flex justify-center my-10" color="primary" />

    return (
        <>
            <Table aria-label="Biến động số dư" className="mt-4">
                <TableHeader>
                    <TableColumn width="15%">MÃ GIAO DỊCH</TableColumn>
                    <TableColumn width="15%">THỜI GIAN</TableColumn>
                    <TableColumn width="30%">NỘI DUNG</TableColumn>
                    <TableColumn width="12%" align="end">
                        SỐ TIỀN (Đ)
                    </TableColumn>
                    <TableColumn width="15%" align="end">
                        SỐ DƯ SAU (Đ)
                    </TableColumn>
                    <TableColumn width="13%">TRẠNG THÁI</TableColumn>
                </TableHeader>
                <TableBody items={data?.items || []} emptyContent="Chưa có giao dịch nào.">
                    {(item) => (
                        <TableRow key={item.id}>
                            <TableCell className="font-medium text-default-600">
                                {item.transactionCode}
                            </TableCell>
                            <TableCell>
                                {dayjs(item.createdAt).format("HH:mm - DD/MM/YYYY")}
                            </TableCell>
                            <TableCell>{item.description}</TableCell>
                            <TableCell>{renderAmount(item.amount)}</TableCell>
                            <TableCell className="text-default-500">
                                {formatCurrency(item.balanceAfter)}
                            </TableCell>
                            <TableCell>
                                <Chip variant="bordered" color={TransactionTypeColors[item.status]}>
                                    {TransactionStatusLabels[item.status]}
                                </Chip>
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
            {(data?.items || []).length > 0 && (data?.totalPages || 0) > 1 && !isLoading && (
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
        </>
    )
}
