import * as yup from "yup"

export interface CreateSwapFormValues {
    targetUserId: string
    reason: string
}

export const createSwapSchema = yup.object().shape({
    targetUserId: yup.string().required("Vui lòng chọn một đồng nghiệp để đổi ca."),
    reason: yup
        .string()
        .trim()
        .max(255, "Lý do không được vượt quá 255 ký tự.")
        .required("Vui lòng nhập lý do đổi ca.")
})
