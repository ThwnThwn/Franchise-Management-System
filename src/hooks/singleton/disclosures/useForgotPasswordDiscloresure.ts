import { DisclosureContext } from "@/providers/DisclosureProvider/DisclosureContext"
import { useDisclosure } from "@heroui/react"
import { useContext } from "react"

export const useForgotPasswordDiscloresureCore = () => {
    return useDisclosure()
}

export const useForgotPasswordDiscloresureSingleton = () => {
    const { useForgotPasswordDiscloresure } = useContext(DisclosureContext)!
    return useForgotPasswordDiscloresure
}
