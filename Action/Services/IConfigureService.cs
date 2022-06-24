using PostMediumGitHubAction.Domain;

namespace PostMediumGitHubAction.Services;

public interface IConfigureService
{
    /// <summary>
    ///     Configure the application with correct settings.
    /// First Set environment variables
    /// Command line arguments will then override environment variables
    /// </summary>
    Settings ConfigureApplication(string[] args);

    /// <summary>
    /// Override Program Settings with values from parameter if they are not null.
    /// </summary>
    /// <param name="originalSettings">Settings that are suppose to be overwritten.</param>
    /// <param name="settingsToReplaceWith">Settings that can be used to override.</param>
    Settings OverrideSettings(Settings originalSettings, Settings settingsToReplaceWith);

    /// <summary>
    ///     Checks if settings are filled in correctly.
    /// </summary>
    void CheckForValidSettings(Settings settingsToCheck);
}