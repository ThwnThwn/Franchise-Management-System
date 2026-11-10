using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Queries;

// ── GetAllApplications ───────────────────────────────────────────────────────
public class GetAllApplicationsQuery : IRequest<GetAllApplicationsResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public FranchiseApplicationStatus? Status { get; set; }
    public string? Keyword { get; set; }
}

public class GetAllApplicationsResult
{
    public List<FranchiseApplication> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class GetAllApplicationsHandler : IRequestHandler<GetAllApplicationsQuery, GetAllApplicationsResult>
{
    private readonly IFranchiseApplicationRepository _repo;
    public GetAllApplicationsHandler(IFranchiseApplicationRepository repo) => _repo = repo;

    public async Task<GetAllApplicationsResult> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repo.GetPagedAsync(
            request.Page, request.PageSize, request.Status, request.Keyword, cancellationToken);

        return new GetAllApplicationsResult
        {
            Items = paged.Items.ToList(),
            TotalCount = (int)paged.TotalCount,
            TotalPages = paged.TotalPages,
            CurrentPage = request.Page
        };
    }
}

// ── GetApplicationWithContracts ──────────────────────────────────────────────
public class GetApplicationDetailQuery : IRequest<FranchiseApplication?>
{
    public Guid Id { get; set; }
}

public class GetApplicationDetailHandler : IRequestHandler<GetApplicationDetailQuery, FranchiseApplication?>
{
    private readonly IFranchiseApplicationRepository _repo;
    public GetApplicationDetailHandler(IFranchiseApplicationRepository repo) => _repo = repo;

    public async Task<FranchiseApplication?> Handle(GetApplicationDetailQuery request, CancellationToken cancellationToken)
        => await _repo.GetWithContractsAsync(request.Id, cancellationToken);
}
