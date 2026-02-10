export const NAME_REGEX = /^[\p{L}\s]+$/u
export const PASSWORD_REGEX = /^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{8,}$/
export const PHONE_REGEX = /^(0[0-9]{9})$/
export const EMAIL_REGEX = /^[\w.-]+@[\w.-]+\.[a-zA-Z]{2,}$/
export const NUMBER_REGEX = /^[0-9]+$/
export const HOUR_MINUTE_REGEX = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/
