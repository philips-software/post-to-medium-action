using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CommandLine;

namespace PostMediumGitHubAction.Services
{
    internal class ConfigureService
    {
        public ConfigureService()
        {
        }
        public ConfigureService(string[] args)
        {
            ConfigureApplication(args);
        }

        /// <summary>
        ///     Configure the application with correct settings.
        /// First Set environment variables
        /// Command line arguments will then override environment variables
        /// </summary>
        private void ConfigureApplication(string[] args)
        {
            Program.Settings.File = Environment.GetEnvironmentVariable(nameof(Program.Settings.File));
            Program.Settings.Content = Environment.GetEnvironmentVariable(nameof(Program.Settings.Content));
            Program.Settings.ContentFormat = Environment.GetEnvironmentVariable(nameof(Program.Settings.ContentFormat));
            Program.Settings.CanonicalUrl = Environment.GetEnvironmentVariable(nameof(Program.Settings.CanonicalUrl));
            Program.Settings.IntegrationToken =
                Environment.GetEnvironmentVariable(nameof(Program.Settings.IntegrationToken));
            Program.Settings.License = Environment.GetEnvironmentVariable(nameof(Program.Settings.License));
            Program.Settings.NotifyFollowers =
                Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(Program.Settings.NotifyFollowers)));
            Program.Settings.PublicationId =
                Environment.GetEnvironmentVariable(nameof(Program.Settings.PublicationId));
            Program.Settings.PublicationName =
                Environment.GetEnvironmentVariable(nameof(Program.Settings.PublicationName));
            Program.Settings.PublishStatus = Environment.GetEnvironmentVariable(nameof(Program.Settings.PublishStatus));
            Program.Settings.ParseFrontmatter = Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(Program.Settings.ParseFrontmatter)));
            Program.Settings.Tags = Environment.GetEnvironmentVariable(nameof(Program.Settings.Tags))?.Split(',');
            Program.Settings.Title = Environment.GetEnvironmentVariable(nameof(Program.Settings.Title));

            // Command Line arguments will overwrite environment file.
            if (args.Length > 0)
                Parser.Default.ParseArguments<Settings>(args).WithParsed(s => { Program.Settings = s; });

            Program.Client = new HttpClient
            {
                BaseAddress = new Uri("https://api.medium.com/v1/")
            };
            Program.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Program.Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Program.Settings.IntegrationToken}");
        }

        /// <summary>
        /// Override Program Settings with values from parameter if they are not null.
        /// </summary>
        /// <param name="settingsToReplace">Settings to replace Program settings with</param>
        public void OverrideSettings(Settings settingsToReplace)
        {
            if (settingsToReplace.CanonicalUrl != null)
            {
                Program.Settings.CanonicalUrl = settingsToReplace.CanonicalUrl;
            }

            if (settingsToReplace.ContentFormat != null)
            {
                Program.Settings.ContentFormat = settingsToReplace.ContentFormat;
            }

            if (settingsToReplace.Tags.Any())
            {
                Program.Settings.Tags = settingsToReplace.Tags;
            }

            if (settingsToReplace.License != null)
            {
                Program.Settings.License = settingsToReplace.License;
            }

            if (settingsToReplace.PublishStatus != null)
            {
                Program.Settings.PublishStatus = settingsToReplace.PublishStatus;
            }

            if (settingsToReplace.Title != null)
            {
                Program.Settings.Title = settingsToReplace.Title;
            }
        }
        /// <summary>
        ///     Checks if settings are filled in correctly.
        /// </summary>
        public void CheckForValidSettings(Settings settingsToCheck)
        {
            if (string.IsNullOrWhiteSpace(settingsToCheck.IntegrationToken))
                throw new ArgumentNullException(nameof(Program.Settings.IntegrationToken),
                    $"The {nameof(Program.Settings.IntegrationToken)} parameter was not set successfully.");

            if (string.IsNullOrWhiteSpace(settingsToCheck.Title))
                throw new ArgumentNullException(nameof(Program.Settings.Title),
                    $"The {nameof(Program.Settings.Title)} parameter was not set successfully.");

            if (string.IsNullOrWhiteSpace(settingsToCheck.ContentFormat))
                throw new ArgumentNullException(nameof(Program.Settings.ContentFormat),
                    $"The {nameof(Program.Settings.ContentFormat)} parameter was not set successfully.");

            if (string.IsNullOrWhiteSpace(settingsToCheck.File) && string.IsNullOrEmpty(settingsToCheck.Content))
                throw new ArgumentNullException(nameof(Program.Settings.Content),
                    "Either the parameter Content or File should be filled in.");
        }
    }
}