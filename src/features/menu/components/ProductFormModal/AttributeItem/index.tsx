import React from "react"
import { Button } from "@heroui/react"
import { X } from "lucide-react"
import { InputStyled } from "@/components"
import { FormikProps } from "formik"
import { ProductFormValues } from "../types"

interface AttributeItemProps {
    index: number
    variantIndex: number
    formik: FormikProps<ProductFormValues>
    remove: () => void
}

export const AttributeItem = React.memo(function AttributeItem({
    index,
    variantIndex,
    formik,
    remove
}: AttributeItemProps) {
    return (
        <div className="flex gap-2 items-center">
            <InputStyled
                placeholder="Tên (VD: Màu)"
                size="sm"
                className="w-1/3"
                {...formik.getFieldProps(`variants.${variantIndex}.attributes.${index}.key`)}
            />
            <InputStyled
                placeholder="Giá trị (VD: Đen)"
                size="sm"
                className="w-1/3"
                {...formik.getFieldProps(`variants.${variantIndex}.attributes.${index}.value`)}
            />
            <Button isIconOnly size="sm" color="danger" variant="flat" onPress={remove}>
                <X size={14} />
            </Button>
        </div>
    )
})
