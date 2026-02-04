using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Wallets.Command.Create;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;

namespace SalesService.API.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public sealed class WalletsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalletsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet(
            [FromBody] CreateWalletCommand command)
        {
            var walletId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(CreateWallet),
                new { id = walletId },
                new { walletId });
        }

        [HttpPost("{walletId}/add-balance")]
        public async Task<IActionResult> AddBalance(
        Guid walletId,
        [FromBody] AddBalanceCommand.AddBalanceCommandRequest request)
        {

            var command = new AddBalanceCommand(walletId,request );

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpPost("{walletId}/deduct-balance")]
        public async Task<IActionResult> DeductBalance(
        Guid walletId,
        [FromBody] DeductBalanceCommand.DeductBalanceCommandRequest request)
        {
            var command = new DeductBalanceCommand(walletId, request);

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpPatch("{walletId}/status")]
        public async Task<IActionResult> UpdateWalletStatus(
        Guid walletId,
        [FromBody] UpdateWalletStatusCommand.UpdateWalletStatusRequest request)
        {
            var command = new UpdateWalletStatusCommand(walletId, request);

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _mediator.Send(
                new GetWalletByUserIdQuery(userId));

            return result.Status switch
            {
                Ardalis.Result.ResultStatus.Ok =>
                    Ok(result.Value),

                Ardalis.Result.ResultStatus.NotFound =>
                    NotFound(result.Errors),

                _ =>
                    BadRequest(result.Errors)
            };
        }
    }
}
