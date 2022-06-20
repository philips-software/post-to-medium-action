using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PostMediumGitHubAction;
using PostMediumGitHubAction.Domain;
using PostMediumGitHubAction.Services;

namespace Tests
{
    public class MediumServiceTests
    {
        private string integrationToken = "";

        [SetUp]
        public void BeforeEach()
        {
        }

        [Test]
        public async Task GetCurrentMediumUserAsync_ShouldReturnCurrentMediumUser()
        {
            
            ConfigureService configureService = new ConfigureService();
            string[] args = new string[] { "--integration-token", integrationToken };
            
            Settings configuredSettings = configureService.ConfigureApplication(args);
            MediumService service = new MediumService(configuredSettings);
            User user = await service.GetCurrentMediumUserAsync();
            Assert.AreEqual(user.Id, "1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5");
        }

        [Test]
        public void GetCurrentMediumUserAsync_WithInvalidToken_ShouldReturnException()
        {
            ConfigureService configureService = new ConfigureService();
            string[] args = new string[] { "-t", integrationToken };

            Settings configuredSettings = configureService.ConfigureApplication(args);
            MediumService service = new MediumService(configuredSettings);
            HttpRequestException ex = Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetCurrentMediumUserAsync());
            Assert.That(ex!.Message, Is.EqualTo("Response status code does not indicate success: 401 (Unauthorized)."));
        }

        [Test]
        public void FindMatchingPublicationAsync_WithMissingOrEmptyParameters_ShouldReturnException()
        {
            ConfigureService configureService = new ConfigureService();
            string[] args = new string[] { "-t", integrationToken };

            Settings configuredSettings = configureService.ConfigureApplication(args);
            MediumService service = new MediumService(configuredSettings);
            ArgumentException ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.FindMatchingPublicationAsync(null, null, null));
            Assert.That(ex!.Message, Is.EqualTo("Missing, null or empty parameter (Parameter 'mediumUserId')"));

            ArgumentException ex2 = Assert.ThrowsAsync<ArgumentException>(async () => await service.FindMatchingPublicationAsync("user-id", "", ""));
            Assert.That(ex2!.Message, Is.EqualTo("Missing, null or empty parameter (Parameter 'publicationToLookFor')"));
        }

        [Test]
        public void FindMatchingPublicationAsync_WithParameters_ShouldNotReturnException()
        {
            ConfigureService configureService = new ConfigureService();
            string[] args = new string[] { "-t", integrationToken };

            Settings configuredSettings = configureService.ConfigureApplication(args);
            MediumService service = new MediumService(configuredSettings);

            //TODO: Mock API Call
            Assert.DoesNotThrowAsync(async () => await service.FindMatchingPublicationAsync("user-id", "something", null));
            Assert.DoesNotThrowAsync(async () => await service.FindMatchingPublicationAsync("user-id", "", "something"));
        }

        [Test]
        public void SubmitNewContentAsync_WithInvalidPublication_ShouldReturnException()
        {
            // MediumService service = new MediumService(integrationToken);
            Mock<MediumService> mediumServiceMock = new Mock<MediumService>(integrationToken);
            MediumService service = mediumServiceMock.Object;
            mediumServiceMock.Setup(service => service.FindMatchingPublicationAsync("user-id", "", ""))
                .ReturnsAsync(() => null);

            Exception ex = Assert.ThrowsAsync<Exception>(async () => await service.SubmitNewContentAsync());
            Assert.That(ex!.Message, Is.EqualTo("Could not find publication, did you enter the correct name or id?"));
        }

        [Test]
        public void SubmitNewContentAsync_WithAuthor_ShouldCreatePostAsAuthor()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithPublication_ShouldCreatePostAsPublication()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithParseFrontmatterAndMarkdown_ShouldParseFrontmatter()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithParseFrontmatterWithoutMarkdown_ShouldNotParseFrontmatter()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithCorrectFilePath_ShouldHaveValidSettings()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithIncorrectFilePath_ShouldReturnException()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithUpperCaseSettings_ShouldMigrateToLower()
        {

        }

        [Test]
        public void SubmitNewContentAsync_WithSuccessfullCreation_ShouldSetCorrectWorkflowOutputs()
        {

        }

        [Test]
        public void SetWorkflowOutputs_WithMediumPost_ShouldLogCorrectOutputs()
        {

        }

        [Test]
        public void SetWorkflowOutputs_WithoutMediumPost_ShouldReturnException()
        {

        }

    }
}
