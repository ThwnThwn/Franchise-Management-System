import { DisclosureContext } from "@/providers/DisclosureProvider/DisclosureContext"
import { useDisclosure } from "@heroui/react"
import { useContext } from "react"

export const useRegisterDiscloresureCore = () => {
    return useDisclosure()
}

export const useRegisterDiscloresureSingleton = () => {
    const { useRegisterDiscloresure } = useContext(DisclosureContext)!
    return useRegisterDiscloresure
}
