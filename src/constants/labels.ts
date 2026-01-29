import { Gender, RoleSlug } from "./enums"

export const RoleSlugLabels: Record<RoleSlug, string> = {
    [RoleSlug.Admin]: "Quản trị viên",
    [RoleSlug.Manager]: "Quản lí",
    [RoleSlug.Staff]: "Nhân viên",
    [RoleSlug.Customer]: "Khách hàng"
}

export const GenderLabels: Record<Gender, string> = {
    [Gender.Male]: "Nam",
    [Gender.Female]: "Nữ"
}
