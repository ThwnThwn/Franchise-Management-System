import { animate } from "framer-motion"
import { useCallback } from "react"

export function useScrollToCenter() {
    const scrollToCenter = useCallback(
        (container: HTMLElement | null, target: HTMLElement | null) => {
            if (!container || !target) return

            const rect = target.getBoundingClientRect()
            const parentRect = container.getBoundingClientRect()

            // ... (Giữ nguyên logic tính toán của bạn) ...
            const itemCenter = rect.left + rect.width / 2
            const containerCenter = parentRect.left + parentRect.width / 2
            const scrollOffset = itemCenter - containerCenter
            const targetScroll = container.scrollLeft + scrollOffset
            const maxScroll = container.scrollWidth - container.clientWidth
            const newX = Math.max(0, Math.min(targetScroll, maxScroll))

            animate(container.scrollLeft, newX, {
                duration: 0.5,
                ease: [0.25, 0.1, 0.25, 1.0],
                onUpdate: (v) => {
                    container.scrollLeft = v
                }
            })
        },
        []
    )

    return scrollToCenter
}
