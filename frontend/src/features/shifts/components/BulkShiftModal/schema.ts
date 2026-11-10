import * as Yup from "yup"
import { HOUR_MINUTE_REGEX } from "@/constants/regexs"

export const bulkShiftSchema = Yup.object().shape({
    type: Yup.string().oneOf(["manual", "template"]).required(),

    // 1. Validate Khoảng ngày (Dùng chung cho cả 2 loại)
    startDate: Yup.string().required("Vui lòng chọn ngày bắt đầu"),
    endDate: Yup.string()
        .required("Vui lòng chọn ngày kết thúc")
        .test("is-after-start", "Ngày kết thúc không được trước ngày bắt đầu", function (value) {
            const { startDate } = this.parent
            if (!startDate || !value) return true

            return new Date(value) >= new Date(startDate)
        }),

    // 2. Validate cho loại Thủ công (Manual)
    startTime: Yup.string().when("type", {
        is: "manual",
        then: (schema) =>
            schema
                .required("Vui lòng chọn giờ bắt đầu")
                .matches(HOUR_MINUTE_REGEX, "Định dạng giờ không hợp lệ (HH:mm)")
    }),
    endTime: Yup.string().when("type", {
        is: "manual",
        then: (schema) =>
            schema
                .required("Vui lòng chọn giờ kết thúc")
                .matches(HOUR_MINUTE_REGEX, "Định dạng giờ không hợp lệ (HH:mm)")
                .test("is-not-same", "Giờ kết thúc không được trùng giờ bắt đầu", function (value) {
                    const { startTime } = this.parent
                    return value !== startTime
                })
    }),
    daysOfWeek: Yup.array()
        .of(Yup.number())
        .when("type", {
            is: "manual",
            then: (schema) => schema.min(1, "Chọn ít nhất 1 thứ trong tuần")
        }),

    // 3. Validate cho loại Theo mẫu (Template)
    templateId: Yup.string().when("type", {
        is: "template",
        then: (schema) => schema.required("Vui lòng chọn một mẫu ca làm việc")
    }),

    // 4. Các thông số định mức (Bắt buộc cho cả 2)
    requiredStaffCount: Yup.number()
        .min(1, "Tối thiểu 1 nhân viên")
        .required("Vui lòng nhập số lượng"),
    breakDurationMinutes: Yup.number()
        .min(0, "Không được nhỏ hơn 0")
        .integer("Phải là số nguyên")
        .default(0),
    note: Yup.string().nullable()
})

// Type phục vụ Formik trong Component
export type BulkShiftFormValues = Yup.InferType<typeof bulkShiftSchema>
