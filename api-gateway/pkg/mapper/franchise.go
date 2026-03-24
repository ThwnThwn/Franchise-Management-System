package mapper

import (
	franchiseModel "api-gateway/internal/domain/models/franchise"
	franchisePb "api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"
)

// MapFranchiseApplicationResponse chuyển từ gRPC response sang DTO
func MapFranchiseApplicationResponse(pb *franchisePb.FranchiseApplicationResponse) franchiseModel.FranchiseApplicationResponse {
	if pb == nil {
		return franchiseModel.FranchiseApplicationResponse{}
	}

	app := franchiseModel.FranchiseApplicationResponse{
		Id:                  pb.Id,
		StoreId:             pb.StoreId,
		ManagerId:           pb.ManagerId,
		FullName:            pb.FullName,
		Email:               pb.Email,
		Phone:               pb.Phone,
		IdentityNumber:      pb.IdentityNumber,
		ProvinceId:          pb.ProvinceId,
		DistrictId:          pb.DistrictId,
		WardId:              pb.WardId,
		StoreName:           pb.StoreName,
		StoreAddress:        pb.StoreAddress,
		StoreLatitude:       pb.StoreLatitude,
		StoreLongitude:      pb.StoreLongitude,
		StoreSizeSqm:        pb.StoreSizeSqm,
		LandCertificateUrls: pb.LandCertificateUrls,
		StoreFrontUrl:       pb.StoreFrontUrl,
		Status:              pb.Status,
		AdminNote:           pb.AdminNote,
	}

	if pb.CreatedAt != nil {
		app.CreatedAt = pb.CreatedAt.AsTime()
	}
	if pb.UpdatedAt != nil {
		app.UpdatedAt = pb.UpdatedAt.AsTime()
	}
	if pb.ReviewedBy != nil {
		app.ReviewedBy = stringutils.ToPtr(*pb.ReviewedBy)
	}
	if pb.ReviewedAt != nil {
		reviewedTime := pb.ReviewedAt.AsTime()
		app.ReviewedAt = &reviewedTime
	}

	// Map nested contracts
	if len(pb.Contracts) > 0 {
		app.Contracts = make([]franchiseModel.FranchiseContractResponse, 0, len(pb.Contracts))
		for _, c := range pb.Contracts {
			app.Contracts = append(app.Contracts, MapFranchiseContractResponse(c))
		}
	}

	return app
}

// MapFranchiseContractResponse chuyển từ gRPC contract sang DTO
func MapFranchiseContractResponse(pb *franchisePb.FranchiseContractResponse) franchiseModel.FranchiseContractResponse {
	if pb == nil {
		return franchiseModel.FranchiseContractResponse{}
	}

	contract := franchiseModel.FranchiseContractResponse{
		Id:                     pb.Id,
		ApplicationId:          pb.ApplicationId,
		DocuSealSubmissionId:   pb.DocusealSubmissionId,
		ContractPdfUrl:         pb.ContractPdfUrl,
		DepositAmount:          pb.DepositAmount,
		DepositStatus:          pb.DepositStatus,
		FranchiseFee:           pb.FranchiseFee,
		RevenueSharePercentage: pb.RevenueSharePercentage,
		Status:                 pb.Status,
		SetupStatus:            pb.SetupStatus,
	}

	if pb.ValidFrom != nil {
		validFrom := pb.ValidFrom.AsTime()
		contract.ValidFrom = &validFrom
	}
	if pb.ValidUntil != nil {
		validUntil := pb.ValidUntil.AsTime()
		contract.ValidUntil = &validUntil
	}
	if pb.CreatedAt != nil {
		contract.CreatedAt = pb.CreatedAt.AsTime()
	}
	if pb.UpdatedAt != nil {
		contract.UpdatedAt = pb.UpdatedAt.AsTime()
	}

	return contract
}
