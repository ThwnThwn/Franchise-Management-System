"use client"

import { Card, CardBody, Chip, Image, Spinner } from "@heroui/react"
import { FileText, ImageIcon, MapPin, User } from "lucide-react"

import { ApplicationReviewActions } from "@/features/franchise/components"
import {
    FRANCHISE_APPLICATION_STATUS_COLORS,
    FRANCHISE_APPLICATION_STATUS_LABELS
} from "@/features/franchise/constants"
import { useGetApplicationDetail } from "@/features/franchise/hooks/useFranchise"
import { DashboardHeader } from "@/features/dashboard"

interface Props {
    id: string
}

export default function ApplicationDetailAdminView({ id }: Props) {
    const { data, isLoading } = useGetApplicationDetail(id)

    if (isLoading) {
        return (
            <div className="w-full h-[60vh] flex items-center justify-center">
                <Spinner label="Đang tải thông tin hồ sơ..." color="primary" labelColor="primary" />
            </div>
        )
    }

    if (!data) return <div className="p-8 text-center font-bold">Không tìm thấy dữ liệu hồ sơ.</div>

    const { application } = data

    return (
        <div className="max-w-7xl mx-auto p-4 md:p-6 space-y-6">
            {/* --- HEADER --- */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <DashboardHeader
                    icon={<FileText className="w-6 h-6 text-white" />}
                    title={`Chi tiết hồ sơ #${application.id.slice(-6).toUpperCase()}`}
                    description="Theo dõi đơn đăng kí nhượng quyền."
                />
                <Chip
                    color={FRANCHISE_APPLICATION_STATUS_COLORS[application.status]}
                    variant="flat"
                    className="font-bold uppercase px-4"
                >
                    {FRANCHISE_APPLICATION_STATUS_LABELS[application.status]}
                </Chip>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                {/* --- CỘT TRÁI (8): THÔNG TIN CHI TIẾT & HÌNH ẢNH --- */}
                <div className="lg:col-span-8 space-y-6">
                    {/* Thẻ thông tin chữ */}
                    <Card shadow="sm" className="border border-default-100">
                        <CardBody className="p-6 grid grid-cols-1 md:grid-cols-2 gap-8">
                            {/* Khối Khách hàng */}
                            <div className="space-y-4">
                                <h4 className="flex items-center gap-2 font-black text-primary-700 border-b border-default-100 pb-2 uppercase text-xs tracking-wider">
                                    <User size={16} /> Thông tin khách hàng
                                </h4>
                                <div className="space-y-2">
                                    <p className="text-sm">
                                        <b>Họ tên:</b> {application.fullName}
                                    </p>
                                    <p className="text-sm">
                                        <b>Số điện thoại:</b> {application.phone}
                                    </p>
                                    <p className="text-sm">
                                        <b>Số CCCD:</b> {application.identityNumber}
                                    </p>
                                    <p className="text-sm">
                                        <b>Email:</b> {application.email}
                                    </p>
                                </div>
                            </div>

                            {/* Khối Mặt bằng */}
                            <div className="space-y-4">
                                <h4 className="flex items-center gap-2 font-black text-secondary-600 border-b border-default-100 pb-2 uppercase text-xs tracking-wider">
                                    <MapPin size={16} /> Thông tin mặt bằng
                                </h4>
                                <div className="space-y-2">
                                    <p className="text-sm">
                                        <b>Tên quán:</b> {application.storeName}
                                    </p>
                                    <p className="text-sm">
                                        <b>Địa chỉ:</b> {application.storeAddress}
                                    </p>
                                    <p className="text-sm">
                                        <b>Diện tích:</b> {application.storeSizeSqm} m²
                                    </p>
                                    <p className="text-sm italic text-default-400 font-mono text-[10px]">
                                        Tọa độ: {application.storeLatitude},{" "}
                                        {application.storeLongitude}
                                    </p>
                                </div>
                            </div>
                        </CardBody>
                    </Card>

                    {/* Thẻ Hình ảnh */}
                    <Card shadow="sm" className="border border-default-100">
                        <CardBody className="p-6 space-y-6">
                            <h4 className="flex items-center gap-2 font-black text-warning-600 border-b border-default-100 pb-2 uppercase text-xs tracking-wider">
                                <ImageIcon size={16} /> Hồ sơ hình ảnh thẩm định
                            </h4>

                            {/* Thêm items-start để căn thẳng hàng ở đỉnh top khi chiều cao ảnh khác nhau */}
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
                </div>

                {/* --- CỘT PHẢI (4): HÀNH ĐỘNG DUYỆT (Review Panel) --- */}
                <div className="lg:col-span-4">
                    <div className="sticky top-20">
                        <ApplicationReviewActions application={application} />
                    </div>
                </div>
            </div>
        </div>
    )
}
