using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PostMediumGitHubAction.Domain;

namespace PostMediumGitHubAction.Services
{
    public class MediumService
    {
        public async Task SubmitNewContentAsync()
        {
            User user = await GetCurrentMediumUserAsync();
            Publication pub = await FindMatchingPublicationAsync(user.Id, Program.Settings.PublicationName,
                Program.Settings.PublicationId);
            if (pub == null) throw new Exception("Could not find publication, did you enter the correct name or id?");

            if (!string.IsNullOrEmpty(Program.Settings.File))
                Program.Settings.Content = await ReadFileFromPath(Program.Settings.File);

            MediumCreatedPost post = await CreateNewPostUnderPublicationAsync(pub.Id);
            SetWorkflowOutputs(post);
        }

        /// <summary>
        /// Retrieves current authenticated user
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
            int? publicationId)
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
                if (publicationId != null && Convert.ToInt32(pub.Id) == publicationId) return pub;
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
            Post post = new Post
            {
                Content = Program.Settings.Content,
                ContentFormat = Program.Settings.ContentFormat,
                PublishStatus = Program.Settings.PublishStatus,
                Tags = Program.Settings.Tags,
                Title = Program.Settings.Title
            };
            HttpResponseMessage response = await Program.Client.PostAsync($"publications/{publicationId}/posts",
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
        /// See: https://docs.github.com/en/actions/learn-github-actions/workflow-commands-for-github-actions#setting-an-output-parameter
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