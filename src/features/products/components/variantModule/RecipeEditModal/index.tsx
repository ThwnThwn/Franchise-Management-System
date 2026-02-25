"use client"

import React, { useEffect, useState } from "react"
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
import { useGetIngredients } from "@/features/inventory/hooks/useInventory"
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
// Component
// ============================================================

export function RecipeEditModal({ variantId, variantName, isOpen, onOpenChange }: Props) {
    // --- Fetch dữ liệu ---
    const { data: recipesData, isLoading: isLoadingRecipes } = useGetRecipes({
        req: { variantIds: [variantId] },
        enabled: isOpen && !!variantId
    })

    const { data: ingredientsData, isLoading: isLoadingIngredients } = useGetIngredients({
        enabled: isOpen
    })
    const ingredients = ingredientsData?.items ?? []

    // --- State nội bộ (local list): sync từ server khi modal mở ---
    const [rows, setRows] = useState<RecipeRow[]>([])
    const [isDirty, setIsDirty] = useState(false)

    // Sync dữ liệu từ server vào local state khi modal mở
    // - isOpen trong deps để re-sync khi mở lại dù data đã cache
    useEffect(() => {
        // Không sync khi modal đóng
        if (!isOpen) return
        // Chờ cả hai nguồn dữ liệu sẵn sàng
        if (!recipesData || ingredients.length === 0) return

        const variantRecipes = recipesData.filter((r) => r.variantId === variantId)
        if (variantRecipes.length === 0) {
            setRows([])
            setIsDirty(false)
            return
        }

        const resolved = variantRecipes.map((r) => {
            // Ưu tiên dùng ingredientId từ BE (sau khi rebuild)
            // Fallback: match theo name nếu BE chưa trả về ingredientId
            const ingredientId =
                r.ingredientId && r.ingredientId !== "00000000-0000-0000-0000-000000000000"
                    ? r.ingredientId
                    : ingredients.find((ing) => ing.name === r.name)?.id ?? ""

            return { ingredientId, quantity: r.quantity }
        })

        setRows(resolved)
        setIsDirty(false)
    }, [isOpen, recipesData, variantId, ingredients])

    // --- Mutation lưu ---
    const updateMutation = useUpdateVariantRecipes()

    const handleClose = () => {
        setIsDirty(false)
        onOpenChange()
    }

    // --- Handlers thêm / xóa dòng ---
    const handleAddRow = () => {
        setRows((prev) => [...prev, { ingredientId: "", quantity: 0 }])
        setIsDirty(true)
    }

    const handleRemoveRow = (index: number) => {
        setRows((prev) => prev.filter((_, i) => i !== index))
        setIsDirty(true)
    }

    const handleChangeIngredient = (index: number, ingredientId: string) => {
        // Không cho phép chọn trùng nguyên liệu
        const isDuplicate = rows.some((r, i) => i !== index && r.ingredientId === ingredientId)
        if (isDuplicate) {
            addToast({
                title: "Trùng nguyên liệu",
                description: "Nguyên liệu này đã có trong danh sách.",
                color: "warning"
            })
            return
        }
        setRows((prev) =>
            prev.map((r, i) => (i === index ? { ...r, ingredientId } : r))
        )
        setIsDirty(true)
    }

    const handleChangeQuantity = (index: number, val: string) => {
        const n = parseFloat(val)
        setRows((prev) =>
            prev.map((r, i) => (i === index ? { ...r, quantity: isNaN(n) ? 0 : n } : r))
        )
        setIsDirty(true)
    }

    // --- Lưu ---
    const handleSave = async () => {
        const invalidRows = rows.filter((r) => !r.ingredientId || r.quantity <= 0)
        if (invalidRows.length > 0) {
            addToast({
                title: "Dữ liệu chưa đầy đủ",
                description: "Vui lòng chọn nguyên liệu và nhập số lượng hợp lệ (> 0) cho tất cả các dòng.",
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
            setIsDirty(false)
            onOpenChange()
        } catch {
            addToast({
                title: "Lưu thất bại",
                description: "Có lỗi xảy ra, vui lòng thử lại.",
                color: "danger"
            })
        }
    }

    // --- Helper: lấy unit cho dòng hiện tại ---
    const getUnit = (ingredientId: string) => {
        return ingredients.find((i) => i.id === ingredientId)?.baseUnit ?? ""
    }

    const isLoading = isLoadingRecipes || isLoadingIngredients

    return (
        <ModalStyled
            isDismissable={!isDirty}
            isOpen={isOpen}
            onOpenChange={handleClose}
            size="lg"
        >
            <ModalContent className="max-w-xl">
                {/* ---- Header ---- */}
                <ModalHeader className="flex flex-col gap-1 pb-2">
                    <div className="flex items-center gap-2">
                        <FlaskConical size={20} className="text-primary" />
                        <span className="text-lg font-semibold">Nguyên liệu</span>
                    </div>
                    {variantName && (
                        <span className="text-sm text-default-400 font-normal">
                            Biến thể: <span className="text-default-600 font-medium">{variantName}</span>
                        </span>
                    )}
                </ModalHeader>

                <Divider />

                {/* ---- Body ---- */}
                <ModalBody className="py-4 gap-4 min-h-[200px]">
                    {isLoading ? (
                        <div className="flex justify-center items-center h-32">
                            <Spinner label="Đang tải..." />
                        </div>
                    ) : (
                        <>
                            {/* Header của bảng */}
                            {rows.length > 0 && (
                                <div className="grid grid-cols-12 gap-2 px-1 text-xs font-semibold text-default-400 uppercase">
                                    <span className="col-span-6">Nguyên liệu</span>
                                    <span className="col-span-4">Số lượng</span>
                                    <span className="col-span-1">Đơn vị</span>
                                    <span className="col-span-1"></span>
                                </div>
                            )}

                            {/* Danh sách dòng nguyên liệu */}
                            <div className="flex flex-col gap-3">
                                {rows.map((row, index) => {
                                    const unit = getUnit(row.ingredientId)
                                    return (
                                        <div
                                            key={index}
                                            className="grid grid-cols-12 gap-2 items-center animate-in fade-in duration-200"
                                        >
                                            {/* Dropdown chọn nguyên liệu */}
                                            <div className="col-span-6">
                                                <Select
                                                    size="sm"
                                                    placeholder="Chọn nguyên liệu"
                                                    selectedKeys={
                                                        row.ingredientId ? [row.ingredientId] : []
                                                    }
                                                    onSelectionChange={(keys) => {
                                                        const key = Array.from(keys)[0] as string
                                                        if (key) handleChangeIngredient(index, key)
                                                    }}
                                                    variant="bordered"
                                                    classNames={{ trigger: "h-9" }}
                                                >
                                                    {ingredients.map((ing) => {
                                                        // Kiểm tra xem nguyên liệu này đã được dùng ở dòng khác chưa
                                                        const usedInOtherRow = rows.find(
                                                            (r, i) => i !== index && r.ingredientId === ing.id
                                                        )
                                                        return (
                                                            <SelectItem
                                                                key={ing.id}
                                                                textValue={ing.name}
                                                                isDisabled={!!usedInOtherRow}
                                                                className={usedInOtherRow ? "opacity-50" : ""}
                                                            >
                                                                <div className="flex items-center justify-between w-full gap-2">
                                                                    <div className="flex flex-col min-w-0">
                                                                        <span className="text-sm truncate">{ing.name}</span>
                                                                        <span className="text-xs text-default-400">
                                                                            {ing.baseUnit}
                                                                        </span>
                                                                    </div>
                                                                    {usedInOtherRow && (
                                                                        <span className="text-xs text-secondary font-semibold shrink-0 bg-secondary/10 px-1.5 py-0.5 rounded-full">
                                                                            đã có: {usedInOtherRow.quantity} {ing.baseUnit}
                                                                        </span>
                                                                    )}
                                                                </div>
                                                            </SelectItem>
                                                        )
                                                    })}
                                                </Select>
                                            </div>

                                            {/* Input số lượng */}
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

                                            {/* Đơn vị (auto) */}
                                            <div className="col-span-1 flex justify-center">
                                                {unit ? (
                                                    <Chip size="sm" variant="flat" color="default" className="text-xs h-7">
                                                        {unit}
                                                    </Chip>
                                                ) : (
                                                    <span className="text-default-300 text-xs">—</span>
                                                )}
                                            </div>

                                            {/* Xóa dòng */}
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

                            {/* Nút thêm dòng mới */}
                            <ButtonStyled
                                variant="ghost"
                                color="primary"
                                startContent={<Plus size={16} />}
                                className="w-full border-dashed border-2 mt-1"
                                onPress={handleAddRow}
                            >
                                Thêm nguyên liệu
                            </ButtonStyled>

                            {/* Empty state */}
                            {rows.length === 0 && (
                                <div className="flex flex-col items-center justify-center py-6 text-default-400 gap-2">
                                    <FlaskConical size={32} className="opacity-30" />
                                    <p className="text-sm italic">
                                        Biến thể này chưa có nguyên liệu nào.
                                    </p>
                                    <p className="text-xs">
                                        Bấm &quot;Thêm nguyên liệu&quot; để bắt đầu thiết lập công thức.
                                    </p>
                                </div>
                            )}
                        </>
                    )}
                </ModalBody>

                <Divider />

                {/* ---- Footer ---- */}
                <ModalFooter className="gap-2">
                    <ButtonStyled variant="flat" color="default" onPress={handleClose}>
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
            </ModalContent>
        </ModalStyled>
    )
}
