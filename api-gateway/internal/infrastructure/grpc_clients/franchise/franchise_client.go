package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	franchiseModel "api-gateway/internal/domain/models/franchise"
	"api-gateway/pkg/mapper"
	franchisePb "api-gateway/pkg/pb/franchise"
	"context"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
	"google.golang.org/protobuf/types/known/timestamppb"
)

type franchiseGrpcClient struct {
	client franchisePb.FranchiseGrpcServiceClient
}

// ProvideFranchiseClient là constructor nhúng vào file wire/factory của ní
func ProvideFranchiseClient(f *clientFactory) franchiseGW.IFranchiseService {
	return &franchiseGrpcClient{client: franchisePb.NewFranchiseGrpcServiceClient(f.conn)}
}

// SubmitApplication implements IFranchiseService.
func (c *franchiseGrpcClient) SubmitApplication(ctx context.Context, managerId uuid.UUID, req *franchiseModel.SubmitApplicationRequest, storeFront *franchiseModel.FileItem, landCerts []*franchiseModel.FileItem) (*franchiseModel.FranchiseApplicationResponse, error) {
	pbReq := &franchisePb.SubmitApplicationRequest{
		ManagerId:      managerId.String(),
		FullName:       req.FullName,
		Email:          req.Email,
		Phone:          req.Phone,
		IdentityNumber: req.IdentityNumber,
		ProvinceId:     req.ProvinceId,
		DistrictId:     req.DistrictId,
		WardId:         req.WardId,
		StoreName:      req.StoreName,
		StoreAddress:   req.StoreAddress,
		StoreLatitude:  req.StoreLatitude,
		StoreLongitude: req.StoreLongitude,
		StoreSizeSqm:   req.StoreSizeSqm,
	}

	// Map Frontend File -> gRPC Bytes
	if storeFront != nil {
		pbReq.StoreFrontData = storeFront.Data
		pbReq.StoreFrontFilename = storeFront.FileName
	}

	for _, cert := range landCerts {
		pbReq.LandCertificateFiles = append(pbReq.LandCertificateFiles, &franchisePb.LandCertificateFile{
			Data:        cert.Data,
			Filename:    cert.FileName,
			ContentType: cert.ContentType,
		})
	}

	res, err := c.client.SubmitFranchiseApplication(ctx, pbReq)
	if err != nil {
		return nil, err
	}

	mapped := mapper.MapFranchiseApplicationResponse(res)
	return mapped, nil
}

// GetApplicationDetail implements IFranchiseService.
func (c *franchiseGrpcClient) GetApplicationDetail(ctx context.Context, id uuid.UUID) (*franchiseModel.FranchiseApplicationDetailResponse, error) {
	res, err := c.client.GetApplicationDetails(ctx, &franchisePb.GetApplicationRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}

	mappedApp := mapper.MapFranchiseApplicationResponse(res.Application)

	contracts := make([]*franchiseModel.FranchiseContractResponse, 0, len(res.Contracts))
	for _, ct := range res.Contracts {
		contracts = append(contracts, mapper.MapFranchiseContractResponse(ct))
	}

	return &franchiseModel.FranchiseApplicationDetailResponse{
		Application: mappedApp,
		Contracts:   contracts,
	}, nil
}

