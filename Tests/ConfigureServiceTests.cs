using System;
using AutoFixture;
using NUnit.Framework;
using PostMediumGitHubAction.Services;

namespace PostMediumGitHubAction.Tests
{
    public class ConfigureServiceTests
    {
        private Fixture _fixture;
        private ConfigureService _configureService;
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

            settings.Content = null;
            Assert.DoesNotThrow(() => _configureService.CheckForValidSettings(settings));

            settings.File = null;
            Assert.Throws<ArgumentNullException>(() => _configureService.CheckForValidSettings(settings));

            settings.Content = "bla";
            Assert.DoesNotThrow(() => _configureService.CheckForValidSettings(settings));
        }
    }
}
