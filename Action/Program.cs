using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetEnv;
using PostMediumGitHubAction.Services;

namespace PostMediumGitHubAction
{
    internal class Program
    {

        private static async Task Main(string[] args)
        {
            // Load .env file if present
            // Env.TraversePath().Load();
            ConfigureService configureService = new ConfigureService();
            Settings configuredSettings = configureService.ConfigureApplication(args);
            MediumService mediumService = new MediumService(configuredSettings);
            await mediumService.SubmitNewContentAsync();
        }
    }
}