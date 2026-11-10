using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Commands.UploadApplicationImage;

public sealed record UploadApplicationImageCommand(
    Guid ApplicationId,
    byte[] ImageData,
    string FileName,
    string ContentType,
    string ImageType) : IRequest<UploadImageCommandResponse>;

public sealed record UploadImageCommandResponse(
    Guid ApplicationId,
    string ImageUrl,
    string ImagePublicId
);
