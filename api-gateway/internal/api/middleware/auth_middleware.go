package middleware

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/cache"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/jwttype"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/jwtutils"
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
		_, err := contextutils.GetRoleSlug(c)
		if err != nil {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		// isLoaded, err := a.permCache.IsRoleLoaded(c, roleSlug)
		// if err != nil {
		// 	abortWithErr(c, err)
		// 	return
		// }

		// if !isLoaded {
		// 	perms, err := a.rpService.GetPermissionsBySlug(c, roleSlug)
		// 	if err != nil {
		// 		abortWithErr(c, err)
		// 		return
		// 	}

		// 	if !slices.Contains(perms, permSlug) {
		// 		abortWithErr(c, errorcode.ErrForbidden)
		// 		return
		// 	}
		// } else {
		// 	hasPerm, err := a.permCache.HasPermission(c, roleSlug, permSlug)
		// 	if err != nil {
		// 		abortWithErr(c, err)
		// 		return
		// 	}

		// 	if !hasPerm {
		// 		abortWithErr(c, errorcode.ErrForbidden)
		// 		return
		// 	}
		// }

		c.Next()
	}
}

func (a *AuthMiddleware) ValidateToken(secret []byte) gin.HandlerFunc {
	return func(c *gin.Context) {
		// CHECK NHANH: Nếu đã xác thực rồi thì bỏ qua (Skip)
		// Kiểm tra xem key "userID" đã tồn tại trong context chưa
		if _, exists := c.Get(contextutils.CtxUserId); exists {
			c.Next()
			return
		}

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

		userId, err := uuid.Parse(claims.Subject)
		if err != nil {
			abortWithErr(c, errorcode.ErrInvalidAccessToken)
			return
		}

		c.Set(contextutils.CtxUserId, userId)
		c.Set(contextutils.CtxAccessToken, token)
		c.Set(contextutils.CtxRoleSlug, claims.RoleSlug)
		if claims.StoreId != nil {
			c.Set(contextutils.CtxStoreId, *claims.StoreId)
		}

		grpcCtx, err := grpcutils.BuildAuthContext(c)
		if err != nil {
			abortWithErr(c, err)
			return
		}

		// 3. GHI ĐÈ Request Context hiện tại bằng Context mới đã có gRPC Metadata
		c.Request = c.Request.WithContext(grpcCtx)

		c.Next()
	}
}

func abortWithErr(c *gin.Context, err error) {
	c.Error(err)
	c.Abort()
}
