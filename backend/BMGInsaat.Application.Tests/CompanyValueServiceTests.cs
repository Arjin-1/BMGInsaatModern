using BMGInsaat.Application.Interfaces;
using BMGInsaat.Application.Services;
using BMGInsaat.Domain.Entities;
using Moq;
using Xunit;
using BMGInsaat.Application.DTOs.About;

namespace BMGInsaat.Application.Tests;

public class CompanyValueServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenValuesExist_ShouldReturnMappedValues()
    {
        // Arrange
        var repositoryMock = new Mock<ICompanyValueRepository>();
        var translationMock = new Mock<ITranslationService>();

        IReadOnlyList<CompanyValue> values = new List<CompanyValue>
        {
            new CompanyValue
            {
                Id = 1,
                TitleTr = "Guven",
                TitleEn = "Trust",
                DescriptionTr = "Guvenilir hizmet",
                DescriptionEn = "Reliable service",
                DisplayOrder = 1,
                IsPublished = true
            },
            new CompanyValue
            {
                Id = 2,
                TitleTr = "Kalite",
                TitleEn = "Quality",
                DescriptionTr = "Yuksek kalite",
                DescriptionEn = "High quality",
                DisplayOrder = 2,
                IsPublished = false
            }
        };

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(values);

        var service = new CompanyValueService(
            repositoryMock.Object,
            translationMock.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("Guven", result[0].TitleTr);
        Assert.Equal("Trust", result[0].TitleEn);
        Assert.Equal(1, result[0].DisplayOrder);
        Assert.True(result[0].IsPublished);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("Kalite", result[1].TitleTr);
        Assert.Equal("Quality", result[1].TitleEn);
        Assert.Equal(2, result[1].DisplayOrder);
        Assert.False(result[1].IsPublished);

        repositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        translationMock.VerifyNoOtherCalls();
    }
    
[Fact]
public async Task GetByIdAsync_WhenValueExists_ShouldReturnMappedValue()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    var companyValue = new CompanyValue
    {
        Id = 5,
        TitleTr = "Surdurulebilirlik",
        TitleEn = "Sustainability",
        DescriptionTr = "Cevreye duyarli projeler",
        DescriptionEn = "Environmentally responsible projects",
        DisplayOrder = 3,
        IsPublished = true,
        CreatedAtUtc = new DateTime(
            2026, 10, 26, 8, 0, 0, DateTimeKind.Utc)
    };

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(companyValue);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.GetByIdAsync(5);

    // Assert
    Assert.NotNull(result);

    Assert.Equal(5, result.Id);
    Assert.Equal("Surdurulebilirlik", result.TitleTr);
    Assert.Equal("Sustainability", result.TitleEn);
    Assert.Equal(
        "Cevreye duyarli projeler",
        result.DescriptionTr);
    Assert.Equal(
        "Environmentally responsible projects",
        result.DescriptionEn);
    Assert.Equal(3, result.DisplayOrder);
    Assert.True(result.IsPublished);
    Assert.Equal(companyValue.CreatedAtUtc, result.CreatedAtUtc);

    repositoryMock.Verify(
        r => r.GetByIdAsync(
            5, It.IsAny<CancellationToken>()),
        Times.Once);

    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task GetByIdAsync_WhenValueDoesNotExist_ShouldReturnNull()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((CompanyValue?)null);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.GetByIdAsync(999);

    // Assert
    Assert.Null(result);

    repositoryMock.Verify(
        r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()),
        Times.Once);

    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task CreateAsync_WhenEnglishIsEmpty_ShouldTranslateAndSave()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    CompanyValue? savedValue = null;

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Guven",
            It.IsAny<CancellationToken>()))
        .ReturnsAsync("Trust");

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Guvenilir hizmet",
            It.IsAny<CancellationToken>()))
        .ReturnsAsync("Reliable service");

    repositoryMock
        .Setup(r => r.AddAsync(
            It.IsAny<CompanyValue>(),
            It.IsAny<CancellationToken>()))
        .Callback<CompanyValue, CancellationToken>(
            (value, _) =>
            {
                savedValue = value;
                value.Id = 10;
            })
        .Returns(Task.CompletedTask);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "  Guven  ",
        DescriptionTr = "  Guvenilir hizmet  ",
        TitleEn = "",
        DescriptionEn = "",
        DisplayOrder = 1,
        IsPublished = true
    };

    // Act
    var result = await service.CreateAsync(request);

    // Assert
    Assert.NotNull(savedValue);

    Assert.Equal("Guven", savedValue.TitleTr);
    Assert.Equal("Trust", savedValue.TitleEn);

    Assert.Equal("Guvenilir hizmet", savedValue.DescriptionTr);
    Assert.Equal("Reliable service", savedValue.DescriptionEn);

    Assert.Equal(1, savedValue.DisplayOrder);
    Assert.True(savedValue.IsPublished);

    Assert.Equal(10, result.Id);
    Assert.Equal("Trust", result.TitleEn);
    Assert.Equal("Reliable service", result.DescriptionEn);

    repositoryMock.Verify(r => r.AddAsync(
        It.IsAny<CompanyValue>(),
        It.IsAny<CancellationToken>()), Times.Once);

    translationMock.Verify(t => t.TranslateToEnglishAsync(
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()), Times.Exactly(2));
}


