import { HOUR_MINUTE_REGEX } from "@/constants/regexs"
import dayjs from "dayjs"
import * as Yup from "yup"

export const shiftAvailabilitySchema = Yup.object().shape({
    isAvailable: Yup.boolean(),
    isRecurring: Yup.boolean(),

    // Logic cho specificDate: Nếu isRecurring là FALSE thì BẮT BUỘC có ngày
    specificDate: Yup.string().when("isRecurring", {
        is: false,
        then: (schema) =>
            schema
                .required("Vui lòng chọn ngày cụ thể")
                .test("is-future", "Phải đăng ký trước ít nhất 1 ngày", (value) => {
                    if (!value) return false

                    // Ngày hiện tại (gạt bỏ giờ phút giây để so sánh ngày thuần túy)
                    const today = dayjs().startOf("day")
                    const selectedDate = dayjs(value).startOf("day")

                    // Trả về true nếu ngày chọn cách ngày hiện tại ít nhất 2 ngày (tức là từ "ngày kia" trở đi)
                    // Ví dụ: Hôm nay là 12/02, chỉ được báo bận từ 14/02
                    return selectedDate.diff(today, "day") >= 2
                }),
        otherwise: (schema) => schema.nullable().notRequired()
    }),

    // Logic cho dayOfWeek: Nếu isRecurring là TRUE thì BẮT BUỘC chọn thứ
    dayOfWeek: Yup.number().when("isRecurring", {
        is: true,
        then: (schema) => schema.required("Vui lòng chọn thứ trong tuần"),
        otherwise: (schema) => schema.nullable().notRequired()
    }),

    startTime: Yup.string()
        .required("Vui lòng chọn giờ bắt đầu")
        .matches(HOUR_MINUTE_REGEX, "Định dạng giờ không hợp lệ (HH:mm)"),

    endTime: Yup.string()
        .required("Vui lòng chọn giờ kết thúc")
        .matches(HOUR_MINUTE_REGEX, "Định dạng giờ không hợp lệ (HH:mm)")
        .test("is-not-same", "Giờ kết thúc không được trùng giờ bắt đầu", function (value) {
            const { startTime } = this.parent
            return value !== startTime
        }),

    note: Yup.string()
        .max(255, "Ghi chú không được quá 255 ký tự")
        // Nếu là báo bận (isAvailable = false) thì có thể bắt buộc nhập lý do
        .when("isAvailable", {
            is: false,
            then: (schema) => schema.required("Vui lòng nhập lý do bận"),
            otherwise: (schema) => schema.notRequired()
        })
})
