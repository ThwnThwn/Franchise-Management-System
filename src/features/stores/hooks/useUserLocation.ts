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
        if (!navigator.geolocation) {
            setLocation({
                latitude: null,
                longitude: null,
                error: "Geolocation is not supported by your browser",
                loading: false
            })
            return
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                setLocation({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    error: null,
                    loading: false
                })
            },
            (error) => {
                setLocation({
                    latitude: null,
                    longitude: null,
                    error: error.message,
                    loading: false
                })
            }
        )
    }, [])

    return location
}
