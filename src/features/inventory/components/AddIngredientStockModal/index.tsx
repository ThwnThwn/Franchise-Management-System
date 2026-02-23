"use client"

import React, { useState, useEffect } from "react"
import {
    ModalStyled,
    InputStyled,
    ButtonStyled,
    DatePickerStyled
} from "@/components"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Select,
    SelectItem,
    Autocomplete,
    AutocompleteItem
} from "@heroui/react"
import { PackagePlus, Truck, Store } from "lucide-react"
import { IngredientItem } from "../../models/response"
import { StoreDetailDto } from "@/features/stores/models/schema/response"
import { parseDate } from "@internationalized/date"

// ===========================================================
// CONSTANTS
// ===========================================================

const UNITS_OF_MEASURE = [
    { label: "Grams (g)", value: "g" },
    { label: "Kilograms (kg)", value: "kg" },
    { label: "Mililiters (ml)", value: "ml" },
    { label: "Liters (l)", value: "l" },
    { label: "Units (pcs)", value: "pcs" },
    { label: "Tablespoon (tbsp)", value: "tbsp" },
    { label: "Teaspoon (tsp)", value: "tsp" }
] as const

type FormState = {
    storeId: string
    ingredientId: string
    ingredientName: string
    quantity: string
    minThreshold: string
    unit: string
    supplierInfo: string
    expirationDate: string
}

const INITIAL_FORM: FormState = {
    storeId: "",
    ingredientId: "",
    ingredientName: "",
    quantity: "",
    minThreshold: "",
    unit: "",
    supplierInfo: "",
    expirationDate: ""
}

type FormErrors = {
    storeId?: string
    ingredientId?: string
    quantity?: string
    minThreshold?: string
    unit?: string
}

// ===========================================================
// PROPS
// ===========================================================

type AddIngredientStockModalProps = {
    isOpen: boolean
    onClose: () => void
    ingredients: IngredientItem[]
    stores: StoreDetailDto[]
    onSubmit: (params: {
        storeId: string
        ingredientId: string
        quantity: number
        minThreshold?: number
        unit: string
        supplierInfo: string
        expirationDate: string
        reason: string
    }) => void
    isLoading?: boolean
}

// ===========================================================
// COMPONENT
// ===========================================================

