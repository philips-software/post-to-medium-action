using System;
using System.Linq;
using CommandLine;
using PostMediumGitHubAction.Domain;

namespace PostMediumGitHubAction.Services;

public class ConfigureService : IConfigureService
{
    /// <summary>
    ///     Configure the application with correct settings.
    /// First Set environment variables
    /// Command line arguments will then override environment variables
    /// </summary>
    public Settings ConfigureApplication(string[] args)
    {
        Settings settingsToReturn = new();
        settingsToReturn.File = Environment.GetEnvironmentVariable(nameof(settingsToReturn.File));
        settingsToReturn.Content = Environment.GetEnvironmentVariable(nameof(settingsToReturn.Content));
        settingsToReturn.ContentFormat = Environment.GetEnvironmentVariable(nameof(settingsToReturn.ContentFormat));
        settingsToReturn.CanonicalUrl = Environment.GetEnvironmentVariable(nameof(settingsToReturn.CanonicalUrl));
        settingsToReturn.IntegrationToken =
            Environment.GetEnvironmentVariable(nameof(settingsToReturn.IntegrationToken));
        settingsToReturn.License = Environment.GetEnvironmentVariable(nameof(settingsToReturn.License));
        settingsToReturn.NotifyFollowers =
            Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(settingsToReturn.NotifyFollowers)));
        settingsToReturn.PublicationId =
            Environment.GetEnvironmentVariable(nameof(settingsToReturn.PublicationId));
        settingsToReturn.PublicationName =
            Environment.GetEnvironmentVariable(nameof(settingsToReturn.PublicationName));
        settingsToReturn.PublishStatus = Environment.GetEnvironmentVariable(nameof(settingsToReturn.PublishStatus));
        settingsToReturn.ParseFrontmatter =
            Convert.ToBoolean(Environment.GetEnvironmentVariable(nameof(settingsToReturn.ParseFrontmatter)));
        settingsToReturn.Tags = Environment.GetEnvironmentVariable(nameof(settingsToReturn.Tags))?.Split(',');
        settingsToReturn.Title = Environment.GetEnvironmentVariable(nameof(settingsToReturn.Title));

        // Command Line arguments will overwrite environment file.
        if (args.Length > 0)
            Parser.Default.ParseArguments<Settings>(args).WithParsed(s => { settingsToReturn = s; });

        return settingsToReturn;
    }

    /// <summary>
    /// Override Program Settings with values from parameter if they are not null.
    /// </summary>
    /// <param name="originalSettings">Settings that are suppose to be overwritten.</param>
    /// <param name="settingsToReplaceWith">Settings that can be used to override.</param>
    public Settings OverrideSettings(Settings originalSettings, Settings settingsToReplaceWith)
    {
        if (settingsToReplaceWith.CanonicalUrl != null)
            originalSettings.CanonicalUrl = settingsToReplaceWith.CanonicalUrl;

        if (settingsToReplaceWith.ContentFormat != null)
            originalSettings.ContentFormat = settingsToReplaceWith.ContentFormat;

        if (settingsToReplaceWith.Tags.Any()) originalSettings.Tags = settingsToReplaceWith.Tags;

        if (settingsToReplaceWith.License != null) originalSettings.License = settingsToReplaceWith.License;

        if (settingsToReplaceWith.PublishStatus != null)
            originalSettings.PublishStatus = settingsToReplaceWith.PublishStatus;

        if (settingsToReplaceWith.Title != null) originalSettings.Title = settingsToReplaceWith.Title;

        return originalSettings;
    }

    /// <summary>
    ///     Checks if settings are filled in correctly.
    /// </summary>
    public void CheckForValidSettings(Settings settingsToCheck)
    {
        if (string.IsNullOrWhiteSpace(settingsToCheck.IntegrationToken))
            throw new ArgumentNullException(nameof(settingsToCheck.IntegrationToken),
                "The Integration Token parameter was not set successfully.");

        if (string.IsNullOrWhiteSpace(settingsToCheck.Title))
            throw new ArgumentNullException(nameof(settingsToCheck.Title),
                "The Title parameter was not set successfully.");

        if (string.IsNullOrWhiteSpace(settingsToCheck.ContentFormat))
            throw new ArgumentNullException(nameof(settingsToCheck.ContentFormat),
                "The Content Format parameter was not set successfully.");

        if (string.IsNullOrWhiteSpace(settingsToCheck.File) && string.IsNullOrEmpty(settingsToCheck.Content))
            throw new ArgumentNullException(nameof(settingsToCheck.Content),
                "Either the parameter Content or File should be filled in.");
    }
}