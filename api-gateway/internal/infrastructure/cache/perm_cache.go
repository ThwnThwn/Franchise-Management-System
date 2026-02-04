package cache

import (
	"api-gateway/config"
	"api-gateway/internal/application/cache"
	"api-gateway/pkg/logger"
	"context"
	"fmt"

	"github.com/redis/go-redis/v9"
)

type permCache struct {
	rdb *redis.Client
}

func NewPermCache(rCfg *config.Redis, logger logger.Interface) cache.IPermCache {
	return &permCache{rdb: NewRedis(rCfg, rCfg.PermDb, logger)}
}

// HasPermission implements cache.IPermCache.
func (p *permCache) HasPermission(ctx context.Context, roleSlug string, permSlug string) (bool, error) {
	key := p.getKey(roleSlug)

	// SIsMember: Set Is Member -> Trả về bool luôn
	isExist, err := p.rdb.SIsMember(ctx, key, permSlug).Result()
	if err != nil {
		return false, err
	}

	return isExist, nil
}

// IsRoleLoaded implements cache.IPermCache.
func (p *permCache) IsRoleLoaded(ctx context.Context, roleSlug string) (bool, error) {
	key := p.getKey(roleSlug)

	// Exists trả về số lượng key tìm thấy (0 hoặc 1)
	count, err := p.rdb.Exists(ctx, key).Result()
	if err != nil {
		return false, err
	}

	return count > 0, nil
}

func (p *permCache) getKey(roleSlug string) string {
	return fmt.Sprintf("auth:role:%s", roleSlug)
}
