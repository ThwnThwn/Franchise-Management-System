import * as Yup from "yup"

export const shiftAvailabilitySchema = Yup.object().shape({
    isAvailable: Yup.boolean(),
    isRecurring: Yup.boolean(),

    // Logic cho specificDate: Nếu isRecurring là FALSE thì BẮT BUỘC có ngày
    specificDate: Yup.string().when("isRecurring", {
        is: false,
        then: (schema) => schema.required("Vui lòng chọn ngày cụ thể"),
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
        .matches(/^([01]\d|2[0-3]):([0-5]\d)$/, "Định dạng giờ không hợp lệ (HH:mm)"),

    endTime: Yup.string()
        .required("Vui lòng chọn giờ kết thúc")
        .matches(/^([01]\d|2[0-3]):([0-5]\d)$/, "Định dạng giờ không hợp lệ (HH:mm)"),

    note: Yup.string()
        .max(255, "Ghi chú không được quá 255 ký tự")
        // Nếu là báo bận (isAvailable = false) thì có thể bắt buộc nhập lý do
        .when("isAvailable", {
            is: false,
            then: (schema) => schema.required("Vui lòng nhập lý do bận"),
            otherwise: (schema) => schema.notRequired()
        })
})
