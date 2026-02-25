import React, { useState } from "react"
import { Switch, Button, useDisclosure } from "@heroui/react"
import { FieldArray } from "formik"
import { Trash2, Plus, FlaskConical } from "lucide-react"
import { InputStyled, ButtonIconStyled } from "@/components"
import { AttributeItem } from "../AttributeItem"
import { RecipeIngredientList } from "../RecipeIngredientList"
import { FormikProps } from "formik"
import { ProductFormValues, VariantUI } from "@/features/products/types"
import { RecipeEditModal } from "../RecipeEditModal"
import { RecipeViewRes } from "@/features/products/models/recipes/response"

interface VariantItemProps {
    index: number
    variant: VariantUI
    formik: FormikProps<ProductFormValues>
    remove: () => void
    isDeleteAble: boolean
    /** ID thực từ BE — chỉ có khi đang Edit Mode, undefined khi variant mới chưa lưu */
    variantId?: string
    /** Tất cả recipes của product — dùng để hiển thị preview read-only trên card */
    recipes?: RecipeViewRes[]
}

export const VariantItem = React.memo(function VariantItem({
    index,
    variant,
    formik,
    remove,
    isDeleteAble,
    variantId,
    recipes = []
}: VariantItemProps) {
    const { isOpen: isRecipeOpen, onOpen: onRecipeOpen, onOpenChange: onRecipeOpenChange } = useDisclosure()
    const [recipeBtnHover, setRecipeBtnHover] = useState(false)

    const hasRecipes = variantId
        ? recipes.some((r) => r.variantId === variantId)
        : false

    return (
        <div className="border border-gray-200 rounded-xl p-4 bg-gray-50/50 relative group">
            {/* ---- Header ---- */}
            <div className="flex justify-between items-center mb-3">
                <span className="text-xs font-bold text-slate-400 uppercase">
                    Biến thể #{index + 1}
                </span>
                <div className="flex items-center gap-2">
                    {/* Nút mở modal Nguyên liệu — chỉ hiển thị khi variant đã có ID (Edit Mode) */}
                    {variantId && (
                        <Button
                            size="sm"
                            variant={hasRecipes ? "solid" : "flat"}
                            color="secondary"
                            startContent={
                                <FlaskConical
                                    size={14}
                                    className={recipeBtnHover ? "animate-pulse" : ""}
                                />
                            }
                            className="h-7 text-xs font-medium px-2"
                            onPress={onRecipeOpen}
                            onMouseEnter={() => setRecipeBtnHover(true)}
                            onMouseLeave={() => setRecipeBtnHover(false)}
                        >
                            Nguyên liệu
                            {hasRecipes && (
                                <span className="ml-1 bg-white/20 rounded-full px-1.5 text-[10px] font-bold">
                                    {recipes.filter((r) => r.variantId === variantId).length}
                                </span>
                            )}
                        </Button>
                    )}
                    {isDeleteAble && (
                        <ButtonIconStyled
                            isIconOnly
                            size="sm"
                            color="danger"
                            variant="light"
                            onPress={remove}
                        >
                            <Trash2 size={18} />
                        </ButtonIconStyled>
                    )}
                    <Switch
                        size="sm"
                        isSelected={variant.isActive}
                        onValueChange={(val) =>
                            formik.setFieldValue(`variants.${index}.isActive`, val)
                        }
                    />
                </div>
            </div>

            {/* ---- Fields ---- */}
            <div className="grid grid-cols-1 md:grid-cols-12 gap-3 mb-4">
                <div className="md:col-span-4">
                    <InputStyled
                        label="Tên phân loại"
                        size="sm"
                        isRequired
                        {...formik.getFieldProps(`variants.${index}.name`)}
                    />
                </div>
                <div className="md:col-span-4">
                    <InputStyled
                        label="Mã SKU"
                        size="sm"
                        isRequired
                        {...formik.getFieldProps(`variants.${index}.sku`)}
                    />
                </div>
                <div className="md:col-span-2">
                    <InputStyled
                        label="Giá bán"
                        type="number"
                        size="sm"
                        isRequired
                        {...formik.getFieldProps(`variants.${index}.basePrice`)}
                    />
                </div>
                <div className="md:col-span-2">
                    <InputStyled
                        label="Giá vốn"
                        type="number"
                        size="sm"
                        {...formik.getFieldProps(`variants.${index}.cost`)}
                    />
                </div>
            </div>

            {/* ---- Attributes ---- */}
            <div className="bg-white p-3 rounded-lg border border-dashed border-gray-300">
                <FieldArray name={`variants.${index}.attributes`}>
                    {(attrHelpers) => (
                        <div className="flex flex-col gap-2">
                            {variant.attributes.map((_, attrIndex) => (
                                <AttributeItem
                                    key={attrIndex}
                                    index={attrIndex}
                                    variantIndex={index}
                                    formik={formik}
                                    remove={() => attrHelpers.remove(attrIndex)}
                                />
                            ))}
                            <Button
                                size="sm"
                                variant="light"
                                color="primary"
                                className="w-fit"
                                startContent={<Plus size={14} />}
                                onPress={() => attrHelpers.push({ key: "", value: "" })}
                            >
                                Thêm thuộc tính
                            </Button>
                        </div>
                    )}
                </FieldArray>
            </div>

            {/* ---- Recipe Preview (Read-only) — chỉ hiển thị khi có variantId ---- */}
            {variantId && (
                <div className="mt-3">
                    <p className="text-xs text-slate-400 font-semibold uppercase mb-1.5 flex items-center gap-1">
                        <FlaskConical size={11} />
                        Công thức nguyên liệu
                    </p>
                    <RecipeIngredientList
                        variantId={variantId}
                        recipes={recipes}
                    />
                </div>
            )}

            {/* ---- Recipe Edit Modal ---- */}
            {variantId && (
                <RecipeEditModal
                    variantId={variantId}
                    variantName={variant.name || variant.sku}
                    isOpen={isRecipeOpen}
                    onOpenChange={onRecipeOpenChange}
                />
            )}
        </div>
    )
})
