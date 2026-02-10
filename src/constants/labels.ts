import { Gender, RoleSlug } from "./enums"

export const RoleSlugLabels: Record<RoleSlug, string> = {
    [RoleSlug.Admin]: "Quản trị viên",
    [RoleSlug.Manager]: "Quản lý cửa hàng",
    [RoleSlug.Staff]: "Nhân viên",
    [RoleSlug.Customer]: "Khách hàng"
}

export const GenderLabels: Record<Gender, string> = {
    [Gender.Male]: "Nam",
    [Gender.Female]: "Nữ"
}
