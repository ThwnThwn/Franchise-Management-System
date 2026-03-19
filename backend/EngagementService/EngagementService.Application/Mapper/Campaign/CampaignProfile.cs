using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;

namespace EngagementService.Application.Mapper.Campaign
{
    public class CampaignProfile : Profile
    {
        public CampaignProfile()
        {
            //Command Mapping
            CreateMap<CreateCampaginReq, Domain.Entities.Campaign>();

            //Respone Mapping
            CreateMap<Domain.Entities.Campaign, CampaignRes>();
            CreateMap<Domain.Entities.Voucher, Dtos.Campaign.Response.Voucher>();
            CreateMap<Domain.Entities.VoucherItem, Dtos.Campaign.Response.VoucherItem>();
        }
    }
}