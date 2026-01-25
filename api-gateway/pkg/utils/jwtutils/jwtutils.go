package jwtutils

import (
	"api-gateway/internal/application/constants/errorcode"

	"github.com/golang-jwt/jwt/v4"
)

func ValidateToken(secret []byte, tokenString string) (*jwt.RegisteredClaims, error) {
	claims := &jwt.RegisteredClaims{}
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
