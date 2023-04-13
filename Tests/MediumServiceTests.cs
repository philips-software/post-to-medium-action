using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PostMediumGitHubAction.Domain;
using PostMediumGitHubAction.Services;

namespace Tests;

public class MediumServiceTests
{
    private readonly HttpResponseMessage _successfullCurrentMediumUserHttpResponse = new()
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(
            "{\"data\":{\"id\":\"1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5\",\"username\":\"some-username\",\"name\":\"some-name\",\"url\":\"https://medium.com/@philips\",\"imageUrl\":\"https://some-url.com\"}}")
    };

    private readonly HttpResponseMessage _successfullGetPublicationsFromMediumUserHttpResponse = new()
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(
            "{\n    \"data\": [\n        {\n            \"id\": \"28ccdb7d334d\",\n            \"name\": \"Philips Experience Design Blog\",\n            \"description\": \"Learn more how we reimagine healthcare in transformative ways to ensure care is provided to those most in need.\",\n            \"url\": \"https://medium.com/philips-experience-design-blog\",\n            \"imageUrl\": \"https://cdn-images-1.medium.com/fit/c/400/400/1*wcWtJZ6UHni4n1vnRf5kpQ.jpeg\"\n        }]}")
    };

    private static Mock<HttpMessageHandler> MockMediumCall(HttpResponseMessage response)
    {
        return MockMediumCall(null, response, "");
    }

    private static Mock<HttpMessageHandler> MockMediumCall(HttpResponseMessage response, string absoluteUrlToMatch)
    {
        return MockMediumCall(null, response, absoluteUrlToMatch);
    }

    private static Mock<HttpMessageHandler> MockMediumCall(Mock<HttpMessageHandler>? handlerMock,
        HttpResponseMessage response, string absoluteUrlToMatch)
    {
        handlerMock ??= new Mock<HttpMessageHandler>();

        if (!string.IsNullOrWhiteSpace(absoluteUrlToMatch))
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x =>
                        x.RequestUri != null && x.RequestUri.AbsoluteUri.Contains(absoluteUrlToMatch)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        else
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        return handlerMock;
    }

    [Test]
    public async Task GetCurrentMediumUserAsync_ShouldReturnCurrentMediumUser()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args = { "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown" };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        Mock<HttpMessageHandler> handlerMock = MockMediumCall(_successfullCurrentMediumUserHttpResponse);
        IMediumService service =
            new MediumService(configuredSettings, new HttpClient(handlerMock.Object));

        User user = await service.GetCurrentMediumUserAsync();
        Assert.AreEqual("1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5", user.Id);
    }

    [Test]
    public void GetCurrentMediumUserAsync_WithInvalidToken_ShouldReturnException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args = { "-t", "invalidToken", "-e", "some-title", "-a", "tag", "-o", "markdown" };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        HttpResponseMessage response = new()
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("{\"errors\":[{\"message\":\"Token was invalid.\",\"code\":6003}]}")
        };
        Mock<HttpMessageHandler> handlerMock = MockMediumCall(response);

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));
        HttpRequestException? ex =
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetCurrentMediumUserAsync());
        Assert.That(ex!.Message, Is.EqualTo("Response status code does not indicate success: 401 (Unauthorized)."));
    }

    [Test]
    public void FindMatchingPublicationAsync_WithMissingOrEmptyParameters_ShouldReturnException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args = { "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown" };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        MediumService service = new(configuredSettings);
        ArgumentException? ex =
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.FindMatchingPublicationAsync(null, null, null));
        Assert.That(ex!.Message, Is.EqualTo("Missing, null or empty parameter (Parameter 'mediumUserId')"));

        ArgumentException? ex2 = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.FindMatchingPublicationAsync("user-id", "", ""));
        Assert.That(ex2!.Message, Is.EqualTo("Missing, null or empty parameter (Parameter 'publicationToLookFor')"));
    }

    [Test]
    public async Task FindMatchingPublicationAsync_WithParameters_ShouldNotReturnException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args = { "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown" };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        HttpResponseMessage response = new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                "{\n    \"data\": [\n        {\n            \"id\": \"28ccdb7d334d\",\n            \"name\": \"Philips Experience Design Blog\",\n            \"description\": \"Learn more how we reimagine healthcare in transformative ways to ensure care is provided to those most in need.\",\n            \"url\": \"https://medium.com/philips-experience-design-blog\",\n            \"imageUrl\": \"https://cdn-images-1.medium.com/fit/c/400/400/1*wcWtJZ6UHni4n1vnRf5kpQ.jpeg\"\n        }]}")
        };
        Mock<HttpMessageHandler> handlerMock = MockMediumCall(response);

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.DoesNotThrowAsync(async () => await service.FindMatchingPublicationAsync("user-id", "something", null));
        Assert.DoesNotThrowAsync(async () => await service.FindMatchingPublicationAsync("user-id", "", "something"));
        Publication pub = await service.FindMatchingPublicationAsync("user-id", "", "28ccdb7d334d");
        Assert.AreEqual("28ccdb7d334d", pub.Id);

        pub = await service.FindMatchingPublicationAsync("user-id", "Philips Experience Design Blog", "");
        Assert.AreEqual("Philips Experience Design Blog", pub.Name);
    }

    [Test]
    public void SubmitNewContentAsync_WithInvalidPublication_ShouldReturnException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
            { "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "-n", "some-unfindable-name" };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        Mock<HttpMessageHandler> handlerMock =
            MockMediumCall(_successfullCurrentMediumUserHttpResponse, "https://api.medium.com/v1/me");

        handlerMock = MockMediumCall(handlerMock, _successfullGetPublicationsFromMediumUserHttpResponse,
            "https://api.medium.com/v1/users/1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5/publications");

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        ArgumentException? ex =
            Assert.ThrowsAsync<ArgumentException>(async () => await service.SubmitNewContentAsync());
        Assert.That(ex!.Message, Is.EqualTo("Could not find publication, did you enter the correct name or id?"));
    }

    [Test]
    public void SubmitNewContentAsync_WithMediumApiReturningErrors_ShouldReturnHttpRequestException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--content", "Content"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        Mock<HttpMessageHandler> handlerMock =
            MockMediumCall(_successfullCurrentMediumUserHttpResponse, "https://api.medium.com/v1/me");

        handlerMock = MockMediumCall(handlerMock, _successfullGetPublicationsFromMediumUserHttpResponse,
            "https://api.medium.com/v1/users/1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5/publications");

        handlerMock = MockMediumCall(handlerMock, new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(
                "{\n    \"errors\": [\n        {\n            \"message\": \"Invalid contentFormat specified: markdown,\",\n            \"code\": 2003\n        }\n    ]\n}")
        }, "https://api.medium.com/v1/publications/28ccdb7d334d/posts");

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        HttpRequestException? ex =
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SubmitNewContentAsync());
        Assert.That(ex!.Message,
            Is.EqualTo("Something went wrong when posting: Invalid contentFormat specified: markdown,"));
    }

    [Test]
    public async Task SubmitNewContentAsync_WithAuthor_ShouldCreatePostAsAuthor()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
            { "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "--content", "Content" };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        Mock<HttpMessageHandler> handlerMock =
            MockMediumCall(_successfullCurrentMediumUserHttpResponse, "https://api.medium.com/v1/me");

        handlerMock = MockMediumCall(handlerMock, new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created,
            Content = new StringContent(
                "{ \"data\": {\n        \"id\": \"e855b9f3048a\",\n        \"title\": \"some-content\",\n        \"authorId\": \"1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5\",\n        \"url\": \"https://medium.com/@philips/some-content-e855b9f3048a\",\n        \"canonicalUrl\": \"\",\n        \"publishStatus\": \"\",\n        \"publishedAt\": 1655898280492,\n        \"license\": \"\",\n        \"licenseUrl\": \"https://policy.medium.com/medium-terms-of-service-9db0094a1e0f\",\n        \"tags\": [],\n        \"publicationId\": \"536bc4016034\"\n    }}")
        }, "https://api.medium.com/v1/users/1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5/posts");

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.DoesNotThrowAsync(async () => await service.SubmitNewContentAsync());
        MediumCreatedPost post = await service.SubmitNewContentAsync();
        Assert.AreEqual("1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5", post.AuthorId);
    }

    [Test]
    public async Task SubmitNewContentAsync_WithPublication_ShouldCreatePostAsPublication()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--content", "Content"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        Mock<HttpMessageHandler> handlerMock =
            MockMediumCall(_successfullCurrentMediumUserHttpResponse, "https://api.medium.com/v1/me");

        handlerMock = MockMediumCall(handlerMock, _successfullGetPublicationsFromMediumUserHttpResponse,
            "https://api.medium.com/v1/users/1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5/publications");

        handlerMock = MockMediumCall(handlerMock, new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created,
            Content = new StringContent(
                "{ \"data\": {\n        \"id\": \"e855b9f3048a\",\n        \"title\": \"some-content\",\n        \"authorId\": \"1620245129e9d0ed50b8ebf3416552bed81e7100b561f1ab0f92f071739bc6033\",\n        \"url\": \"https://medium.com/@philips/some-content-e855b9f3048a\",\n        \"canonicalUrl\": \"\",\n        \"publishStatus\": \"\",\n        \"publishedAt\": 1655898280492,\n        \"license\": \"\",\n        \"licenseUrl\": \"https://policy.medium.com/medium-terms-of-service-9db0094a1e0f\",\n        \"tags\": [],\n        \"publicationId\": \"536bc4016034\"\n    }}")
        }, "https://api.medium.com/v1/publications/28ccdb7d334d/posts");

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.DoesNotThrowAsync(async () => await service.SubmitNewContentAsync());
        MediumCreatedPost post = await service.SubmitNewContentAsync();
        Assert.AreEqual("some-content", post.Title);
    }

    [Test]
    public async Task
        SubmitNewContentAsync_WithUnusedAttributesInFrontmatter_ShouldIgnoreUnusedfrontmatterAndCreatePostAsPublication()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--file", "test-with-unused-frontmatter.md", "--parse-frontmatter", "true"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);

        Mock<HttpMessageHandler> handlerMock =
            MockMediumCall(_successfullCurrentMediumUserHttpResponse, "https://api.medium.com/v1/me");

        handlerMock = MockMediumCall(handlerMock, _successfullGetPublicationsFromMediumUserHttpResponse,
            "https://api.medium.com/v1/users/1840a7bacce6d851c032cfb7de25919c500506726fe203254bb43b629755919b5/publications");

        handlerMock = MockMediumCall(handlerMock, new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created,
            Content = new StringContent(
                "{ \"data\": {\n        \"id\": \"e855b9f3048a\",\n        \"title\": \"some-content\",\n        \"authorId\": \"1620245129e9d0ed50b8ebf3416552bed81e7100b561f1ab0f92f071739bc6033\",\n        \"url\": \"https://medium.com/@philips/some-content-e855b9f3048a\",\n        \"canonicalUrl\": \"\",\n        \"publishStatus\": \"\",\n        \"publishedAt\": 1655898280492,\n        \"license\": \"\",\n        \"licenseUrl\": \"https://policy.medium.com/medium-terms-of-service-9db0094a1e0f\",\n        \"tags\": [],\n        \"publicationId\": \"536bc4016034\"\n    }}")
        }, "https://api.medium.com/v1/publications/28ccdb7d334d/posts");

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.DoesNotThrowAsync(async () => await service.SubmitNewContentAsync());
        MediumCreatedPost post = await service.SubmitNewContentAsync();
        Assert.AreEqual("some-content", post.Title);
    }

    [Test]
    public async Task
        SubmitNewContentAsync_WithVariousNamingConventionsInFrontmatter_ShouldHaveProperSettingsAndCreatePostAsPublication()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--file", "test-with-various-naming-conventions.md",
            "--parse-frontmatter", "true"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        configuredSettings.Content =
            "---\nTitle: \"Hello World\"\ndate: 2023-03-05T16:44:36+08:00\npublishdate: 2023-03-20T04:44:36+08:00\ncanonical_url: 'https://some-url.com'\nPublishStatus: 'unlisted'\ntags: ['helloworld']\ncomments: true\ndraft: true\n---\nHello world";
        MediumService service = new(configuredSettings, new HttpClient());

        Assert.DoesNotThrowAsync(async () => await service.ParseFrontmatter(configuredSettings.Content));
        Assert.AreEqual("Hello World", configuredSettings.Title);
        Assert.AreEqual("https://some-url.com", configuredSettings.CanonicalUrl);
        Assert.AreEqual("unlisted", configuredSettings.PublishStatus);
        Assert.AreEqual("helloworld", configuredSettings.Tags.First());
    }

    [Test]
    public void SetWorkflowOutputs_WithMediumPost_ShouldSetOutputs()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--content", "Content"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        Fixture fixture = new();
        MediumCreatedPost fakePost = fixture.Create<MediumCreatedPost>();
        Mock<HttpMessageHandler> handlerMock = new();

        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.DoesNotThrow(() => service.SetWorkflowOutputs(fakePost));
    }

    [Test]
    public void SetWorkflowOutputs_WithoutMediumPost_ShouldReturnException()
    {
        IConfigureService configureService = new ConfigureService();
        string[] args =
        {
            "-t", "validToken", "-e", "some-title", "-a", "tag", "-o", "markdown", "-n",
            "Philips Experience Design Blog", "--content", "Content"
        };
        Settings configuredSettings = configureService.ConfigureApplication(args);
        Mock<HttpMessageHandler> handlerMock = new();
        MediumService service = new(configuredSettings, new HttpClient(handlerMock.Object));

        Assert.Throws<ArgumentNullException>(() => service.SetWorkflowOutputs(null));
    }
}