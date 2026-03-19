"use client"

import { Button, Card, CardBody, Chip, Select, SelectItem } from "@heroui/react"
import { FormikProvider, useFormik } from "formik"
import { ArrowLeft, Calendar, PackageOpen, Plus, Save, Ticket, X } from "lucide-react"
import { useRouter } from "next/navigation"
import { useState } from "react"

import { DateTimePickerStyled, InputStyled, TextareaStyled } from "@/components/styled"
import { useCreateCampaign } from "@/features/campaigns/hooks/useCampaigns"
import { CreateCampaignRequest } from "@/features/campaigns/models/request"
import { CampaignResponse } from "@/features/campaigns/models/response"
import { VoucherModal, VoucherViewModal } from "@/features/vouchers/components"
import { VoucherTypeLabels, VoucherTypeStyles } from "@/features/vouchers/constants"
import { useGetAllVouchers } from "@/features/vouchers/hooks/useVoucher"
import { useDay, useFormikHelpers } from "@/hooks"
import dayjs from "dayjs"
import { CampaignSchema } from "./schema"
import { Voucher } from "@/features/vouchers/models/response"

// Nếu có prop campaign (Edit Mode), thì truyền vào. Ở đây tui focus làm Create Mode.
interface Props {
    campaignId?: string
    initialData?: CampaignResponse
}

