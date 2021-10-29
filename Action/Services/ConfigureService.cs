using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CommandLine;

namespace PostMediumGitHubAction.Services
{
    internal class ConfigureService
    {
        public ConfigureService(string[] args)
        {
            ConfigureApplication(args);
        }
        /// <summary>
        /// Configure the application with correct settings.
        /// </summary>
        private void ConfigureApplication(string[] args)
        {
            Program.Settings.File = Environment.GetEnvironmentVariable(nameof(Program.Settings.File));
            Program.Settings.Content = Environment.GetEnvironmentVariable(nameof(Program.Settings.Content));
            Program.Settings.ContentFormat = Environment.GetEnvironmentVariable(nameof(Program.Settings.ContentFormat));
            Program.Settings.CanonicalUrl = Environment.GetEnvironmentVariable(nameof(Program.Settings.CanonicalUrl));
            Program.Settings.IntegrationToken = Environment.GetEnvironmentVariable(nameof(Program.Settings.IntegrationToken));
            Program.Settings.License = Environment.GetEnvironmentVariable(nameof(Program.Settings.License));
            Program.Settings.NotifyFollowers =
                Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(Program.Settings.NotifyFollowers)));
            Program.Settings.PublicationId =
                Environment.GetEnvironmentVariable(nameof(Program.Settings.PublicationId));
            Program.Settings.PublicationName = Environment.GetEnvironmentVariable(nameof(Program.Settings.PublicationName));
            Program.Settings.PublishStatus = Environment.GetEnvironmentVariable(nameof(Program.Settings.PublishStatus));
            Program.Settings.Tags = Environment.GetEnvironmentVariable(nameof(Program.Settings.Tags))?.Split(',');
            Program.Settings.Title = Environment.GetEnvironmentVariable(nameof(Program.Settings.Title));

            // Command Line arguments will overwrite environment file.
            if (args.Length > 0)
            {
                Parser.Default.ParseArguments<Settings>(args).WithParsed(s =>
                {
                    Program.Settings = s;
                    Console.WriteLine(s.IntegrationToken);
                });
            }

            Program.Client = new HttpClient
            {
                BaseAddress = new Uri("https://api.medium.com/v1/")
            };
            Program.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Program.Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Program.Settings.IntegrationToken}");

            CheckForValidSettings();

            // Ensure lower case, API is case sensitive sadly
            Program.Settings.License = Program.Settings.License?.ToLower();
            Program.Settings.PublishStatus = Program.Settings.PublishStatus?.ToLower();
            Program.Settings.ContentFormat = Program.Settings.ContentFormat?.ToLower();
        }

        /// <summary>
        /// Checks if settings are filled in correctly.
        /// </summary>
        private void CheckForValidSettings()
        {
            if (string.IsNullOrEmpty(Program.Settings.IntegrationToken))
                throw new ArgumentNullException(nameof(Program.Settings.IntegrationToken),
                    $"The {nameof(Program.Settings.IntegrationToken)} parameter was not set successfully.");
            
            if (string.IsNullOrEmpty(Program.Settings.PublicationId) &&
                string.IsNullOrEmpty(Program.Settings.PublicationName))
                throw new ArgumentNullException(nameof(Program.Settings.PublicationId),
                    "Either the parameter PublicationId or PublicationName should be filled in.");
            
            if (string.IsNullOrEmpty(Program.Settings.Title))
                throw new ArgumentNullException(nameof(Program.Settings.Title),
                    $"The {nameof(Program.Settings.Title)} parameter was not set successfully.");
            
            if (string.IsNullOrEmpty(Program.Settings.ContentFormat))
                throw new ArgumentNullException(nameof(Program.Settings.ContentFormat),
                    $"The {nameof(Program.Settings.ContentFormat)} parameter was not set successfully.");
            
            if (string.IsNullOrEmpty(Program.Settings.File) && string.IsNullOrEmpty(Program.Settings.Content))
                throw new ArgumentNullException(nameof(Program.Settings.Content),
                    "Either the parameter Content or File should be filled in.");
        }
    }
}
