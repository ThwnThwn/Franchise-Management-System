using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Commands.Create;
using EngagementService.Application.Persistences.Repository;
using FluentAssertions;
using Moq;

namespace EngagementService.Application.Tests.Features.CampaignTests;

public class CreateCampaginHandlerTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<ICampaignRepository> _campaignRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateCampaginHanlder _handler;

    public CreateCampaginHandlerTests()
    {
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _campaignRepoMock = new Mock<ICampaignRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new CreateCampaginHanlder(
            _voucherRepoMock.Object,
            _campaignRepoMock.Object,
            _mapperMock.Object);
    }

    // 1. Phủ nhánh Exception (Dòng 26-28): Khi vouchers rỗng VÀ số lượng mismatch
    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenVouchersNotFoundAndCountMismatched()
    {
        // Arrange
        var request = new CreateCampaginReq
        {
            Name = "Mùa Xuân 2026",
            Description = "Chiến dịch Tết",
            StartDate = DateTimeOffset.Now,
            EndDate = DateTimeOffset.Now.AddDays(10),
            VoucherIds = new List<Guid> { Guid.NewGuid() }
        };
        var command = new CreateCampaginCommand(request);

        // Mock trả về danh sách rỗng để !vouchers.Any() = true
        _voucherRepoMock.Setup(x => x.GetVoucherByIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new List<Domain.Entities.Voucher>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Vouchers failed to be added into campaign - CreateHandler");
    }

    // 2. Phủ xanh Happy Path (Dòng 29-41): Toàn bộ logic gán Campaign vào Voucher
    [Fact]
    public async Task Handle_ShouldCreateCampaignSuccessfully_WhenAllVouchersExist()
    {
        // Arrange
        var voucherId1 = Guid.NewGuid();
        var voucherId2 = Guid.NewGuid();
        var request = new CreateCampaginReq
        {
            Name = "Tet 2026",
            Description = "Lì xì đầu năm",
            StartDate = DateTimeOffset.Now,
            EndDate = DateTimeOffset.Now.AddDays(7),
            VoucherIds = new List<Guid> { voucherId1, voucherId2 }
        };
        var command = new CreateCampaginCommand(request);

        var voucherEntities = new List<Domain.Entities.Voucher>
        {
            new Domain.Entities.Voucher { Id = voucherId1 },
            new Domain.Entities.Voucher { Id = voucherId2 }
        };

        var campaignEntity = new Domain.Entities.Campaign { Id = Guid.NewGuid() };
        var expectedResponse = new CampaignRes { Id = campaignEntity.Id, Name = request.Name };

        // Mock 1: Trả về đủ 2 voucher để vượt qua IF dòng 26
        _voucherRepoMock.Setup(x => x.GetVoucherByIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(voucherEntities);

        // Mock 2: Map Request -> Entity
        _mapperMock.Setup(x => x.Map<Domain.Entities.Campaign>(request))
            .Returns(campaignEntity);

        // Mock 3: Map Entity -> Response
        _mapperMock.Setup(x => x.Map<CampaignRes>(campaignEntity))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Tet 2026");

        // Kiểm tra logic quan trọng: Voucher phải được gắn CampaignId và Campaign object (Dòng 35-37)
        foreach (var v in voucherEntities)
        {
            v.CampaignId.Should().Be(campaignEntity.Id);
            v.Campaign.Should().Be(campaignEntity);
        }

        _campaignRepoMock.Verify(x => x.AddAsync(campaignEntity), Times.Once);
    }

    // 3. Phủ nhánh phụ: Khi vouchers KHÔNG rỗng nhưng vẫn mismatch (Case IF False)
    [Fact]
    public async Task Handle_ShouldProceed_WhenVouchersArePartiallyFound()
    {
        // Arrange
        var request = new CreateCampaginReq { VoucherIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
        var command = new CreateCampaginCommand(request);

        // Trả về 1 cái -> !vouchers.Any() là FALSE -> Cả IF False (Code chạy tiếp)
        var partialVouchers = new List<Domain.Entities.Voucher> { new Domain.Entities.Voucher { Id = Guid.NewGuid() } };

        _voucherRepoMock.Setup(x => x.GetVoucherByIdsAsync(It.IsAny<Guid[]>())).ReturnsAsync(partialVouchers);
        _mapperMock.Setup(x => x.Map<Domain.Entities.Campaign>(It.IsAny<CreateCampaginReq>())).Returns(new Domain.Entities.Campaign());
        _mapperMock.Setup(x => x.Map<CampaignRes>(It.IsAny<Domain.Entities.Campaign>())).Returns(new CampaignRes());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _campaignRepoMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Campaign>()), Times.Once);
    }
}