"use client"
import {
    useLoginDiscloresureCore,
    useRegisterDiscloresureCore,
    useForgotPasswordDiscloresureCore
} from "@/hooks"
import { createContext } from "react"

export interface DisclosureContextType {
    useLoginDiscloresure: ReturnType<typeof useLoginDiscloresureCore>
    useRegisterDiscloresure: ReturnType<typeof useRegisterDiscloresureCore>
    useForgotPasswordDiscloresure: ReturnType<typeof useForgotPasswordDiscloresureCore>
}

export const DisclosureContext = createContext<DisclosureContextType | null>(null)
