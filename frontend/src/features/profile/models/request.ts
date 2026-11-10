import { Gender } from "@/constants/enums"

export type UserUpdateReq = {
    firstName?: string
    lastName?: string
    phone?: string
    address?: string
    gender?: Gender
    dateOfBirth?: string
}
