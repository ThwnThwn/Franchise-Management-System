export const ERRORS = {
    AUTH: {
        EMAIL_REQUIRED: "Vui lòng nhập email",
        PASSWORD_REQUIRED: "Vui lòng nhập mật khẩu",
        INVALID_EMAIL: "Vui lòng nhập đúng định dạng email",
        PASSWORD_TOO_SHORT: "Vui lòng nhập mật khẩu có độ bảo mật cao hơn"
    } as const
}

export const TEXT = {
    TITLES: {
        EMAIL: "Email",
        PASSWORD: "Mật khẩu",
        LOGIN: "LOGIN"
    } as const,
    STRINGS: {
        WELCOME: "Xin chào",
        LOGIN_CONTINUE: "Tiếp tục đăng nhập",
        REMEMBER_ME: "Ghi nhớ tôi",
        NOT_HAVE_ACCOUNT: "Tạo tài khoản mới",
        OR: "Hoặc"
    } as const
}
