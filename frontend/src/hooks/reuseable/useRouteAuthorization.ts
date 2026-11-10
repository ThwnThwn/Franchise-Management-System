import { useEffect, useMemo } from "react"
import { useRouter } from "next/navigation"
import { RoleSlug } from "@/constants/enums"
import { MenuItem } from "@/constants/menu/types"

interface UseRouteAuthOptions {
    menuItems: MenuItem[]
    pathname: string
    fallbackUrl?: string
    rootPath?: string
    roleSlug?: RoleSlug
    isLoadingRole?: boolean
}

export const useRouteAuthorization = ({
    menuItems,
    pathname,
    fallbackUrl = "/",
    rootPath = "/dashboard",
    roleSlug,
    isLoadingRole = false
}: UseRouteAuthOptions) => {
    const router = useRouter()

    // 1. Lấy danh sách tất cả các path được phép (Quét đệ quy N cấp)
    const allowedPaths = useMemo(() => {
        const paths: string[] = []
        const extractPaths = (items: MenuItem[]) => {
            items.forEach((item) => {
                const isAllowed =
                    !item.allowedRoles ||
                    item.allowedRoles.length === 0 ||
                    (roleSlug && item.allowedRoles.includes(roleSlug))

                if (isAllowed) {
                    if (item.href) paths.push(item.href)
                    if (item.subItems) extractPaths(item.subItems)
                }
            })
        }
        extractPaths(menuItems)
        return paths
    }, [menuItems, roleSlug])

    // 2. Kiểm tra xem Path hiện tại có hợp lệ không
    const isAuthorized = useMemo(() => {
        if (isLoadingRole) return false
        if (pathname === rootPath) return true // Tạm tha cho Root để tính toán Redirect
        return allowedPaths.some((path) => pathname === path || pathname.startsWith(`${path}/`))
    }, [isLoadingRole, pathname, allowedPaths, rootPath])

    // 3. TÍNH TOÁN REDIRECT TARGET NGAY TRONG RENDER PHASE (Derived State)
    const redirectTarget = useMemo(() => {
        if (isLoadingRole) return null // Đang load thì khoan hẵng đá đi đâu

        // Case A: Đang đứng ở trang Gốc nhưng không có quyền
        if (pathname === rootPath && !allowedPaths.includes(rootPath)) {
            return allowedPaths[0] || fallbackUrl
        }

        // Case B: Đi lạc vào trang con không có quyền
        if (!isAuthorized) {
            return fallbackUrl
        }

        // Case C: Hợp lệ, không cần đi đâu cả
        return null
    }, [isLoadingRole, pathname, rootPath, allowedPaths, isAuthorized, fallbackUrl])

    // 4. useEffect CHỈ làm đúng 1 việc: Gọi Side Effect chuyển trang
    useEffect(() => {
        if (redirectTarget) {
            router.replace(redirectTarget)
        }
    }, [redirectTarget, router])

    // 5. Kết luận sinh mệnh của UI
    // An toàn khi: Load xong role VÀ không bị dính án chuyển trang
    const isSafeToRender = !isLoadingRole && redirectTarget === null

    return {
        isSafeToRender,
        allowedPaths
    }
}
