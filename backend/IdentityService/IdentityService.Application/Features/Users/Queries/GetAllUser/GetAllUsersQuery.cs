using IdentityService.Application.Dtos.User.Respone;
using MediatR;
using BuildingBlocks.Core.Pagination;
namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public class GetAllUsersQuery() : PaginationRequest, IRequest<PagedResult<UserRep>>
{
    public string? RoleSlug {  get; set; }
    public Guid? Id { get; set; }
    public Guid? StoreId { get; set; }
    public bool? HasHourlyWage { get; set; }
    public string? Keyword { get; set; }
    public Guid? TierId { get; set; }
    public int? OrderCount { get; set; }
    public decimal? StartAt { get; set; }
    public decimal? EndAt { get; set; }
}
