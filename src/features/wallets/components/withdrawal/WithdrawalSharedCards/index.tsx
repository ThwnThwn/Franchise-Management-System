"use client"

import { Chip, Button, Image } from "@heroui/react"
import { Eye, EyeOff, FileText, CheckCircle2, Landmark, ShieldCheck } from "lucide-react"
import dayjs from "dayjs"
import { WithdrawalDetailRes } from "@/features/wallets/models/response"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import { WithdrawalStatusColors, WithdrawalStatusLabels } from "@/features/wallets/constants"
import { AlertStyled } from "@/components"

// 1. Khối Tổng quan Số tiền
export const OverviewCard = ({ detail }: { detail: WithdrawalDetailRes }) => (
    <div className="flex justify-between items-center bg-default-50 p-4 rounded-xl border border-default-100 shadow-sm">
        <div>
            <p className="text-sm text-default-500 font-bold uppercase mb-1">Tổng tiền rút</p>
            <p className="text-3xl font-black text-danger">
                {formatCurrencyWithSymbol(detail.amount)}
            </p>
        </div>
        <Chip
            color={WithdrawalStatusColors[detail.status]}
            variant="flat"
            size="lg"
            className="font-bold uppercase tracking-wider"
        >
            {WithdrawalStatusLabels[detail.status]}
        </Chip>
    </div>
)

// 2. Khối Thông tin Ngân hàng
export const BankInfoCard = ({
    detail,
    showRaw,
    onToggleRaw
}: {
    detail: WithdrawalDetailRes
    showRaw: boolean
    onToggleRaw: () => void
}) => (
    <div className="bg-default-50 p-4 rounded-xl border border-default-100 flex flex-col gap-3 shadow-sm border-t-4 border-t-primary">
        <h4 className="text-sm font-bold text-default-900 mb-1 flex items-center gap-2">
            <Landmark size={18} className="text-primary" /> Thông tin thụ hưởng
        </h4>
        <div className="flex justify-between border-b pb-2 items-center">
            <span className="text-default-500">Ngân hàng</span>
            <span className="font-bold text-right">{detail.bankName}</span>
        </div>
        <div className="flex justify-between border-b pb-2 items-center">
            <span className="text-default-500">Số tài khoản</span>
            <div className="flex items-center gap-2">
                <span className="font-bold tracking-widest text-primary">
                    {showRaw ? detail.rawAccountNumber : detail.maskedAccountNumber}
                </span>
                <Button isIconOnly size="sm" variant="light" onPress={onToggleRaw}>
                    {showRaw ? <EyeOff size={16} /> : <Eye size={16} />}
                </Button>
            </div>
        </div>
        <div className="flex justify-between pb-1 items-center">
            <span className="text-default-500">Chủ tài khoản</span>
            <span className="font-bold uppercase text-right">{detail.accountName}</span>
        </div>
    </div>
)

// 3. Khối Chi tiết & Timeline
export const TimelineCard = ({ detail }: { detail: WithdrawalDetailRes }) => (
    <div className="bg-default-50 p-4 rounded-xl border border-default-100 flex flex-col gap-3 shadow-sm">
        <h4 className="text-sm font-bold text-default-900 mb-1 flex items-center gap-2">
            <FileText size={18} /> Chi tiết giao dịch
        </h4>
        <div className="flex justify-between border-b pb-2">
            <span className="text-default-500">Mã lệnh</span>
            <span className="font-semibold">{detail.requestCode}</span>
        </div>
        <div className="flex justify-between border-b pb-2">
            <span className="text-default-500">Ngày tạo lệnh</span>
            <span className="font-semibold text-right">
                {dayjs(detail.createdAt).format("HH:mm - DD/MM/YYYY")}
            </span>
        </div>
        {Number(detail.feeAmount) > 0 && (
            <div className="flex justify-between border-b pb-2">
                <span className="text-default-500">Phí giao dịch</span>
                <span className="font-semibold text-danger">
                    {formatCurrencyWithSymbol(detail.feeAmount)}
                </span>
            </div>
        )}
        {detail.notes && (
            <div className="flex flex-col pb-2">
                <span className="text-default-500 mb-1">Ghi chú của khách</span>
                <span className="font-semibold bg-white p-2 border rounded-md">{detail.notes}</span>
            </div>
        )}
        {detail.bankTransactionId && (
            <div className="flex justify-between pb-1">
                <span className="text-default-500">Mã GD Ngân hàng</span>
                <span className="font-bold text-success text-right">
                    {detail.bankTransactionId}
                </span>
            </div>
        )}
    </div>
)

// 4. Khối Audit Trail (Chỉ Admin)
export const AuditTrailCard = ({ detail }: { detail: WithdrawalDetailRes }) => {
    if (!detail.processedByName && !detail.completedAt && !detail.internalNotes) return null
    return (
        <div className="bg-slate-100 p-4 rounded-xl border border-slate-200 flex flex-col gap-3">
            <h4 className="text-sm font-bold text-slate-700 mb-1 flex items-center gap-2">
                <ShieldCheck size={18} /> Lưu vết hệ thống (Nội bộ)
            </h4>
            {detail.processedByName && (
                <div className="flex justify-between border-b border-slate-200 pb-2">
                    <span className="text-slate-500">Người xử lý</span>
                    <span className="font-bold text-slate-800">{detail.processedByName}</span>
                </div>
            )}
            {detail.completedAt && (
                <div className="flex justify-between border-b border-slate-200 pb-2">
                    <span className="text-slate-500">Hoàn tất lúc</span>
                    <span className="font-bold text-slate-800">
                        {dayjs(detail.completedAt).format("HH:mm - DD/MM/YYYY")}
                    </span>
                </div>
            )}
            {detail.internalNotes && (
                <div className="flex flex-col pb-1">
                    <span className="text-slate-500 mb-1">Ghi chú nội bộ</span>
                    <span className="font-semibold text-slate-800 italic bg-white p-2 rounded border">
                        {detail.internalNotes}
                    </span>
                </div>
            )}
        </div>
    )
}

// 5. Khối Lời nhắn & Cảnh báo (Alerts)
export const AlertCard = ({ detail }: { detail: WithdrawalDetailRes }) => {
    if (!detail.rejectionReason && !detail.cancellationReason && !detail.approvalNotes) return null
    const isSuccess = !!detail.approvalNotes

    return (
        <AlertStyled
            title={isSuccess ? "Ghi chú từ Quản trị viên" : "Lý do hủy/từ chối"}
            description={
                detail.rejectionReason || detail.cancellationReason || detail.approvalNotes
            }
            color={isSuccess ? "primary" : "danger"}
            className="max-w-full py-4"
        />
    )
}

// 6. Khối Biên lai
export const ReceiptCard = ({ detail }: { detail: WithdrawalDetailRes }) => {
    if (!detail.receiptImageUrl) return null
    return (
        <div className="flex flex-col gap-2 bg-success-50 p-4 rounded-xl border border-success-200 shadow-sm">
            <span className="text-sm font-bold text-success-700 uppercase flex items-center gap-2">
                <CheckCircle2 size={18} /> Biên lai chuyển khoản
            </span>
            <div className="flex justify-center bg-white p-2 rounded-lg border">
                <Image
                    src={detail.receiptImageUrl}
                    alt="Biên lai"
                    className="max-h-60 object-contain rounded-md"
                />
            </div>
        </div>
    )
}
