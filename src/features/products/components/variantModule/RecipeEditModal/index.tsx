"use client"

import React, { useMemo, useState } from "react"
import {
    ModalBody,
    ModalContent,
    ModalHeader,
    ModalFooter,
    Divider,
    Select,
    SelectItem,
    Spinner,
    Chip,
    addToast
} from "@heroui/react"
import { Trash2, Plus, FlaskConical } from "lucide-react"
import { ModalStyled, ButtonStyled, ButtonIconStyled, InputStyled } from "@/components"
import { useGetRecipes } from "@/features/products/hooks/useRecipe"
import { useUpdateVariantRecipes } from "@/features/products/hooks"
import { useGetIngredients } from "@/features/stores/inventory/hooks/useInventory"
import { UpdateRecipeItem } from "@/features/products/models/recipes/request"

// ============================================================
// Types
// ============================================================

interface RecipeRow {
    ingredientId: string
    quantity: number
}

interface Props {
    variantId: string
    variantName?: string
    isOpen: boolean
    onOpenChange: () => void
}

// ============================================================
// COMPONENT 1: WRAPPER (Lo logic Fetch API & Đợi dữ liệu)
// ============================================================
export function RecipeEditModal({ variantId, variantName, isOpen, onOpenChange }: Props) {
    const { data: recipesData, isLoading: isLoadingRecipes } = useGetRecipes({
        req: { variantIds: [variantId] },
        enabled: isOpen && !!variantId
    })

    const { data: ingredientsData, isLoading: isLoadingIngredients } = useGetIngredients({
        enabled: isOpen
    })

    const isLoading = isLoadingRecipes || isLoadingIngredients
    const ingredients = useMemo(() => {
        return ingredientsData?.items ?? []
    }, [ingredientsData?.items])

    // Xử lý dữ liệu thô từ server thành định dạng initial cho Form
    const initialRows = useMemo<RecipeRow[]>(() => {
        if (!recipesData || ingredients.length === 0) return []

        return recipesData
            .filter((r) => r.variantId === variantId)
            .map((r) => ({
                ingredientId:
                    r.ingredientId && r.ingredientId !== "00000000-0000-0000-0000-000000000000"
                        ? r.ingredientId
                        : (ingredients.find((ing) => ing.name === r.name)?.id ?? ""),
                quantity: r.quantity
            }))
    }, [recipesData, ingredients, variantId])

    const unitMap = useMemo(() => {
        return ingredients.reduce(
            (acc, ing) => {
                acc[ing.id] = ing.baseUnit
                return acc
            },
            {} as Record<string, string>
        )
    }, [ingredients])

    return (
        <ModalStyled isOpen={isOpen} onOpenChange={onOpenChange} size="lg">
            <ModalContent className="max-w-xl">
                <ModalHeader className="flex flex-col gap-1 pb-2">
                    <div className="flex items-center gap-2">
                        <FlaskConical size={20} className="text-primary" />
                        <span className="text-lg font-semibold">Nguyên liệu</span>
                    </div>
                    {variantName && (
                        <span className="text-sm text-default-400 font-normal">
                            Biến thể:{" "}
                            <span className="text-default-600 font-medium">{variantName}</span>
                        </span>
                    )}
                </ModalHeader>
                <Divider />

                {/* NẾU ĐANG LOADING: Chỉ hiện Spinner */}
                {isLoading || !recipesData ? (
                    <ModalBody className="py-4 gap-4 min-h-50">
                        <div className="flex justify-center items-center h-32">
                            <Spinner label="Đang tải..." />
                        </div>
                    </ModalBody>
                ) : (
                    /* NẾU ĐÃ CÓ DATA: Mount Component Form, ném initialRows vào thẳng State */
                    <RecipeFormContent
                        variantId={variantId}
                        initialRows={initialRows}
                        ingredients={ingredients}
                        unitMap={unitMap}
                        onClose={onOpenChange}
                    />
                )}
            </ModalContent>
        </ModalStyled>
    )
}

