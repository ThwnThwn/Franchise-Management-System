package cache

import "context"

type IBlacklistCache interface {
	GetByToken(ctx context.Context, token string) (string, error)
}
