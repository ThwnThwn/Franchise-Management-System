package abstractions

import "context"

type IBlacklistUseCase interface {
	IsInBlacklist(ctx context.Context, token string) (bool, error)
}
