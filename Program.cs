using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PostMediumGitHubAction
{
    class Program
    {
        public static Settings settings = new Settings();
        public static HttpClient Client;
        static async Task Main(string[] args)
        {
            // Load .env file if present
            DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
            ConfigureApplication();
            MediumService mediumService = new MediumService();
            await mediumService.SubmitNewContentAsync();
        }

        private static void ConfigureApplication() {
            settings.File = Environment.GetEnvironmentVariable(nameof(settings.File));
            settings.Content = Environment.GetEnvironmentVariable(nameof(settings.Content));
            settings.ContentFormat = Environment.GetEnvironmentVariable(nameof(settings.ContentFormat));
            settings.CanonicalUrl = Environment.GetEnvironmentVariable(nameof(settings.CanonicalUrl));
            settings.IntegrationToken = Environment.GetEnvironmentVariable(nameof(settings.IntegrationToken));
            settings.License = Environment.GetEnvironmentVariable(nameof(settings.License).ToLower());
            settings.NotifyFollowers = Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(settings.NotifyFollowers)).ToLower());
            settings.PublicationId = Convert.ToInt32(Environment.GetEnvironmentVariable(nameof(settings.PublicationId)));
            settings.PublicationName = Environment.GetEnvironmentVariable(nameof(settings.PublicationName));
            settings.PublishStatus = Environment.GetEnvironmentVariable(nameof(settings.PublishStatus)).ToLower();
            settings.Tags = Environment.GetEnvironmentVariable(nameof(settings.Tags)).Split(',');
            settings.Title = Environment.GetEnvironmentVariable(nameof(settings.Title));
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.medium.com/v1/");
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + settings.IntegrationToken);
        }
        // TODO: Create method that retrieves the current user.
        // TODO: Create method that creates a new post.
        // TODO: Create method that reads HTML from a file.
        // TODO: Create method that reads Markdown from a file.
    }
}
