import React from "react"
import { Button } from "@heroui/react"
import { X } from "lucide-react"
import { InputStyled } from "@/components"
import { FormikProps } from "formik"
import { ProductFormValues } from "@/features/products/types"
import { useFormikHelpers } from "@/hooks"

interface AttributeItemProps {
    index: number
    variantIndex: number
    formik: FormikProps<ProductFormValues>
    remove: () => void
    isReadOnly: boolean
}

export const AttributeItem = React.memo(function AttributeItem({
    index,
    variantIndex,
    formik,
    remove,
    isReadOnly
}: AttributeItemProps) {
    const { isInvalid, getErrorMessage } = useFormikHelpers(formik)
    const getFormikPath = (field: string) => `variants.${variantIndex}.attributes.${index}.${field}`

    return (
        <div className="flex gap-2 items-center">
            <InputStyled
                placeholder="Tên (VD: Màu)"
                size="sm"
                className="w-1/3"
                {...formik.getFieldProps(`variants.${variantIndex}.attributes.${index}.key`)}
                isInvalid={isInvalid(getFormikPath("key"))}
                errorMessage={getErrorMessage(getFormikPath("key"))}
                isReadOnly={isReadOnly}
            />
            <InputStyled
                placeholder="Giá trị (VD: Đen)"
                size="sm"
                className="w-1/3"
                {...formik.getFieldProps(`variants.${variantIndex}.attributes.${index}.value`)}
                isInvalid={isInvalid(getFormikPath("value"))}
                errorMessage={getErrorMessage(getFormikPath("value"))}
                isReadOnly={isReadOnly}
            />
            <Button
                isIconOnly
                size="sm"
                color="danger"
                variant="flat"
                onPress={remove}
                hidden={isReadOnly}
            >
                <X size={14} />
            </Button>
        </div>
    )
})
