package middleware

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/cache"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/jwttype"
	"api-gateway/pkg/utils/jwtutils"
	"slices"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type AuthMiddleware struct {
	permCache   cache.IPermCache
	rpService   identity.IRolePermissionService
	blackListUC abstractions.IBlacklistUseCase
}

func NewAuthMiddleware(
	permCache cache.IPermCache,
	rpService identity.IRolePermissionService,
	blackListUC abstractions.IBlacklistUseCase) *AuthMiddleware {
	return &AuthMiddleware{
		permCache:   permCache,
		rpService:   rpService,
		blackListUC: blackListUC,
	}
}

func (a *AuthMiddleware) PermissionCheck(permSlug string) gin.HandlerFunc {
	return func(c *gin.Context) {
		roleSlug := c.GetString("roleSlug")
		if roleSlug == "" {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		isLoaded, err := a.permCache.IsRoleLoaded(c, roleSlug)
		if err != nil {
			abortWithErr(c, err)
			return
		}

		if !isLoaded {
			perms, err := a.rpService.GetPermissionsBySlug(c, roleSlug)
			if err != nil {
				abortWithErr(c, err)
				return
			}

			if !slices.Contains(perms, permSlug) {
				abortWithErr(c, errorcode.ErrForbidden)
				return
			}
		} else {
			hasPerm, err := a.permCache.HasPermission(c, roleSlug, permSlug)
			if err != nil {
				abortWithErr(c, err)
				return
			}

			if !hasPerm {
				abortWithErr(c, errorcode.ErrForbidden)
				return
			}
		}

		c.Next()
	}
}

func (a *AuthMiddleware) ValidateToken(secret []byte) gin.HandlerFunc {
	return func(c *gin.Context) {
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		if !strings.HasPrefix(authHeader, "Bearer ") {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		token := strings.TrimPrefix(authHeader, "Bearer ")
		claims, err := jwtutils.ValidateToken(secret, token, jwttype.AccessToken)
		if err != nil {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		ctx := c.Request.Context()
		isRevoked, _ := a.blackListUC.IsInBlacklist(ctx, token)
		if isRevoked {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		userID, err := uuid.Parse(claims.Subject)
		if err != nil {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		c.Set("userID", userID)
		c.Set("accessToken", token)
		c.Set("roleSlug", claims.RoleSlug)
		c.Next()
	}
}

func abortWithErr(c *gin.Context, err error) {
	c.Error(err)
	c.Abort()
}
