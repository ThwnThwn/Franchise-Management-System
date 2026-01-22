import React, { type PropsWithChildren } from "react"
import { DisclosureContext } from "./DisclosureContext"
import {
    useForgotPasswordDiscloresureCore,
    useLoginDiscloresureCore,
    useRegisterDiscloresureCore
} from "@/hooks"

export const DisclosureProvider = ({ children }: PropsWithChildren) => {
    const useLoginDiscloresure = useLoginDiscloresureCore()
    const useRegisterDiscloresure = useRegisterDiscloresureCore()
    const useForgotPasswordDiscloresure = useForgotPasswordDiscloresureCore()

    return (
        <DisclosureContext.Provider
            value={{
                useLoginDiscloresure,
                useRegisterDiscloresure,
                useForgotPasswordDiscloresure
            }}
        >
            {children}
        </DisclosureContext.Provider>
    )
}
