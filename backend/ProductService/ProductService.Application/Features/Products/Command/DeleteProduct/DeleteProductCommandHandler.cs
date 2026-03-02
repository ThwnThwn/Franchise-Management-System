using Ardalis.Result;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using SharedKernel.Events;

namespace ProductService.Application.Features.Products.Command.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IProductUow uow,
    IIntegrationEventBus eventBus
) : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductUow _uow = uow;
    private readonly IIntegrationEventBus _eventBus = eventBus;

    public async Task<Result<bool>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return Result.NotFound($"Product with ID '{request.ProductId}' not found.");
        }

        // 1. Bắt đầu Transaction
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Thực hiện Delete Only (Không SaveChanges)
            await _productRepository.DeleteOnlyAsync(product.Id);

            // 3. Bắn event xóa để đồng bộ Elasticsearch
            await _eventBus.PublishAsync("product.deleted",
                new ProductDeletedIntegrationEvent(product.Id), cancellationToken);

            // 4. CHỐT SỔ
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
        catch (Exception)
        {
            await _uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}