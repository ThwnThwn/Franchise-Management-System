"use client"

import {
    Button,
    Chip,
    Table,
    TableBody,
    TableCell,
    TableColumn,
    TableHeader,
    TableRow,
    Tooltip
} from "@heroui/react"
import { CheckCircle2, FileText, Eye, Search, XCircle, ArrowRight } from "lucide-react"
import { useRouter } from "next/navigation"
import React, { useState } from "react"

import { InputStyled, PaginationStyled } from "@/components"
import { DashboardHeader } from "@/features/dashboard"
import {
    FRANCHISE_APPLICATION_STATUS_COLORS,
    FRANCHISE_APPLICATION_STATUS_LABELS,
    FranchiseApplicationStatus
} from "@/features/franchise/constants"
import {
    useGetAllApplications,
    useGetMyApplications,
    useReviewApplication
} from "@/features/franchise/hooks/useFranchise"
import { FranchiseApplicationResponse } from "@/features/franchise/models/response"
import { PaginationParams } from "@/models/common/request"
import { RoleSlug } from "@/constants/enums"
import { useTokenStore } from "@/hooks"

export default function FranchiseApplicationsListView() {
    const router = useRouter()
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)

    const [pagination, setPagination] = useState<PaginationParams>({ page: 1, pageSize: 10 })
    const [searchKey, setSearchKey] = useState("")

    const { data, isLoading: isAllLoading } = useGetAllApplications(
        {
            page: pagination.page,
            pageSize: pagination.pageSize,
            searchKey
        },
        isAdmin
    )

    const { data: myApps, isLoading: isMyAppsLoading } = useGetMyApplications({
        enabled: !isAdmin
    })

    const apps = (isAdmin ? data?.items : myApps) || []
    const isLoading = isAllLoading || isMyAppsLoading

    const handleRegisterClick = () => {
        router.push("/franchise/register")
    }

    const handleViewDetail = (id: string) => {
        const url = isAdmin
            ? `/dashboard/franchise/applications/${id}`
            : `/profile/franchise/applications/${id}`

        router.push(url)
    }

    const { reviewWithConfirm } = useReviewApplication()

    const renderCell = (item: FranchiseApplicationResponse, columnKey: React.Key) => {
        switch (columnKey) {
            case "fullName":
                return (
                    <div className="flex flex-col">
                        <p className="text-sm font-bold text-default-700">{item.fullName}</p>
                        <p className="text-tiny text-default-400">{item.email}</p>
                    </div>
                )
            case "storeName":
                return (
                    <div className="flex flex-col">
                        <p className="text-sm font-medium">{item.storeName}</p>
                        <p className="text-tiny text-default-400 truncate max-w-50">
                            {item.storeAddress}
                        </p>
                    </div>
                )
            case "status":
                return (
                    <Chip
                        color={FRANCHISE_APPLICATION_STATUS_COLORS[item.status]}
                        variant="flat"
                        size="sm"
                        className="font-bold uppercase"
                    >
                        {FRANCHISE_APPLICATION_STATUS_LABELS[item.status]}
                    </Chip>
                )
            case "actions":
                return (
                    <div className="flex items-center justify-center gap-2">
                        <Tooltip content="Xem chi tiết">
                            <Button
                                isIconOnly
                                size="sm"
                                variant="light"
                                onPress={() => handleViewDetail(item.id)}
                            >
                                <Eye size={18} className="text-default-500" />
                            </Button>
                        </Tooltip>

                        {isAdmin && item.status === FranchiseApplicationStatus.Pending && (
                            <>
                                <Tooltip content="Duyệt hồ sơ" color="success">
                                    <Button
                                        isIconOnly
                                        size="sm"
                                        variant="light"
                                        color="success"
                                        onPress={() => reviewWithConfirm(item.id, true)}
                                    >
                                        <CheckCircle2 size={18} />
                                    </Button>
                                </Tooltip>
                                <Tooltip content="Từ chối" color="danger">
                                    <Button
                                        isIconOnly
                                        size="sm"
                                        variant="light"
                                        color="danger"
                                        onPress={() => reviewWithConfirm(item.id, false)}
                                    >
                                        <XCircle size={18} />
                                    </Button>
                                </Tooltip>
                            </>
                        )}
                    </div>
                )
            default: {
                const value = item[columnKey as keyof FranchiseApplicationResponse]
                return typeof value === "object" ? null : String(value)
            }
        }
    }

    return (
        <div className="p-6">
            <div className="flex justify-between">
                <DashboardHeader
                    icon={<FileText className="w-6 h-6 text-white" />}
                    title="Hồ sơ Nhượng quyền"
                    description="Thẩm định các mặt bằng và thông tin đối tác đăng ký."
                />
                {!isAdmin && (myApps?.length ?? 0) <= 0 && (
                    <Button
                        color="primary"
                        size="md"
                        className="font-bold px-8 shadow-lg shadow-primary/30"
                        endContent={<ArrowRight size={18} />}
                        onPress={handleRegisterClick}
                    >
                        ĐĂNG KÝ NGAY
                    </Button>
                )}
            </div>

            <div className="flex flex-col gap-4">
                <div className="flex justify-between items-end gap-3">
                    <InputStyled
                        isClearable
                        className="w-full sm:max-w-[44%]"
                        placeholder="Tìm khách hàng, tên quán..."
                        startContent={<Search size={18} />}
                        value={searchKey}
                        onValueChange={setSearchKey}
                    />
                </div>

                <Table aria-label="Franchise Application Table">
                    <TableHeader>
                        <TableColumn key="fullName">ĐỐI TÁC</TableColumn>
                        <TableColumn key="storeName">QUÁN & ĐỊA CHỈ</TableColumn>
                        <TableColumn key="phone">LIÊN HỆ</TableColumn>
                        <TableColumn key="status">TRẠNG THÁI</TableColumn>
                        <TableColumn key="actions" align="center">
                            THAO TÁC
                        </TableColumn>
                    </TableHeader>
                    <TableBody
                        items={apps}
                        isLoading={isLoading}
                        emptyContent="Không có hồ sơ nào."
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
