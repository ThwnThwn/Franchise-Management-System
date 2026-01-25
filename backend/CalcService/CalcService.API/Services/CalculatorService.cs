using Grpc.Core;
using CalcService.Protos;

namespace CalcService.API.Services
{
    public class CalculatorService : Calculator.CalculatorBase
    {
        private readonly ILogger<CalculatorService> _logger;

        public CalculatorService(ILogger<CalculatorService> logger)
        {
            _logger = logger;
        }

        public override Task<AddResponse> Add(AddRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Add request: {request.NumberA} + {request.NumberB}");

            var sum = request.NumberA + request.NumberB;

            return Task.FromResult(new AddResponse
            {
                Result = sum
            });
        }
    }
}