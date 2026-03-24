using FranchiseService.Application.ClientInterfaces;
using FranchiseService.Application.Features.FranchiseApplications.Commands.CreateFranchiseApplication;
using FranchiseService.Application.Features.FranchiseApplications.Commands.ReviewFranchiseApplication;
using FranchiseService.Application.Features.FranchiseApplications.Commands.UploadApplicationImage;
using FranchiseService.Application.Features.FranchiseApplications.Queries;
using FranchiseService.Application.Features.FranchiseContracts.Commands.ConfirmDepositPayment;
using FranchiseService.Application.Features.FranchiseContracts.Commands.HandleDocuSealWebhook;
using FranchiseService.Application.Features.FranchiseContracts.Commands.UpdateSetupStatus;
using FranchiseService.Application.Features.FranchiseContracts.Queries;
using FranchiseService.Application.Interfaces;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Franchise.Grpc;
using FranchiseService.Infrastructure.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FranchiseService.API.Services;

/// <summary>
/// gRPC Service xử lý toàn bộ quy trình nhượng quyền (Franchise).
/// Map proto FranchiseGrpcService → MediatR Commands/Queries.
///
/// Luồng chính:
///   SubmitFranchiseApplication → ReviewApplication (Approve) → [DocuSeal ký] → ConfirmDepositPayment → Active
/// </summary>
public class FranchiseGrpcService : Franchise.Grpc.FranchiseGrpcService.FranchiseGrpcServiceBase
{
    private readonly IMediator _mediator;
    private readonly IDocuSealService _docuSeal;
    private readonly IImageService _imageService;
    private readonly ILogger<FranchiseGrpcService> _logger;
    private readonly IOptions<DocuSealSettings> _docuSealSettings;
    private readonly IIdentityServiceClient _identityClient;

