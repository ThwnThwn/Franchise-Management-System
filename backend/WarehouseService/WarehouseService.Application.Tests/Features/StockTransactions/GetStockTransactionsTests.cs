namespace WarehouseService.Application.Tests.Features.StockTransactions;

using FluentAssertions;
using FluentValidation.TestHelper;
using MockQueryable;
using Moq;
using WarehouseService.Application.Features.StockTransactions.Query.GetAll;
using WarehouseService.Application.Repositories;
using WarehouseService.Domain.Entities;
using WarehouseService.Domain.Enums;

public class GetStockTransactionsTests
{
    private readonly Mock<IStockTransactionRepository> _stockTransactionRepositoryMock;
    private readonly Mock<IWarehouseItemRepository> _warehouseItemRepositoryMock;
    private readonly GetStockTransactionsQueryHandler _handler;
    private readonly GetStockTransactionsQueryValidator _validator;

    public GetStockTransactionsTests()
    {
        _stockTransactionRepositoryMock = new Mock<IStockTransactionRepository>();
        _warehouseItemRepositoryMock = new Mock<IWarehouseItemRepository>();
        _handler = new GetStockTransactionsQueryHandler(
            _stockTransactionRepositoryMock.Object,
            _warehouseItemRepositoryMock.Object);
        _validator = new GetStockTransactionsQueryValidator();
    }

    // ==========================================================
    // VALIDATOR TESTS
    // ==========================================================

