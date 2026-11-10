package cache

import (
	"api-gateway/config"
	"api-gateway/internal/application/cache"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/logger"
	"context"
	"fmt"

	"github.com/redis/go-redis/v9"
)

const BLACKLIST_KEY_PREFIX = "blacklist:token:%s"

type blacklistCache struct {
	rdb *redis.Client
}

func NewBlacklistCache(rCfg *config.Redis, logger logger.Interface) cache.IBlacklistCache {
	return &blacklistCache{rdb: NewRedis(rCfg, rCfg.BlaclistDb, logger)}
}

// GetByToken implements cache.IBlacklistCache.
func (b *blacklistCache) GetByToken(ctx context.Context, token string) (string, error) {
	key := fmt.Sprintf(BLACKLIST_KEY_PREFIX, token)
	token, err := b.rdb.Get(ctx, key).Result()
	if err != nil {
		if err == redis.Nil {
			return "", errorcode.ErrTokenNotFound
		}
		return "", err
	}
	return token, nil
}
