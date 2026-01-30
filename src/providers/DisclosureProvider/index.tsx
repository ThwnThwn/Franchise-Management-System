import React, { type PropsWithChildren } from "react"
import { DisclosureContext } from "./DisclosureContext"
import {
    useCartDrawerDiscloresureCore,
    useForgotPasswordDiscloresureCore,
    useLoginDiscloresureCore,
    useRegisterDiscloresureCore
} from "@/hooks"

export const DisclosureProvider = ({ children }: PropsWithChildren) => {
    const useCartDrawerDiscloresure = useCartDrawerDiscloresureCore()
    const useLoginDiscloresure = useLoginDiscloresureCore()
    const useRegisterDiscloresure = useRegisterDiscloresureCore()
    const useForgotPasswordDiscloresure = useForgotPasswordDiscloresureCore()

    return (
        <DisclosureContext.Provider
            value={{
                useCartDrawerDiscloresure,
                useLoginDiscloresure,
                useRegisterDiscloresure,
                useForgotPasswordDiscloresure
            }}
        >
            {children}
        </DisclosureContext.Provider>
    )
}
