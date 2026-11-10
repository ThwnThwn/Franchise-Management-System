import { useGeolocation } from "@/hooks"
import { useCheckIn, useCheckOut } from "@/features/shifts/hooks"
import { calculateDistance } from "@/utils/helpers/locationHelper"
import { ATTENDANCE_CONFIG } from "@/features/shifts/constants/config"

export const useAttendanceActions = () => {
    const { getPosition, isLoading: isLocating } = useGeolocation()
    const checkInMutation = useCheckIn()
    const checkOutMutation = useCheckOut()

    const handleAction = async ({
        type,
        id,
        target,
        userNote
    }: {
        type: "IN" | "OUT"
        id: string
        target?: { lat: number; lng: number } // Tọa độ Store
        userNote?: string
    }) => {
        // 1. Lấy vị trí thực tế
        const location = await getPosition()

        // 2. Xử lý "Note hệ thống" (System Note)
        const systemNotes: string[] = []

        if (location.error) {
            // Trường hợp máy lỗi, không lấy được GPS
            systemNotes.push(`[Hệ thống: ${location.error}]`)
        } else if (location.lat && location.lng && target) {
            // Trường hợp có GPS nhưng cần check xem có "vượt rào" không
            const distKm = calculateDistance(location.lat, location.lng, target.lat, target.lng)
            const distM = Math.round(distKm * 1000)

            if (distM > ATTENDANCE_CONFIG.MAX_GPS_DISTANCE_METERS) {
                systemNotes.push(`[Hệ thống: Ngoài bán kính - Cách ${distM}m]`)
            }
        }

        // 3. Hợp nhất Note: Note của Staff trước, Note hệ thống sau
        const finalNote = [userNote, ...systemNotes]
            .filter(Boolean) // Loại bỏ undefined/null/empty string
            .join(" ")
            .trim()

        // 4. Chuẩn bị payload
        const payload = {
            latitude: location.lat,
            longitude: location.lng,
            note: finalNote || undefined
        }

        // 5. Thực thi
        if (type === "IN") {
            return checkInMutation.mutateAsync({ assignmentId: id, payload })
        } else {
            return checkOutMutation.mutateAsync({ logId: id, payload })
        }
    }

    return {
        handleAction,
        isLoading: isLocating || checkInMutation.isPending || checkOutMutation.isPending
    }
}
