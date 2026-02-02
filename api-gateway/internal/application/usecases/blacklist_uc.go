package usecases

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/cache"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/pkg/logger"
	"context"
	"errors"

	"go.uber.org/zap"
)

type blacklistUseCase struct {
	blCache cache.IBlacklistCache
	logger  logger.Interface
}

// Hàm khởi tạo (Constructor)
func NewBlacklistUseCase(cache cache.IBlacklistCache, logger logger.Interface) abstractions.IBlacklistUseCase {
	return &blacklistUseCase{
		blCache: cache,
		logger:  logger,
	}
}

// IsInBlacklist implements abstractions.IBlacklistUseCase.
func (b *blacklistUseCase) IsInBlacklist(ctx context.Context, token string) (bool, error) {
	_, err := b.blCache.GetByToken(ctx, token)
	if err != nil {
		if errors.Is(err, errorcode.ErrTokenNotFound) {
			return false, nil
		}
		b.logger.Warn("Error in blacklist cache", zap.Error(err))
		return false, nil
	}
	return true, nil
}
