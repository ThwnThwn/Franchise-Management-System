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
import { Pencil } from "lucide-react"
import { IngredientItem, IngredientCategory } from "../../models/response"
import { UpdateIngredientRequest } from "../../models/request"

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

type EditIngredientModalProps = {
    isOpen: boolean
    onClose: () => void
    ingredient: IngredientItem | null
    categories: IngredientCategory[]
    onSubmit: (id: string, req: UpdateIngredientRequest) => void
    isLoading?: boolean
}

export function EditIngredientModal({
    isOpen,
    onClose,
    ingredient,
    categories,
    onSubmit,
    isLoading = false
}: EditIngredientModalProps) {
    const [form, setForm] = useState<FormState>({ name: "", baseUnit: "", categoryId: "" })
    const [errors, setErrors] = useState<FormErrors>({})

    // Pre-fill form whenever ingredient changes or modal opens
    useEffect(() => {
        if (isOpen && ingredient) {
            setForm({
                name: ingredient.name,
                baseUnit: ingredient.baseUnit,
                categoryId: ingredient.categoryId ?? ""
            })
            setErrors({})
        }
    }, [isOpen, ingredient])

    const handleChange = (field: keyof FormState, value: string) => {
        setForm((prev) => ({ ...prev, [field]: value }))
        setErrors((prev) => ({ ...prev, [field]: undefined }))
    }

    const validate = (): boolean => {
        const newErrors: FormErrors = {}
        if (!form.name.trim()) newErrors.name = "Tên nguyên liệu là bắt buộc"
        if (!form.baseUnit) newErrors.baseUnit = "Đơn vị đo lường là bắt buộc"
        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const handleSubmit = () => {
        if (!validate() || !ingredient) return
        onSubmit(ingredient.id, {
            name: form.name.trim(),
            baseUnit: form.baseUnit,
            categoryId: form.categoryId || null
        })
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="md">
            <ModalContent>
                <ModalHeader className="flex items-center gap-2">
                    <Pencil className="w-5 h-5 text-warning-500" />
                    <span>Chỉnh sửa nguyên liệu</span>
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
                                    trigger: "border-slate-200 hover:border-warning-400 h-12"
                                }}
                            >
                                {UNIT_OPTIONS.map((u) => (
                                    <SelectItem key={u.value} textValue={u.label}>
                                        {u.label}
                                    </SelectItem>
                                ))}
                            </Select>
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
                                    trigger: "border-slate-200 hover:border-warning-400 h-12"
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
                    </div>
                </ModalBody>

                <ModalFooter>
                    <Button color="danger" variant="light" onPress={onClose} isDisabled={isLoading}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="warning"
                        onPress={handleSubmit}
                        isLoading={isLoading}
                        startContent={!isLoading && <Pencil className="w-4 h-4" />}
                    >
                        Lưu thay đổi
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