[Fact]
public async Task CreateAsync_WhenTitleTrIsEmpty_ShouldNotSave()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "   ",
        DescriptionTr = "Guvenilir hizmet",
        DisplayOrder = 1,
        IsPublished = true
    };

    // Act
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => service.CreateAsync(request));

    // Assert
    Assert.Equal(
        "Deger basligi bos olamaz.",
        exception.Message);

    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<CompanyValue>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task CreateAsync_WhenDescriptionTrIsEmpty_ShouldNotSave()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "Kalite",
        DescriptionTr = "   ",
        DisplayOrder = 2,
        IsPublished = true
    };

    // Act
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => service.CreateAsync(request));

    // Assert
    Assert.Equal(
        "Deger aciklamasi bos olamaz.",
        exception.Message);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task CreateAsync_WhenTranslationFails_ShouldNotSave()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(
            new HttpRequestException("Translation service unavailable."));

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "Guven",
        DescriptionTr = "Guvenilir hizmet",
        TitleEn = "",
        DescriptionEn = "",
        DisplayOrder = 1,
        IsPublished = true
    };

    // Act
    await Assert.ThrowsAsync<HttpRequestException>(
        () => service.CreateAsync(request));

    // Assert
    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<CompanyValue>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.VerifyNoOtherCalls();

    translationMock.Verify(
        t => t.TranslateToEnglishAsync(
            "Guven",
            It.IsAny<CancellationToken>()),
        Times.Once);
}


[Fact]
public async Task UpdateAsync_WhenValueExists_ShouldUpdateSuccessfully()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    var createdAt = new DateTime(
        2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

    var companyValue = new CompanyValue
    {
        Id = 5,
        TitleTr = "Eski Baslik",
        TitleEn = "Old Title",
        DescriptionTr = "Eski aciklama",
        DescriptionEn = "Old description",
        DisplayOrder = 1,
        IsPublished = false,
        CreatedAtUtc = createdAt
    };

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(companyValue);

    repositoryMock
        .Setup(r => r.UpdateAsync(
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "Kalite",
        TitleEn = "Quality",
        DescriptionTr = "Yuksek kalite",
        DescriptionEn = "High quality",
        DisplayOrder = 2,
        IsPublished = true
    };

    // Act
    var result = await service.UpdateAsync(5, request);

    // Assert
    Assert.True(result);

    Assert.Equal("Kalite", companyValue.TitleTr);
    Assert.Equal("Quality", companyValue.TitleEn);

    Assert.Equal("Yuksek kalite", companyValue.DescriptionTr);
    Assert.Equal("High quality", companyValue.DescriptionEn);

    Assert.Equal(2, companyValue.DisplayOrder);
    Assert.True(companyValue.IsPublished);

    Assert.Equal(createdAt, companyValue.CreatedAtUtc);
    Assert.NotNull(companyValue.UpdatedAtUtc);
    Assert.True(companyValue.UpdatedAtUtc > createdAt);

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Once);

    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task UpdateAsync_WhenValueDoesNotExist_ShouldReturnFalse()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((CompanyValue?)null);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new SaveCompanyValueRequest
    {
        TitleTr = "Guven",
        DescriptionTr = "Guvenilir hizmet",
        DisplayOrder = 1,
        IsPublished = true
    };

    // Act
    var result = await service.UpdateAsync(999, request);

    // Assert
    Assert.False(result);

    repositoryMock.Verify(
        r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task DeleteAsync_WhenValueExists_ShouldDeleteSuccessfully()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    var companyValue = new CompanyValue
    {
        Id = 5,
        TitleTr = "Guven",
        TitleEn = "Trust",
        DescriptionTr = "Guvenilir hizmet",
        DescriptionEn = "Reliable service",
        DisplayOrder = 1,
        IsPublished = true
    };

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            5, It.IsAny<CancellationToken>()))
        .ReturnsAsync(companyValue);

    repositoryMock
        .Setup(r => r.DeleteAsync(
            It.IsAny<CompanyValue>(),
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.DeleteAsync(5);

    // Assert
    Assert.True(result);

    repositoryMock.Verify(
        r => r.GetByIdAsync(
            5, It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.DeleteAsync(
            It.Is<CompanyValue>(v => v.Id == 5),
            It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task DeleteAsync_WhenValueDoesNotExist_ShouldReturnFalse()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    repositoryMock
        .Setup(r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((CompanyValue?)null);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.DeleteAsync(999);

    // Assert
    Assert.False(result);

    repositoryMock.Verify(
        r => r.GetByIdAsync(
            999, It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.DeleteAsync(
            It.IsAny<CompanyValue>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task GetPublishedAsync_ShouldReturnMappedPublishedValues()
{
    // Arrange
    var repositoryMock = new Mock<ICompanyValueRepository>();
    var translationMock = new Mock<ITranslationService>();

    IReadOnlyList<CompanyValue> publishedValues = new List<CompanyValue>
    {
        new CompanyValue
        {
            Id = 1,
            TitleTr = "Guven",
            TitleEn = "Trust",
            DescriptionTr = "Guvenilir hizmet",
            DescriptionEn = "Reliable service",
            DisplayOrder = 1,
            IsPublished = true
        },
        new CompanyValue
        {
            Id = 3,
            TitleTr = "Kalite",
            TitleEn = "Quality",
            DescriptionTr = "Kaliteli projeler",
            DescriptionEn = "Quality projects",
            DisplayOrder = 2,
            IsPublished = true
        }
    };

    repositoryMock
        .Setup(r => r.GetPublishedAsync(
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(publishedValues);

    var service = new CompanyValueService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.GetPublishedAsync();

    // Assert
    Assert.Equal(2, result.Count);

    Assert.Equal("Guven", result[0].TitleTr);
    Assert.Equal("Trust", result[0].TitleEn);
    Assert.True(result[0].IsPublished);

    Assert.Equal("Kalite", result[1].TitleTr);
    Assert.Equal("Quality", result[1].TitleEn);
    Assert.True(result[1].IsPublished);

    repositoryMock.Verify(
        r => r.GetPublishedAsync(
            It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.GetAllAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);

    translationMock.VerifyNoOtherCalls();
}


}