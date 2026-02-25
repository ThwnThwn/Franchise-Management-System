"use client"

import React, { useMemo } from "react"
import { useFormik } from "formik"
import { Card, CardBody, Divider, Switch, Spinner, Chip } from "@heroui/react"
import { useTokenStore } from "@/hooks/singleton/store"
import { shiftRuleSchema, ShiftRuleFormValues } from "./schema"
import { AlertStyled, NumberInputStyled, UnsavedChangesNotifier } from "@/components"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ShieldCheck, Zap, Store } from "lucide-react"
import { useGetShiftRuleSettings, useSaveShiftRule } from "@/features/shifts/hooks"
import { SystemDefaultHint } from "./SystemDefaultHint"
import { RoleSlug } from "@/constants/enums"

export default function ShiftRulesManagerPageView() {
    const storeId = useTokenStore((s) => s.storeId)
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)

    // React Query Hooks
    const { data, isLoading } = useGetShiftRuleSettings(storeId || "")
    const { mutate: saveRule, isPending: isSaving } = useSaveShiftRule()

    // 1. Xác định Rule của Store từ mảng
    const currentStoreRule = useMemo(() => {
        if (!data?.stores || data.stores.length === 0) return undefined
        return data.stores.find((s) => s.storeId === storeId)
    }, [data, storeId])

    // 2. Khởi tạo Formik: Ưu tiên Store > Global > Default
    const initialValues: ShiftRuleFormValues = useMemo(
        () => ({
            storeId: storeId || undefined,
            allowShuttle: currentStoreRule?.allowShuttle ?? data?.global?.allowShuttle ?? false,
            allowShiftSwap:
                currentStoreRule?.allowShiftSwap ?? data?.global?.allowShiftSwap ?? true,
            minHoursBetweenShifts:
                currentStoreRule?.minHoursBetweenShifts ?? data?.global?.minHoursBetweenShifts ?? 8,
            maxHoursPerWeek:
                currentStoreRule?.maxHoursPerWeek ?? data?.global?.maxHoursPerWeek ?? 48
        }),
        [data, currentStoreRule, storeId]
    )

    const formik = useFormik({
        initialValues,
        enableReinitialize: true,
        validationSchema: shiftRuleSchema,
        onSubmit: (values) => saveRule(values)
    })

    if (isLoading)
        return (
            <div className="flex flex-col items-center justify-center p-20 gap-4">
                <Spinner size="lg" label="Đang tải cấu hình..." />
            </div>
        )

    return (
        <div className="max-w-7xl mx-auto min-h-screen p-4 md:p-6 lg:p-8">
            <DashboardHeader
                icon={<ShieldCheck className="w-6 h-6 text-white" />}
                title="Quy tắc ca làm"
                description="Thiết lập các giới hạn giờ làm và chính sách đổi ca để đảm bảo vận hành ổn định."
            />

            {/* Banner thông báo trạng thái */}
            {!currentStoreRule && !isAdmin && (
                // <div className="mb-6 p-4 bg-primary-50 border border-primary-100 rounded-2xl flex items-start gap-4 animate-appearance-in">
                //     <div className="p-2 bg-primary-100 rounded-xl text-primary">
                //         <Info size={20} />
                //     </div>
                //     <div>
                //         <p className="font-bold text-primary-900 text-sm">Chế độ kế thừa</p>
                // <p className="text-xs text-primary-700 leading-relaxed">
                //     Cửa hàng chưa có cấu hình riêng và đang sử dụng <b>Luật chung</b>. Khi
                //     bạn lưu thay đổi, hệ thống sẽ tự động tạo bộ luật riêng cho chi nhánh
                //     này.
                // </p>
                //     </div>
                // </div>
                <AlertStyled
                    color="warning"
                    className="mb-3"
                    title="Chế độ kế thừa"
                    description={
                        <p>
                            Cửa hàng chưa có cấu hình riêng và đang sử dụng <b>Luật chung</b>. Khi
                            bạn lưu thay đổi, hệ thống sẽ tự động tạo bộ luật riêng cho chi nhánh
                            này.
                        </p>
                    }
                />
            )}

            <div className="grid grid-cols-1 gap-8">
                {/* NHÓM 1: CHÍNH SÁCH VẬN HÀNH */}
                <Card shadow="sm" className="border-none bg-white/70 backdrop-blur-md">
                    <CardBody className="p-8 gap-6">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2.5 text-primary font-bold uppercase text-xs tracking-widest">
                                <Zap size={18} /> Chính sách vận hành
                            </div>
                            {currentStoreRule && (
                                <Chip
                                    variant="flat"
                                    color="success"
                                    size="sm"
                                    startContent={<Store size={12} />}
                                >
                                    Cấu hình riêng
                                </Chip>
                            )}
                        </div>
                        <Divider />

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
                            {/* Toggle Đổi ca */}
                            <div className="space-y-1">
                                <div className="flex items-center justify-between group">
                                    <div className="pr-4">
                                        <p className="font-bold text-base">
                                            Cho phép đổi ca (Swap)
                                        </p>
                                        <p className="text-xs text-default-400">
                                            Nhân viên được phép gửi yêu cầu hoán đổi ca trực tiếp
                                            trên App
                                        </p>
                                    </div>
                                    <Switch
                                        isSelected={formik.values.allowShiftSwap}
                                        onValueChange={(v) =>
                                            formik.setFieldValue("allowShiftSwap", v)
                                        }
                                        isDisabled={isSaving}
                                    />
                                </div>
                                <SystemDefaultHint
                                    value={data?.global?.allowShiftSwap}
                                    type="boolean"
                                />
                            </div>

                            {/* Toggle Shuttle */}
                            <div className="space-y-1">
                                <div className="flex items-center justify-between group">
                                    <div className="pr-4">
                                        <p className="font-bold text-base">
                                            Làm liên cửa hàng (Shuttle)
                                        </p>
                                        <p className="text-xs text-default-400">
                                            Cho phép nhân viên đăng ký ca làm ở các chi nhánh khác
                                            trong chuỗi
                                        </p>
                                    </div>
                                    <Switch
                                        isSelected={formik.values.allowShuttle}
                                        onValueChange={(v) =>
                                            formik.setFieldValue("allowShuttle", v)
                                        }
                                        isDisabled={isSaving}
                                    />
                                </div>
                                <SystemDefaultHint
                                    value={data?.global?.allowShuttle}
                                    type="boolean"
                                />
                            </div>
                        </div>
                    </CardBody>
                </Card>

                {/* NHÓM 2: AN TOÀN LAO ĐỘNG */}
                <Card shadow="sm" className="border-none bg-white/70 backdrop-blur-md">
                    <CardBody className="p-8 gap-6">
                        <div className="flex items-center gap-2.5 text-primary font-bold uppercase text-xs tracking-widest">
                            <ShieldCheck size={18} /> Giới hạn & An toàn lao động
                        </div>
                        <Divider />
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
                            <div className="space-y-1">
                                <NumberInputStyled
                                    label="Thời gian nghỉ tối thiểu (Giờ)"
                                    description="Khoảng trống bắt buộc giữa 2 ca làm việc liên tiếp."
                                    value={formik.values.minHoursBetweenShifts}
                                    onValueChange={(v) =>
                                        formik.setFieldValue("minHoursBetweenShifts", v)
                                    }
                                    isReadOnly={isSaving}
                                />
                                <SystemDefaultHint value={data?.global?.minHoursBetweenShifts} />
                            </div>

                            <div className="space-y-1">
                                <NumberInputStyled
                                    label="Giới hạn giờ làm / tuần"
                                    description="Tổng số giờ làm tối đa nhân viên được phép đăng ký trong 1 tuần."
                                    value={formik.values.maxHoursPerWeek}
                                    onValueChange={(v) =>
                                        formik.setFieldValue("maxHoursPerWeek", v)
                                    }
                                    isReadOnly={isSaving}
                                />
                                <SystemDefaultHint value={data?.global?.maxHoursPerWeek} />
                            </div>
                        </div>
                    </CardBody>
                </Card>
            </div>

            {/* Fixed Footer Notifier */}
            <UnsavedChangesNotifier
                hasChanges={formik.dirty}
                isLoading={isSaving}
                onSave={formik.handleSubmit}
                onReset={formik.resetForm}
            />
        </div>
    )
}
