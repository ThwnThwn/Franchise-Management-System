using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Campaign.Commands.Create
{
    public class CreateCampaginHanlder : IRequestHandler<CreateCampaginCommand, CampaginRes>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMapper _mapper;

        public CreateCampaginHanlder(IVoucherRepository voucherRepository, ICampaignRepository campaignRepository,IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _campaignRepository = campaignRepository;
            _mapper = mapper;
        }

        public async Task<CampaginRes> Handle(CreateCampaginCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            IEnumerable<Domain.Entities.Voucher> vouchers = await _voucherRepository.GetVoucherByIdsAsync(request.VoucherIds.ToArray());
            if(!vouchers.Any())
            {
                throw new ArgumentException("Vouchers failed to be added into campaign - CreateHandler");
            }
            Domain.Entities.Campaign campaign = _mapper.Map<Domain.Entities.Campaign>(request);
            campaign.Vouchers = vouchers.ToList();
            campaign.IsActive = false;
            foreach (Domain.Entities.Voucher voucher in vouchers)
            {
                voucher.CampaignId = campaign.Id;
                voucher.Campaign = campaign;
                voucher.UpdatedAt = DateTimeOffset.Now;
            }
            await _campaignRepository.AddAsync(campaign);
            return _mapper.Map<CampaginRes>(campaign);
        }
    }
}
