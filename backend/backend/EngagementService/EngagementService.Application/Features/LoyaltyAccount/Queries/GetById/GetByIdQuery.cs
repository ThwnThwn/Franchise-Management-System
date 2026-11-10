namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public record class GetByIdQuery(Guid Id) : MediatR.IRequest<Domain.Entities.LoyaltyAccount>
    {
    }
}