export function AddIngredientStockModal({
    isOpen,
    onClose,
    ingredients,
    stores,
    onSubmit,
    isLoading = false
}: AddIngredientStockModalProps) {
    const [form, setForm] = useState<FormState>(INITIAL_FORM)
    const [errors, setErrors] = useState<FormErrors>({})

    useEffect(() => {
        if (isOpen) {
            setForm(INITIAL_FORM)
            setErrors({})
        }
    }, [isOpen])

    const handleIngredientSelect = (id: string) => {
        const found = ingredients.find((i) => i.id === id)
        if (!found) return
        setForm((prev) => ({
            ...prev,
            ingredientId: id,
            ingredientName: found.name,
            unit: found.baseUnit
        }))
        setErrors((prev) => ({ ...prev, ingredientId: undefined }))
    }

    const handleChange = (field: keyof FormState, value: string) => {
        setForm((prev) => ({ ...prev, [field]: value }))
        setErrors((prev) => ({ ...prev, [field]: undefined }))
    }

    const validate = (): boolean => {
        const newErrors: FormErrors = {}

        if (!form.storeId) {
            newErrors.storeId = "Vui lòng chọn cửa hàng"
        }
        if (!form.ingredientId) {
            newErrors.ingredientId = "Vui lòng chọn nguyên liệu"
        }
        const qty = parseFloat(form.quantity)
        if (!form.quantity || isNaN(qty) || qty <= 0) {
            newErrors.quantity = "Số lượng phải là số dương"
        }
        if (!form.unit) {
            newErrors.unit = "Vui lòng chọn đơn vị đo lường"
        }

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const handleSubmit = () => {
        if (!validate()) return
        onSubmit({
            storeId: form.storeId,
            ingredientId: form.ingredientId,
            quantity: parseFloat(form.quantity),
            minThreshold: form.minThreshold ? parseFloat(form.minThreshold) : undefined,
            unit: form.unit,
            supplierInfo: form.supplierInfo,
            expirationDate: form.expirationDate,
            reason: "Supplier delivery"
        })
    }

    return (
        <ModalStyled isOpen={isOpen} onClose={onClose} size="lg">
            <ModalContent>
                <ModalHeader className="flex items-center gap-2">
                    <PackagePlus className="w-5 h-5 text-success-500" />
                    <span>Nhập kho nguyên liệu</span>
                </ModalHeader>

                <ModalBody>
                    <div className="space-y-4">

                        {/* ── Reason (fixed) ── */}
                        <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 px-4 py-3 rounded-xl">
                            <Truck className="w-4 h-4 text-amber-600 shrink-0" />
                            <div>
                                <p className="text-xs text-amber-600 font-medium">Lý do nhập kho</p>
                                <p className="text-sm font-semibold text-amber-800">Supplier delivery</p>
                            </div>
                        </div>

                        {/* ── Store Select ── */}
                        <div className="flex flex-col gap-1">
                            <label className="text-sm font-medium text-slate-700">
                                Cửa hàng <span className="text-danger">*</span>
                            </label>
                            <Select
                                placeholder="Chọn cửa hàng nhập kho"
                                selectedKeys={form.storeId ? [form.storeId] : []}
                                isInvalid={!!errors.storeId}
                                errorMessage={errors.storeId}
                                onChange={(e) => handleChange("storeId", e.target.value)}
                                variant="bordered"
                                startContent={<Store className="w-4 h-4 text-slate-400" />}
                                classNames={{
                                    trigger: "border-slate-200 hover:border-primary-400 h-12"
                                }}
                            >
                                {stores.map((store) => (
                                    <SelectItem key={store.id} textValue={store.name}>
                                        <div className="flex flex-col">
                                            <span className="font-medium">{store.name}</span>
                                            <span className="text-xs text-slate-400">{store.address}</span>
                                        </div>
                                    </SelectItem>
                                ))}
                            </Select>
                        </div>

                        {/* ── Ingredient Name (Autocomplete) ── */}
                        <Autocomplete
                            label="Ingredient Name"
                            placeholder="Chọn hoặc tìm kiếm nguyên liệu…"
                            isRequired
                            isInvalid={!!errors.ingredientId}
                            errorMessage={errors.ingredientId}
                            onSelectionChange={(key) => {
                                if (key) handleIngredientSelect(String(key))
                            }}
                            defaultItems={ingredients}
                            className="w-full"
                            labelPlacement="outside"
                            variant="bordered"
                            classNames={{
                                base: "w-full",
                                listboxWrapper: "max-h-60"
                            }}
                        >
                            {(item) => (
                                <AutocompleteItem key={item.id} textValue={item.name}>
                                    <div className="flex flex-col">
                                        <span className="text-sm">{item.name}</span>
                                        <span className="text-xs text-slate-400">{item.baseUnit}</span>
                                    </div>
                                </AutocompleteItem>
                            )}
                        </Autocomplete>

                        {/* ── Quantity + MinThreshold + Unit ── */}
                        <div className="grid grid-cols-2 gap-3">
                            <InputStyled
                                type="number"
                                label="Quantity to Add"
                                placeholder="0"
                                isRequired
                                isInvalid={!!errors.quantity}
                                errorMessage={errors.quantity}
                                value={form.quantity}
                                min={0}
                                step={0.01}
                                onChange={(e) => handleChange("quantity", e.target.value)}
                                endContent={
                                    form.unit ? (
                                        <span className="text-xs text-slate-400 whitespace-nowrap">
                                            {form.unit}
                                        </span>
                                    ) : undefined
                                }
                            />

                            <div className="flex flex-col gap-1">
                                <label className="text-sm font-medium text-slate-700">
                                    Unit of Measure <span className="text-danger">*</span>
                                </label>
                                <Select
                                    placeholder="Chọn đơn vị"
                                    selectedKeys={form.unit ? [form.unit] : []}
                                    isInvalid={!!errors.unit}
                                    errorMessage={errors.unit}
                                    onChange={(e) => handleChange("unit", e.target.value)}
                                    variant="bordered"
                                    classNames={{
                                        trigger: "border-slate-200 hover:border-primary-400 h-12",
                                        popoverContent: "z-50"
                                    }}
                                >
                                    {UNITS_OF_MEASURE.map((u) => (
                                        <SelectItem key={u.value} textValue={u.label}>
                                            {u.label}
                                        </SelectItem>
                                    ))}
                                </Select>
                            </div>
                        </div>

                        {/* ── Min Threshold ── */}
                        <InputStyled
                            type="number"
                            label="Ngưỡng tối thiểu (Min Threshold)"
                            placeholder="Ví dụ: 5 (tùy chọn)"
                            isInvalid={!!errors.minThreshold}
                            errorMessage={errors.minThreshold}
                            value={form.minThreshold}
                            min={0}
                            step={0.01}
                            onChange={(e) => handleChange("minThreshold", e.target.value)}
                            endContent={
                                form.unit ? (
                                    <span className="text-xs text-slate-400 whitespace-nowrap">
                                        {form.unit}
                                    </span>
                                ) : undefined
                            }
                            description="Hệ thống cảnh báo khi số lượng tồn xuống dưới mức này"
                        />

                        {/* ── Supplier Info ── */}
                        <InputStyled
                            label="Supplier Info"
                            placeholder="Tên nhà cung cấp, mã đơn hàng… (tuỳ chọn)"
                            value={form.supplierInfo}
                            onChange={(e) => handleChange("supplierInfo", e.target.value)}
                        />

                        {/* ── Expiration Date ── */}
                        <DatePickerStyled
                            label="Expiration Date (optional)"
                            placeholderValue={parseDate("2025-01-01")}
                            onChange={(date) =>
                                handleChange(
                                    "expirationDate",
                                    date
                                        ? `${date.year}-${String(date.month).padStart(2, "0")}-${String(date.day).padStart(2, "0")}`
                                        : ""
                                )
                            }
                            minValue={parseDate(new Date().toISOString().split("T")[0])}
                        />

                    </div>
                </ModalBody>

                <ModalFooter>
                    <Button color="danger" variant="light" onPress={onClose} isDisabled={isLoading}>
                        Hủy
                    </Button>
                    <ButtonStyled
                        color="success"
                        onPress={handleSubmit}
                        isLoading={isLoading}
                        startContent={!isLoading && <PackagePlus className="w-4 h-4" />}
                    >
                        Nhập kho
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
