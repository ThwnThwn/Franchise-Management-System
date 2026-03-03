package contextutils

const (
	// 1. Context Keys (Dùng cho c.Get/c.Set nội bộ server)
	CtxAccessToken = "accessToken"
	CtxUserId      = "userId"
	CtxRoleSlug    = "roleSlug"
	CtxStoreId     = "storeId"

	// 2. Cookie Names (Dùng cho c.Cookie / c.SetCookie)
	CookieRefreshToken        = "refresh_token"
	CookieForgotPasswordToken = "forgot_password_token"

	// 3. gRPC Metadata Keys (Dùng cho gRPC headers)
	HeaderEmail    = "x-email"
	HeaderUserId   = "x-user-id"
	HeaderRoleslug = "x-role-slug"
	HeaderStoreId  = "x-store-id"
)
