
using BMGInsaat.Application.DTOs.About;
using BMGInsaat.Application.Interfaces;
using BMGInsaat.Application.Services;
using Moq;
using Xunit;
using BMGInsaat.Domain.Entities;

namespace BMGInsaat.Application.Tests;

public class AboutServiceTests
{
    [Fact]
    public async Task SaveAsync_WhenAboutTrIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var repositoryMock = new Mock<IAboutRepository>();
        var translationMock = new Mock<ITranslationService>();

        var service = new AboutService(
            repositoryMock.Object,
            translationMock.Object);

        var request = new UpdateAboutContentRequest
        {
            AboutTr = "",
            MissionTr = "Sirket misyonu",
            VisionTr = "Sirket vizyonu"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveAsync(request));

        // Assert
        Assert.Equal(
            "Sirket tanitimi bos olamaz.",
            exception.Message);

        repositoryMock.VerifyNoOtherCalls();
        translationMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveAsync_WhenMissionTrIsEmpty_ShouldThrowArgumentException()
    {
        
        //Arrange
        var repositoryMock = new Mock<IAboutRepository>();
        var translationMock = new Mock<ITranslationService>();

        var service = new AboutService(
            repositoryMock.Object,
            translationMock.Object
        );

        var request = new UpdateAboutContentRequest
        {
        AboutTr = "BMG Insaat sirket tanitimi",
        MissionTr = "",
        VisionTr = "Sirket vizyonu"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveAsync(request));

        // Assert
        Assert.Equal(
            "Misyon bos olamaz.",
            exception.Message);

        repositoryMock.VerifyNoOtherCalls();
        translationMock.VerifyNoOtherCalls();
        
    }

    
[Fact]
public async Task SaveAsync_WhenVisionTrIsEmpty_ShouldThrowArgumentException()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new UpdateAboutContentRequest
    {
        AboutTr = "BMG Insaat sirket tanitimi",
        MissionTr = "Sirket misyonu",
        VisionTr = ""
    };

    // Act
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => service.SaveAsync(request));

    // Assert
    Assert.Equal(
        "Vizyon bos olamaz.",
        exception.Message);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task SaveAsync_WhenContentDoesNotExist_ShouldCreateWithTranslations()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    repositoryMock
        .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync((AboutContent?)null);

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Sirket tanitimi", It.IsAny<CancellationToken>()))
        .ReturnsAsync("Company introduction");

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Sirket misyonu", It.IsAny<CancellationToken>()))
        .ReturnsAsync("Company mission");

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Sirket vizyonu", It.IsAny<CancellationToken>()))
        .ReturnsAsync("Company vision");

    AboutContent? savedContent = null;

    repositoryMock
        .Setup(r => r.AddAsync(
            It.IsAny<AboutContent>(),
            It.IsAny<CancellationToken>()))
        .Callback<AboutContent, CancellationToken>(
            (content, _) => savedContent = content)
        .Returns(Task.CompletedTask);

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new UpdateAboutContentRequest
    {
        AboutTr = "Sirket tanitimi",
        MissionTr = "Sirket misyonu",
        VisionTr = "Sirket vizyonu"
    };

    // Act
    await service.SaveAsync(request);

    // Assert
    Assert.NotNull(savedContent);
    Assert.Equal(1, savedContent.Id);

    Assert.Equal("Sirket tanitimi", savedContent.AboutTr);
    Assert.Equal("Company introduction", savedContent.AboutEn);

    Assert.Equal("Sirket misyonu", savedContent.MissionTr);
    Assert.Equal("Company mission", savedContent.MissionEn);

    Assert.Equal("Sirket vizyonu", savedContent.VisionTr);
    Assert.Equal("Company vision", savedContent.VisionEn);

    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<AboutContent>(),
            It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Never);

    translationMock.Verify(
        t => t.TranslateToEnglishAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
        Times.Exactly(3));
}


