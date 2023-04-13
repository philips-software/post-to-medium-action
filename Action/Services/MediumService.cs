using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using PostMediumGitHubAction.Domain;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PostMediumGitHubAction.Services;

public class MediumService : IMediumService
{
    private readonly IConfigureService _configureService = new ConfigureService();
    private readonly HttpClient _httpClient;
    private Settings _settings;
    private const string ApiMediumUrl = "https://api.medium.com/v1/";

    public MediumService(Settings settings) : this(settings, new HttpClient())
    {
    }

    public MediumService(Settings settings, HttpClient httpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiMediumUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.IntegrationToken}");
    }

    public async Task<MediumCreatedPost> SubmitNewContentAsync()
    {
        User user = await GetCurrentMediumUserAsync();
        Publication pub = null;
        if (!string.IsNullOrEmpty(_settings.PublicationName) || !string.IsNullOrEmpty(_settings.PublicationId))
        {
            pub = await FindMatchingPublicationAsync(user.Id, _settings.PublicationName,
                _settings.PublicationId);
            if (pub == null)
                throw new ArgumentException("Could not find publication, did you enter the correct name or id?");
        }

        if (!string.IsNullOrEmpty(_settings.File))
            _settings.Content = await ReadFileFromPath(_settings.File);
        if (_settings.ContentFormat == "markdown" && _settings.ParseFrontmatter)
            await ParseFrontmatter(_settings.Content);

        _configureService.CheckForValidSettings(_settings);

        // Ensure lower case, API is case sensitive sadly
        _settings.License = _settings.License?.ToLower();
        _settings.PublishStatus = _settings.PublishStatus?.ToLower();
        _settings.ContentFormat = _settings.ContentFormat?.ToLower();
        if (pub != null)
            return await CreateNewPostUnderPublicationAsync(pub.Id);

        return await CreateNewPostWithoutPublicationAsync(user.Id);
    }

    /// <summary>
    ///     Parse markdown content and look for frontmatter.
    ///     Convert the markdown into HTML to remove the frontmatter.
    /// </summary>
    /// <param name="content">Content in markdown</param>
    /// <returns></returns>
    public async Task ParseFrontmatter(string content)
    {
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        StringWriter writer = new();
        HtmlRenderer renderer = new(writer);
        pipeline.Setup(renderer);

        MarkdownDocument document = Markdown.Parse(content, pipeline);

        // extract the front matter from markdown document
        YamlFrontMatterBlock yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

        if (yamlBlock != null)
        {
            string yaml = yamlBlock.Lines.ToString();

            // Define a list of naming conventions to support
            INamingConvention[] supportedNamingConventions = new INamingConvention[]
            {
                PascalCaseNamingConvention.Instance,
                UnderscoredNamingConvention.Instance,
                CamelCaseNamingConvention.Instance,
                NullNamingConvention.Instance,
                HyphenatedNamingConvention.Instance
            };

            foreach (INamingConvention namingConvention in supportedNamingConventions)
            {
                // Deserialize YAML with the current naming convention
                IDeserializer deserializer = new DeserializerBuilder()
                    .WithNamingConvention(namingConvention)
                    .IgnoreUnmatchedProperties()
                    .Build();
                Settings metadata = deserializer.Deserialize<Settings>(yaml);

                // Override settings with the deserialized metadata
                _settings = _configureService.OverrideSettings(_settings, metadata);
            }

            // finally we can render the markdown content as html if necessary
            renderer.Render(document);
            await writer.FlushAsync();
            string html = writer.ToString();
            _settings.Content = html;
            _settings.ContentFormat = "html";
        }
    }

    /// <summary>
    ///     Retrieves current authenticated user
    /// </summary>
    /// <returns>Medium User</returns>
    public async Task<User> GetCurrentMediumUserAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("me").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        User user = JsonSerializer.Deserialize<User>(
            JsonDocument.Parse(
                    await response.Content.ReadAsByteArrayAsync())
                .RootElement.EnumerateObject().First().Value.ToString());
        return user;
    }

    /// <summary>
    ///     Retrieve matching publication
    /// </summary>
    /// <param name="mediumUserId">Id of the Medium User</param>
    /// <param name="publicationToLookFor">Name of the Publication to look for</param>
    /// <param name="publicationId">Optional Id of the publication</param>
    /// <returns>Medium Publication</returns>
    public async Task<Publication> FindMatchingPublicationAsync(string mediumUserId, string publicationToLookFor,
        string publicationId)
    {
        if (string.IsNullOrWhiteSpace(mediumUserId))
            throw new ArgumentException("Missing, null or empty parameter", nameof(mediumUserId));

        if (string.IsNullOrWhiteSpace(publicationId) && string.IsNullOrWhiteSpace(publicationToLookFor))
            throw new ArgumentException("Missing, null or empty parameter", nameof(publicationToLookFor));

        return await RetrievePublicationAsync(mediumUserId, publicationToLookFor, publicationId);
    }

    private async Task<Publication> RetrievePublicationAsync(string mediumUserId, string publicationToLookFor,
        string publicationId)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync($"users/{mediumUserId}/publications").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        Publication[] publications = JsonSerializer.Deserialize<Publication[]>(
            JsonDocument.Parse(
                    await response.Content.ReadAsByteArrayAsync())
                .RootElement.EnumerateObject().First().Value.ToString());
        if (publications == null) return null;
        foreach (Publication pub in publications)
        {
            if (!string.IsNullOrEmpty(publicationToLookFor) && pub.Name == publicationToLookFor) return pub;

            if (publicationId != null && pub.Id == publicationId) return pub;
        }

        return null;
    }

    /// <summary>
    ///     Create a new post under a publication
    /// </summary>
    /// <param name="publicationId">The id of the publication</param>
    /// <returns>Medium Created Post</returns>
    public async Task<MediumCreatedPost> CreateNewPostUnderPublicationAsync(string publicationId)
    {
        return await CreateNewPostAsync($"publications/{publicationId}/posts");
    }

    /// <summary>
    ///     Create a new post without a publication.
    /// </summary>
    /// <param name="authorId">The id of the author</param>
    /// <returns>Medium Created Post</returns>
    public async Task<MediumCreatedPost> CreateNewPostWithoutPublicationAsync(string authorId)
    {
        return await CreateNewPostAsync($"users/{authorId}/posts");
    }

    /// <summary>
    ///     Create a new post for either a publication or author
    /// </summary>
    /// <param name="requestUri">The uri of the endpoint</param>
    /// <returns>Medium Created Post</returns>
    public async Task<MediumCreatedPost> CreateNewPostAsync(string requestUri)
    {
        Post post = new()
        {
            Content = _settings.Content,
            ContentFormat = _settings.ContentFormat,
            PublishStatus = _settings.PublishStatus,
            Tags = _settings.Tags.ToArray(),
            Title = _settings.Title,
            CanonicalUrl = _settings.CanonicalUrl,
            License = _settings.License
        };
        HttpResponseMessage response = await _httpClient.PostAsync($"{requestUri}",
                new StringContent(JsonSerializer.Serialize(post), Encoding.UTF8, "application/json"))
            .ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            MediumError[] errors = JsonSerializer.Deserialize<MediumError[]>(
                JsonDocument.Parse(
                        await response.Content.ReadAsByteArrayAsync())
                    .RootElement.EnumerateObject()
                    .First().Value.ToString());
            if (errors != null)
                throw new HttpRequestException("Something went wrong when posting: " + errors[0].Message);
        }

        MediumCreatedPost createdPostData = JsonSerializer.Deserialize<MediumCreatedPost>(
            JsonDocument.Parse(
                    await response.Content.ReadAsByteArrayAsync())
                .RootElement.EnumerateObject().First().Value.ToString());
        return createdPostData;
    }

    /// <summary>
    ///     Reads a file from path and returns result
    /// </summary>
    /// <param name="filePath">Path of the file</param>
    /// <returns>String of content</returns>
    private static async Task<string> ReadFileFromPath(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException();
        string result = await File.ReadAllTextAsync(filePath);
        // TODO: Parse any urls found in the content and do something with it like migrating URLS or Uploading images
        return result;
    }

    /// <summary>
    ///     Sets the output variables in the GitHub workflow
    ///     See:
    ///     https://docs.github.com/en/actions/learn-github-actions/workflow-commands-for-github-actions#setting-an-output-parameter
    /// </summary>
    /// <param name="post">Newly created post that is used to set output variables</param>
    /// <returns></returns>
    public void SetWorkflowOutputs(MediumCreatedPost post)
    {
        if (post == null) throw new ArgumentNullException(nameof(post), "Medium Post is null");
        Console.WriteLine($"::set-output name=id::{post.Id}");
        Console.WriteLine($"::set-output name=authorId::{post.AuthorId}");
        Console.WriteLine($"::set-output name=canonicalUrl::{post.CanonicalUrl}");
        Console.WriteLine($"::set-output name=license::{post.License}");
        Console.WriteLine($"::set-output name=licenseUrl::{post.LicenseUrl}");
        Console.WriteLine($"::set-output name=publicationId::{post.PublicationId}");
        Console.WriteLine($"::set-output name=publicationStatus::{post.PublishStatus}");
        Console.WriteLine($"::set-output name=title::{post.Title}");
        Console.WriteLine($"::set-output name=tags::{string.Join(",", post.Tags)}");
        Console.WriteLine($"::set-output name=url::{post.Url}");
    }
}