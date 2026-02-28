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
    userId: string | null
    roleSlug: RoleSlug | null
    storeId: string | null
    isHydrated: boolean
    hydrate: () => void
    setAccessToken: (token: string, rememberMe?: boolean) => void
    removeAccessToken: () => void
    setRememberMe: (remember: boolean) => void
    removeRememberMe: () => void
}

// --- Helper trích xuất UserId từ sub ---
const getUserIdFromToken = (token: string | null): string | null => {
    if (!token) return null
    try {
        const decoded = jwtDecode<JwtPayload>(token)
        return decoded.sub || null
    } catch {
        return null
    }
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

    return {
        accessToken: initialToken,
        userId: getUserIdFromToken(initialToken),
        roleSlug: getRoleFromToken(initialToken),
        storeId: getStoreIdFromToken(initialToken),
        isHydrated: false,

        hydrate: () => {
            const token = getStoredToken()
            set({
                accessToken: token,
                userId: getUserIdFromToken(token),
                roleSlug: getRoleFromToken(token),
                storeId: getStoreIdFromToken(token),
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

            set({
                accessToken: token,
                userId: getUserIdFromToken(token),
                roleSlug: getRoleFromToken(token),
                storeId: getStoreIdFromToken(token)
            })
        },

        removeAccessToken: () => {
            if (typeof window === "undefined") return
            localStorage.removeItem("access_token")
            localStorage.removeItem("remember_me")
            sessionStorage.removeItem("access_token")

            set({
                accessToken: null,
                userId: null,
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
