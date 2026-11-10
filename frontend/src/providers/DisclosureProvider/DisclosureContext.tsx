"use client"
import {
    useCartDrawerDiscloresureCore,
    useLoginDiscloresureCore,
    useRegisterDiscloresureCore,
    useForgotPasswordDiscloresureCore
} from "@/hooks"
import { createContext } from "react"

export interface DisclosureContextType {
    useLoginDiscloresure: ReturnType<typeof useLoginDiscloresureCore>
    useRegisterDiscloresure: ReturnType<typeof useRegisterDiscloresureCore>
    useForgotPasswordDiscloresure: ReturnType<typeof useForgotPasswordDiscloresureCore>
    useCartDrawerDiscloresure: ReturnType<typeof useCartDrawerDiscloresureCore>
}

export const DisclosureContext = createContext<DisclosureContextType | null>(null)
