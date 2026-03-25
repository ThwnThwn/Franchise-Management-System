using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseContracts.Queries;

public class GetMyContractsQuery : IRequest<IEnumerable<FranchiseContract>>
{
    public Guid UserId { get; set; }
}

public class GetMyContractsHandler(
    IFranchiseContractRepository repo
) : IRequestHandler<GetMyContractsQuery, IEnumerable<FranchiseContract>>
{
    public async Task<IEnumerable<FranchiseContract>> Handle(GetMyContractsQuery request, CancellationToken cancellationToken)
    {
        var result = await repo.GetAllAsync(
            includes: [x => x.Application],
            predicate: x => x.Application.ManagerId == request.UserId
        );
        return result ?? [];
    }
}