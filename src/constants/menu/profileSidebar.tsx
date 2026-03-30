import { Lock, UserIcon } from "lucide-react"
import { MenuItem } from "./types"

export const PROFILE_MENU_ITEMS: MenuItem[] = [
    {
        label: "Hồ sơ",
        href: "/profile",
        icon: <UserIcon className="w-5 h-5" />,
        allowedRoles: []
    },
    {
        label: "Bảo mật",
        href: "/profile/security",
        icon: <Lock className="w-5 h-5" />,
        allowedRoles: []
    }
]
