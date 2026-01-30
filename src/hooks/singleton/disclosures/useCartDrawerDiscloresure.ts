import { DisclosureContext } from "@/providers/DisclosureProvider/DisclosureContext"
import { useDisclosure } from "@heroui/react"
import { useContext } from "react"

export const useCartDrawerDiscloresureCore = () => {
    return useDisclosure()
}

export const useCartDrawerDiscloresureSingleton = () => {
    const { useCartDrawerDiscloresure } = useContext(DisclosureContext)!
    return useCartDrawerDiscloresure
}