export const CampaignDetailView = ({ campaignId, initialData }: Props) => {
    const router = useRouter()
    const isViewMode = !!campaignId && campaignId !== "new"
    const { toZonedDateTime, dateNow } = useDay()
    const minDateStr = dateNow(0)

    const [viewingVoucher, setViewingVoucher] = useState<Voucher | undefined>(undefined)

    // Fetch data kho voucher có sẵn để Admin chọn
    const { data: availableVouchers = [], isLoading: isLoadingVouchers } = useGetAllVouchers(
        undefined,
        { enabled: !isViewMode }
    )

    // API Mutation
    const { mutate: createCampaign, isPending: isSaving } = useCreateCampaign({
        onSuccess: () => {
            router.push("/dashboard/campaigns")
        }
    })

    // State mở Modal Tạo Voucher Nhanh
    const [isVoucherModalOpen, setIsVoucherModalOpen] = useState(false)

    // Setup Formik
    const formik = useFormik<CreateCampaignRequest>({
        initialValues:
            isViewMode && initialData
                ? {
                      name: initialData.name,
                      description: initialData.description || "",
                      startDate: initialData.startDate ?? "",
                      endDate: initialData.endDate ?? "",
                      voucherIds: initialData.vouchers?.map((v) => v.id) || []
                  }
                : {
                      name: "",
                      description: "",
                      startDate: "",
                      endDate: "",
                      voucherIds: []
                  },
        validationSchema: CampaignSchema,
        onSubmit: (values) => {
            if (isViewMode) return

            const payload = {
                ...values,
                startDate: values.startDate ? dayjs(values.startDate).format() : "",
                endDate: values.endDate ? dayjs(values.endDate).format() : ""
            }
            createCampaign(payload)
        }
    })
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)
    const { values, handleChange, handleBlur, handleSubmit, setFieldValue } = formik

    // Derived Data: Lấy ra danh sách các Object Voucher đang được chọn dựa trên IDs
    const selectedVoucherObjects = isViewMode
        ? initialData?.vouchers || []
        : availableVouchers.filter((v) => values.voucherIds.includes(v.id))

    return (
        <FormikProvider value={formik}>
            <div className="p-6 space-y-6 max-w-7xl mx-auto">
                {/* --- HEADER --- */}
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                        <Button
                            isIconOnly
                            variant="light"
                            onPress={() => router.push("/dashboard/campaigns")}
                        >
                            <ArrowLeft size={20} />
                        </Button>
                        <h1 className="text-2xl font-black text-default-900">
                            {isViewMode ? "Chi tiết Sự kiện" : "Tạo Sự kiện mới"}
                        </h1>
                    </div>
                    {!isViewMode && (
                        <Button
                            color="primary"
                            className="font-bold shadow-md px-8"
                            startContent={<Save size={18} />}
                            isLoading={isSaving}
                            onPress={() => handleSubmit()}
                        >
                            LƯU CHIẾN DỊCH
                        </Button>
                    )}
                </div>

                {/* --- BODY (GRID 12 CỘT) --- */}
                <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
                    {/* CỘT TRÁI (7): THÔNG TIN SỰ KIỆN */}
                    <div className="lg:col-span-7 space-y-6">
                        <Card shadow="sm" className="border border-default-200">
                            <CardBody className="p-6 space-y-6 bg-white">
                                <h3 className="text-sm font-black uppercase text-primary-700 tracking-wider mb-2 flex items-center gap-2">
                                    <Calendar size={18} />
                                    Thông tin chung
                                </h3>

                                <InputStyled
                                    label="Tên chiến dịch / Sự kiện"
                                    placeholder="VD: Siêu Sale Giáng Sinh 2026..."
                                    labelPlacement="outside"
                                    name="name"
                                    value={values.name}
                                    onChange={handleChange}
                                    onBlur={handleBlur}
                                    isInvalid={isInvalid("name")}
                                    errorMessage={getErrorMessage("name")}
                                    isReadOnly={isViewMode}
                                    classNames={{ label: "font-bold text-default-700" }}
                                />

                                <TextareaStyled
                                    label="Mô tả sự kiện"
                                    placeholder="Nhập thông điệp truyền thông..."
                                    labelPlacement="outside"
                                    name="description"
                                    value={values.description}
                                    onChange={handleChange}
                                    minRows={4}
                                    isReadOnly={isViewMode}
                                    classNames={{ label: "font-bold text-default-700" }}
                                />

                                <div className="grid grid-cols-2 gap-4">
                                    <DateTimePickerStyled
                                        label="Thời gian bắt đầu"
                                        labelPlacement="outside"
                                        value={toZonedDateTime(values.startDate)}
                                        onChange={(val) => {
                                            setFieldValue("startDate", val ? val.toString() : "")
                                        }}
                                        isInvalid={isInvalid("startDate")}
                                        errorMessage={getErrorMessage("startDate")}
                                        minValue={toZonedDateTime(minDateStr)}
                                        isReadOnly={isViewMode}
                                        classNames={{ label: "font-bold text-default-700" }}
                                    />

                                    <DateTimePickerStyled
                                        label="Thời gian kết thúc"
                                        labelPlacement="outside"
                                        value={toZonedDateTime(values.endDate)}
                                        onChange={(val) => {
                                            setFieldValue("endDate", val ? val.toString() : "")
                                        }}
                                        isInvalid={isInvalid("endDate")}
                                        errorMessage={getErrorMessage("endDate")}
                                        minValue={toZonedDateTime(minDateStr)}
                                        isReadOnly={isViewMode}
                                        classNames={{ label: "font-bold text-default-700" }}
                                    />
                                </div>
                            </CardBody>
                        </Card>
                    </div>

                    {/* CỘT PHẢI (5): QUẢN LÝ VOUCHER */}
                    <div className="lg:col-span-5 space-y-6">
                        <Card shadow="sm" className="border border-default-200 h-full">
                            <CardBody className="p-6 bg-default-50/30">
                                <div className="flex items-center justify-between mb-6">
                                    <h3 className="text-sm font-black uppercase text-warning-600 tracking-wider flex items-center gap-2">
                                        <Ticket size={18} />
                                        Mã Giảm Giá ({values.voucherIds.length})
                                    </h3>
                                    {/* NÚT MAGIC: Mở form tạo nhanh Voucher */}
                                    {!isViewMode && (
                                        <Button
                                            size="sm"
                                            color="warning"
                                            variant="flat"
                                            className="font-bold"
                                            startContent={<Plus size={14} />}
                                            onPress={() => setIsVoucherModalOpen(true)}
                                        >
                                            Tạo Nhanh
                                        </Button>
                                    )}
                                </div>

                                {/* Chọn Voucher từ kho có sẵn */}
                                {!isViewMode && (
                                    <Select
                                        label="Chọn từ kho Voucher"
                                        placeholder="Bấm để chọn mã có sẵn..."
                                        selectionMode="multiple"
                                        isLoading={isLoadingVouchers}
                                        selectedKeys={new Set(values.voucherIds)}
                                        onSelectionChange={(keys) => {
                                            setFieldValue("voucherIds", Array.from(keys))
                                        }}
                                        classNames={{
                                            trigger: "bg-white border-1 border-default-200"
                                        }}
                                    >
                                        {availableVouchers.map((v) => (
                                            <SelectItem key={v.id} textValue={v.code}>
                                                <div className="flex justify-between items-center w-full">
                                                    <span className="font-bold">{v.code}</span>
                                                    <span className="text-tiny text-default-400">
                                                        Tồn: {v.currentQuantity}
                                                    </span>
                                                </div>
                                            </SelectItem>
                                        ))}
                                    </Select>
                                )}

                                {/* Mini-list hiển thị các Voucher đang được nhúng */}
                                <div className={`flex flex-col gap-3 ${!isViewMode ? "mt-6" : ""}`}>
                                    {selectedVoucherObjects.length === 0 ? (
                                        <div className="text-center py-8 border-2 border-dashed border-default-200 rounded-xl bg-white">
                                            <PackageOpen
                                                size={32}
                                                className="mx-auto text-default-300 mb-2"
                                            />
                                            <p className="text-sm font-medium text-default-500">
                                                Chưa có voucher nào được gắn.
                                            </p>
                                        </div>
                                    ) : (
                                        selectedVoucherObjects.map((voucher) => (
                                            <div
                                                key={voucher.id}
                                                onClick={() => setViewingVoucher(voucher)}
                                                className="flex items-center justify-between p-3 bg-white border border-default-200 rounded-xl shadow-sm hover:border-primary-300 cursor-pointer transition-colors"
                                            >
                                                <div className="flex flex-col gap-1">
                                                    <span className="font-black text-primary-700 leading-none">
                                                        {voucher.code}
                                                    </span>
                                                    <Chip
                                                        size="sm"
                                                        variant="flat"
                                                        className={`${VoucherTypeStyles[voucher.type]} text-[10px]`}
                                                    >
                                                        {VoucherTypeLabels[voucher.type]}
                                                    </Chip>
                                                </div>
                                                {!isViewMode && (
                                                    <Button
                                                        isIconOnly
                                                        size="sm"
                                                        variant="light"
                                                        color="danger"
                                                        onPress={() => {
                                                            const newIds = values.voucherIds.filter(
                                                                (id) => id !== voucher.id
                                                            )
                                                            setFieldValue("voucherIds", newIds)
                                                        }}
                                                    >
                                                        <X size={16} />
                                                    </Button>
                                                )}
                                            </div>
                                        ))
                                    )}
                                </div>
                            </CardBody>
                        </Card>
                    </div>
                </div>
            </div>

            {/* --- MODAL TẠO VOUCHER NHANH --- */}
            <VoucherModal
                isOpen={isVoucherModalOpen}
                onOpenChange={setIsVoucherModalOpen}
                onSuccessCallback={(newVoucherId) => {
                    setFieldValue("voucherIds", [...values.voucherIds, newVoucherId])
                }}
            />

            <VoucherViewModal
                isOpen={!!viewingVoucher}
                onOpenChange={(isOpen) => {
                    if (!isOpen) setViewingVoucher(undefined)
                }}
                voucher={viewingVoucher}
            />
        </FormikProvider>
    )
}
