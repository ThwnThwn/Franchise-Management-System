import * as Yup from "yup"
import { HOUR_MINUTE_REGEX } from "@/constants/regexs"

export const validationSchema = Yup.object({
    code: Yup.string().required("Mã ca không được để trống"),
    name: Yup.string().required("Tên ca không được để trống"),
    startTime: Yup.string()
        .required("Vui lòng chọn giờ")
        .matches(HOUR_MINUTE_REGEX, "Giờ không hợp lệ"),
    endTime: Yup.string()
        .required("Vui lòng chọn giờ")
        .matches(HOUR_MINUTE_REGEX, "Giờ không hợp lệ"),
    breakDurationMinutes: Yup.number()
        .min(0, "Thời gian nghỉ phải >= 0")
        .required("Nhập 0 nếu không nghỉ")
})
