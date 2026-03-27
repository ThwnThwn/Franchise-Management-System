using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Validator.Campaign;
using FluentValidation.TestHelper;
using Xunit;
using System;
using System.Collections.Generic;

namespace EngagementService.Application.Tests.ValidatorTests;

public class CreateCampaignValidatorTests
{
    private readonly CreateCampaginValidator _validator;

    public CreateCampaignValidatorTests()
    {
        _validator = new CreateCampaginValidator();
    }

    // 1. Phủ xanh Happy Path: Mọi thứ đều hợp lệ
    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        // Arrange
        var model = new CreateCampaginReq
        {
            Name = "Campaign Tet 2026",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(7),
            VoucherIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // 2. Phủ nhánh: Name trống (NotEmpty)
    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new CreateCampaginReq { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    // 3. Phủ nhánh: StartDate và EndDate trống (NotEmpty)
    [Fact]
    public void Should_Have_Error_When_Dates_Are_Empty()
    {
        var model = new CreateCampaginReq { StartDate = default, EndDate = default };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.StartDate).WithErrorMessage("StartDate is required");
        result.ShouldHaveValidationErrorFor(x => x.EndDate).WithErrorMessage("EndDate is required");
    }

    // 4. Phủ nhánh: EndDate <= StartDate (GreaterThan)
    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Before_StartDate()
    {
        var model = new CreateCampaginReq
        {
            StartDate = DateTime.Now.AddDays(10),
            EndDate = DateTime.Now.AddDays(5) // Vi phạm GreaterThan
        };

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate must be greater than StartDate");
    }

    // 5. Phủ nhánh: VoucherIds null (NotNull)
    [Fact]
    public void Should_Have_Error_When_VoucherIds_Is_Null()
    {
        var model = new CreateCampaginReq { VoucherIds = null! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.VoucherIds)
              .WithErrorMessage("Vouchers cannot be null");
    }
}