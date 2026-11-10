import * as Yup from "yup"
import { RoleSlug } from "@/constants/enums"

export const userSchema = Yup.object().shape({
    id: Yup.string().optional(),
    username: Yup.string().when("id", {
        is: (val: string) => !val,
        then: (schema) => schema.required("Vui lòng nhập tên đăng nhập"),
        otherwise: (schema) => schema.optional()
    }),
    email: Yup.string()
        .email("Email không hợp lệ")
        .when("id", {
            is: (val: string) => !val,
            then: (schema) => schema.required("Vui lòng nhập email"),
            otherwise: (schema) => schema.optional()
        }),
    phone: Yup.string().when("id", {
        is: (val: string) => !val,
        then: (schema) => schema.required("Vui lòng nhập số điện thoại"),
        otherwise: (schema) => schema.optional()
    }),
    firstName: Yup.string().when("id", {
        is: (val: string) => !val,
        then: (schema) => schema.required("Nhập tên"),
        otherwise: (schema) => schema.optional()
    }),
    lastName: Yup.string().optional(),
    address: Yup.string().optional(),
    gender: Yup.number().default(0),
    dateOfBirth: Yup.string().optional(),

    // Logic Role
    roleSlug: Yup.mixed<RoleSlug>().required("Vui lòng chọn vai trò"),

    // Staff & Manager phải có Store
    homeStoreId: Yup.string().when("roleSlug", {
        is: (val: RoleSlug) => [RoleSlug.Manager, RoleSlug.Staff].includes(val),
        then: (schema) => schema.required("Vui lòng chọn cửa hàng trực thuộc"),
        otherwise: (schema) => schema.optional().nullable()
    }),

    // Chỉ Staff mới bắt buộc có Lương
    currentHourlyWage: Yup.number()
        .typeError("Mức lương phải là một con số")
        .when("roleSlug", {
            is: RoleSlug.Staff,
            then: (schema) =>
                schema
                    .required("Vui lòng nhập mức lương theo giờ")
                    .min(1000, "Mức lương tối thiểu là 1.000 VNĐ"),
            otherwise: (schema) => schema.optional().nullable()
        })
})

export type UserFormValues = Yup.InferType<typeof userSchema>
