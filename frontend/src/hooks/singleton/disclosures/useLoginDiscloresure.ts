import { DisclosureContext } from "@/providers/DisclosureProvider/DisclosureContext"
import { useDisclosure } from "@heroui/react"
import { useContext } from "react"

export const useLoginDiscloresureCore = () => {
    return useDisclosure()
}

export const useLoginDiscloresureSingleton = () => {
    const { useLoginDiscloresure } = useContext(DisclosureContext)!
    return useLoginDiscloresure
}
