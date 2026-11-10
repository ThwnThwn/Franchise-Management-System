import { STRINGS } from "@/constants/strings"

// Khai báo Type để dùng lại khắp nơi
export type MenuKey = "home" | "stores" | "menu" | "about" | "contact" | "vouchers" | undefined
export type MenuItem = { key: Exclude<MenuKey, undefined>; label: string; href: string }

export const MAIN_MENUS: MenuItem[] = [
    { key: "home", label: STRINGS.NAVBAR.HOME, href: "/" },
    { key: "menu", label: "Menu", href: "/menu" },
    { key: "stores", label: "Cửa hàng", href: "/stores" },
    { key: "vouchers", label: "Đổi quà", href: "/vouchers" }
]

export const getMenuKeyFromPath = (pathname: string): MenuKey => {
    const segment = pathname.split("/").filter(Boolean)[0]

    // Nếu segment match với 1 trong các key của MAIN_MENUS thì return
    const isValidKey = MAIN_MENUS.some((m) => m.key === segment)
    if (isValidKey) return segment as MenuKey

    // Xử lý case mặc định (trang chủ)
    if (!segment) return "home"

    return undefined
}
