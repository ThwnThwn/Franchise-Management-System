"use client"

import * as Yup from "yup"
import { TimeLogStatus } from "@/features/shifts/constants"

export const updateTimeLogSchema = Yup.object().shape({
    status: Yup.number()
        .oneOf([TimeLogStatus.Processing, TimeLogStatus.Valid, TimeLogStatus.Invalid])
        .required("Vui lòng chọn trạng thái"),

    managerNote: Yup.string()
        .nullable()
        .when("status", {
            is: TimeLogStatus.Invalid,
            then: (schema) =>
                schema.required("Vui lòng nhập lý do khi từ chối công của nhân viên!"),
            otherwise: (schema) => schema.nullable()
        })
})

export type UpdateTimeLogFormValues = Yup.InferType<typeof updateTimeLogSchema>
