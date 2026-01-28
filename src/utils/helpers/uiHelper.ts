export const getPixelSizeFromClass = (className?: string) => {
    if (!className) return { width: 0, height: 0 }

    const widthMatch = className.match(/\bw-([0-9.]+)\b/)
    const heightMatch = className.match(/\bh-([0-9.]+)\b/)

    const width = widthMatch ? parseFloat(widthMatch[1]) * 4 : 0
    const height = heightMatch ? parseFloat(heightMatch[1]) * 4 : 0

    return { width, height }
}
