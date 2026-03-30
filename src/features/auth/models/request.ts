import { Gender } from "@/constants/enums"

export type UserRegisterReq = {
    email: string
    username: string
    firstName: string
    lastName?: string
    password: string
    confirmPassword: string
    phone: string
    address?: string
    gender: Gender
    dateOfBirth: string
}

export interface ChangePasswordReq {
    oldPassword: string
    newPassword: string
    confirmNewPassword: string
    forgetAll: boolean
}
