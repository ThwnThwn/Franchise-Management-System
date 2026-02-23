package jwtutils

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/constants/jwttype"

	"github.com/golang-jwt/jwt/v4"
	"github.com/google/uuid"
)

type CustomClaims struct {
	RoleSlug string     `json:"role_slug"`
	Type     string     `json:"type"`
	StoreId  *uuid.UUID `json:"store_id"`
	jwt.RegisteredClaims
}

func ValidateToken(secret []byte, tokenString string, jwtType jwttype.JwtType) (*CustomClaims, error) {
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

	if claims.Type != string(jwtType) {
		return nil, errorcode.ErrInvalidToken
	}

	return claims, nil
}
