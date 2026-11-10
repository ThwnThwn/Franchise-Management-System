import { ATTENDANCE_CONFIG } from "@/features/shifts/constants/config"
import { calculateDistance } from "@/utils/helpers/locationHelper"
import { useMemo } from "react"

export const useDistanceToTarget = (
    source?: { lat: number; lng: number } | null,
    target?: { lat: number; lng: number } | null
) => {
    return useMemo(() => {
        if (!source || !target) return { distanceM: null, isWithinRange: false }

        const distKm = calculateDistance(source.lat, source.lng, target.lat, target.lng)
        const distM = Math.round(distKm * 1000)

        return {
            distanceM: distM,
            isWithinRange: distM <= ATTENDANCE_CONFIG.MAX_GPS_DISTANCE_METERS
        }
    }, [source, target])
}
