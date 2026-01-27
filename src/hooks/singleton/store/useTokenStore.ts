import { create } from "zustand"
import { jwtDecode } from "jwt-decode"

interface JwtPayload {
    sub?: string
    role_slug?: string
}

type TokenStore = {
    accessToken: string | null
    roleSlug: string | null
    isHydrated: boolean
    hydrate: () => void
    setAccessToken: (token: string, rememberMe?: boolean) => void
    removeAccessToken: () => void
    setRememberMe: (remember: boolean) => void
    removeRememberMe: () => void
}

const getRoleFromToken = (token: string | null): string | null => {
    if (!token) return null
    try {
        const decoded = jwtDecode<JwtPayload>(token)
        return decoded.role_slug || null
    } catch (error) {
        console.error("Invalid token:", error)
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

    return {
        accessToken: initialToken,
        roleSlug: initialRole,
        isHydrated: false,

        hydrate: () => {
            const token = getStoredToken()
            const role = getRoleFromToken(token)
            set({ accessToken: token, roleSlug: role, isHydrated: true })
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

            set({ accessToken: token, roleSlug: role })
        },

        removeAccessToken: () => {
            if (typeof window === "undefined") return
            localStorage.removeItem("access_token")
            localStorage.removeItem("remember_me")
            sessionStorage.removeItem("access_token")

            set({ accessToken: null, roleSlug: null })
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
