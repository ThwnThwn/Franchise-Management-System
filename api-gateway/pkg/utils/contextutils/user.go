package contextutils

import (
	"api-gateway/internal/application/constants/errorcode"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

// GetUserId lấy userID từ context
func GetUserId(c *gin.Context) (uuid.UUID, error) {
	val, exists := c.Get("userID")
	if !exists {
		return uuid.Nil, errorcode.ErrInvalidToken
	}

	uid, ok := val.(uuid.UUID)
	if !ok {
		return uuid.Nil, errorcode.ErrInvalidToken
	}

	return uid, nil
}

// GetRoleSlug lấy vai trò của người dùng
func GetRoleSlug(c *gin.Context) (string, error) {
	val, exists := c.Get("roleSlug")
	if !exists {
		return "", errorcode.ErrInvalidToken
	}

	role, ok := val.(string)
	if !ok {
		return "", errorcode.ErrInvalidToken
	}

	return role, nil
}

// GetStoreId lấy storeID từ context (Dành cho Manager/Staff bị giới hạn theo Store)
func GetStoreId(c *gin.Context) (uuid.UUID, error) {
	val, exists := c.Get("storeId")
	if !exists {
		return uuid.Nil, nil
	}

	sid, ok := val.(uuid.UUID)
	if !ok {
		return uuid.Nil, errorcode.ErrInvalidToken
	}

	return sid, nil
}
