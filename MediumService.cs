using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace PostMediumGitHubAction
{
    public class MediumService
    {
        public async Task SubmitNewContentAsync()
        {
            if (string.IsNullOrEmpty(Program.settings.IntegrationToken))
            {
                throw new ArgumentNullException("IntegrationToken");
            }
            MediumUser user = await GetCurrentMediumUserAsync();
            Publication pub = await FindMatchingPublicationAsync(user.Data.Id, Program.settings.PublicationName, Program.settings.PublicationId);
            if (pub == null)
            {
                throw new Exception("Could not find publication, did you enter the correct name or id?");
            }
            if (!string.IsNullOrEmpty(Program.settings.File))
            {
                Program.settings.Content = await ReadFileFromPath(Program.settings.File);
            }
            await CreateNewPostUnderPublicationAsync(user.Data.Id, pub.Id);
        }

        // Retrieves current authenticated user
        public async Task<MediumUser> GetCurrentMediumUserAsync()
        {
            HttpResponseMessage response = await Program.Client.GetAsync("me").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            MediumUser mediumUser = JsonSerializer.Deserialize<MediumUser>(await response.Content.ReadAsByteArrayAsync());
            return mediumUser;
        }

        /// <summary>
        /// Retrieve matching publication
        /// </summary>
        /// <param name="mediumUserId">Id of the Medium User</param>
        /// <param name="publicationToLookFor">Name of the Publication to look for</param>
        /// <returns></returns>
        public async Task<Publication> FindMatchingPublicationAsync(string mediumUserId, string publicationToLookFor, int? publicationId)
        {
            HttpResponseMessage response = await Program.Client.GetAsync($"users/{mediumUserId}/publications").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            MediumPublication mediumPublications = JsonSerializer.Deserialize<MediumPublication>
            (await response.Content.ReadAsByteArrayAsync());
            foreach (Publication pub in mediumPublications.Publications)
            {
                if (!string.IsNullOrEmpty(publicationToLookFor) && pub.Name == publicationToLookFor)
                {
                    return pub;
                }
                if (publicationId != null && Convert.ToInt32(pub.Id) == publicationId)
                {
                    return pub;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new post under a publication
        /// </summary>
        /// <param name="mediumUserId">The id of the user</param>
        /// <param name="publicationId">The id of the publication</param>
        /// <returns></returns>
        public async Task CreateNewPostUnderPublicationAsync(string mediumUserId, string publicationId)
        {
            Post post = new Post();
            post.Content = Program.settings.Content;
            post.ContentFormat = Program.settings.ContentFormat;
            post.PublishStatus = Program.settings.PublishStatus;
            post.Tags = Program.settings.Tags;
            post.Title = Program.settings.Title;
            HttpResponseMessage response = await Program.Client.
            PostAsync($"publications/{publicationId}/posts",
            new StringContent(JsonSerializer.Serialize(post), System.Text.Encoding.UTF8, "application/json"))
            .ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                Root error = JsonSerializer.Deserialize<Root>(await response.Content.ReadAsStringAsync());
                throw new Exception("Something went wrong when posting: " + error.Errors[0].Message);
            }
            // TODO: Create new object and return it
        }
        /// <summary>
        /// Reads a file from path and returns result
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns></returns>
        public async Task<string> ReadFileFromPath(string filePath)
        {
            string result = await File.ReadAllTextAsync(filePath);
            // TODO: Parse any urls found in the content and do something with it like migrating URLS or Uploading images
            return result;
        }
    }
}