    public FranchiseGrpcService(
        IMediator mediator,
        IDocuSealService docuSeal,
        IImageService imageService,
        ILogger<FranchiseGrpcService> logger,
        IOptions<DocuSealSettings> docuSealSettings,
        IIdentityServiceClient identityClient)
    {
        _mediator = mediator;
        _docuSeal = docuSeal;
        _imageService = imageService;
        _logger = logger;
        _docuSealSettings = docuSealSettings;
        _identityClient = identityClient;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 1. SubmitFranchiseApplication
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseApplicationResponse> SubmitFranchiseApplication(
        SubmitApplicationRequest request, ServerCallContext context)
    {
        // Lấy managerId từ metadata header x-user-id (được Gateway gắn sau khi verify JWT)
        var userIdHeader = context.RequestHeaders.GetValue("x-user-id");
        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var managerId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Không xác định được user. Vui lòng đăng nhập."));

        // StoreId được tự generate (đối tác chưa có store khi nộp hồ sơ)
        //var storeId = Guid.NewGuid();

        // Map repeated LandCertificateFile → List<FileUploadItem>
        var landCertFiles = request.LandCertificateFiles
            .Select(f => new FileUploadItem
            {
                Data = f.Data.ToByteArray(),
                FileName = f.Filename,
                ContentType = string.IsNullOrWhiteSpace(f.ContentType) ? "image/jpeg" : f.ContentType
            })
            .ToList();

        FileUploadItem? storeFront = null;
        if (request.StoreFrontData is { Length: > 0 })
        {
            storeFront = new FileUploadItem
            {
                Data = request.StoreFrontData.ToByteArray(),
                FileName = request.StoreFrontFilename,
                ContentType = "image/jpeg"
            };
        }

        var command = new CreateFranchiseApplicationCommand
        {
            //StoreId = storeId,
            ManagerId = managerId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            IdentityNumber = request.IdentityNumber,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            WardId = request.WardId,
            StoreName = request.StoreName,
            StoreAddress = request.StoreAddress,
            StoreLatitude = (decimal)request.StoreLatitude,
            StoreLongitude = (decimal)request.StoreLongitude,
            StoreSizeSqm = (decimal)request.StoreSizeSqm,
            LandCertificateFiles = landCertFiles,
            StoreFrontFile = storeFront
        };

        var id = await _mediator.Send(command, context.CancellationToken);

        // Lấy lại application vừa tạo để trả về response đầy đủ
        var app = await _mediator.Send(new GetApplicationDetailQuery { Id = id }, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.Internal, "Không thể lấy thông tin hồ sơ vừa tạo"));

        return MapToApplicationResponse(app);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 2. GetApplicationDetails
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseApplicationDetailResponse> GetApplicationDetails(
        GetApplicationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ID không hợp lệ"));

        var app = await _mediator.Send(new GetApplicationDetailQuery { Id = id }, context.CancellationToken);
        if (app == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy hồ sơ với ID: {request.Id}"));

        var response = new FranchiseApplicationDetailResponse
        {
            Application = MapToApplicationResponse(app)
        };
        response.Contracts.AddRange(app.Contracts.Select(MapToContractResponse));
        return response;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 3. GetAllApplications
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<GetAllApplicationsResponse> GetAllApplications(
        GetAllApplicationsRequest request, ServerCallContext context)
    {
        FranchiseApplicationStatus? statusFilter = request.HasStatus
            ? (FranchiseApplicationStatus)request.Status
            : null;

        var result = await _mediator.Send(new GetAllApplicationsQuery
        {
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 10,
            Status = statusFilter,
            Keyword = request.HasSearchKey ? request.SearchKey : null
        }, context.CancellationToken);

        var response = new GetAllApplicationsResponse
        {
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            CurrentPage = result.CurrentPage,
        };
        response.Applications.AddRange(result.Items.Select(MapToApplicationResponse));
        return response;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 4. ReviewApplication
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<ReviewFranchiseApplicationResponse> ReviewApplication(
        ReviewFranchiseApplicationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var appId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ID không hợp lệ"));

        var userIdHeader = context.RequestHeaders.FirstOrDefault(h => h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var reviewedBy))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Không xác định được người duyệt. Vui lòng đăng nhập."));

        // Xác thực Role trực tiếp từ IdentityService dựa trên x-user-id
        var userInfo = await _identityClient.GetUserByIdAsync(reviewedBy);
        if (userInfo == null || !string.Equals(userInfo.RoleSlug, "admin", StringComparison.OrdinalIgnoreCase))
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Chỉ Admin mới có quyền duyệt hồ sơ nhượng quyền."));

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = appId,
            ReviewedBy = reviewedBy,
            Decision = (FranchiseApplicationStatus)request.Action,
            AdminNote = request.Note,
            DocuSealTemplateId = _docuSealSettings.Value.TemplateId,
            DepositAmount = request.DepositAmount > 0 ? (decimal)request.DepositAmount : null,
            FranchiseFee = request.FranchiseFee > 0 ? (decimal)request.FranchiseFee : null,
            RevenueSharePercentage = request.RevenueSharePercentage > 0 ? (decimal)request.RevenueSharePercentage : null,
            ValidFrom = request.ValidFrom != null ? request.ValidFrom.ToDateTimeOffset() : null,
            ValidUntil = request.ValidUntil != null ? request.ValidUntil.ToDateTimeOffset() : null
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        return new ReviewFranchiseApplicationResponse
        {
            Success = result.Success,
            Message = result.Message,
            ContractId = result.ContractId?.ToString() ?? string.Empty,
            DocusealSubmissionId = result.DocuSealSubmissionId ?? 0
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 5. GetAllContracts
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<GetAllContractsResponse> GetAllContracts(
        GetAllContractsRequest request, ServerCallContext context)
    {
        ContractStatus? statusFilter = request.HasStatus
            ? (ContractStatus)request.Status
            : null;

        var result = await _mediator.Send(new GetAllContractsQuery
        {
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 10,
            Status = statusFilter
        }, context.CancellationToken);

        var response = new GetAllContractsResponse
        {
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            CurrentPage = result.CurrentPage
        };
        response.Contracts.AddRange(result.Items.Select(MapToContractResponse));
        return response;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 6. GetContractDetail
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseContractResponse> GetContractDetail(
        GetContractDetailRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ContractId, out var contractId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "contract_id không hợp lệ"));

        var contract = await _mediator.Send(
            new GetContractDetailQuery { ContractId = contractId }, context.CancellationToken);

        if (contract == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy hợp đồng"));

        return MapToContractResponse(contract);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 7. ConfirmDepositPayment
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<ConfirmDepositPaymentResponse> ConfirmDepositPayment(
        ConfirmDepositPaymentRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ContractId, out var contractId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "contract_id không hợp lệ"));
        if (!Guid.TryParse(request.ConfirmedBy, out var confirmedBy))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "confirmed_by không hợp lệ"));

        await _mediator.Send(new ConfirmDepositPaymentCommand
        {
            ContractId = contractId,
            ConfirmedBy = confirmedBy,
            Note = request.Note
        }, context.CancellationToken);

        return new ConfirmDepositPaymentResponse
        {
            Success = true,
            Message = "Hợp đồng đã được kích hoạt thành công."
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 8. UpdateSetupStatus
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseContractResponse> UpdateSetupStatus(
        UpdateSetupStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ContractId, out var contractId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "contract_id không hợp lệ"));
        if (!Guid.TryParse(request.UpdatedBy, out var updatedBy))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "updated_by không hợp lệ"));

        await _mediator.Send(new UpdateSetupStatusCommand
        {
            ContractId = contractId,
            SetupStatus = (ContractSetupStatus)request.SetupStatus,
            UpdatedBy = updatedBy
        }, context.CancellationToken);

        var contract = await _mediator.Send(
            new GetContractDetailQuery { ContractId = contractId }, context.CancellationToken);

        return MapToContractResponse(contract!);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 9. UpdateContractStatus (Suspend, Terminate,...)
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseContractResponse> UpdateContractStatus(
        UpdateContractStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ContractId, out var contractId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "contract_id không hợp lệ"));

