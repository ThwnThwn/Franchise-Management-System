import React from "react"
import { Switch, Button } from "@heroui/react"
import { FieldArray } from "formik"
import { Trash2, Plus } from "lucide-react"
import { InputStyled, ButtonIconStyled } from "@/components"
import { AttributeItem } from "../AttributeItem"
import { FormikProps } from "formik"
import { ProductFormValues, VariantUI } from "@/features/products/types"

interface VariantItemProps {
    index: number
    variant: VariantUI
    formik: FormikProps<ProductFormValues>
    remove: () => void
    isDeleteAble: boolean
}

export const VariantItem = React.memo(function VariantItem({
    index,
    variant,
    formik,
    remove,
    isDeleteAble
}: VariantItemProps) {
    return (
        <div className="border border-gray-200 rounded-xl p-4 bg-gray-50/50 relative group">
            <div className="flex justify-between items-center mb-3">
                <span className="text-xs font-bold text-slate-400 uppercase">
                    Biến thể #{index + 1}
                </span>
                <div className="flex items-center gap-2">
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
        </div>
    )
})
