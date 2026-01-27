package jwtutils

import (
	"api-gateway/internal/application/constants/errorcode"

	"github.com/golang-jwt/jwt/v4"
)

type CustomClaims struct {
	// RoleSlug string `json:"role_slug"`
	jwt.RegisteredClaims
}

func ValidateToken(secret []byte, tokenString string) (*CustomClaims, error) {
	claims := &CustomClaims{}
	token, err := jwt.ParseWithClaims(tokenString, claims, func(t *jwt.Token) (any, error) {
		if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, errorcode.ErrUnexpectedSigningToken
		}
		return secret, nil
	})
	if err != nil {
		return nil, err
	}

	if !token.Valid {
		return nil, errorcode.ErrInvalidToken
	}

	return claims, nil
}
