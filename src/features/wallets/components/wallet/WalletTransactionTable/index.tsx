"use client"

import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Spinner
} from "@heroui/react"
import dayjs from "dayjs"
import { useGetMyTransactions } from "@/features/wallets/hooks"
import { PaginationStyled } from "@/components"
import { PaginationParams } from "@/models/common/request"
import { useState } from "react"
import { formatCurrency, normalizeCurrency } from "@/utils/helpers/currency"

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
                    <TableColumn>MÃ GIAO DỊCH</TableColumn>
                    <TableColumn>THỜI GIAN</TableColumn>
                    <TableColumn>NỘI DUNG</TableColumn>
                    <TableColumn align="end">SỐ TIỀN (Đ)</TableColumn>
                    <TableColumn align="end">SỐ DƯ SAU (Đ)</TableColumn>
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
