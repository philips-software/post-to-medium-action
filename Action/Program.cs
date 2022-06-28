using System.Threading.Tasks;
using DotNetEnv;
using PostMediumGitHubAction.Domain;
using PostMediumGitHubAction.Services;

namespace PostMediumGitHubAction;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // Load .env file if present
        Env.TraversePath().Load();
        IConfigureService configureService = new ConfigureService();
        Settings configuredSettings = configureService.ConfigureApplication(args);
        IMediumService mediumService = new MediumService(configuredSettings);
        MediumCreatedPost post = await mediumService.SubmitNewContentAsync();
        mediumService.SetWorkflowOutputs(post);
    }
}