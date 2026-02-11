import { DayOfWeek, Gender, RoleSlug } from "./enums"

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

export const DayOfWeekLabel: Record<DayOfWeek, string> = {
    [DayOfWeek.SUNDAY]: "Chủ Nhật",
    [DayOfWeek.MONDAY]: "Thứ 2",
    [DayOfWeek.TUESDAY]: "Thứ 3",
    [DayOfWeek.WEDNESDAY]: "Thứ 4",
    [DayOfWeek.THURSDAY]: "Thứ 5",
    [DayOfWeek.FRIDAY]: "Thứ 6",
    [DayOfWeek.SATURDAY]: "Thứ 7"
}
