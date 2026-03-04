package engagement

import (
	"api-gateway/internal/application/constants/errorcode"
	engagementGW "api-gateway/internal/application/gateways/engagement"
	"api-gateway/internal/domain/models/voucher"
	"context"

	"api-gateway/pkg/mapper"
	engagementPb "api-gateway/pkg/pb/engagement"
	"api-gateway/pkg/utils/timeutils"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type voucherGrpcClient struct {
	client engagementPb.VoucherGrpcClient
}

func ProvideVoucherClient(f *clientFactory) engagementGW.IVoucherService {
	return &voucherGrpcClient{client: engagementPb.NewVoucherGrpcClient(f.conn)}
}

// GetAll implements engagement.IVoucherService.
func (v *voucherGrpcClient) GetAll(ctx context.Context) ([]*voucher.VoucherResponse, error) {
	res, err := v.client.GetAllVouchers(ctx, &emptypb.Empty{})
	if err != nil {
		return []*voucher.VoucherResponse{}, err
	}

	items := make([]*voucher.VoucherResponse, 0, len(res.Vouchers))
	for _, v := range res.Vouchers {
		items = append(items, mapper.MapVoucherResponsePbToModel(v))
	}

	return items, nil
}

// GetByCode implements engagement.IVoucherService.
func (v *voucherGrpcClient) GetByCode(ctx context.Context, code string) (*voucher.VoucherResponse, error) {
	res, err := v.client.GetVoucherByCode(ctx, &engagementPb.GetVoucherByCodeRequest{
		Code: code,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapVoucherResponsePbToModel(res), nil
}

// Create implements engagement.IVoucherService.
func (v *voucherGrpcClient) Create(ctx context.Context, req *voucher.CreateVoucherRequest) error {
	items := make([]*engagementPb.CreateVoucherItemReq, 0, len(req.VoucherItems))
	for _, item := range req.VoucherItems {
		if _, ok := engagementPb.VoucherItemType_name[int32(item.Type)]; !ok {
			return errorcode.NewBadRequest("Kiểu voucher item không hợp lệ")
		}

		items = append(items, &engagementPb.CreateVoucherItemReq{
			VariantId: item.VariantId,
			Quantity:  item.Quantity,
			Type:      engagementPb.VoucherItemType(item.Type),
		})
	}

	if _, ok := engagementPb.VoucherType_name[int32(req.Type)]; !ok {
		return errorcode.NewBadRequest("Kiểu voucher không hợp lệ")
	}
	_, err := v.client.CreateVoucher(ctx, &engagementPb.CreateVoucherRequest{
		Code:             req.Code,
		Type:             engagementPb.VoucherType(req.Type),
		DiscountValue:    req.DiscountValue,
		MaxDiscountValue: req.MaxDiscountValue,
		MinOrderValue:    req.MinOrderValue,
		PointCost:        req.PointCost,
		InitQuantity:     req.InitQuantity,
		MaxUsagePerUser:  req.MaxUsagePerUser,
		EndDate:          timeutils.ParseStringPtrToTimestamp(req.EndDate),
		VoucherItems:     items,
	})

	return err
}

func (v *voucherGrpcClient) Update(ctx context.Context, id uuid.UUID, req *voucher.UpdateVoucherRequest) error {
	items := make([]*engagementPb.UpdateVoucherItemReq, 0, len(req.VoucherItems))
	for _, item := range req.VoucherItems {
		var grpcTypePtr *engagementPb.VoucherItemType = nil

		if item.Type != nil {
			val := int32(*item.Type)
			if _, ok := engagementPb.VoucherItemType_name[val]; !ok {
				return errorcode.NewBadRequest("Kiểu voucher item không hợp lệ")
			}

			t := engagementPb.VoucherItemType(val)
			grpcTypePtr = &t
		}

		items = append(items, &engagementPb.UpdateVoucherItemReq{
			Id:        item.Id,
			VariantId: item.VariantId,
			Quantity:  item.Quantity,
			Type:      grpcTypePtr,
		})
	}

	_, err := v.client.UpdateVoucher(ctx, &engagementPb.UpdateVoucherRequest{
		Id:               id.String(),
		DiscountValue:    req.DiscountValue,
		MaxDiscountValue: req.MaxDiscountValue,
		MinOrderValue:    req.MinOrderValue,
		PointCost:        req.PointCost,
		MaxUsagePerUser:  req.MaxUsagePerUser,
		EndDate:          timeutils.ParseStringPtrToTimestamp(req.EndDate),
		VoucherItems:     items,
	})

	return err
}

// UpdateStatus implements engagement.IVoucherService.
func (v *voucherGrpcClient) UpdateStatus(ctx context.Context, id uuid.UUID, req *voucher.UpdateVoucherStatusRequest) error {
	_, err := v.client.UpdateVoucherStatus(ctx, &engagementPb.UpdateVoucherStatusRequest{
		Id:       id.String(),
		IsActive: req.IsActive,
	})
	return err
}

// Delete implements engagement.IVoucherService.
func (v *voucherGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := v.client.DeleteVoucher(ctx, &engagementPb.DeleteVoucherRequest{
		Id: id.String(),
	})
	return err
}