// GetAllApplications implements IFranchiseService.
func (c *franchiseGrpcClient) GetAllApplications(ctx context.Context, req *franchiseModel.GetAllApplicationsRequest) (*franchiseModel.ApplicationPaginationResult, error) {
	pbReq := &franchisePb.GetAllApplicationsRequest{
		Page:     req.Page,
		PageSize: req.PageSize,
	}

	if req.Status != nil {
		pbReq.Status = req.Status
	}
	if req.SearchKey != "" {
		pbReq.SearchKey = &req.SearchKey
	}

	res, err := c.client.GetAllApplications(ctx, pbReq)
	if err != nil {
		return nil, err
	}

	items := make([]*franchiseModel.FranchiseApplicationResponse, 0, len(res.Applications))
	for _, item := range res.Applications {
		items = append(items, mapper.MapFranchiseApplicationResponse(item))
	}

	return &franchiseModel.ApplicationPaginationResult{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// GetMyApplications implements franchise.IFranchiseService.
func (c *franchiseGrpcClient) GetMyApplications(ctx context.Context) ([]*franchiseModel.FranchiseApplicationResponse, error) {
	res, err := c.client.GetMyApplications(ctx, &emptypb.Empty{})
	if err != nil {
		return []*franchiseModel.FranchiseApplicationResponse{}, err
	}

	items := make([]*franchiseModel.FranchiseApplicationResponse, 0, len(res.Applications))
	for _, item := range res.Applications {
		items = append(items, mapper.MapFranchiseApplicationResponse(item))
	}

	return items, nil
}

// ReviewApplication implements IFranchiseService.
func (c *franchiseGrpcClient) ReviewApplication(ctx context.Context, id uuid.UUID, req *franchiseModel.ReviewFranchiseApplicationRequest) (*franchiseModel.ReviewApplicationResponse, error) {
	pbReq := &franchisePb.ReviewFranchiseApplicationRequest{
		Id:                     id.String(),
		Action:                 req.Action,
		Note:                   req.Note,
		DepositAmount:          req.DepositAmount,
		FranchiseFee:           req.FranchiseFee,
		RevenueSharePercentage: req.RevenueSharePercentage,
	}

	if !req.ValidFrom.IsZero() {
		pbReq.ValidFrom = timestamppb.New(req.ValidFrom)
	}
	if !req.ValidUntil.IsZero() {
		pbReq.ValidUntil = timestamppb.New(req.ValidUntil)
	}

	res, err := c.client.ReviewApplication(ctx, pbReq)
	if err != nil {
		return nil, err
	}

	return &franchiseModel.ReviewApplicationResponse{
		Message:              res.Message,
		ContractId:           res.ContractId,
		DocuSealSubmissionId: res.DocusealSubmissionId,
	}, nil
}

// GetAllContracts implements IFranchiseService.
func (c *franchiseGrpcClient) GetAllContracts(ctx context.Context, req *franchiseModel.GetAllContractsRequest) (*franchiseModel.ContractPaginationResult, error) {
	pbReq := &franchisePb.GetAllContractsRequest{
		Page:     req.Page,
		PageSize: req.PageSize,
	}
	if req.Status != nil {
		pbReq.Status = req.Status
	}

	res, err := c.client.GetAllContracts(ctx, pbReq)
	if err != nil {
		return nil, err
	}

	items := make([]*franchiseModel.FranchiseContractResponse, 0, len(res.Contracts))
	for _, item := range res.Contracts {
		items = append(items, mapper.MapFranchiseContractResponse(item))
	}

	return &franchiseModel.ContractPaginationResult{
		Items:      items,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// GetMyContracts implements franchise.IFranchiseService.
func (c *franchiseGrpcClient) GetMyContracts(ctx context.Context) ([]*franchiseModel.FranchiseContractResponse, error) {
	res, err := c.client.GetMyContracts(ctx, &emptypb.Empty{})
	if err != nil {
		return []*franchiseModel.FranchiseContractResponse{}, err
	}

	items := make([]*franchiseModel.FranchiseContractResponse, 0, len(res.Contracts))
	for _, item := range res.Contracts {
		items = append(items, mapper.MapFranchiseContractResponse(item))
	}
	return items, nil
}

// GetContractDetail implements IFranchiseService.
func (c *franchiseGrpcClient) GetContractDetail(ctx context.Context, contractId uuid.UUID) (*franchiseModel.FranchiseContractResponse, error) {
	res, err := c.client.GetContractDetail(ctx, &franchisePb.GetContractDetailRequest{
		ContractId: contractId.String(),
	})
	if err != nil {
		return nil, err
	}

	mapped := mapper.MapFranchiseContractResponse(res)
	return mapped, nil
}

// ConfirmDepositPayment implements IFranchiseService.
func (c *franchiseGrpcClient) ConfirmDepositPayment(ctx context.Context, contractId uuid.UUID, accountantId uuid.UUID, req *franchiseModel.ConfirmDepositPaymentRequest) error {
	_, err := c.client.ConfirmDepositPayment(ctx, &franchisePb.ConfirmDepositPaymentRequest{
		ContractId:  contractId.String(),
		ConfirmedBy: accountantId.String(),
		Note:        req.Note,
	})
	return err
}

// UpdateSetupStatus implements IFranchiseService.
func (c *franchiseGrpcClient) UpdateSetupStatus(ctx context.Context, contractId uuid.UUID, adminId uuid.UUID, req *franchiseModel.UpdateSetupStatusRequest) (*franchiseModel.FranchiseContractResponse, error) {
	res, err := c.client.UpdateSetupStatus(ctx, &franchisePb.UpdateSetupStatusRequest{
		ContractId:  contractId.String(),
		SetupStatus: req.SetupStatus,
		UpdatedBy:   adminId.String(),
	})
	if err != nil {
		return nil, err
	}

	mapped := mapper.MapFranchiseContractResponse(res)
	return mapped, nil
}

// UpdateContractStatus implements IFranchiseService.
func (c *franchiseGrpcClient) UpdateContractStatus(ctx context.Context, contractId uuid.UUID, req *franchiseModel.UpdateContractStatusRequest) (*franchiseModel.FranchiseContractResponse, error) {
	res, err := c.client.UpdateContractStatus(ctx, &franchisePb.UpdateContractStatusRequest{
		ContractId: contractId.String(),
		Status:     req.Status,
		Reason:     req.Reason,
	})
	if err != nil {
		return nil, err
	}

	mapped := mapper.MapFranchiseContractResponse(res)
	return mapped, nil
}

// RenewContract implements IFranchiseService.
func (c *franchiseGrpcClient) RenewContract(ctx context.Context, contractId uuid.UUID, req *franchiseModel.RenewContractRequest) (*franchiseModel.FranchiseContractResponse, error) {
	res, err := c.client.RenewContract(ctx, &franchisePb.RenewContractRequest{
		ContractId:     contractId.String(),
		ExtensionYears: req.ExtensionYears,
		Note:           req.Note,
	})
	if err != nil {
		return nil, err
	}

	mapped := mapper.MapFranchiseContractResponse(res)
	return mapped, nil
}

// HandleDocuSealWebhook implements IFranchiseService.
func (c *franchiseGrpcClient) HandleDocuSealWebhook(ctx context.Context, signature string, rawBody string) error {
	_, err := c.client.HandleDocuSealWebhook(ctx, &franchisePb.DocuSealWebhookRequest{
		SignatureHeader: signature,
		RawBody:         rawBody,
	})
	return err
}
