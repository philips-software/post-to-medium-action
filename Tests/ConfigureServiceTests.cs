using System;
using AutoFixture;
using NUnit.Framework;
using PostMediumGitHubAction.Domain;
using PostMediumGitHubAction.Services;

namespace Tests;

public class ConfigureServiceTests
{
    private Fixture _fixture = null!;
    private ConfigureService _configureService = null!;

    [SetUp]
    public void BeforeEach()
    {
        _fixture = new Fixture();
        _configureService = new ConfigureService();
    }

    [Test]
    public void CheckForValidSettings_WithValidSettings_ShouldNotThrowException()
    {
        Settings settings = _fixture.Create<Settings>();
        settings.ContentFormat = "markdown";

        Assert.DoesNotThrow(() => _configureService.CheckForValidSettings(settings));
    }

    [Test]
    public void CheckForValidSettings_WithInvalidToken_ShouldThrowException()
    {
        Settings settings = _fixture.Create<Settings>();


        settings.IntegrationToken = null;
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));

        settings.IntegrationToken = " ";
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));

        settings.IntegrationToken = "";
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));
    }

    [Test]
    public void CheckForValidSettings_WithInvalidFileAndContent_ShouldThrowException()
    {
        Settings settings = _fixture.Create<Settings>();
        settings.ContentFormat = "markdown";

        settings.Content = null;
        Assert.DoesNotThrow(() => _configureService.CheckForValidSettings(settings));

        settings.File = null;
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));

        settings.Content = "bla";
        Assert.DoesNotThrow(() => _configureService.CheckForValidSettings(settings));
    }

    [Test]
    public void CheckForValidSettings_WithInvalidContentFormat_ShouldThrowException()
    {
        Settings settings = _fixture.Create<Settings>();

        settings.ContentFormat = null;
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));

        settings.ContentFormat = "mark";
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));
    }

    [Test]
    public void CheckForValidSettings_WithInvalidTitle_ShouldThrowException()
    {
        Settings settings = _fixture.Create<Settings>();

        settings.Title = null;
        Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));
    }

    [Test]
    public void OverrideSettings_WithValidSettings_ShouldReplaceValues()
    {
        Settings originalSettings = _fixture.Create<Settings>();
        Settings settingsToReplaceWith = _fixture.Create<Settings>();

        Settings returnedSettings = _configureService.OverrideSettings(originalSettings, settingsToReplaceWith);
        Assert.AreEqual(settingsToReplaceWith.CanonicalUrl, returnedSettings.CanonicalUrl);
        Assert.AreEqual(settingsToReplaceWith.ContentFormat, returnedSettings.ContentFormat);
        Assert.AreEqual(settingsToReplaceWith.Tags, returnedSettings.Tags);
        Assert.AreEqual(settingsToReplaceWith.License, returnedSettings.License);
        Assert.AreEqual(settingsToReplaceWith.PublishStatus, returnedSettings.PublishStatus);
        Assert.AreEqual(settingsToReplaceWith.Title, returnedSettings.Title);
    }
}