"use client"

import React, { useState, useEffect } from "react"
import { ModalStyled, InputStyled, ButtonStyled } from "@/components"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Select,
    SelectItem
} from "@heroui/react"
import { FlaskConical } from "lucide-react"
import { IngredientCategory } from "../../models/response"
import { CreateIngredientRequest } from "../../models/request"

// Đơn vị cơ bản phổ biến để gợi ý
const UNIT_OPTIONS = [
    { label: "Grams (g)", value: "g" },
    { label: "Kilograms (kg)", value: "kg" },
    { label: "Mililiters (ml)", value: "ml" },
    { label: "Liters (l)", value: "l" },
    { label: "Units (pcs)", value: "pcs" },
    { label: "Tablespoon (tbsp)", value: "tbsp" },
    { label: "Teaspoon (tsp)", value: "tsp" }
]

type FormState = {
    name: string
    baseUnit: string
    categoryId: string
}

type FormErrors = {
    name?: string
    baseUnit?: string
}

const INITIAL_FORM: FormState = { name: "", baseUnit: "", categoryId: "" }

// ──────────────────────────────────────────────────────────

type CreateIngredientModalProps = {
    isOpen: boolean
    onClose: () => void
    categories: IngredientCategory[]
    onSubmit: (req: CreateIngredientRequest) => void
    isLoading?: boolean
}

export function CreateIngredientModal({
    isOpen,
    onClose,
    categories,
    onSubmit,
    isLoading = false
}: CreateIngredientModalProps) {
    const [form, setForm] = useState<FormState>(INITIAL_FORM)
    const [errors, setErrors] = useState<FormErrors>({})

    // Reset khi mở modal
    useEffect(() => {
        if (isOpen) {
            setForm(INITIAL_FORM)
            setErrors({})
        }
    }, [isOpen])

    const handleChange = (field: keyof FormState, value: string) => {
        setForm((prev) => ({ ...prev, [field]: value }))
        if (field === "name") setErrors((prev) => ({ ...prev, name: undefined }))
        if (field === "baseUnit") setErrors((prev) => ({ ...prev, baseUnit: undefined }))
    }

    const validate = (): boolean => {
        const newErrors: FormErrors = {}
        if (!form.name.trim()) newErrors.name = "Tên nguyên liệu là bắt buộc"
        if (!form.baseUnit) newErrors.baseUnit = "Đơn vị đo lường là bắt buộc"
        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const handleSubmit = () => {
        if (!validate()) return
        onSubmit({
            name: form.name.trim(),
            baseUnit: form.baseUnit,
            categoryId: form.categoryId || undefined
        })
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="md">
            <ModalContent>
                <ModalHeader className="flex items-center gap-2">
                    <FlaskConical className="w-5 h-5 text-primary-500" />
                    <span>Thêm nguyên liệu mới</span>
                </ModalHeader>

                <ModalBody>
                    <div className="space-y-4">
                        {/* Name */}
                        <InputStyled
                            label="Tên nguyên liệu"
                            placeholder="VD: Đường, Sữa tươi, Bột mì..."
                            isRequired
                            value={form.name}
                            onChange={(e) => handleChange("name", e.target.value)}
                            isInvalid={!!errors.name}
                            errorMessage={errors.name}
                            autoFocus
                        />

                        {/* Base Unit */}
                        <div className="flex flex-col gap-1">
                            <label className="text-sm font-medium text-slate-700">
                                Đơn vị đo lường <span className="text-danger">*</span>
                            </label>
                            <Select
                                placeholder="Chọn đơn vị"
                                selectedKeys={form.baseUnit ? [form.baseUnit] : []}
                                isInvalid={!!errors.baseUnit}
                                errorMessage={errors.baseUnit}
                                onChange={(e) => handleChange("baseUnit", e.target.value)}
                                variant="bordered"
                                classNames={{
                                    trigger: "border-slate-200 hover:border-primary-400 h-12"
                                }}
                            >
                                {UNIT_OPTIONS.map((u) => (
                                    <SelectItem key={u.value} textValue={u.label}>
                                        {u.label}
                                    </SelectItem>
                                ))}
                            </Select>
                            {errors.baseUnit && (
                                <p className="text-xs text-danger">{errors.baseUnit}</p>
                            )}
                        </div>

                        {/* Category (optional) */}
                        <div className="flex flex-col gap-1">
                            <label className="text-sm font-medium text-slate-700">
                                Danh mục{" "}
                                <span className="text-slate-400 font-normal text-xs">(tuỳ chọn)</span>
                            </label>
                            <Select
                                placeholder="Chọn danh mục nguyên liệu"
                                selectedKeys={form.categoryId ? [form.categoryId] : []}
                                onChange={(e) => handleChange("categoryId", e.target.value)}
                                variant="bordered"
                                classNames={{
                                    trigger: "border-slate-200 hover:border-primary-400 h-12"
                                }}
                            >
                                {categories.map((cat) => (
                                    <SelectItem key={cat.id} textValue={cat.name}>
                                        <div className="flex flex-col">
                                            <span>{cat.name}</span>
                                            {cat.description && (
                                                <span className="text-xs text-slate-400">
                                                    {cat.description}
                                                </span>
                                            )}
                                        </div>
                                    </SelectItem>
                                ))}
                            </Select>
                        </div>

                        {/* Preview payload */}
                        {(form.name || form.baseUnit) && (
                            <div className="bg-slate-50 rounded-xl p-3 border border-slate-100">
                                <p className="text-xs text-slate-400 mb-1 font-mono">Preview payload</p>
                                <pre className="text-xs text-slate-700 font-mono whitespace-pre-wrap">
                                    {JSON.stringify(
                                        {
                                            name: form.name || "...",
                                            baseUnit: form.baseUnit || "...",
                                            ...(form.categoryId && { categoryId: form.categoryId })
                                        },
                                        null,
                                        2
                                    )}
                                </pre>
                            </div>
                        )}
                    </div>
                </ModalBody>

                <ModalFooter>
                    <Button color="danger" variant="light" onPress={onClose} isDisabled={isLoading}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="primary"
                        onPress={handleSubmit}
                        isLoading={isLoading}
                        startContent={!isLoading && <FlaskConical className="w-4 h-4" />}
                    >
                        Tạo nguyên liệu
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