// ============================================================
// COMPONENT 2: FORM CONTENT (Nơi chứa logic State thực sự - KHÔNG USEEFFECT)
// ============================================================
interface FormProps {
    variantId: string
    initialRows: RecipeRow[]
    ingredients: any[] // Thay bằng type Ingredient của bạn
    unitMap: Record<string, string>
    onClose: () => void
}

function RecipeFormContent({ variantId, initialRows, ingredients, unitMap, onClose }: FormProps) {
    // Component này chỉ mount khi đã có đủ initialRows, nên truyền thẳng vào default state!
    const [rows, setRows] = useState<RecipeRow[]>(initialRows)
    const [isDirty, setIsDirty] = useState(false)

    const updateMutation = useUpdateVariantRecipes()

    const handleAddRow = () => {
        setRows((prev) => [...prev, { ingredientId: "", quantity: 0 }])
        setIsDirty(true)
    }

    const handleRemoveRow = (index: number) => {
        setRows((prev) => prev.filter((_, i) => i !== index))
        setIsDirty(true)
    }

    const handleChangeIngredient = (index: number, ingredientId: string) => {
        const isDuplicate = rows.some((r, i) => i !== index && r.ingredientId === ingredientId)
        if (isDuplicate) {
            addToast({
                title: "Trùng nguyên liệu",
                description: "Nguyên liệu này đã có trong danh sách.",
                color: "warning"
            })
            return
        }
        setRows((prev) => prev.map((r, i) => (i === index ? { ...r, ingredientId } : r)))
        setIsDirty(true)
    }

    const handleChangeQuantity = (index: number, val: string) => {
        const n = parseFloat(val)
        setRows((prev) =>
            prev.map((r, i) => (i === index ? { ...r, quantity: isNaN(n) ? 0 : n } : r))
        )
        setIsDirty(true)
    }

    const handleSave = async () => {
        const invalidRows = rows.filter((r) => !r.ingredientId || r.quantity <= 0)
        if (invalidRows.length > 0) {
            addToast({
                title: "Dữ liệu chưa đầy đủ",
                description:
                    "Vui lòng chọn nguyên liệu và nhập số lượng hợp lệ (> 0) cho tất cả các dòng.",
                color: "warning"
            })
            return
        }

        const payload: UpdateRecipeItem[] = rows.map((r) => ({
            ingredientId: r.ingredientId,
            quantity: r.quantity
        }))

        try {
            await updateMutation.mutateAsync({ variantId, items: payload })
            addToast({
                title: "Lưu thành công",
                description: "Công thức nguyên liệu đã được cập nhật.",
                color: "success"
            })
            onClose()
        } catch {
            addToast({
                title: "Lưu thất bại",
                description: "Có lỗi xảy ra, vui lòng thử lại.",
                color: "danger"
            })
        }
    }

    return (
        <>
            <ModalBody className="py-4 gap-4 min-h-50">
                {rows.length > 0 && (
                    <div className="grid grid-cols-12 gap-2 px-1 text-xs font-semibold text-default-400 uppercase">
                        <span className="col-span-6">Nguyên liệu</span>
                        <span className="col-span-4">Số lượng</span>
                        <span className="col-span-1">Đơn vị</span>
                        <span className="col-span-1"></span>
                    </div>
                )}

                <div className="flex flex-col gap-3">
                    {rows.map((row, index) => {
                        const unit = unitMap[row.ingredientId] ?? ""
                        const disabledIngredientIds = rows
                            .map((r) => r.ingredientId)
                            .filter((id) => id !== undefined && id !== null)

                        return (
                            <div
                                key={index}
                                className="grid grid-cols-12 gap-2 items-center animate-in fade-in duration-200"
                            >
                                <div className="col-span-6">
                                    <Select
                                        size="sm"
                                        placeholder="Chọn nguyên liệu"
                                        selectedKeys={row.ingredientId ? [row.ingredientId] : []}
                                        disabledKeys={disabledIngredientIds}
                                        onSelectionChange={(keys) => {
                                            const key = Array.from(keys)[0] as string
                                            if (key) handleChangeIngredient(index, key)
                                        }}
                                        variant="bordered"
                                        classNames={{ trigger: "h-9" }}
                                    >
                                        {ingredients.map((ing) => {
                                            const usedInOtherRow = rows.find(
                                                (r, i) => i !== index && r.ingredientId === ing.id
                                            )
                                            return (
                                                <SelectItem
                                                    key={ing.id}
                                                    textValue={ing.name}
                                                    className={usedInOtherRow ? "opacity-50" : ""}
                                                >
                                                    <div className="flex items-center justify-between w-full gap-2">
                                                        <div className="flex flex-col min-w-0">
                                                            <span className="text-sm truncate">
                                                                {ing.name}
                                                            </span>
                                                            <span className="text-xs text-default-400">
                                                                {ing.baseUnit}
                                                            </span>
                                                        </div>
                                                        {usedInOtherRow && (
                                                            <span className="text-xs text-secondary font-semibold shrink-0 bg-secondary/10 px-1.5 py-0.5 rounded-full">
                                                                đã có: {usedInOtherRow.quantity}{" "}
                                                                {ing.baseUnit}
                                                            </span>
                                                        )}
                                                    </div>
                                                </SelectItem>
                                            )
                                        })}
                                    </Select>
                                </div>

                                <div className="col-span-4">
                                    <InputStyled
                                        size="sm"
                                        type="number"
                                        min={0}
                                        step={0.01}
                                        value={row.quantity === 0 ? "" : String(row.quantity)}
                                        onChange={(e) =>
                                            handleChangeQuantity(index, e.target.value)
                                        }
                                        placeholder="0"
                                        variant="bordered"
                                    />
                                </div>

                                <div className="col-span-1 flex justify-center">
                                    {unit ? (
                                        <Chip
                                            size="sm"
                                            variant="flat"
                                            color="default"
                                            className="text-xs h-7"
                                        >
                                            {unit}
                                        </Chip>
                                    ) : (
                                        <span className="text-default-300 text-xs">—</span>
                                    )}
                                </div>

                                <div className="col-span-1 flex justify-end">
                                    <ButtonIconStyled
                                        isIconOnly
                                        size="sm"
                                        variant="light"
                                        color="danger"
                                        onPress={() => handleRemoveRow(index)}
                                    >
                                        <Trash2 size={15} />
                                    </ButtonIconStyled>
                                </div>
                            </div>
                        )
                    })}
                </div>

                <ButtonStyled
                    variant="ghost"
                    color="primary"
                    startContent={<Plus size={16} />}
                    className="w-full border-dashed border-2 mt-1"
                    onPress={handleAddRow}
                >
                    Thêm nguyên liệu
                </ButtonStyled>

                {rows.length === 0 && (
                    <div className="flex flex-col items-center justify-center py-6 text-default-400 gap-2">
                        <FlaskConical size={32} className="opacity-30" />
                        <p className="text-sm italic">Biến thể này chưa có nguyên liệu nào.</p>
                        <p className="text-xs">
                            Bấm &quot;Thêm nguyên liệu&quot; để bắt đầu thiết lập công thức.
                        </p>
                    </div>
                )}
            </ModalBody>

            <Divider />

            <ModalFooter className="gap-2">
                <ButtonStyled variant="flat" color="default" onPress={onClose}>
                    Hủy
                </ButtonStyled>
                <ButtonStyled
                    color="primary"
                    onPress={handleSave}
                    isLoading={updateMutation.isPending}
                    isDisabled={!isDirty || updateMutation.isPending}
                >
                    Lưu công thức
                </ButtonStyled>
            </ModalFooter>
        </>
    )
}
