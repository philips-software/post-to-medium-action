using System;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace PostMediumGitHubAction.Services
{
    public class MediumService
    {
        private ConfigureService _configureService = new ConfigureService();
        public async Task SubmitNewContentAsync()
        {
            User user = await GetCurrentMediumUserAsync();
            Publication pub = null;
            if (!string.IsNullOrEmpty(Program.Settings.PublicationName) || !string.IsNullOrEmpty(Program.Settings.PublicationId))
            {
                pub = await FindMatchingPublicationAsync(user.Id, Program.Settings.PublicationName,
                    Program.Settings.PublicationId);
                if (pub == null) throw new Exception("Could not find publication, did you enter the correct name or id?");
            }
            

            if (!string.IsNullOrEmpty(Program.Settings.File))
                Program.Settings.Content = await ReadFileFromPath(Program.Settings.File);
            if (Program.Settings.ContentFormat == "markdown" && Program.Settings.ParseFrontmatter)
            {
                await ParseFrontmatter(Program.Settings.Content);
            }

            _configureService.CheckForValidSettings(Program.Settings);

            // Ensure lower case, API is case sensitive sadly
            Program.Settings.License = Program.Settings.License?.ToLower();
            Program.Settings.PublishStatus = Program.Settings.PublishStatus?.ToLower();
            Program.Settings.ContentFormat = Program.Settings.ContentFormat?.ToLower();
            MediumCreatedPost post;
            if (pub != null)
            {
                post = await CreateNewPostUnderPublicationAsync(pub.Id);
            }
            else
            {
                post = await CreateNewPostWithoutPublicationAsync(user.Id);
            }
            SetWorkflowOutputs(post);
        }

        /// <summary>
        /// Parse markdown content and look for frontmatter.
        /// Convert the markdown into HTML to remove the frontmatter.
        /// </summary>
        /// <param name="content">Content in markdown</param>
        /// <returns></returns>
        private async Task ParseFrontmatter(string content)
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();

            StringWriter writer = new StringWriter();
            HtmlRenderer renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);

            MarkdownDocument document = Markdown.Parse(content, pipeline);

            // extract the front matter from markdown document
            YamlFrontMatterBlock yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (yamlBlock != null)
            {
                string yaml = yamlBlock.Lines.ToString();


                try
                {
                    // deserialize the yaml block into a custom type
                    IDeserializer deserializer = new DeserializerBuilder()
                        .WithNamingConvention(PascalCaseNamingConvention.Instance)
                        .Build();
                    Settings metadata = deserializer.Deserialize<Settings>(yaml);
                    _configureService.OverrideSettings(metadata);
                }
                catch (YamlException e)
                {
                    IDeserializer deserializer = new DeserializerBuilder()
                        .WithNamingConvention(UnderscoredNamingConvention.Instance)
                        .Build();
                    Settings metadata = deserializer.Deserialize<Settings>(yaml);
                    _configureService.OverrideSettings(metadata);
                }
                // finally we can render the markdown content as html if necessary
                renderer.Render(document);
                await writer.FlushAsync();
                string html = writer.ToString();
                Program.Settings.Content = html;
                Program.Settings.ContentFormat = "html";
            }
        }

        /// <summary>
        ///     Retrieves current authenticated user
        /// </summary>
        /// <returns>Medium User</returns>
        public async Task<User> GetCurrentMediumUserAsync()
        {
            HttpResponseMessage response = await Program.Client.GetAsync("me").ConfigureAwait(false);
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
            HttpResponseMessage response =
                await Program.Client.GetAsync($"users/{mediumUserId}/publications").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Publication[] publications = JsonSerializer.Deserialize<Publication[]>(
                JsonDocument.Parse(
                        await response.Content.ReadAsByteArrayAsync())
                    .RootElement.EnumerateObject().First().Value.ToString());
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
            Post post = new Post
            {
                Content = Program.Settings.Content,
                ContentFormat = Program.Settings.ContentFormat,
                PublishStatus = Program.Settings.PublishStatus,
                Tags = Program.Settings.Tags.ToArray(),
                Title = Program.Settings.Title,
                CanonicalUrl = Program.Settings.CanonicalUrl,
                License = Program.Settings.License
            };
            HttpResponseMessage response = await Program.Client.PostAsync($"{requestUri}",
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
                        .RootElement.EnumerateObject().First().Value.ToString());
                throw new Exception("Something went wrong when posting: " + errors[0].Message);
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
        public async Task<string> ReadFileFromPath(string filePath)
        {
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
}