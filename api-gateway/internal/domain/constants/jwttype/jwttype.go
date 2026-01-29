package jwttype

type JwtType string

const (
	AccessToken    JwtType = "access"
	RefreshToken   JwtType = "refresh"
	ForgotPassword JwtType = "forget-password"
	Register       JwtType = "register"
)
