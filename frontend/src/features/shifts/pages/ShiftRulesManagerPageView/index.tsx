"use client"

import React, { useMemo } from "react"
import { useFormik } from "formik"
import { Card, CardBody, Divider, Switch, Spinner, Chip } from "@heroui/react"
import { useTokenStore } from "@/hooks/singleton/store"
import { shiftRuleSchema, ShiftRuleFormValues } from "./schema"
import { AlertStyled, InputStyled, UnsavedChangesNotifier } from "@/components"
import { DashboardHeader } from "@/features/dashboard/components/general/DashboardHeader"
import { ShieldCheck, Zap, Store, Clock } from "lucide-react"
import { useGetShiftRuleSettings, useSaveShiftRule } from "@/features/shifts/hooks"
import { SystemDefaultHint } from "./SystemDefaultHint"
import { RoleSlug } from "@/constants/enums"
import { useFormikHelpers } from "@/hooks"

export default function ShiftRulesManagerPageView() {
    const storeId = useTokenStore((s) => s.storeId) || undefined
    const isAdmin = useTokenStore((s) => s.roleSlug === RoleSlug.Admin)

    // React Query Hooks
    const { data, isLoading } = useGetShiftRuleSettings({ storeId: storeId })
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
                currentStoreRule?.maxHoursPerWeek ?? data?.global?.maxHoursPerWeek ?? 48,

            gracePeriodMinutes:
                currentStoreRule?.gracePeriodMinutes ?? data?.global?.gracePeriodMinutes ?? 15,
            dailyOtThresholdHours:
                currentStoreRule?.dailyOtThresholdHours ?? data?.global?.dailyOtThresholdHours ?? 8,
            weeklyOtThresholdHours:
                currentStoreRule?.weeklyOtThresholdHours ??
                data?.global?.weeklyOtThresholdHours ??
                40
        }),
        [data, currentStoreRule, storeId]
    )

    const formik = useFormik({
        initialValues,
        enableReinitialize: true,
        validationSchema: shiftRuleSchema,
        onSubmit: (values) => saveRule(values)
    })
    const { isInvalid, getErrorMessage, setNumberValue } = useFormikHelpers(formik)
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
                                <InputStyled
                                    type="number"
                                    label="Thời gian nghỉ tối thiểu (Giờ)"
                                    description="Khoảng trống bắt buộc giữa 2 ca làm việc liên tiếp."
                                    value={formik.values.minHoursBetweenShifts.toString()}
                                    onValueChange={(v) => {
                                        setNumberValue("minHoursBetweenShifts", v)
                                    }}
                                    isReadOnly={isSaving}
                                    isInvalid={isInvalid("minHoursBetweenShifts")}
                                    errorMessage={getErrorMessage("minHoursBetweenShifts")}
                                />
                                <SystemDefaultHint value={data?.global?.minHoursBetweenShifts} />
                            </div>

                            <div className="space-y-1">
                                <InputStyled
                                    type="number"
                                    label="Giới hạn giờ làm / tuần"
                                    description="Tổng số giờ làm tối đa nhân viên được phép đăng ký trong 1 tuần."
                                    value={formik.values.maxHoursPerWeek.toString()}
                                    onValueChange={(v) => setNumberValue("maxHoursPerWeek", v)}
                                    isReadOnly={isSaving}
                                    isInvalid={isInvalid("maxHoursPerWeek")}
                                    errorMessage={getErrorMessage("maxHoursPerWeek")}
                                />
                                <SystemDefaultHint value={data?.global?.maxHoursPerWeek} />
                            </div>
                        </div>
                    </CardBody>
                </Card>

                <Card shadow="sm" className="border-none bg-white/70 backdrop-blur-md">
                    <CardBody className="p-8 gap-6">
                        <div className="flex items-center gap-2.5 text-warning font-bold uppercase text-xs tracking-widest">
                            <Clock size={18} /> Chấm công & Làm thêm giờ (OT)
                        </div>
                        <Divider />
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                            {/* Grace Period */}
                            <div className="space-y-1">
                                <InputStyled
                                    type="number"
                                    label="Thời gian ân hạn (Phút)"
                                    description="Thời gian cho phép đi trễ/về sớm mà không bị tính vi phạm."
                                    value={formik.values.gracePeriodMinutes.toString()}
                                    onValueChange={(v) => setNumberValue("gracePeriodMinutes", v)}
                                    isReadOnly={isSaving}
                                    isInvalid={isInvalid("gracePeriodMinutes")}
                                    errorMessage={getErrorMessage("gracePeriodMinutes")}
                                />
                                <SystemDefaultHint value={data?.global?.gracePeriodMinutes} />
                            </div>

                            {/* Daily OT Threshold */}
                            <div className="space-y-1">
                                <InputStyled
                                    type="number"
                                    label="Ngưỡng tăng ca ngày (Giờ)"
                                    description="Số giờ làm tiêu chuẩn 1 ngày. Phần vượt ngưỡng sẽ tính là OT ngày."
                                    value={formik.values.dailyOtThresholdHours.toString()}
                                    onValueChange={(v) =>
                                        setNumberValue("dailyOtThresholdHours", v)
                                    }
                                    isReadOnly={isSaving}
                                    isInvalid={isInvalid("dailyOtThresholdHours")}
                                    errorMessage={getErrorMessage("dailyOtThresholdHours")}
                                />
                                <SystemDefaultHint value={data?.global?.dailyOtThresholdHours} />
                            </div>

                            {/* Weekly OT Threshold */}
                            <div className="space-y-1">
                                <InputStyled
                                    type="number"
                                    label="Ngưỡng tăng ca tuần (Giờ)"
                                    description="Số giờ làm tiêu chuẩn 1 tuần. Phần vượt ngưỡng sẽ tính là OT tuần."
                                    value={formik.values.weeklyOtThresholdHours.toString()}
                                    onValueChange={(v) =>
                                        setNumberValue("weeklyOtThresholdHours", v)
                                    }
                                    isReadOnly={isSaving}
                                    isInvalid={isInvalid("weeklyOtThresholdHours")}
                                    errorMessage={getErrorMessage("weeklyOtThresholdHours")}
                                />
                                <SystemDefaultHint value={data?.global?.weeklyOtThresholdHours} />
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
