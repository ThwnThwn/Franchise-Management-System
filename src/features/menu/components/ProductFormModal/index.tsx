"use client"

import React, { useMemo } from "react"
import {
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    Button,
    Divider,
    Select,
    SelectItem,
    Switch
} from "@heroui/react"
import { useFormik, FieldArray, FormikProvider } from "formik"
import * as Yup from "yup"
import { Plus } from "lucide-react"
import { InputStyled, ButtonStyled, ModalStyled } from "@/components"
import { useCreateProduct, useUpdateProduct, useGetAllCategories } from "../../hooks"
import { CreateProductRequest, UpdateProductRequest } from "../../models/products/request"
import { Product } from "../../models/products/respone"
import { ProductFormValues } from "./types"
import { VariantItem } from "./VariantItem"
import { parseAttributes } from "../../utils/utils"

// --- 3. COMPONENT CHÍNH ---
export function ProductFormModal({
    isOpen,
    onOpenChange,
    product
}: {
    isOpen: boolean
    onOpenChange: () => void
    product?: Product | null
}) {
    const isEditMode = !!product
    const createMutation = useCreateProduct()
    const updateMutation = useUpdateProduct()
    const { data: categoriesData } = useGetAllCategories()
    const categories = categoriesData || []

    const isLoading = createMutation.isPending || updateMutation.isPending

    const initialValues: ProductFormValues = useMemo(() => {
        if (isEditMode && product) {
            return {
                name: product.name,
                description: product.description || "",
                categoryId: product.categoryId,
                isActive: product.isActive ?? true,
                variants:
                    product.sizes?.map((v) => ({
                        sku: v.sku,
                        name: v.name || "",
                        basePrice: v.basePrice,
                        cost: v.cost,
                        isActive: v.isActive ?? true,
                        attributes: parseAttributes(v.attributes)
                    })) || []
            }
        }
        return {
            name: "",
            description: "",
            categoryId: "",
            isActive: true,
            variants: [
                { sku: "", name: "Size S", basePrice: 0, cost: 0, isActive: true, attributes: [] }
            ]
        }
    }, [isEditMode, product])

    const validationSchema = Yup.object({
        name: Yup.string().required("Vui lòng nhập tên sản phẩm"),
        categoryId: Yup.string().required("Vui lòng chọn danh mục"),
        variants: Yup.array()
            .of(
                Yup.object({
                    sku: Yup.string().required("Bắt buộc"),
                    name: Yup.string().required("Bắt buộc"),
                    basePrice: Yup.number().min(0).required("Nhập giá")
                })
            )
            .min(1)
    })

    const formik = useFormik<ProductFormValues>({
        initialValues,
        validationSchema,
        enableReinitialize: true,
        validateOnChange: false,
        validateOnBlur: true,
        onSubmit: async (values) => {
            try {
                const variantsPayload = values.variants.map((v) => ({
                    sku: v.sku,
                    name: v.name,
                    basePrice: Number(v.basePrice),
                    cost: Number(v.cost),
                    isActive: v.isActive,
                    attributes: JSON.stringify(
                        v.attributes.reduce((acc: Record<string, string>, curr) => {
                            if (curr.key && curr.value) acc[curr.key] = curr.value
                            return acc
                        }, {})
                    )
                }))

                if (isEditMode && product) {
                    const payload: UpdateProductRequest = { ...values, variants: variantsPayload }
                    await updateMutation.mutateAsync({ id: product.id, req: payload })
                } else {
                    const payload: CreateProductRequest = { ...values, variants: variantsPayload }
                    await createMutation.mutateAsync(payload)
                }
                onOpenChange()
            } catch (error) {
                console.error(error)
            }
        }
    })

    return (
        <ModalStyled
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="3xl"
            scrollBehavior="inside"
            onClose={() => formik.resetForm()}
        >
            <ModalContent>
                <ModalHeader className="flex flex-col gap-1">
                    <span>{isEditMode ? "Cập nhật sản phẩm" : "Tạo sản phẩm mới"}</span>
                    <div className="flex items-center gap-2">
                        <Switch
                            isSelected={formik.values.isActive}
                            onValueChange={(val) => formik.setFieldValue("isActive", val)}
                            color="success"
                            size="sm"
                        />
                        <span
                            className={`text-sm ${formik.values.isActive ? "text-success-600 font-medium" : "text-slate-400"}`}
                        >
                            {formik.values.isActive ? "Đang kinh doanh" : "Ngưng kinh doanh"}
                        </span>
                    </div>
                </ModalHeader>
                <ModalBody>
                    <form className="flex flex-col gap-6">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                            <InputStyled
                                label="Tên sản phẩm"
                                isRequired
                                {...formik.getFieldProps("name")}
                                isInvalid={!!(formik.touched.name && formik.errors.name)}
                                errorMessage={formik.errors.name}
                            />
                            <Select
                                label="Danh mục"
                                variant="bordered"
                                isRequired
                                disableAnimation
                                selectedKeys={
                                    formik.values.categoryId ? [formik.values.categoryId] : []
                                }
                                onChange={(e) => formik.setFieldValue("categoryId", e.target.value)}
                                isInvalid={
                                    !!(formik.touched.categoryId && formik.errors.categoryId)
                                }
                                errorMessage={formik.errors.categoryId}
                            >
                                {categories.map((cat: any) => (
                                    <SelectItem key={cat.id}>{cat.name}</SelectItem>
                                ))}
                            </Select>
                            <InputStyled
                                label="Mô tả sản phẩm"
                                {...formik.getFieldProps("description")}
                                isInvalid={
                                    !!(formik.touched.description && formik.errors.description)
                                }
                                errorMessage={formik.errors.description}
                                className="col-span-2"
                            />
                        </div>

                        <Divider />

                        <div className="flex justify-between items-center">
                            <h3 className="text-lg font-semibold">Biến thể sản phẩm</h3>
                        </div>

                        <FormikProvider value={formik}>
                            <FieldArray name="variants">
                                {(variantHelpers) => (
                                    <div className="flex flex-col gap-4">
                                        {formik.values.variants.map((variant, index) => (
                                            <VariantItem
                                                key={index}
                                                index={index}
                                                variant={variant}
                                                formik={formik}
                                                isDeleteAble={formik.values.variants.length > 1}
                                                remove={() => variantHelpers.remove(index)}
                                            />
                                        ))}
                                        <Button
                                            variant="ghost"
                                            color="primary"
                                            startContent={<Plus />}
                                            className="w-full border-dashed border-2"
                                            onPress={() =>
                                                variantHelpers.push({
                                                    sku: "",
                                                    name: "",
                                                    basePrice: 0,
                                                    cost: 0,
                                                    isActive: true,
                                                    attributes: []
                                                })
                                            }
                                        >
                                            Thêm phân loại mới
                                        </Button>
                                    </div>
                                )}
                            </FieldArray>
                        </FormikProvider>
                    </form>
                </ModalBody>
                <ModalFooter>
                    {!isLoading && (
                        <Button
                            color="danger"
                            variant="light"
                            onPress={() => {
                                formik.resetForm()
                                onOpenChange()
                            }}
                        >
                            Hủy
                        </Button>
                    )}
                    <ButtonStyled
                        color="primary"
                        onPress={() => formik.handleSubmit()}
                        isLoading={isLoading}
                    >
                        {isEditMode ? "Lưu thay đổi" : "Tạo sản phẩm"}
                    </ButtonStyled>
                </ModalFooter>
            </ModalContent>
        </ModalStyled>
    )
}
