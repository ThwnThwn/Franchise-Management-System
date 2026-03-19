import {
    Table,
    TableHeader,
    TableColumn,
    TableBody,
    TableRow,
    TableCell,
    Chip,
    Switch
} from "@heroui/react"
import { Calendar, Ticket } from "lucide-react"
import { CampaignResponse } from "@/features/campaigns/models/response"
import { useDay } from "@/hooks"
import { DATE_TIME_VIEW_FORMAT } from "@/constants/constants"

interface CampaignsTableProps {
    campaigns: CampaignResponse[]
    isLoading: boolean
    onToggleStatus: (id: string, currentIsActive: boolean) => void
    onRowAction: (id: string) => void
}

export const CampaignsTable = ({
    campaigns,
    isLoading,
    onToggleStatus,
    onRowAction
}: CampaignsTableProps) => {
    const { formatDateTime } = useDay({ defaultFormat: DATE_TIME_VIEW_FORMAT })

    return (
        <Table
            aria-label="Danh sách Chiến dịch"
            shadow="none"
            classNames={{
                wrapper: "p-0 border-none",
                th: "bg-default-50 text-default-600 font-bold uppercase tracking-wider text-[10px]",
                td: "py-4 border-b border-default-100 last:border-none align-middle"
            }}
        >
            <TableHeader>
                <TableColumn width="30%">CHIẾN DỊCH SỰ KIỆN</TableColumn>
                <TableColumn width="30%">THỜI GIAN DIỄN RA</TableColumn>
                <TableColumn width="20%">SỐ LƯỢNG VOUCHER</TableColumn>
                <TableColumn width="20%" align="center">
                    TRẠNG THÁI
                </TableColumn>
            </TableHeader>

            <TableBody
                items={campaigns}
                isLoading={isLoading}
                emptyContent={isLoading ? "Đang tải dữ liệu sự kiện..." : "Chưa có chiến dịch nào."}
            >
                {(campaign) => (
                    <TableRow
                        key={campaign.id}
                        onClick={() => onRowAction(campaign.id)}
                        className="hover:bg-default-100 hover:cursor-pointer transition-colors"
                    >
                        {/* Cột 1: Tên & Mô tả */}
                        <TableCell>
                            <div className="flex flex-col gap-1">
                                <span className="font-black text-base text-primary-700 uppercase leading-none">
                                    {campaign.name}
                                </span>
                                <span className="text-xs text-default-500 line-clamp-1">
                                    {campaign.description || "Không có mô tả"}
                                </span>
                            </div>
                        </TableCell>

                        {/* Cột 2: Thời gian chạy */}
                        <TableCell>
                            <div className="flex flex-col gap-1">
                                <div className="flex items-center gap-1.5 text-sm font-semibold text-default-700">
                                    <Calendar size={14} className="text-default-400" />
                                    {campaign.startDate
                                        ? formatDateTime({ date: campaign.startDate })
                                        : "Chưa xác định"}
                                </div>
                                <div className="flex items-center gap-1.5 text-xs font-medium text-default-400 ml-5">
                                    đến{" "}
                                    {campaign.endDate
                                        ? formatDateTime({ date: campaign.endDate })
                                        : "Vô thời hạn"}
                                </div>
                            </div>
                        </TableCell>

                        {/* Cột 3: Số lượng Voucher */}
                        <TableCell>
                            <Chip
                                size="sm"
                                variant="flat"
                                color="warning"
                                startContent={<Ticket size={12} className="ml-1" />}
                                className="font-bold font-mono"
                            >
                                {campaign.vouchers?.length || 0} mã
                            </Chip>
                        </TableCell>

                        {/* Cột 4: Trạng thái (Bật/Tắt) */}
                        <TableCell>
                            <div className="flex justify-center">
                                <Switch
                                    isSelected={campaign.isActive}
                                    size="sm"
                                    color="success"
                                    onValueChange={() =>
                                        onToggleStatus(campaign.id, campaign.isActive)
                                    }
                                    classNames={{ wrapper: "shadow-sm" }}
                                />
                            </div>
                        </TableCell>
                    </TableRow>
                )}
            </TableBody>
        </Table>
    )
}
