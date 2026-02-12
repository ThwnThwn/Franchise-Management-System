import { HOUR_MINUTE_REGEX } from "@/constants/regexs"
import * as Yup from "yup"
import { ShiftStatus } from "@/features/shifts/constants"

const shiftStatusValues = Object.values(ShiftStatus).filter((v) => typeof v === "number")

export const shiftSchema = Yup.object().shape({
    id: Yup.string(),
    storeId: Yup.string().required(),
    shiftDate: Yup.date().required("Vui lòng chọn ngày"),

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

    requiredStaffCount: Yup.number()
        .min(1, "Tối thiểu 1 nhân viên")
        .required("Vui lòng nhập số lượng"),
    breakDurationMinutes: Yup.number()
        .min(0, "Không được nhỏ hơn 0")
        .integer("Phải là số nguyên")
        .default(0),
    note: Yup.string().nullable(),
    status: Yup.mixed<ShiftStatus>().oneOf(shiftStatusValues).default(ShiftStatus.Draft),
    shiftTemplateId: Yup.string().nullable()
})

// Type cho Formik Values
export type ShiftFormValues = Yup.InferType<typeof shiftSchema>
