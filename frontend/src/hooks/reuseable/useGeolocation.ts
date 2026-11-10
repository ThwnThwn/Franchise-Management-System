import { useState, useCallback } from "react"

export const useGeolocation = () => {
    const [isLoading, setIsLoading] = useState(false)

    const getPosition = useCallback((): Promise<{
        lat?: number
        lng?: number
        error?: string
        accuracy?: string
    }> => {
        return new Promise((resolve) => {
            if (!navigator.geolocation) {
                resolve({ error: "Trình duyệt không hỗ trợ định vị." })
                return
            }

            setIsLoading(true)

            // Hàm thực hiện định vị với cấu hình tùy chỉnh
            const requestLocation = (highAccuracy: boolean, timeout: number) => {
                navigator.geolocation.getCurrentPosition(
                    (pos) => {
                        setIsLoading(false)
                        resolve({
                            lat: pos.coords.latitude,
                            lng: pos.coords.longitude,
                            accuracy: highAccuracy ? "HIGH" : "NORMAL"
                        })
                    },
                    (err) => {
                        // Nếu là lỗi Timeout và đang ở chế độ High Accuracy -> Thử lại với Normal Accuracy
                        if (err.code === err.TIMEOUT && highAccuracy) {
                            console.warn("GPS Timeout - Switching to Normal Accuracy...")
                            requestLocation(false, 5000) // Thử lại chế độ thường trong 5s
                            return
                        }

                        setIsLoading(false)
                        let errorMsg = "Lỗi GPS không xác định"
                        if (err.code === 1) errorMsg = "Vui lòng cấp quyền truy cập vị trí."
                        if (err.code === 2) errorMsg = "Không thể xác định vị trí."
                        if (err.code === 3) errorMsg = "Hết thời gian chờ định vị (Timeout)."

                        resolve({ error: errorMsg })
                    },
                    {
                        enableHighAccuracy: highAccuracy,
                        timeout: timeout,
                        maximumAge: 30000 // Tận dụng cache 30s để tăng tốc
                    }
                )
            }

            // Bắt đầu với High Accuracy
            requestLocation(true, 10000)
        })
    }, [])

    return { getPosition, isLoading }
}
