import { useState } from "react"
import { useDisclosure } from "@heroui/react"

export const useImageUploadModal = ({
    onBeforeOpenModal
}: { onBeforeOpenModal?: () => void } = {}) => {
    const [imgSrc, setImgSrc] = useState<string | null>(null)
    const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure()

    const onFileSelect = (file: File) => {
        const reader = new FileReader()
        reader.addEventListener("load", () => {
            setImgSrc(reader.result as string)
            onBeforeOpenModal?.()
            onOpen()
        })
        reader.readAsDataURL(file)
    }

    return { imgSrc, setImgSrc, isOpen, onOpen, onOpenChange, onClose, onFileSelect }
}
