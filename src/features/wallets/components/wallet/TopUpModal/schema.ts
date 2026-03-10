import * as yup from "yup"

export const topUpSchema = yup.object().shape({
    amount: yup
        .number()
        .transform((value, originalValue) => {
            if (typeof originalValue === "string") {
                return Number(originalValue.replace(/[^0-9]/g, ""))
            }
            return value
        })
        .typeError("Số tiền không hợp lệ")
        .required("Vui lòng nhập số tiền nạp")
        .min(10000, "Số tiền nạp tối thiểu là 10.000 đ")
        .max(50000000, "Số tiền nạp một lần tối đa là 50.000.000 đ")
})
