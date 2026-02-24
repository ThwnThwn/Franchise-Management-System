using FranchiseService.Grpc;
using FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate;
using FranchiseService.Application.Features.ShiftTemplates.Commands.DeleteTemplate;
using FranchiseService.Application.Features.ShiftTemplates.Commands.UpdateTemplate;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplateById;
using FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplates;
using AppDTOs = FranchiseService.Application.Features.ShiftTemplates.DTOs; // Alias DTO
using Grpc.Core;
using MediatR;
using System;

namespace FranchiseService.API.Services
{
    public class ShiftTemplateGrpcService : FranchiseService.Grpc.ShiftTemplateGrpcService.ShiftTemplateGrpcServiceBase
    {
        private readonly IMediator _mediator;

        public ShiftTemplateGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- GET LIST ---
        public override async Task<GetShiftTemplatesResponse> GetShiftTemplates(GetShiftTemplatesRequest request, ServerCallContext context)
        {
            var query = new GetShiftTemplatesQuery(
                string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId),
                request.HasIsActive ? request.IsActive : null,
                request.Keyword
            );

            var result = await _mediator.Send(query);

            var response = new GetShiftTemplatesResponse();
            response.Items.AddRange(result.Select(MapToProto));
            return response;
        }

        // --- GET BY ID ---
        public override async Task<ShiftTemplateResponse> GetShiftTemplateById(GetShiftTemplateByIdRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id format"));
            }

            var result = await _mediator.Send(new GetShiftTemplateByIdQuery(id));
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Shift template not found"));
            }

            return MapToProto(result);
        }

        // --- CREATE ---
        public override async Task<CreateShiftTemplateResponse> CreateShiftTemplate(CreateShiftTemplateRequest request, ServerCallContext context)
        {
            var command = new CreateTemplateCommand
            {
                StoreId = string.IsNullOrEmpty(request.StoreId) ? null : Guid.Parse(request.StoreId),
                Code = request.Code,
                Name = request.Name,
                StartTime = TimeOnly.Parse(request.StartTime),
                EndTime = TimeOnly.Parse(request.EndTime),
                BreakDurationMinutes = request.BreakDurationMinutes
            };

            var id = await _mediator.Send(command);
            return new CreateShiftTemplateResponse { Id = id.ToString() };
        }

        // --- UPDATE ---
        public override async Task<UpdateShiftTemplateResponse> UpdateShiftTemplate(UpdateShiftTemplateRequest request, ServerCallContext context)
        {
            var command = new UpdateTemplateCommand
            {
                Id = Guid.Parse(request.Id),
                Code = request.HasCode ? request.Code : null,
                Name = request.HasName ? request.Name : null,
                StartTime = request.HasStartTime ? TimeOnly.Parse(request.StartTime) : null,
                EndTime = request.HasEndTime ? TimeOnly.Parse(request.EndTime) : null,
                BreakDurationMinutes = request.HasBreakDurationMinutes ? request.BreakDurationMinutes : null,
                IsActive = request.HasIsActive ? request.IsActive : null
            };

            await _mediator.Send(command);
            return new UpdateShiftTemplateResponse { Success = true };
        }

        // --- DELETE ---
        public override async Task<DeleteShiftTemplateResponse> DeleteShiftTemplate(DeleteShiftTemplateRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                 throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id format"));
            }

            await _mediator.Send(new DeleteTemplateCommand(id));
            return new DeleteShiftTemplateResponse { Success = true };
        }

        // --- HELPER ---
        private FranchiseService.Grpc.ShiftTemplateResponse MapToProto(AppDTOs.ShiftTemplateResponse dto)
        {
            return new FranchiseService.Grpc.ShiftTemplateResponse
            {
                Id = dto.Id.ToString(),
                StoreId = dto.StoreId?.ToString() ?? "",
                Code = dto.Code,
                Name = dto.Name,
                StartTime = dto.StartTime.ToString("HH:mm"),
                EndTime = dto.EndTime.ToString("HH:mm"),
                BreakDurationMinutes = dto.BreakDurationMinutes,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt.ToString("O")
            };
        }
    }
}
