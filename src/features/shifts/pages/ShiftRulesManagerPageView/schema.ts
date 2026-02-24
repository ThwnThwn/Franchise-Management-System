import * as Yup from "yup"

export const shiftRuleSchema = Yup.object().shape({
    allowShuttle: Yup.boolean().required("Bắt buộc"),
    allowShiftSwap: Yup.boolean().required("Bắt buộc"),
    minHoursBetweenShifts: Yup.number()
        .min(0, "Không được nhỏ hơn 0")
        .max(24, "Tối đa 24 giờ")
        .required("Bắt buộc"),
    maxHoursPerWeek: Yup.number()
        .min(1, "Tối thiểu 1 giờ")
        .max(168, "Vượt quá số giờ trong tuần")
        .required("Bắt buộc")
})

export type ShiftRuleFormValues = Yup.InferType<typeof shiftRuleSchema> & {
    storeId?: string
}
