using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Models;
using MediatR;

namespace FranchiseService.Application.Features.FranchiseApplications.Queries;

public class GetMyApplicationsQuery : IRequest<IEnumerable<FranchiseApplication>>
{
    public Guid UserId { get; set; }
}

public class GetMyApplicationsHandler(
    IFranchiseApplicationRepository repo
) : IRequestHandler<GetMyApplicationsQuery, IEnumerable<FranchiseApplication>>
{
    public async Task<IEnumerable<FranchiseApplication>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var result = await repo.GetAllAsync(
            includes: [x => x.Contracts],
            predicate: x => x.ManagerId == request.UserId
        );

        return result ?? [];
    }
}