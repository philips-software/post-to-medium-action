﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CommandLine;
using DotNetEnv;
using PostMediumGitHubAction.Services;

namespace PostMediumGitHubAction
{
    internal class Program
    {
        public static Settings Settings = new Settings();
        public static HttpClient Client;

        private static async Task Main(string[] args)
        {
            // Load .env file if present
            Env.TraversePath().Load();
            new ConfigureService(args);
            MediumService mediumService = new MediumService();
            await mediumService.SubmitNewContentAsync();
        }
    }
}