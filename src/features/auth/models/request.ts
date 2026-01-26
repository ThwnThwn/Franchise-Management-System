import { Gender } from "@/constants/enums"

export type UserRegisterReq = {
    email: string
    userName: string
    password: string
    confirmPassword: string
    dateOfBirth: string
    firstName: string
    lastName?: string
    gender: Gender
    address?: string
    phone: string
}
