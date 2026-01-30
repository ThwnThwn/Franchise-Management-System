"use client"

import React, { useMemo } from "react" // 1. Import useMemo
import {
    Modal,
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
import { Plus, Trash2, X } from "lucide-react"
import { InputStyled, ButtonStyled, ButtonIconStyled } from "@/components"
// 2. Import thêm hook Update và các Type cần thiết
import { useCreateProduct, useUpdateProduct, useGetAllCategories } from "../../hooks"
import { CreateProductRequest, UpdateProductRequest } from "../../models/products/request"
import { Product } from "../../models/products/respone"

interface ProductFormModalProps {
    // Đổi tên interface cho chuẩn
    isOpen: boolean
    onOpenChange: () => void
    product?: Product | null // 3. Thêm prop product (Optional)
}

// ... (Giữ nguyên các Interface AttributeUI, VariantUI cũ của bạn) ...
interface AttributeUI {
    key: string
    value: string
}

interface VariantUI {
    sku: string
    name: string
    basePrice: number
    cost: number
    isActive: boolean
    attributes: AttributeUI[]
}

interface ProductFormValues {
    name: string
    description: string
    categoryId: string
    isActive: boolean
    variants: VariantUI[]
}

export function ProductFormModal({ isOpen, onOpenChange, product }: ProductFormModalProps) {
    // 4. Xác định đang ở chế độ nào
    const isEditMode = !!product

    const createMutation = useCreateProduct()
    const updateMutation = useUpdateProduct() // Gọi hook update
    const { data: categoriesData } = useGetAllCategories()
    const categories = categoriesData || [] // Safe check

    const isLoading = createMutation.isPending || updateMutation.isPending

    // 5. Helper: Parse JSON attributes từ DB ra UI
    const parseAttributes = (jsonString?: string): AttributeUI[] => {
        if (!jsonString) return []
        try {
            const parsed = JSON.parse(jsonString)
            return Object.entries(parsed).map(([key, value]) => ({
                key,
                value: String(value)
            }))
        } catch {
            return []
        }
    }

    // 6. Cấu hình giá trị ban đầu (Dùng useMemo để tính toán 1 lần khi product thay đổi)
    const initialValues: ProductFormValues = useMemo(() => {
        // --- NẾU LÀ UPDATE ---
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

        // --- NẾU LÀ CREATE (Mặc định cũ của bạn) ---
        return {
            name: "",
            description: "",
            categoryId: "",
            isActive: true,
            variants: [
                {
                    sku: "",
                    name: "Size S",
                    basePrice: 0,
                    cost: 0,
                    isActive: true,
                    attributes: []
                }
            ]
        }
    }, [isEditMode, product])

    // 7. Schema Validation (Giữ nguyên)
    const validationSchema = Yup.object({
        name: Yup.string().required("Vui lòng nhập tên sản phẩm"),
        categoryId: Yup.string().required("Vui lòng chọn danh mục"),
        variants: Yup.array()
            .of(
                Yup.object({
                    sku: Yup.string().required("SKU là bắt buộc"),
                    name: Yup.string().required("Tên biến thể là bắt buộc"),
                    basePrice: Yup.number().min(0, "Giá không hợp lệ").required("Nhập giá bán"),
                    cost: Yup.number().min(0, "Giá vốn không hợp lệ"),
                    attributes: Yup.array().of(
                        Yup.object({
                            key: Yup.string().required("Nhập tên"),
                            value: Yup.string().required("Nhập giá trị")
                        })
                    )
                })
            )
            .min(1, "Phải có ít nhất 1 biến thể")
    })

    // 8. Xử lý Submit (Chia luồng Create/Update)
    const handleSubmit = async (values: ProductFormValues) => {
        try {
            // Transform Data chung (Logic cũ của bạn)
            const variantsPayload = values.variants.map((v) => {
                const attrObject = v.attributes.reduce((acc: any, curr) => {
                    if (curr.key && curr.value) acc[curr.key] = curr.value
                    return acc
                }, {})

                return {
                    sku: v.sku,
                    name: v.name,
                    basePrice: v.basePrice,
                    cost: v.cost,
                    isActive: v.isActive,
                    attributes: JSON.stringify(attrObject)
                }
            })

            if (isEditMode && product) {
                // --- GỌI API UPDATE ---
                const payload: UpdateProductRequest = {
                    name: values.name,
                    description: values.description,
                    categoryId: values.categoryId,
                    isActive: values.isActive,
                    variants: variantsPayload
                }
                await updateMutation.mutateAsync({ id: product.id, req: payload })
            } else {
                // --- GỌI API CREATE ---
                const payload: CreateProductRequest = {
                    name: values.name,
                    description: values.description,
                    categoryId: values.categoryId,
                    variants: variantsPayload
                }
                await createMutation.mutateAsync(payload)
            }

            onOpenChange() // Đóng modal
            // Không cần resetForm thủ công vì enableReinitialize sẽ lo khi mở lại
        } catch (error) {
            console.error(error)
        }
    }

    const formik = useFormik<ProductFormValues>({
        initialValues,
        validationSchema,
        onSubmit: handleSubmit,
        enableReinitialize: true, // QUAN TRỌNG: Để form tự điền dữ liệu mới khi mở modal khác
        validateOnChange: false, // QUAN TRỌNG: Tắt cái này đi để hết LAG khi gõ phím
        validateOnBlur: true // Chỉ validate khi blur ô input
    })

    // Helper đóng modal và reset
    const handleClose = () => {
        formik.resetForm()
        onOpenChange()
    }

    return (
        <Modal
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            size="3xl"
            scrollBehavior="inside"
            isDismissable={!isLoading}
            onClose={handleClose} // Reset form khi đóng
        >
            <ModalContent>
                {() => (
                    <>
                        <ModalHeader className="flex flex-col gap-1">
                            {/* Đổi tiêu đề động */}
                            <span>{isEditMode ? "Cập nhật sản phẩm" : "Tạo sản phẩm mới"}</span>
                            <div className="flex items-center gap-2">
                                <span
                                    className={`text-sm ${formik.values.isActive ? "text-success-600 font-medium" : "text-slate-400"}`}
                                >
                                    {formik.values.isActive
                                        ? "Đang kinh doanh"
                                        : "Ngưng kinh doanh"}
                                </span>
                                <Switch
                                    isSelected={formik.values.isActive}
                                    onValueChange={(val) => formik.setFieldValue("isActive", val)}
                                    color="success"
                                    size="sm"
                                />
                            </div>
                        </ModalHeader>
                        <ModalBody>
                            <form onSubmit={formik.handleSubmit} className="flex flex-col gap-6">
                                {/* ... (GIỮ NGUYÊN PHẦN UI CỦA BẠN - KHÔNG CẦN SỬA GÌ) ... */}
                                {/* ... Copy y nguyên phần Input, Select, FieldArray từ code cũ vào đây ... */}
                                {/* Chỉ lưu ý: Code UI của bạn đang rất tốt, không cần sửa logic render */}

                                {/* Ví dụ đoạn Input Name (Giữ nguyên) */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <InputStyled
                                        label="Tên sản phẩm"
                                        placeholder="VD: Cà phê sữa đá"
                                        isRequired
                                        {...formik.getFieldProps("name")}
                                        isInvalid={!!(formik.touched.name && formik.errors.name)}
                                        errorMessage={formik.errors.name}
                                    />
                                    {/* ... Select Category ... */}
                                    <Select
                                        label="Danh mục"
                                        placeholder="Chọn danh mục"
                                        isRequired
                                        selectedKeys={
                                            formik.values.categoryId
                                                ? [formik.values.categoryId]
                                                : []
                                        }
                                        onChange={(e) =>
                                            formik.setFieldValue("categoryId", e.target.value)
                                        }
                                        isInvalid={
                                            !!(
                                                formik.touched.categoryId &&
                                                formik.errors.categoryId
                                            )
                                        }
                                        errorMessage={formik.errors.categoryId}
                                        disableAnimation // Tối ưu performance
                                    >
                                        {categories.map((cat: any) => (
                                            <SelectItem key={cat.id}>{cat.name}</SelectItem>
                                        ))}
                                    </Select>

                                    {/* ... Description ... */}
                                    <div className="md:col-span-2">
                                        <InputStyled
                                            label="Mô tả"
                                            placeholder="Mô tả chi tiết sản phẩm..."
                                            {...formik.getFieldProps("description")}
                                        />
                                    </div>
                                </div>
                                <Divider />

                                {/* ... FieldArray Variants (Giữ nguyên code cũ) ... */}
                                <div className="flex justify-between items-center">
                                    <h3 className="text-lg font-semibold">
                                        Phân loại & Giá (Variants)
                                    </h3>
                                </div>
                                <FormikProvider value={formik}>
                                    {/* Copy nguyên khối FieldArray cũ vào đây */}
                                    <FieldArray name="variants">
                                        {(variantHelpers) => (
                                            <div className="flex flex-col gap-4">
                                                {formik.values.variants.map((variant, index) => {
                                                    const isDeleteAble =
                                                        formik.values.variants.length > 1
                                                    return (
                                                        <div
                                                            key={index}
                                                            className="border border-gray-200 rounded-xl p-4 bg-gray-50/50 relative group"
                                                        >
                                                            <div className="flex justify-between items-center mb-3">
                                                                <span className="text-xs font-bold text-slate-400 uppercase">
                                                                    Biến thể #{index + 1}
                                                                </span>
                                                                <div className="flex items-center gap-2">
                                                                    <span className="text-xs text-slate-500">
                                                                        {variant.isActive
                                                                            ? "Bán"
                                                                            : "Ẩn"}
                                                                    </span>
                                                                    <Switch
                                                                        size="sm"
                                                                        isSelected={
                                                                            variant.isActive
                                                                        }
                                                                        onValueChange={(val) =>
                                                                            formik.setFieldValue(
                                                                                `variants.${index}.isActive`,
                                                                                val
                                                                            )
                                                                        }
                                                                    />
                                                                </div>
                                                            </div>
                                                            {/* ... Copy nội dung variant item ... */}
                                                            <div className="items-center grid grid-cols-1 md:grid-cols-12 gap-3 mb-4">
                                                                {/* ... Inputs SKU, Name, Price ... */}
                                                                <div className="md:col-span-4">
                                                                    <InputStyled
                                                                        label="Tên phân loại / Size"
                                                                        size="sm"
                                                                        isRequired
                                                                        {...formik.getFieldProps(
                                                                            `variants.${index}.name`
                                                                        )}
                                                                        isInvalid={
                                                                            !!(
                                                                                formik.touched
                                                                                    .variants?.[
                                                                                    index
                                                                                ]?.name &&
                                                                                (
                                                                                    formik.errors
                                                                                        .variants?.[
                                                                                        index
                                                                                    ] as any
                                                                                )?.name
                                                                            )
                                                                        }
                                                                        errorMessage={
                                                                            (
                                                                                formik.errors
                                                                                    .variants?.[
                                                                                    index
                                                                                ] as any
                                                                            )?.name
                                                                        }
                                                                    />
                                                                </div>
                                                                {/* ... Các input còn lại giữ nguyên ... */}
                                                                {/* ... Nhớ copy cả đoạn Input SKU, Price, Cost, TrashIcon ... */}

                                                                {/* Example SKU */}
                                                                <div
                                                                    className={`${isDeleteAble ? "md:col-span-3" : "md:col-span-4"}`}
                                                                >
                                                                    <InputStyled
                                                                        label="Mã SKU"
                                                                        size="sm"
                                                                        isRequired
                                                                        {...formik.getFieldProps(
                                                                            `variants.${index}.sku`
                                                                        )}
                                                                        isInvalid={
                                                                            !!(
                                                                                formik.touched
                                                                                    .variants?.[
                                                                                    index
                                                                                ]?.sku &&
                                                                                (
                                                                                    formik.errors
                                                                                        .variants?.[
                                                                                        index
                                                                                    ] as any
                                                                                )?.sku
                                                                            )
                                                                        }
                                                                        errorMessage={
                                                                            (
                                                                                formik.errors
                                                                                    .variants?.[
                                                                                    index
                                                                                ] as any
                                                                            )?.sku
                                                                        }
                                                                    />
                                                                </div>
                                                                <div className="md:col-span-2">
                                                                    <InputStyled
                                                                        label="Giá bán"
                                                                        type="number"
                                                                        size="sm"
                                                                        isRequired
                                                                        {...formik.getFieldProps(
                                                                            `variants.${index}.basePrice`
                                                                        )}
                                                                    />
                                                                </div>
                                                                <div className="md:col-span-2">
                                                                    <InputStyled
                                                                        label="Giá vốn"
                                                                        type="number"
                                                                        size="sm"
                                                                        {...formik.getFieldProps(
                                                                            `variants.${index}.cost`
                                                                        )}
                                                                    />
                                                                </div>
                                                                {isDeleteAble && (
                                                                    <div className="md:col-span-1 flex items-end pb-1">
                                                                        <ButtonIconStyled
                                                                            isIconOnly
                                                                            size="sm"
                                                                            color="danger"
                                                                            variant="light"
                                                                            onPress={() =>
                                                                                variantHelpers.remove(
                                                                                    index
                                                                                )
                                                                            }
                                                                        >
                                                                            <Trash2 size={18} />
                                                                        </ButtonIconStyled>
                                                                    </div>
                                                                )}
                                                            </div>

                                                            {/* Attributes Builder (Giữ nguyên) */}
                                                            <div className="bg-white p-3 rounded-lg border border-dashed border-gray-300">
                                                                <div className="text-xs font-semibold text-slate-500 mb-2 uppercase">
                                                                    Thuộc tính bổ sung (JSON)
                                                                </div>
                                                                <FieldArray
                                                                    name={`variants.${index}.attributes`}
                                                                >
                                                                    {(attrHelpers) => (
                                                                        <div className="flex flex-col gap-2">
                                                                            {variant.attributes.map(
                                                                                (
                                                                                    attr,
                                                                                    attrIndex
                                                                                ) => (
                                                                                    <div
                                                                                        key={
                                                                                            attrIndex
                                                                                        }
                                                                                        className="flex gap-2 items-center"
                                                                                    >
                                                                                        <InputStyled
                                                                                            placeholder="Tên (VD: Màu)"
                                                                                            size="sm"
                                                                                            className="w-1/3"
                                                                                            {...formik.getFieldProps(
                                                                                                `variants.${index}.attributes.${attrIndex}.key`
                                                                                            )}
                                                                                        />
                                                                                        <InputStyled
                                                                                            placeholder="Giá trị (VD: Đen)"
                                                                                            size="sm"
                                                                                            className="w-1/3"
                                                                                            {...formik.getFieldProps(
                                                                                                `variants.${index}.attributes.${attrIndex}.value`
                                                                                            )}
                                                                                        />
                                                                                        <Button
                                                                                            isIconOnly
                                                                                            size="sm"
                                                                                            color="danger"
                                                                                            variant="flat"
                                                                                            onPress={() =>
                                                                                                attrHelpers.remove(
                                                                                                    attrIndex
                                                                                                )
                                                                                            }
                                                                                        >
                                                                                            <X
                                                                                                size={
                                                                                                    14
                                                                                                }
                                                                                            />
                                                                                        </Button>
                                                                                    </div>
                                                                                )
                                                                            )}
                                                                            <Button
                                                                                size="sm"
                                                                                variant="light"
                                                                                color="primary"
                                                                                startContent={
                                                                                    <Plus
                                                                                        size={14}
                                                                                    />
                                                                                }
                                                                                className="w-fit px-2 h-8"
                                                                                onPress={() =>
                                                                                    attrHelpers.push(
                                                                                        {
                                                                                            key: "",
                                                                                            value: ""
                                                                                        }
                                                                                    )
                                                                                }
                                                                            >
                                                                                Thêm thuộc tính
                                                                            </Button>
                                                                        </div>
                                                                    )}
                                                                </FieldArray>
                                                            </div>
                                                        </div>
                                                    )
                                                })}
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
                            <Button color="danger" variant="light" onPress={handleClose}>
                                Hủy bỏ
                            </Button>
                            <ButtonStyled
                                color="primary"
                                onPress={() => formik.handleSubmit()}
                                isLoading={isLoading}
                            >
                                {/* Đổi text nút */}
                                {isEditMode ? "Lưu thay đổi" : "Tạo sản phẩm"}
                            </ButtonStyled>
                        </ModalFooter>
                    </>
                )}
            </ModalContent>
        </Modal>
    )
}
