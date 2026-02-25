import { useEffect, useState } from "react"

type LocationState = {
    latitude: number | null
    longitude: number | null
    error: string | null
    loading: boolean
}

export const useUserLocation = () => {
    const [location, setLocation] = useState<LocationState>({
        latitude: null,
        longitude: null,
        error: null,
        loading: true
    })

    useEffect(() => {
        let isMounted = true

        // Tạo một hàm async để bọc logic lại
        const initLocation = async () => {
            if (!navigator.geolocation) {
                // Đẩy vào microtask để né lỗi "synchronously calling setState"
                await Promise.resolve()

                if (isMounted) {
                    setLocation((prev) => ({
                        ...prev,
                        error: "Lỗi khi lấy GPS",
                        loading: false
                    }))
                }
                return
            }

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    if (isMounted) {
                        setLocation({
                            latitude: position.coords.latitude,
                            longitude: position.coords.longitude,
                            error: null,
                            loading: false
                        })
                    }
                },
                (error) => {
                    if (isMounted) {
                        setLocation({
                            latitude: null,
                            longitude: null,
                            error: error.message,
                            loading: false
                        })
                    }
                }
            )
        }

        initLocation()

        return () => {
            isMounted = false
        }
    }, [])

    return location
}
