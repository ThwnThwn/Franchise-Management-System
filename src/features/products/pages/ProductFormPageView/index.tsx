"use client"

import React, { useMemo } from "react"
import { Button, Divider, Switch, AutocompleteItem, Card, CardBody, Spinner } from "@heroui/react"
import { useFormik, FieldArray, FormikProvider } from "formik"
import * as Yup from "yup"
import { Plus, ArrowLeft } from "lucide-react"
import { useRouter } from "next/navigation"

// Components
import { InputStyled, AutocompleteStyled, UnsavedChangesNotifier } from "@/components"

// Hooks & Utils
import {
    useCreateProduct,
    useUpdateProduct,
    useGetAllCategories,
    useGetProductById
} from "../../hooks"
import { CreateProductRequest, UpdateProductRequest } from "../../models/products/request"
import { parseAttributes } from "../../utils/utils"
import { ProductFormValues } from "../../types"
import { VariantItem } from "../../components/VariantItem"
import { ProductImageUploader } from "../../components"
import { PRODUCTS_ROUTES } from "../../constants/routes"

export default function ProductFormPageView({ productId }: { productId?: string }) {
    const router = useRouter()
    const isEditMode = !!productId

    // 1. Fetch Data
    const { data: product, isLoading: isFetchingProduct } = useGetProductById({
        id: productId || "",
        enabled: isEditMode
    })

    const { data: categoriesData } = useGetAllCategories()
    const categories = categoriesData || []

    const createMutation = useCreateProduct({
        onSuccess: () => {
            router.push(PRODUCTS_ROUTES.LIST)
        }
    })
    const updateMutation = useUpdateProduct()

    const isSubmitting = createMutation.isPending || updateMutation.isPending

    // 2. Setup Form Values
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
        enableReinitialize: true, // Quan trọng: Form reset khi có data mới
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

                if (isEditMode && productId) {
                    const payload: UpdateProductRequest = { ...values, variants: variantsPayload }
                    await updateMutation.mutateAsync({ id: productId, req: payload })
                } else {
                    const payload: CreateProductRequest = { ...values, variants: variantsPayload }
                    await createMutation.mutateAsync(payload)
                }
            } catch (error) {
                console.error(error)
            }
        }
    })

    if (isEditMode && isFetchingProduct) {
        return (
            <div className="flex justify-center items-center h-[50vh]">
                <Spinner label="Đang tải thông tin sản phẩm..." size="lg" />
            </div>
        )
    }

    return (
        <div className="max-w-5xl mx-auto p-4 flex flex-col gap-6 pb-32 relative">
            {/* Header Page */}
            <div className="flex items-center gap-4">
                <Button
                    isIconOnly
                    variant="light"
                    onPress={() => router.push(PRODUCTS_ROUTES.LIST)}
                >
                    <ArrowLeft size={20} />
                </Button>
                <div>
                    <h1 className="text-2xl font-bold">
                        {isEditMode ? "Cập nhật sản phẩm" : "Thêm sản phẩm mới"}
                    </h1>
                    {isEditMode && product && (
                        <p className="text-gray-500 text-sm">SKU Gốc: {product.sizes?.[0]?.sku}</p>
                    )}
                </div>
                {/* Switch Active */}
                <div className="flex justify-end items-center gap-2">
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
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Cột trái: Thông tin chính */}
                <div className="lg:col-span-2 flex flex-col gap-6">
                    <Card>
                        <CardBody className="gap-6 p-6">
                            {/* Inputs */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <InputStyled
                                    label="Tên sản phẩm"
                                    isRequired
                                    {...formik.getFieldProps("name")}
                                    isInvalid={!!(formik.touched.name && formik.errors.name)}
                                    errorMessage={formik.errors.name}
                                />
                                <AutocompleteStyled
                                    label={"Danh mục"}
                                    items={categories}
                                    selectedKey={formik.values.categoryId || ""}
                                    onSelectionChange={(id) =>
                                        formik.setFieldValue("categoryId", id)
                                    }
                                    isRequired
                                    isClearable={false}
                                >
                                    {(categories ?? []).map((item) => (
                                        <AutocompleteItem key={item.id}>
                                            {item.name}
                                        </AutocompleteItem>
                                    ))}
                                </AutocompleteStyled>
                                <InputStyled
                                    label="Mô tả sản phẩm"
                                    {...formik.getFieldProps("description")}
                                    className="col-span-1 md:col-span-2"
                                />
                            </div>
                        </CardBody>
                    </Card>

                    {/* Variants Section */}
                    <Card>
                        <CardBody className="p-6 gap-4">
                            <div className="flex justify-between items-center">
                                <h3 className="text-lg font-semibold">Biến thể sản phẩm</h3>
                            </div>
                            <Divider />

                            <form onSubmit={formik.handleSubmit}>
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
                                                        isDeleteAble={
                                                            formik.values.variants.length > 1
                                                        }
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
                        </CardBody>
                    </Card>
                </div>

                {/* Cột phải: Ảnh & Actions */}
                <div className="flex flex-col gap-6">
                    {/* Component Upload Ảnh (Chỉ hiện khi Update) */}
                    {isEditMode && productId && (
                        <Card>
                            <CardBody className="p-6 flex flex-col items-center gap-4">
                                <h3 className="font-semibold self-start">Hình ảnh</h3>
                                <ProductImageUploader
                                    productId={productId}
                                    currentImageUrl={product?.imageUrl}
                                />
                            </CardBody>
                        </Card>
                    )}

                    {/* Action Buttons (Sticky) */}
                    {/* <Card className="sticky top-6">
                        <CardBody className="p-4 flex flex-col gap-3">
                            <ButtonStyled
                                color="primary"
                                onPress={() => formik.handleSubmit()}
                                isLoading={isSubmitting}
                                className="w-full font-semibold shadow-md"
                                isDisabled={!formik.dirty}
                            >
                                {isEditMode ? "Lưu thay đổi" : "Tạo sản phẩm"}
                            </ButtonStyled>

                            <Button
                                variant="flat"
                                color="default"
                                onPress={() => router.back()}
                                isDisabled={isSubmitting}
                            >
                                Hủy bỏ
                            </Button>
                        </CardBody>
                    </Card> */}
                </div>
            </div>

            {/* --- NOTIFIER COMPONENT --- */}
            <UnsavedChangesNotifier
                hasChanges={formik.dirty} // Formik tự track việc này
                isLoading={isSubmitting}
                onSave={() => formik.handleSubmit()}
                onReset={() => formik.resetForm()}
                title={isEditMode ? undefined : "Thông báo"}
                message={isEditMode ? "Thông tin sản phẩm đã thay đổi." : "Tạo sản phẩm"}
            />
        </div>
    )
}