[Fact]
public async Task SaveAsync_WhenContentExists_ShouldUpdateExistingContent()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    var createdAt = DateTime.UtcNow.AddDays(-1);

    var existingContent = new AboutContent
    {
        Id = 1,
        AboutTr = "Eski tanitim",
        MissionTr = "Eski misyon",
        VisionTr = "Eski vizyon",
        CreatedAtUtc = createdAt
    };

    repositoryMock
        .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingContent);

    repositoryMock
        .Setup(r => r.UpdateAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new UpdateAboutContentRequest
    {
        AboutTr = "Yeni tanitim",
        AboutEn = "New introduction",
        MissionTr = "Yeni misyon",
        MissionEn = "New mission",
        VisionTr = "Yeni vizyon",
        VisionEn = "New vision"
    };

    // Act
    await service.SaveAsync(request);

    // Assert
    Assert.Equal(1, existingContent.Id);

    Assert.Equal("Yeni tanitim", existingContent.AboutTr);
    Assert.Equal("New introduction", existingContent.AboutEn);

    Assert.Equal("Yeni misyon", existingContent.MissionTr);
    Assert.Equal("New mission", existingContent.MissionEn);

    Assert.Equal("Yeni vizyon", existingContent.VisionTr);
    Assert.Equal("New vision", existingContent.VisionEn);

    Assert.Equal(createdAt, existingContent.CreatedAtUtc);
    Assert.NotNull(existingContent.UpdatedAtUtc);

    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<AboutContent>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Once);

    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task SaveAsync_WhenUpdatingWithoutEnglish_ShouldTranslateContent()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    var existingContent = new AboutContent
    {
        Id = 1,
        AboutTr = "Eski tanitim",
        MissionTr = "Eski misyon",
        VisionTr = "Eski vizyon"
    };

    repositoryMock
        .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingContent);

    repositoryMock
        .Setup(r => r.UpdateAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Yeni tanitim", It.IsAny<CancellationToken>()))
        .ReturnsAsync("New introduction");

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Yeni misyon", It.IsAny<CancellationToken>()))
        .ReturnsAsync("New mission");

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            "Yeni vizyon", It.IsAny<CancellationToken>()))
        .ReturnsAsync("New vision");

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new UpdateAboutContentRequest
    {
        AboutTr = "Yeni tanitim",
        MissionTr = "Yeni misyon",
        VisionTr = "Yeni vizyon"
    };

    // Act
    await service.SaveAsync(request);

    // Assert
    Assert.Equal("New introduction", existingContent.AboutEn);
    Assert.Equal("New mission", existingContent.MissionEn);
    Assert.Equal("New vision", existingContent.VisionEn);

    translationMock.Verify(
        t => t.TranslateToEnglishAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
        Times.Exactly(3));

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<AboutContent>(),
            It.IsAny<CancellationToken>()),
        Times.Never);
}


[Fact]
public async Task GetAsync_WhenContentExists_ShouldReturnContent()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    var existingContent = new AboutContent
    {
        Id = 1,
        AboutTr = "BMG Insaat hakkimizda",
        AboutEn = "About BMG Construction",
        MissionTr = "Sirket misyonu",
        MissionEn = "Company mission",
        VisionTr = "Sirket vizyonu",
        VisionEn = "Company vision"
    };

    repositoryMock
        .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingContent);

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.GetAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Same(existingContent, result);

    repositoryMock.Verify(
        r => r.GetAsync(It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task GetAsync_WhenContentDoesNotExist_ShouldReturnNull()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    repositoryMock
        .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync((AboutContent?)null);

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    // Act
    var result = await service.GetAsync();

    // Assert
    Assert.Null(result);

    repositoryMock.Verify(
        r => r.GetAsync(It.IsAny<CancellationToken>()),
        Times.Once);

    repositoryMock.VerifyNoOtherCalls();
    translationMock.VerifyNoOtherCalls();
}


[Fact]
public async Task SaveAsync_WhenTranslationFails_ShouldNotSaveContent()
{
    // Arrange
    var repositoryMock = new Mock<IAboutRepository>();
    var translationMock = new Mock<ITranslationService>();

    translationMock
        .Setup(t => t.TranslateToEnglishAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new HttpRequestException(
            "Translation service unavailable."));

    var service = new AboutService(
        repositoryMock.Object,
        translationMock.Object);

    var request = new UpdateAboutContentRequest
    {
        AboutTr = "Sirket tanitimi",
        MissionTr = "Sirket misyonu",
        VisionTr = "Sirket vizyonu"
    };

    // Act
    await Assert.ThrowsAsync<HttpRequestException>(
        () => service.SaveAsync(request));

    // Assert
    repositoryMock.Verify(
        r => r.AddAsync(
            It.IsAny<AboutContent>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.Verify(
        r => r.UpdateAsync(It.IsAny<CancellationToken>()),
        Times.Never);

    repositoryMock.VerifyNoOtherCalls();

    translationMock.Verify(
        t => t.TranslateToEnglishAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
        Times.Once);
}


}
