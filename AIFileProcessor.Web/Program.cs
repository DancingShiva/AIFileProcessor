//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web
{
    using System;
    using AIFileProcessor.DependencyInjection;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Web.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;

    /// <summary>
    /// Entry point for the AIFileProcessor web application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the web application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Secure the API with Entra ID (Azure AD) JWT bearer tokens. All endpoints require
            // an authenticated caller via the authorization fallback policy below. API keys for
            // the AI providers never leave the server; clients only present their Entra token.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            builder.Services.AddAuthorization(options =>
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

            // Register HTTP context accessor (required for HttpUserContext and WebSettingsProvider)
            builder.Services.AddHttpContextAccessor();

            // Register all core AIFileProcessor services (AI services, text processor,
            // file ops, processing orchestrator, settings managers, etc.)
            // AddAiFileProcessorCore registers FileSettingsProvider as ISettingsProvider
            // by default, but we override it below with the web-specific implementation.
            builder.Services.AddAiFileProcessorCore(builder.Configuration);

            // Override the file-based settings provider with the web-specific one
            // that resolves per-user settings from the HTTP request.
            builder.Services.Replace(
                ServiceDescriptor.Singleton<ISettingsProvider, WebSettingsProvider>());

            // Register the web-specific user context (resolves user from HTTP context)
            builder.Services.AddSingleton<IUserContext, HttpUserContext>();

            // Add ASP.NET Core MVC controllers
            builder.Services.AddControllers();

            WebApplication app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}