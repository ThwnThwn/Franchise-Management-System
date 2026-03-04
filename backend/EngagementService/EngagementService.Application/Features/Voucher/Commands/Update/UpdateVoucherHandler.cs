using Confluent.Kafka;
using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.Update
{
    public class UpdateVoucherHandler : IRequestHandler<UpdateVoucherCommand, Unit>
    {
        private readonly IVoucherRepository _voucherRepository;
        public UpdateVoucherHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<Unit> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            var updateRequest = request.Request;
            if (updateRequest.EndDate.HasValue && updateRequest.EndDate.Value <= DateTimeOffset.UtcNow)
                throw new ArgumentException(VoucherMsg.InvalidEndDate);
            await _voucherRepository.UpdateVoucherAsync(updateRequest);

            return Unit.Value;
        }
    }
}
