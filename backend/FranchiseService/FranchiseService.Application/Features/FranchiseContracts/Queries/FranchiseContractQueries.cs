using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseContracts.Queries;

// ── GetAllContracts ──────────────────────────────────────────────────────────
public class GetAllContractsQuery : IRequest<GetAllContractsResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ContractStatus? Status { get; set; }
}

public class GetAllContractsResult
{
    public List<FranchiseContract> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class GetAllContractsHandler : IRequestHandler<GetAllContractsQuery, GetAllContractsResult>
{
    private readonly IFranchiseContractRepository _repo;
    public GetAllContractsHandler(IFranchiseContractRepository repo) => _repo = repo;

    public async Task<GetAllContractsResult> Handle(GetAllContractsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _repo.GetPagedAsync(request.Page, request.PageSize, request.Status, cancellationToken);
        return new GetAllContractsResult
        {
            Items = paged.Items.ToList(),
            TotalCount = (int)paged.TotalCount,
            TotalPages = paged.TotalPages,
            CurrentPage = request.Page
        };
    }
}

// ── GetContractDetail ────────────────────────────────────────────────────────
public class GetContractDetailQuery : IRequest<FranchiseContract?>
{
    public Guid ContractId { get; set; }
}

public class GetContractDetailHandler : IRequestHandler<GetContractDetailQuery, FranchiseContract?>
{
    private readonly IFranchiseContractRepository _repo;
    public GetContractDetailHandler(IFranchiseContractRepository repo) => _repo = repo;

    public async Task<FranchiseContract?> Handle(GetContractDetailQuery request, CancellationToken cancellationToken)
        => await _repo.GetWithApplicationAsync(request.ContractId, cancellationToken);
}
