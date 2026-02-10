import { create } from "zustand"
import { jwtDecode } from "jwt-decode"
import { RoleSlug } from "@/constants/enums"

interface JwtPayload {
    sub?: string
    role_slug?: string
    store_id?: string
}

type TokenStore = {
    accessToken: string | null
    roleSlug: RoleSlug | null
    storeId: string | null
    isHydrated: boolean
    hydrate: () => void
    setAccessToken: (token: string, rememberMe?: boolean) => void
    removeAccessToken: () => void
    setRememberMe: (remember: boolean) => void
    removeRememberMe: () => void
}

const getRoleFromToken = (token: string | null): RoleSlug | null => {
    if (!token) return null
    try {
        const decoded = jwtDecode<JwtPayload>(token)
        return (decoded.role_slug as RoleSlug) || null
    } catch {
        return null
    }
}

const getStoreIdFromToken = (token: string | null): string | null => {
    if (!token) return null
    try {
        const decoded = jwtDecode<JwtPayload>(token)
        return decoded.store_id || null
    } catch {
        return null
    }
}

const getRemeberMe = () => {
    if (typeof window === "undefined") return false
    return localStorage.getItem("remember_me") === "true"
}

const getStoredToken = () => {
    if (typeof window === "undefined") return null
    return getRemeberMe()
        ? localStorage.getItem("access_token")
        : sessionStorage.getItem("access_token")
}

export const useTokenStore = create<TokenStore>()((set) => {
    const initialToken = getStoredToken()
    const initialRole = getRoleFromToken(initialToken)
    const initialStoreId = getStoreIdFromToken(initialToken)

    return {
        accessToken: initialToken,
        roleSlug: initialRole,
        storeId: initialStoreId,
        isHydrated: false,

        hydrate: () => {
            const token = getStoredToken()
            const role = getRoleFromToken(token)
            const storeId = getStoreIdFromToken(token)
            set({
                accessToken: token,
                roleSlug: role,
                storeId: storeId,
                isHydrated: true
            })
        },

        setAccessToken: (token: string, rememberMe = true) => {
            if (typeof window === "undefined") return

            if (rememberMe) {
                localStorage.setItem("access_token", token)
                localStorage.setItem("remember_me", "true")
                sessionStorage.removeItem("access_token")
            } else {
                sessionStorage.setItem("access_token", token)
                localStorage.removeItem("access_token")
                localStorage.removeItem("remember_me")
            }

            const role = getRoleFromToken(token)
            const storeId = getStoreIdFromToken(token)

            set({
                accessToken: token,
                roleSlug: role,
                storeId: storeId
            })
        },

        removeAccessToken: () => {
            if (typeof window === "undefined") return
            localStorage.removeItem("access_token")
            localStorage.removeItem("remember_me")
            sessionStorage.removeItem("access_token")

            set({
                accessToken: null,
                roleSlug: null,
                storeId: null
            })
        },

        setRememberMe: (rememberMe: boolean) => {
            if (typeof window === "undefined") return
            if (rememberMe) localStorage.setItem("remember_me", "true")
            else localStorage.removeItem("remember_me")
        },

        removeRememberMe: () => {
            if (typeof window === "undefined") return
            localStorage.removeItem("remember_me")
        }
    }
})
