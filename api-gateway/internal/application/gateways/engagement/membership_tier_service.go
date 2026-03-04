package engagement

import (
	"api-gateway/internal/domain/models/membershiptier"
	"context"

	"github.com/google/uuid"
)

type IMembershipTierService interface {
	GetAll(ctx context.Context) ([]*membershiptier.MembershipTierResponse, error)
	Create(ctx context.Context, req *membershiptier.CreateMembershipTierRequest) error
	Update(ctx context.Context, id uuid.UUID, req *membershiptier.UpdateMembershipTierRequest) error
	Delete(ctx context.Context, id uuid.UUID) error
}
