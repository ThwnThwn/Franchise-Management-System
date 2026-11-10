import { Lock, LucideIcon } from "lucide-react"

export interface SecurityMenuItem {
    title: string
    description: string
    icon: LucideIcon
    onClick: () => void
}

interface UseSecurityMenuProps {
    onOpenChangePassword: () => void
}

/**
 * Hook này đóng vai trò là "Data Center"
 * Nó nhận các triggers và trả về mảng chứa cả thông tin + hành động
 */
export const useSecurityMenu = ({
    onOpenChangePassword
}: UseSecurityMenuProps): SecurityMenuItem[] => {
    return [
        {
            title: "Đổi mật khẩu",
            description: "Thay đổi mật khẩu định kỳ để bảo vệ tài khoản tốt hơn.",
            icon: Lock,
            onClick: onOpenChangePassword
        }
    ]
}
