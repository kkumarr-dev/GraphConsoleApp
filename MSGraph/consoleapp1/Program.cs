﻿using System;
using System.Collections.Generic;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;
using Helpers;

namespace consoleapp1
{
    class Program
    {        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config = LoadAppSettings();
            if (config == null)
            {
            Console.WriteLine("Invalid appsettings.json file.");
            return;
            }
            var client = GetAuthenticatedGraphClient(config);

            var graphRequest = client.Users.Request();

            var results = graphRequest.GetAsync().Result;
            foreach(var user in results)
            {
            Console.WriteLine(user.Id + ": " + user.DisplayName + " <" + user.Mail + ">");
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(graphRequest.GetHttpRequestMessage().RequestUri);
        }
        private static GraphServiceClient _graphClient;
        private static IConfigurationRoot LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["applicationSecret"]) ||
                    string.IsNullOrEmpty(config["redirectUri"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }
        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
        var clientId = config["applicationId"];
        var clientSecret = config["applicationSecret"];
        var redirectUri = config["redirectUri"];
        var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

        List<string> scopes = new List<string>();
        scopes.Add("https://graph.microsoft.com/.default");

        var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                .WithAuthority(authority)
                                                .WithRedirectUri(redirectUri)
                                                .WithClientSecret(clientSecret)
                                                .Build();
        return new MsalAuthenticationProvider(cca, scopes.ToArray());
        }
        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var authenticationProvider = CreateAuthorizationProvider(config);
            _graphClient = new GraphServiceClient(authenticationProvider);
            return _graphClient;
        }
    }
}
