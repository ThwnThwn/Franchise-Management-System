using EngagementService.Application.Dtos.Campaign.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Queries.GetAll
{
    public record class GetAllQuery() : IRequest<IEnumerable<CampaginRes>>;
}
