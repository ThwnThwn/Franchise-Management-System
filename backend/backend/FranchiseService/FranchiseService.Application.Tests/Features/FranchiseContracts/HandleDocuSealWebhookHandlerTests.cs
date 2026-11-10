using FluentAssertions;
using FranchiseService.Application.Dtos.Image;
using FranchiseService.Application.Features.FranchiseContracts.Commands.HandleDocuSealWebhook;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace FranchiseService.Application.Tests.Features.FranchiseContracts;

public class HandleDocuSealWebhookHandlerTests
{
    private readonly Mock<IFranchiseContractRepository> _contractRepoMock = new();
    private readonly Mock<IDocuSealService> _docuSealMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<ILogger<HandleDocuSealWebhookHandler>> _loggerMock = new();

    [Fact]
    public async Task Handle_ShouldIgnoreEvent_WhenEventTypeIsNotFormCompleted()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(
            new HandleDocuSealWebhookCommand
            {
                EventType = "form.viewed",
                SubmissionId = 123
            },
            CancellationToken.None);

        result.Should().BeTrue();
        _contractRepoMock.Verify(
            x => x.GetByDocusealSubmissionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenContractNotFound()
    {
        _contractRepoMock
            .Setup(x => x.GetByDocusealSubmissionAsync(701, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FranchiseContract?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new HandleDocuSealWebhookCommand
            {
                EventType = "form.completed",
                SubmissionId = 701
            },
            CancellationToken.None);

        result.Should().BeTrue();
        _contractRepoMock.Verify(x => x.UpdateAsync(It.IsAny<FranchiseContract>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkip_WhenContractAlreadyPastAwaitingSignature()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.Active,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };

        _contractRepoMock
            .Setup(x => x.GetByDocusealSubmissionAsync(801, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new HandleDocuSealWebhookCommand
            {
                EventType = "FORM.COMPLETED",
                SubmissionId = 801
            },
            CancellationToken.None);

        result.Should().BeTrue();
        _docuSealMock.Verify(x => x.GetSubmissionPdfUrlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _contractRepoMock.Verify(x => x.UpdateAsync(It.IsAny<FranchiseContract>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUploadPdfAndUpdateContract_WhenPayloadContainsDocumentUrl()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.AwaitingSignature,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };

        _contractRepoMock
            .Setup(x => x.GetByDocusealSubmissionAsync(901, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _imageServiceMock
            .Setup(x => x.UploadFileFromUrlAsync("https://docuseal.example/signed.pdf", It.IsAny<string>(), "Franchise_Contracts"))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://cloudinary.cafe.vn/contracts/fr-901.pdf",
                PublicId = "fr_901"
            });

        _contractRepoMock
            .Setup(x => x.UpdateAsync(contract))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new HandleDocuSealWebhookCommand
            {
                EventType = "form.completed",
                SubmissionId = 901,
                DocumentUrl = "https://docuseal.example/signed.pdf"
            },
            CancellationToken.None);

        result.Should().BeTrue();
        contract.ContractPdfUrl.Should().Be("https://cloudinary.cafe.vn/contracts/fr-901.pdf");
        contract.Status.Should().Be(ContractStatus.AwaitingPayment);

        _docuSealMock.Verify(x => x.GetSubmissionPdfUrlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _contractRepoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFallbackToOriginalUrl_WhenCloudUploadFails()
    {
        var contract = new FranchiseContract
        {
            Id = Guid.NewGuid(),
            Status = ContractStatus.AwaitingSignature,
            Application = new FranchiseApplication { Contracts = new List<FranchiseContract>() }
        };

        _contractRepoMock
            .Setup(x => x.GetByDocusealSubmissionAsync(1001, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _docuSealMock
            .Setup(x => x.GetSubmissionPdfUrlAsync(1001, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://docuseal.example/fallback.pdf");

        _imageServiceMock
            .Setup(x => x.UploadFileFromUrlAsync("https://docuseal.example/fallback.pdf", It.IsAny<string>(), "Franchise_Contracts"))
            .ThrowsAsync(new InvalidOperationException("Cloudinary timeout"));

        _contractRepoMock
            .Setup(x => x.UpdateAsync(contract))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new HandleDocuSealWebhookCommand
            {
                EventType = "form.completed",
                SubmissionId = 1001,
                DocumentUrl = null
            },
            CancellationToken.None);

        result.Should().BeTrue();
        contract.ContractPdfUrl.Should().Be("https://docuseal.example/fallback.pdf");
        contract.Status.Should().Be(ContractStatus.AwaitingPayment);

        _docuSealMock.Verify(x => x.GetSubmissionPdfUrlAsync(1001, It.IsAny<CancellationToken>()), Times.Once);
        _contractRepoMock.Verify(x => x.UpdateAsync(contract), Times.Once);
    }

    private HandleDocuSealWebhookHandler CreateHandler()
    {
        return new HandleDocuSealWebhookHandler(
            _contractRepoMock.Object,
            _docuSealMock.Object,
            _imageServiceMock.Object,
            _loggerMock.Object);
    }
}
