"use client"

import {
    Button,
    Card,
    CardBody,
    Chip,
    Image,
    Link,
    Select,
    SelectItem,
    Spinner
} from "@heroui/react"
import {
    Construction,
    Download,
    ExternalLink,
    FileText,
    ImageIcon,
    Maximize2,
    Store,
    Wallet
} from "lucide-react"
import { useRouter } from "next/navigation"
import React from "react"

import { DashboardHeader } from "@/features/dashboard"
import {
    CONTRACT_SETUP_STATUS_LABELS,
    CONTRACT_STATUS_COLORS,
    CONTRACT_STATUS_LABELS,
    ContractSetupStatus,
    DepositStatus
} from "@/features/franchise/constants"
import {
    useConfirmPayment,
    useGetApplicationDetail,
    useGetContractDetail, // <--- Thêm hook này
    useUpdateSetupStatus
} from "@/features/franchise/hooks/useFranchise"
import { formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import dayjs from "dayjs"

interface Props {
    id: string
}

export default function ContractDetailAdminView({ id }: Props) {
    const router = useRouter()

    // 1. Lấy data Contract
    const { data: contract, isLoading: isLoadingContract } = useGetContractDetail(id)

    // 2. Lấy thêm data Application để lấy ảnh (chỉ chạy khi đã có contract)
    const { data: appData, isLoading: isLoadingApp } = useGetApplicationDetail(
        contract?.applicationId || "",
        !!contract?.applicationId
    )

    // 3. Các hooks cập nhật
    const { mutate: updateSetup, isPending: isUpdatingSetup } = useUpdateSetupStatus()
    const { confirmWithDialog, isPending: isConfirmingPayment } = useConfirmPayment()

    if (isLoadingContract || (contract && isLoadingApp))
        return <Spinner className="flex justify-center p-12" label="Đang tải dữ liệu..." />

    if (!contract) return <div className="p-12 text-center">Không tìm thấy dữ liệu hợp đồng.</div>

    const application = appData?.application

    const handleSetupStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const newStatus = Number(e.target.value) as ContractSetupStatus
        updateSetup({ id, req: { setupStatus: newStatus } })
    }

    return (
        <div className="max-w-7xl mx-auto p-6 space-y-6">
            <DashboardHeader
                icon={<FileText className="w-6 h-6 text-white" />}
                title={`Hợp đồng #${contract.id.slice(-6).toUpperCase()}`}
                description="Theo dõi tiến độ thanh toán và thi công cửa hàng."
            />

            <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                {/* --- CỘT TRÁI (8): CHI TIẾT, ẢNH & TIẾN ĐỘ --- */}
                <div className="lg:col-span-8 space-y-6">
                    {/* 1. Trạng thái tổng quan */}
                    <Card className="border-none bg-default-50/50 shadow-none">
                        <CardBody className="p-6 flex flex-row justify-between items-center">
                            <div className="space-y-1">
                                <p className="text-tiny uppercase font-black text-default-400">
                                    Trạng thái hiện tại
                                </p>
                                <Chip
                                    color={CONTRACT_STATUS_COLORS[contract.status]}
                                    variant="flat"
                                    className="font-bold px-4"
                                >
                                    {CONTRACT_STATUS_LABELS[contract.status]}
                                </Chip>
                            </div>
                            <div className="text-right">
                                <p className="text-tiny uppercase font-black text-default-400">
                                    Ngày tạo
                                </p>
                                <p className="font-bold">
                                    {dayjs(contract.createdAt).format("DD/MM/YYYY")}
                                </p>
                            </div>
                        </CardBody>
                    </Card>

                    {/* 2. Hình ảnh hiện trạng (PHẦN MỚI THÊM) */}
                    {application && (
                        <Card>
                            <CardBody className="p-6 space-y-4">
                                <h4 className="flex items-center gap-2 font-bold text-warning-600 border-b pb-2 uppercase text-xs tracking-wider">
                                    <ImageIcon size={16} /> Hình ảnh hiện trạng mặt bằng
                                </h4>
                                <div className="grid grid-cols-2 md:grid-cols-3 gap-4 items-start">
                                    {/* Ảnh mặt tiền */}
                                    <div className="space-y-2">
                                        <Image
                                            src={application.storeFrontUrl}
                                            alt="Mặt tiền"
                                            className="rounded-xl w-full h-auto border-2 border-default-100 bg-default-50"
                                        />
                                        <p className="text-[10px] font-black text-center uppercase text-default-500">
                                            Ảnh mặt tiền
                                        </p>
                                    </div>

                                    {/* Danh sách ảnh sổ đỏ */}
                                    {application.landCertificateUrls.map((url, index) => (
                                        <div key={index} className="space-y-2">
                                            <Image
                                                src={url}
                                                alt={`Sổ đỏ ${index + 1}`}
                                                className="rounded-xl w-full h-auto border-2 border-default-100 bg-default-50"
                                            />
                                            <p className="text-[10px] font-black text-center uppercase text-default-500">
                                                Giấy tờ đất {index + 1}
                                            </p>
                                        </div>
                                    ))}
                                </div>
                            </CardBody>
                        </Card>
                    )}

                    {/* 3. Thông tin tài chính */}
                    <Card>
                        <CardBody className="p-6 space-y-6">
                            <h4 className="flex items-center gap-2 font-bold text-primary-600 border-b pb-2 uppercase text-xs tracking-wider">
                                <Wallet size={16} /> Chi tiết tài chính
                            </h4>
                            <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
                                <div>
                                    <p className="text-xs text-default-500">Phí nhượng quyền</p>
                                    <p className="text-lg font-black">
                                        {formatCurrencyWithSymbol(contract.franchiseFee)}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs text-default-500">Tiền cọc (Hoàn lại)</p>
                                    <p className="text-lg font-black">
                                        {formatCurrencyWithSymbol(contract.depositAmount)}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs text-default-500">Trạng thái cọc</p>
                                    <div className="flex items-center gap-2 mt-1">
                                        <Chip
                                            size="sm"
                                            color={
                                                contract.depositStatus === DepositStatus.Paid
                                                    ? "success"
                                                    : "warning"
                                            }
                                        >
                                            {contract.depositStatus === DepositStatus.Paid
                                                ? "Đã thanh toán"
                                                : "Chờ thanh toán"}
                                        </Chip>
                                        {contract.depositStatus ===
                                            DepositStatus.AwaitingPayment && (
                                            <Button
                                                size="sm"
                                                color="warning"
                                                variant="flat"
                                                className="h-7 px-2"
                                                onPress={() => confirmWithDialog(contract.id)}
                                                isLoading={isConfirmingPayment}
                                            >
                                                Xác nhận nhận tiền
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </CardBody>
                    </Card>

                    {/* 4. Tài liệu & Hợp đồng */}
                    <Card>
                        <CardBody className="p-6 space-y-4">
                            <h4 className="flex items-center gap-2 font-bold text-default-700 border-b pb-2 uppercase text-xs tracking-wider">
                                <FileText size={16} /> Văn bản pháp lý
                            </h4>
                            <div className="flex items-center justify-between p-4 border border-default-200 rounded-xl bg-default-50/30">
                                <div className="flex items-center gap-3">
                                    <div className="p-2 bg-danger-100 text-danger rounded-lg">
                                        <FileText size={24} />
                                    </div>
                                    <div>
                                        <p className="text-sm font-bold">
                                            Hợp đồng nhượng quyền điện tử
                                        </p>
                                        <p className="text-tiny text-default-400">
                                            ID DocuSeal: {contract.docuSealSubmissionId}
                                        </p>
                                    </div>
                                </div>
                                <div className="flex gap-2">
                                    <Button
                                        as={Link}
                                        href={contract.contractPdfUrl}
                                        target="_blank"
                                        size="sm"
                                        variant="flat"
                                        startContent={<Maximize2 size={14} />}
                                    >
                                        Toàn màn hình
                                    </Button>
                                    <Button
                                        as="a"
                                        // Chèn fl_attachment vào sau /upload/
                                        href={contract.contractPdfUrl.replace(
                                            "/upload/",
                                            "/upload/fl_attachment/"
                                        )}
                                        // Thuộc tính download vẫn nên giữ để làm fallback
                                        download={`Hop_Dong_${contract.id}.pdf`}
                                        size="sm"
                                        color="primary"
                                        startContent={<Download size={14} />}
                                    >
                                        Tải xuống
                                    </Button>
                                </div>
                            </div>
                        </CardBody>
                    </Card>
                </div>

                {/* --- CỘT PHẢI (4): QUẢN LÝ SETUP --- */}
                <div className="lg:col-span-4 space-y-6">
                    <Card className="border-none shadow-lg">
                        <CardBody className="p-6 space-y-6">
                            <div className="flex items-center gap-2 font-bold uppercase text-xs tracking-widest text-primary-200">
                                <Construction size={18} /> Cập nhật tiến độ quán
                            </div>
                            <div className="space-y-4">
                                <Select
                                    label="Trạng thái thi công"
                                    variant="bordered"
                                    selectedKeys={[contract.setupStatus.toString()]}
                                    onChange={handleSetupStatusChange}
                                    isLoading={isUpdatingSetup}
                                    disallowEmptySelection
                                >
                                    {Object.entries(CONTRACT_SETUP_STATUS_LABELS).map(
                                        ([key, label]) => (
                                            <SelectItem key={key} textValue={label}>
                                                {label}
                                            </SelectItem>
                                        )
                                    )}
                                </Select>

                                <div className="p-4 rounded-xl bg-white/10 border border-white/20 space-y-4">
                                    <div className="flex justify-between text-sm">
                                        <span className="text-primary-200">
                                            Khai trương dự kiến:
                                        </span>
                                        <span className="font-bold">
                                            {contract.validFrom
                                                ? dayjs(contract.validFrom).format("DD/MM/YYYY")
                                                : "---"}
                                        </span>
                                    </div>
                                    <div className="flex justify-between text-sm">
                                        <span className="text-primary-200">Hết hạn HĐ:</span>
                                        <span className="font-bold">
                                            {contract.validUntil
                                                ? dayjs(contract.validUntil).format("DD/MM/YYYY")
                                                : "---"}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </CardBody>
                    </Card>

                    <Card>
                        <CardBody className="p-4 space-y-3">
                            <h5 className="text-xs font-black uppercase text-default-400">
                                Liên kết hồ sơ
                            </h5>
                            <Button
                                variant="flat"
                                fullWidth
                                className="justify-between"
                                onPress={() =>
                                    router.push(
                                        `/dashboard/franchise/applications/${contract.applicationId}`
                                    )
                                }
                            >
                                <span className="flex items-center gap-2">
                                    <Store size={16} /> Xem lại Hồ sơ gốc
                                </span>
                                <ExternalLink size={14} />
                            </Button>
                        </CardBody>
                    </Card>
                </div>
            </div>
        </div>
    )
}