    [Fact]
    public void Should_HaveError_WhenPageIsZeroOrLess()
    {
        var query = new GetStockTransactionsQuery(Page: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page number must be greater than 0.");
    }

    [Fact]
    public void Should_HaveError_WhenPageSizeIsInvalid()
    {
        var queryUnder = new GetStockTransactionsQuery(PageSize: 0);
        var queryOver = new GetStockTransactionsQuery(PageSize: 101);

        var resultUnder = _validator.TestValidate(queryUnder);
        var resultOver = _validator.TestValidate(queryOver);

        resultUnder.ShouldHaveValidationErrorFor(x => x.PageSize);
        resultOver.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void Should_HaveError_WhenTypeIsInvalid()
    {
        var query = new GetStockTransactionsQuery(Type: (TransactionType)999);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Transaction type is invalid.");
    }

    [Fact]
    public void Should_HaveError_WhenStatusIsInvalid()
    {
        var query = new GetStockTransactionsQuery(Status: (TransactionStatus)999);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Transaction status is invalid.");
    }

    [Fact]
    public void Should_HaveError_WhenToDateIsBeforeFromDate()
    {
        var fromDate = DateTimeOffset.UtcNow;
        var toDate = fromDate.AddDays(-1);
        var query = new GetStockTransactionsQuery(FromDate: fromDate, ToDate: toDate);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.ToDate)
            .WithErrorMessage("ToDate must be greater than or equal to FromDate.");
    }

    [Fact]
    public void Should_NotHaveError_WhenRequestIsValid()
    {
        var query = new GetStockTransactionsQuery(
            Type: TransactionType.StockIn,
            Status: TransactionStatus.Completed,
            FromDate: DateTimeOffset.UtcNow.AddDays(-1),
            ToDate: DateTimeOffset.UtcNow,
            Page: 1,
            PageSize: 20
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ==========================================================
    // HANDLER TESTS
    // ==========================================================

    [Fact]
    public async Task Handle_ShouldReturnAllTransactions_WhenNoFiltersApplied()
    {
        var warehouseId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            CreateTransaction(Guid.NewGuid(), warehouseId, itemId, "SI-ARABICA-01", TransactionType.StockIn),
            CreateTransaction(Guid.NewGuid(), warehouseId, itemId, "SO-ROBUSTA-02", TransactionType.StockOut)
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem>
        {
            new() { Id = itemId, IngredientId = ingredientId, StockTransactions = [] }
        }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByWarehouseId_WhenProvided()
    {
        var targetWarehouseId = Guid.NewGuid(); // Kho Sài Gòn
        var itemId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), WarehouseId = targetWarehouseId, WarehouseItemId = itemId, TransactionCode = "SI-SG-01" },
            new() { Id = Guid.NewGuid(), WarehouseId = Guid.NewGuid(), WarehouseItemId = itemId, TransactionCode = "SI-HN-02" }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(WarehouseId: targetWarehouseId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("SI-SG-01");
    }

    [Fact]
    public async Task Handle_ShouldFilterByWarehouseItemId_WhenProvided()
    {
        var targetItemId = Guid.NewGuid(); // Hạt Moka
        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), WarehouseItemId = targetItemId, TransactionCode = "IN-MOKA" },
            new() { Id = Guid.NewGuid(), WarehouseItemId = Guid.NewGuid(), TransactionCode = "IN-ROBUSTA" }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem>
        {
            new() { Id = targetItemId },
            new() { Id = Guid.NewGuid() }
        }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(WarehouseItemId: targetItemId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("IN-MOKA");
    }

    [Fact]
    public async Task Handle_ShouldFilterBySupplierId_WhenProvided()
    {
        var targetSupplierId = Guid.NewGuid(); // Nhà cung cấp Vinamilk
        var itemId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), SupplierId = targetSupplierId, WarehouseItemId = itemId, TransactionCode = "PO-VINAMILK" },
            new() { Id = Guid.NewGuid(), SupplierId = Guid.NewGuid(), WarehouseItemId = itemId, TransactionCode = "PO-MONIN" }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(SupplierId: targetSupplierId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("PO-VINAMILK");
    }

    [Fact]
    public async Task Handle_ShouldFilterByDateRange_WhenDatesAreProvided()
    {
        var fromDate = DateTimeOffset.UtcNow.AddDays(-2);
        var toDate = DateTimeOffset.UtcNow.AddDays(-1);
        var itemId = Guid.NewGuid();

        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), CreatedAt = fromDate.AddHours(1), TransactionCode = "SI-OLD", WarehouseItemId = itemId, Status = TransactionStatus.Completed },
            new() { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, TransactionCode = "SI-NEW", WarehouseItemId = itemId, Status = TransactionStatus.Completed }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(FromDate: fromDate, ToDate: toDate);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("SI-OLD");
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatusAndType_WhenProvided()
    {
        var itemId = Guid.NewGuid();
        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), Type = TransactionType.StockIn, Status = TransactionStatus.Pending, TransactionCode = "P-01", WarehouseItemId = itemId },
            new() { Id = Guid.NewGuid(), Type = TransactionType.StockOut, Status = TransactionStatus.Completed, TransactionCode = "C-01", WarehouseItemId = itemId }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(Type: TransactionType.StockIn, Status: TransactionStatus.Pending);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("P-01");
    }

    [Fact]
    public async Task Handle_ShouldApplyPaginationCorrectly()
    {
        var itemId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 15).Select(i => new StockTransaction
        {
            Id = Guid.NewGuid(),
            TransactionCode = $"BATCH-{i}",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(i),
            WarehouseItemId = itemId,
            Status = TransactionStatus.Completed
        }).ToList().BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(Page: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ShouldFilterByReferenceCode_WhenTrimmedMatchFound()
    {
        var refCode = "PO-123";
        var itemId = Guid.NewGuid();
        var transactions = new List<StockTransaction>
        {
            new() { Id = Guid.NewGuid(), ReferenceCode = refCode, TransactionCode = "SI-01", WarehouseItemId = itemId, Status = TransactionStatus.Completed },
            new() { Id = Guid.NewGuid(), ReferenceCode = "OTHER", TransactionCode = "SI-02", WarehouseItemId = itemId, Status = TransactionStatus.Completed }
        }.BuildMock();

        var warehouseItems = new List<WarehouseItem> { new() { Id = itemId } }.BuildMock();

        _stockTransactionRepositoryMock.Setup(x => x.GetQueryable()).Returns(transactions);
        _warehouseItemRepositoryMock.Setup(x => x.GetQueryable()).Returns(warehouseItems);

        var query = new GetStockTransactionsQuery(ReferenceCode: $"  {refCode}  ");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].TransactionCode.Should().Be("SI-01");
    }

    private static StockTransaction CreateTransaction(Guid id, Guid whId, Guid itemId, string code, TransactionType type)
    {
        return new StockTransaction
        {
            Id = id,
            WarehouseId = whId,
            WarehouseItemId = itemId,
            TransactionCode = code,
            Type = type,
            Status = TransactionStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid(),
            Warehouse = new Warehouse { Name = "Kho Cà Phê" },
            WarehouseItem = new WarehouseItem { Unit = "Kg", StockTransactions = [] }
        };
    }
}