        var userIdHeader = context.RequestHeaders.FirstOrDefault(h => h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Không xác định được người thao tác. Vui lòng đăng nhập."));

        // Xác thực Role trực tiếp từ IdentityService
        var userInfo = await _identityClient.GetUserByIdAsync(userId);
        if (userInfo == null || !string.Equals(userInfo.RoleSlug, "admin", StringComparison.OrdinalIgnoreCase))
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Chỉ Admin mới có quyền thao tác hợp đồng."));

        await _mediator.Send(new Application.Features.FranchiseContracts.Commands.UpdateContractStatus.UpdateContractStatusCommand
        {
            ContractId = contractId,
            Status = (ContractStatus)request.Status,
            Reason = request.Reason
        }, context.CancellationToken);

        var contract = await _mediator.Send(
            new GetContractDetailQuery { ContractId = contractId }, context.CancellationToken);

        return MapToContractResponse(contract!);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 10. RenewContract (Gia hạn)
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<FranchiseContractResponse> RenewContract(
        RenewContractRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ContractId, out var contractId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "contract_id không hợp lệ"));

        var userIdHeader = context.RequestHeaders.FirstOrDefault(h => h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Không xác định được người thao tác. Vui lòng đăng nhập."));

        // Xác thực Role trực tiếp từ IdentityService
        var userInfo = await _identityClient.GetUserByIdAsync(userId);
        if (userInfo == null || !string.Equals(userInfo.RoleSlug, "admin", StringComparison.OrdinalIgnoreCase))
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Chỉ Admin mới có quyền gia hạn hợp đồng."));

        await _mediator.Send(new Application.Features.FranchiseContracts.Commands.RenewContract.RenewContractCommand
        {
            ContractId = contractId,
            ExtensionYears = request.ExtensionYears,
            Note = request.Note
        }, context.CancellationToken);

        var contract = await _mediator.Send(
            new GetContractDetailQuery { ContractId = contractId }, context.CancellationToken);

        return MapToContractResponse(contract!);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 11. HandleDocuSealWebhook (Internal — gọi từ Gateway hoặc ngrok endpoint)
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<DocuSealWebhookResponse> HandleDocuSealWebhook(
        DocuSealWebhookRequest request, ServerCallContext context)
    {
        // Verify HMAC signature
        if (!string.IsNullOrWhiteSpace(request.SignatureHeader))
        {
            if (!_docuSeal.VerifyWebhookSignature(request.RawBody, request.SignatureHeader))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Chữ ký Webhook không hợp lệ"));
        }

        var result = await _mediator.Send(new HandleDocuSealWebhookCommand
        {
            EventType = request.EventType,
            SubmissionId = request.SubmissionId,
            DocumentUrl = string.IsNullOrWhiteSpace(request.DocumentUrl) ? null : request.DocumentUrl,
            CompletedAt = request.CompletedAt?.ToDateTime()
        }, context.CancellationToken);

        return new DocuSealWebhookResponse
        {
            Success = result,
            Message = result ? "Webhook processed." : "Webhook ignored."
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 12. UploadImage (Dùng để Client upload ảnh độc lập, lấy URL)
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<UploadImageResponse> UploadImage(
        UploadImageRequest request, ServerCallContext context)
    {
        if (request.ImageData == null || request.ImageData.Length == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Image data is required."));

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "File name is required."));

        var result = await _imageService.UploadImageFromBytesAsync(
            request.ImageData.ToByteArray(),
            request.FileName,
            request.ContentType ?? "image/jpeg");

        return new UploadImageResponse
        {
            Url = result.Url,
            PublicId = result.PublicId
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 13. UploadApplicationImage (Upload ảnh gắn trực tiếp vào một Application)
    // ──────────────────────────────────────────────────────────────────────────
    public override async Task<UploadApplicationImageResponse> UploadApplicationImage(
        UploadApplicationImageRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ApplicationId, out var applicationId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "application_id không hợp lệ"));

        if (request.ImageData == null || request.ImageData.Length == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Image data is required."));

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "File name is required."));

        var command = new UploadApplicationImageCommand(
            ApplicationId: applicationId,
            ImageData: request.ImageData.ToByteArray(),
            FileName: request.FileName,
            ContentType: request.ContentType ?? "image/jpeg",
            ImageType: request.ImageType
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        return new UploadApplicationImageResponse
        {
            ApplicationId = result.ApplicationId.ToString(),
            ImageUrl = result.ImageUrl,
            ImagePublicId = result.ImagePublicId
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Map helpers
    // ──────────────────────────────────────────────────────────────────────────
    private static FranchiseApplicationResponse MapToApplicationResponse(FranchiseApplication app)
    {
        var response = new FranchiseApplicationResponse
        {
            Id = app.Id.ToString(),
            StoreId = app.StoreId.ToString(),
            ManagerId = app.ManagerId.ToString(),
            FullName = app.FullName,
            Email = app.Email,
            Phone = app.Phone,
            IdentityNumber = app.IdentityNumber,
            ProvinceId = app.ProvinceId ?? string.Empty,
            DistrictId = app.DistrictId ?? string.Empty,
            WardId = app.WardId ?? string.Empty,
            StoreName = app.StoreName,
            StoreAddress = app.StoreAddress,
            StoreLatitude = (double)(app.StoreLatitude ?? 0),
            StoreLongitude = (double)(app.StoreLongitude ?? 0),
            StoreSizeSqm = (double)(app.StoreSizeSqm ?? 0),
            StoreFrontUrl = app.StoreFrontUrl ?? string.Empty,
            Status = (int)app.Status,
            AdminNote = app.AdminNote ?? string.Empty,
            CreatedAt = Timestamp.FromDateTimeOffset(app.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(app.UpdatedAt)
        };

        if (app.LandCertificates?.Length > 0)
            response.LandCertificateUrls.AddRange(app.LandCertificates);

        if (app.ReviewedBy.HasValue)
            response.ReviewedBy = app.ReviewedBy.ToString();

        if (app.ReviewedAt.HasValue)
            response.ReviewedAt = Timestamp.FromDateTimeOffset(app.ReviewedAt.Value);

        if (app.Contracts != null && app.Contracts.Any())
        {
            response.Contracts.AddRange(app.Contracts.Select(MapToContractResponse));
        }

        return response;
    }

    private static FranchiseContractResponse MapToContractResponse(FranchiseContract contract)
    {
        var response = new FranchiseContractResponse
        {
            Id = contract.Id.ToString(),
            ApplicationId = contract.ApplicationId.ToString(),
            DocusealSubmissionId = contract.DocusealSubmissionId ?? 0,
            ContractPdfUrl = contract.ContractPdfUrl ?? string.Empty,
            DepositAmount = (double)(contract.DepositAmount ?? 0),
            DepositStatus = (int)contract.DepositStatus,
            FranchiseFee = (double)(contract.FranchiseFee ?? 0),
            RevenueSharePercentage = (double)(contract.RevenueSharePercentage ?? 0),
            Status = (int)contract.Status,
            SetupStatus = (int)contract.SetupStatus,
            CreatedAt = Timestamp.FromDateTimeOffset(contract.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(contract.UpdatedAt)
        };

        if (contract.ValidFrom.HasValue)
            response.ValidFrom = Timestamp.FromDateTimeOffset(contract.ValidFrom.Value);
        if (contract.ValidUntil.HasValue)
            response.ValidUntil = Timestamp.FromDateTimeOffset(contract.ValidUntil.Value);

        return response;
    }
}