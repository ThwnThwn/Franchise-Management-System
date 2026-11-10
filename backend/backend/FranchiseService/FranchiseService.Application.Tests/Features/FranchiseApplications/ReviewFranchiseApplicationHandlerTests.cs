using FluentAssertions;
using FranchiseService.Application.Features.FranchiseApplications.Commands.ReviewFranchiseApplication;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Settings;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseApplications;

public class ReviewFranchiseApplicationHandlerTests
{
    private readonly Mock<IFranchiseApplicationRepository> _appRepoMock = new();
    private readonly Mock<IFranchiseContractRepository> _contractRepoMock = new();
    private readonly Mock<IDocuSealService> _docuSealMock = new();
    private readonly Mock<ILogger<ReviewFranchiseApplicationHandler>> _loggerMock = new();
    private readonly FranchiseSettings _settings = new()
    {
        DefaultDurationYears = 4,
        DefaultRevenueSharePercentage = 6.5m,
        DefaultDepositAmount = 75000000m,
        DefaultFranchiseFee = 120000000m
    };

    [Fact]
    public async Task Handle_ShouldThrow_WhenApplicationNotFound()
    {
        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            Decision = FranchiseApplicationStatus.Rejected,
            ReviewedBy = Guid.NewGuid()
        };

        _appRepoMock
            .Setup(x => x.GetByIdAsync(command.ApplicationId))
            .ReturnsAsync((FranchiseApplication?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenApplicationIsNotPending()
    {
        var application = BuildPendingApplication();
        application.Status = FranchiseApplicationStatus.Approved;

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Rejected,
            ReviewedBy = Guid.NewGuid()
        };

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldRejectApplication_WhenDecisionIsRejected()
    {
        var application = BuildPendingApplication();

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);
        _appRepoMock
            .Setup(x => x.UpdateAsync(application))
            .ReturnsAsync(1);

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Rejected,
            ReviewedBy = Guid.NewGuid(),
            AdminNote = "Vi tri quan sua dac chua dat"
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("từ chối");
        application.Status.Should().Be(FranchiseApplicationStatus.Rejected);
        application.ReviewedBy.Should().Be(command.ReviewedBy);
        _appRepoMock.Verify(x => x.UpdateAsync(application), Times.Once);
        _contractRepoMock.Verify(x => x.AddAsync(It.IsAny<FranchiseContract>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDecisionIsInvalid()
    {
        var application = BuildPendingApplication();

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Pending,
            ReviewedBy = Guid.NewGuid()
        };

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Decision*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTemplateIdIsInvalidForApprove()
    {
        var application = BuildPendingApplication();

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Approved,
            ReviewedBy = Guid.NewGuid(),
            DocuSealTemplateId = 0
        };

        var handler = CreateHandler();
        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DocuSealTemplateId*");
    }

    [Fact]
    public async Task Handle_ShouldApproveAndCreateContract_WhenDocuSealSubmissionSucceeded()
    {
        var application = BuildPendingApplication();
        var contracts = new List<FranchiseContract>();
        Dictionary<string, string>? metadataCaptured = null;

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        _appRepoMock
            .Setup(x => x.UpdateAsync(application))
            .ReturnsAsync(1);

        _contractRepoMock
            .Setup(x => x.AddAsync(It.IsAny<FranchiseContract>()))
            .Callback<FranchiseContract>(contracts.Add)
            .ReturnsAsync(Guid.NewGuid());

        _contractRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<FranchiseContract>()))
            .ReturnsAsync(1);

        _docuSealMock
            .Setup(x => x.CreateSubmissionAsync(
                19,
                application.Email,
                application.FullName,
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, string, string, Dictionary<string, string>?, CancellationToken>((_, _, _, metadata, _) =>
            {
                metadataCaptured = metadata;
            })
            .ReturnsAsync(new DocuSealCreateSubmissionResult
            {
                Id = 7812,
                Status = "sent",
                SignUrl = "https://docuseal.example/sign/7812"
            });

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Approved,
            ReviewedBy = Guid.NewGuid(),
            AdminNote = "Ho so Arabica dat yeu cau",
            DocuSealTemplateId = 19
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ContractId.Should().NotBeNull();
        result.DocuSealSubmissionId.Should().Be(7812);

        contracts.Should().ContainSingle();
        var contract = contracts[0];
        contract.Status.Should().Be(ContractStatus.AwaitingSignature);
        contract.DepositAmount.Should().Be(_settings.DefaultDepositAmount);
        contract.FranchiseFee.Should().Be(_settings.DefaultFranchiseFee);
        contract.RevenueSharePercentage.Should().Be(_settings.DefaultRevenueSharePercentage);
        contract.DocusealSubmissionId.Should().Be(7812);
        contract.Code.Should().StartWith("FR-");

        metadataCaptured.Should().NotBeNull();
        metadataCaptured!["franchise_name"].Should().Be(application.FullName);
        metadataCaptured["franchise_email"].Should().Be(application.Email);
        metadataCaptured["store_address"].Should().Be(application.StoreAddress);

        _appRepoMock.Verify(x => x.UpdateAsync(application), Times.Exactly(2));
        _contractRepoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDocuSealThrowsException()
    {
        var application = BuildPendingApplication();
        var contracts = new List<FranchiseContract>();

        _appRepoMock
            .Setup(x => x.GetByIdAsync(application.Id))
            .ReturnsAsync(application);

        _appRepoMock
            .Setup(x => x.UpdateAsync(application))
            .ReturnsAsync(1);

        _contractRepoMock
            .Setup(x => x.AddAsync(It.IsAny<FranchiseContract>()))
            .Callback<FranchiseContract>(contracts.Add)
            .ReturnsAsync(Guid.NewGuid());

        _contractRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<FranchiseContract>()))
            .ReturnsAsync(1);

        _docuSealMock
            .Setup(x => x.CreateSubmissionAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DocuSeal timeout"));

        var command = new ReviewFranchiseApplicationCommand
        {
            ApplicationId = application.Id,
            Decision = FranchiseApplicationStatus.Approved,
            ReviewedBy = Guid.NewGuid(),
            DocuSealTemplateId = 12,
            DepositAmount = 65000000m,
            FranchiseFee = 130000000m,
            RevenueSharePercentage = 7.2m
        };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("DocuSeal timeout");
        application.Status.Should().Be(FranchiseApplicationStatus.Pending);

        contracts.Should().ContainSingle();
        contracts[0].Status.Should().Be(ContractStatus.Draft);
        _contractRepoMock.Verify(x => x.UpdateAsync(contracts[0]), Times.Once);
    }

    private ReviewFranchiseApplicationHandler CreateHandler()
    {
        return new ReviewFranchiseApplicationHandler(
            _appRepoMock.Object,
            _contractRepoMock.Object,
            _docuSealMock.Object,
            _loggerMock.Object,
            Options.Create(_settings));
    }

    private static FranchiseApplication BuildPendingApplication()
    {
        return new FranchiseApplication
        {
            Id = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            FullName = "Pham Arabica",
            Email = "pham.arabica@cfms.vn",
            Phone = "0909123000",
            IdentityNumber = "079123450000",
            StoreName = "Arabica Reserve",
            StoreAddress = "12 Pho Robusta",
            StoreLatitude = 10.77m,
            StoreLongitude = 106.69m,
            StoreFrontUrl = "https://cdn.cafe.vn/store/arabica-front.jpg",
            Status = FranchiseApplicationStatus.Pending,
            Contracts = new List<FranchiseContract>()
        };
    }
}

