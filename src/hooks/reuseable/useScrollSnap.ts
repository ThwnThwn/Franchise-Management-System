import { useEffect, useRef } from "react"

interface ScrollSnapOptions {
    offset?: number // Khoảng cách dừng cách đỉnh màn hình (VD: 24px)
    thresholdTop?: number // Vùng bẫy trên (VD: -10px)
    thresholdBottom?: number // Vùng bẫy dưới (VD: 150px)
    isActive?: boolean // Chỉ bật khi cần thiết (VD: khi ở view calendar)
}

export const useScrollSnap = (options: ScrollSnapOptions = {}) => {
    const { offset = 100, thresholdTop = -30, thresholdBottom = 150, isActive = true } = options

    const targetRef = useRef<HTMLDivElement>(null)
    const isSnapping = useRef(false) // BIẾN KHÓA: Tránh giằng co khi đang kéo

    useEffect(() => {
        if (!isActive) return

        // let scrollTimeout: ReturnType<typeof setTimeout>

        const handleScrollEnd = () => {
            if (isSnapping.current || !targetRef.current) return

            const rect = targetRef.current.getBoundingClientRect()

            if (rect.top >= thresholdTop && rect.top <= thresholdBottom) {
                isSnapping.current = true

                window.scrollTo({
                    top: window.scrollY + rect.top - offset,
                    behavior: "smooth"
                })

                // Tự động mở khóa khi đã trượt xong
                setTimeout(() => {
                    isSnapping.current = false
                }, 500)
            }
        }

        window.addEventListener("scrollend", handleScrollEnd)
        return () => window.removeEventListener("scrollend", handleScrollEnd)
    }, [offset, thresholdTop, thresholdBottom, isActive])

    return targetRef
}
