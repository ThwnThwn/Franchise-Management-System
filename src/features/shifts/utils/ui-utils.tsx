import { AlertCircle, Clock, LogIn } from "lucide-react"
import { AttendanceStatus, AttendanceUIConfig } from "../types"

export const getAttendanceUIConfig = (
    status: AttendanceStatus | undefined
): AttendanceUIConfig | undefined => {
    if (!status) return undefined

    if (status.isLate) {
        return {
            color: "danger",
            hexColor: "bg-danger",
            icon: <AlertCircle size={20} />,
            label: "Vào ca ngay",
            description: "Bạn đã muộn ca làm việc!"
        }
    }

    if (status.isOvertime) {
        return {
            color: "warning",
            hexColor: "bg-warning",
            icon: <Clock size={20} />,
            label: "Tan ca ngay",
            description: "Đã hết giờ làm, vui lòng Checkout."
        }
    }

    return {
        color: status.isOngoing ? "secondary" : "primary",
        hexColor: status.isOngoing ? "bg-secondary" : "bg-primary",
        icon: status.isOngoing ? <Clock size={20} /> : <LogIn size={20} />,
        label: status.isOngoing ? "Tan ca" : "Vào ca",
        description: status.isOngoing ? "Bạn đang trong ca làm" : "Sẵn sàng vào ca"
    }
}
