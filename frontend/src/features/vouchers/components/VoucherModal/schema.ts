import * as Yup from "yup"
import { VoucherType } from "@/features/vouchers/constants"

export const VoucherSchema = Yup.object().shape({
    code: Yup.string().required("Mã code không được để trống"),
    type: Yup.number().required(),
    discountValue: Yup.string().when("type", {
        is: (val: number) => val !== VoucherType.GIFT,
        then: (schema) => schema.required("Vui lòng nhập giá trị giảm"),
        otherwise: (schema) => schema.optional()
    }),
    maxDiscountValue: Yup.string().optional(),
    minOrderValue: Yup.string().optional(),
    pointCost: Yup.string().optional(),
    initQuantity: Yup.number().min(1, "Tối thiểu là 1").required("Nhập số lượng phát hành"),
    maxUsagePerUser: Yup.number().min(1).required(),
    endDate: Yup.string().optional(),
    voucherItems: Yup.array()
        .of(
            Yup.object().shape({
                sku: Yup.string().required("Chọn sản phẩm"),
                quantity: Yup.number().min(1).required(),
                type: Yup.number().required()
            })
        )
        .optional()
})
