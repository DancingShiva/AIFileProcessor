//-----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Extension methods for registering AIFileProcessor core services in a DI container.
    /// Call <see cref="AddAiFileProcessorCore"/> from each frontend's startup code
    /// (WinForms Program.cs, Web Program.cs, etc.) to register shared services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all core AIFileProcessor services (AI services, text processor,
        /// file operations, processing orchestrator, and settings managers) in the DI container.
        /// </summary>
        /// <param name="services">The service collection to add registrations to.</param>
        /// <param name="configuration">The application configuration containing settings sections.</param>
        /// <returns>The service collection, for chaining.</returns>
        public static IServiceCollection AddAiFileProcessorCore(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Register IConfiguration in the container so that services registered
            // by this method (e.g., FileSettingsProvider) can resolve it directly.
            // The generic host already does this in production, but registering here
            // makes the dependency explicit and ensures tests work without manual setup.
            services.AddSingleton<IConfiguration>(configuration);

            // Bind settings sections to strongly-typed options
            services.Configure<DeepSeekSettings>(configuration.GetSection("DeepSeekSettings"));
            services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));
            services.Configure<OpenAiSettings>(configuration.GetSection("OpenAiSettings"));
            services.Configure<OllamaSettings>(configuration.GetSection("OllamaSettings"));

            // Configure per-model token limits from the Processing section of appsettings.json.
            var processingSettings = new ProcessingSettings();
            configuration.GetSection("Processing").Bind(processingSettings);
            ModelTokenLimits.Configure(processingSettings.ModelTokenLimitsMap, processingSettings.DefaultTokenLimit);

            // Register IOptionsMonitor<ProcessingSettings> for hot-reloadable processing limits.
            services.Configure<ProcessingSettings>(configuration.GetSection("Processing"));
            services.AddSingleton<ProcessingLimits>();

            // Register Typed HttpClients against concrete service types so each gets
            // its own DI-managed HttpClient with the correct timeout and base address.
            services.AddHttpClient<GeminiService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<GeminiSettings>>().CurrentValue;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 300);

                if (!string.IsNullOrWhiteSpace(settings.Endpoint))
                {
                    client.BaseAddress = new Uri(settings.Endpoint);
                }

                if (settings.RequestHeaders != null)
                {
                    foreach (var header in settings.RequestHeaders)
                    {
                        if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
            });

            services.AddHttpClient<DeepSeekService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<DeepSeekSettings>>().CurrentValue;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 300);

                if (settings.RequestHeaders != null)
                {
                    foreach (var header in settings.RequestHeaders)
                    {
                        if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
            });

            services.AddHttpClient<OpenAiService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<OpenAiSettings>>().CurrentValue;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 300);

                if (settings.RequestHeaders != null)
                {
                    foreach (var header in settings.RequestHeaders)
                    {
                        if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
            });

            services.AddHttpClient<OllamaService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<OllamaSettings>>().CurrentValue;
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 600);

                client.BaseAddress = new Uri(
                    string.IsNullOrWhiteSpace(settings.Endpoint)
                        ? "http://localhost:11434"
                        : settings.Endpoint,
                    UriKind.Absolute);

                if (settings.RequestHeaders != null)
                {
                    foreach (var header in settings.RequestHeaders)
                    {
                        if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
            });

            // Register all four AI services as IAiService implementations so that
            // IEnumerable<IAiService> resolves all of them.
            services.AddTransient<IAiService>(sp => sp.GetRequiredService<GeminiService>());
            services.AddTransient<IAiService>(sp => sp.GetRequiredService<DeepSeekService>());
            services.AddTransient<IAiService>(sp => sp.GetRequiredService<OpenAiService>());
            services.AddTransient<IAiService>(sp => sp.GetRequiredService<OllamaService>());

            // Register core services
            services.AddSingleton<IFileOps, FileOps>();
            services.AddTransient<IChineseTextProcessor, ChineseTextProcessor>();

            // Register the file-based settings provider (per-user settings storage).
            // Frontends register IUserContext separately (e.g., WindowsUserContext for WinForms).
            services.AddSingleton<ISettingsProvider, FileSettingsProvider>();

            // Register the processing orchestrator
            services.AddTransient<ProcessingOrchestrator>();

            // Register factory delegates for transient services
            services.AddTransient<Func<IChineseTextProcessor>>(sp => () => sp.GetRequiredService<IChineseTextProcessor>());

            return services;
        }
    }
}