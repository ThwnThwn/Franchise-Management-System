import { normalizeCurrency, formatCurrencyWithSymbol } from "@/utils/helpers/currency"
import * as yup from "yup"

export const getWithdrawalSchema = (maxBalance: string) => {
    return yup.object().shape({
        amount: yup
            .number()
            .transform((value, originalValue) => {
                // Xóa mọi ký tự không phải số để ép kiểu an toàn
                if (typeof originalValue === "string") {
                    return Number(originalValue.replace(/[^0-9]/g, ""))
                }
                return value
            })
            .typeError("Số tiền không hợp lệ")
            .required("Vui lòng nhập số tiền rút")
            .positive("Số tiền rút phải lớn hơn 0")
            .max(
                normalizeCurrency(maxBalance),
                `Số dư ví không đủ. Tối đa: ${formatCurrencyWithSymbol(maxBalance)}`
            ),

        isManual: yup.boolean(),

        bankCode: yup.string().when("isManual", {
            is: false,
            then: (schema) => schema.required("Vui lòng chọn ngân hàng thụ hưởng"),
            otherwise: (schema) => schema.optional()
        }),

        bankName: yup.string().required("Vui lòng nhập tên ngân hàng"),

        accountNumber: yup
            .string()
            .required("Vui lòng nhập số tài khoản")
            .matches(/^[0-9]+$/, "Số tài khoản chỉ được chứa chữ số"),

        accountName: yup
            .string()
            .required("Vui lòng nhập tên chủ tài khoản")
            .min(2, "Tên quá ngắn"),

        notes: yup.string().optional()
    })
}
