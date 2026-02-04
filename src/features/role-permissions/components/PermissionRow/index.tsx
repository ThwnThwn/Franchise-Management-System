import React, { useState } from "react"
import { PermissionDetail } from "../../models/respone"
import { ButtonIconStyled, CardStyled } from "@/components"
import { Info, ShieldCheck, Edit3, X, Check } from "lucide-react"
import { Chip, Switch } from "@heroui/react"
import { useFormik } from "formik"
import * as Yup from "yup"
import { InputStyled } from "@/components"
import { useUpdatePermission } from "../../hooks"

interface PermissionRowProps {
    permission: PermissionDetail
    isSelected?: boolean
    onChange?: (slug: string, checked: boolean) => void
    canEdit?: boolean
}

export function PermissionRow({
    permission,
    isSelected = false,
    onChange,
    canEdit = false
}: PermissionRowProps) {
    const [isEditing, setIsEditing] = useState(false)

    const updateMutation = useUpdatePermission({
        id: permission.id,
        onSuccess: () => setIsEditing(false)
    })

    const formik = useFormik({
        initialValues: {
            name: permission.name,
            description: permission.description || ""
        },
        validationSchema: Yup.object({
            name: Yup.string().required("Không được để trống")
        }),
        enableReinitialize: true,
        onSubmit: async (values) => {
            if (formik.dirty) {
                await updateMutation.mutateAsync({
                    name: values.name,
                    description: values.description
                })
            } else {
                setIsEditing(false)
            }
        }
    })

    const handleSwitchChange = (checked: boolean) => {
        if (onChange) {
            onChange(permission.slug, checked)
        }
    }

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === "Escape") {
            handleCancel()
        }
    }

    const handleCancel = () => {
        formik.resetForm()
        setIsEditing(false)
    }

    return (
        <CardStyled
            className={`p-4 transition-all border-2 ${isEditing ? "border-primary shadow-md" : "border-transparent shadow-none bg-white"} group`}
        >
            <div className="flex flex-col gap-4">
                <div className="flex items-start justify-between gap-4">
                    {/* Left side: Icon & Main Info */}
                    <div className="flex items-start gap-4 flex-1">
                        <div
                            className={`p-2 rounded-lg shrink-0 ${isEditing ? "bg-primary text-white" : "bg-primary-50 text-primary"}`}
                        >
                            <ShieldCheck size={20} />
                        </div>

                        <div className="flex flex-col gap-2 w-full">
                            {isEditing ? (
                                <form
                                    onSubmit={formik.handleSubmit}
                                    onKeyDown={handleKeyDown}
                                    className="flex flex-col gap-3 animate-in fade-in zoom-in-95 duration-200"
                                >
                                    <InputStyled
                                        size="sm"
                                        label="Tên quyền"
                                        {...formik.getFieldProps("name")}
                                        isInvalid={!!(formik.touched.name && formik.errors.name)}
                                    />
                                    <InputStyled
                                        size="sm"
                                        label="Mô tả"
                                        {...formik.getFieldProps("description")}
                                    />
                                    <div className="flex gap-2 mt-1">
                                        <ButtonIconStyled
                                            type="submit"
                                            size="sm"
                                            color="primary"
                                            variant="solid"
                                            onPress={() => formik.handleSubmit()}
                                            isLoading={updateMutation.isPending}
                                            isDisabled={!formik.dirty}
                                        >
                                            <Check size={12} />
                                        </ButtonIconStyled>
                                        {!updateMutation.isPending && (
                                            <ButtonIconStyled
                                                type="button"
                                                size="sm"
                                                variant="flat"
                                                onPress={handleCancel}
                                                isDisabled={updateMutation.isPending}
                                            >
                                                <X size={12} />
                                            </ButtonIconStyled>
                                        )}
                                    </div>
                                </form>
                            ) : (
                                // CHẾ ĐỘ HIỂN THỊ
                                <div className="flex flex-col gap-1">
                                    <div className="flex items-center gap-2 flex-wrap">
                                        <h4 className="font-semibold text-slate-800">
                                            {permission.name}
                                        </h4>
                                        <Chip
                                            size="sm"
                                            variant="flat"
                                            className="text-[10px] uppercase font-mono bg-slate-100"
                                        >
                                            {permission.slug}
                                        </Chip>

                                        {canEdit && (
                                            <button
                                                onClick={() => setIsEditing(true)}
                                                className="opacity-0 group-hover:opacity-100 transition-opacity p-1 hover:text-primary text-slate-400"
                                            >
                                                <Edit3 size={16} />
                                            </button>
                                        )}
                                    </div>
                                    {permission.description && (
                                        <div className="flex items-start gap-1.5 text-slate-500 text-sm">
                                            <Info size={14} className="shrink-0 mt-0.5" />
                                            <p className="leading-relaxed">
                                                {permission.description}
                                            </p>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Right side: Switch (Ẩn khi đang edit để tránh bấm nhầm) */}
                    {onChange && !isEditing && (
                        <div className="flex items-center">
                            <Switch
                                size="sm"
                                color="primary"
                                isSelected={isSelected}
                                onValueChange={handleSwitchChange}
                            />
                        </div>
                    )}
                </div>
            </div>
        </CardStyled>
    )
}
