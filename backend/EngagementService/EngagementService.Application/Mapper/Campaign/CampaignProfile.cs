using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Grpc.Protos.CampaignGrpc;
using Google.Protobuf.WellKnownTypes;
namespace EngagementService.Application.Mapper.Campaign
{
    public class CampaignProfile : Profile
    {
        public CampaignProfile() 
        {
            //Command Mapping
            CreateMap<CreateCampaginReq, Domain.Entities.Campaign>();

            //Respone Mapping
            CreateMap<Domain.Entities.Campaign, CampaginRes>();
            CreateMap<Domain.Entities.Voucher, Dtos.Campaign.Response.Voucher>();
            CreateMap<Domain.Entities.VoucherItem, Dtos.Campaign.Response.VoucherItem>();
        }

    }
}
