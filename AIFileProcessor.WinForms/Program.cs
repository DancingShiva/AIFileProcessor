//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AIFileProcessor
{
    using System;
    using System.Windows.Forms;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.DependencyInjection;
    using AIFileProcessor.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            // Register global exception handlers for UI thread and background/AppDomain failures
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unhandled thread exception occurred: {e.Exception.Message}",
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                string message = exception != null ? exception.Message : "Unknown error";
                MessageBox.Show(
                    $"A critical system-level error occurred: {message}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder(args);

            // Load configuration.
            // Host.CreateApplicationBuilder already loads appsettings.json by default.
            // Clear the default sources and explicitly add appsettings.json so there is
            // no ambiguity about which file is loaded (avoids double-loading on case-
            // insensitive file systems and missing-file errors on case-sensitive ones).
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
            }

            builder.Configuration.AddEnvironmentVariables();

            // Register all core services (AI services, text processor, file ops,
            // settings managers, processing orchestrator, etc.) in one call.
            builder.Services.AddAiFileProcessorCore(builder.Configuration);

            // Register WinForms-specific services
            builder.Services.AddSingleton<MainForm>();
            builder.Services.AddSingleton<System.Windows.Forms.ApplicationContext, StartupAppContext>();

            // Register the Windows user context for per-user settings resolution.
            // On a Terminal Server, each session resolves to a different user automatically.
            builder.Services.AddSingleton<IUserContext, WindowsUserContext>();

            // Register factory delegate for transient SettingsForm.
            // The default Microsoft DI container does not automatically provide Func<T>.
            builder.Services.AddTransient<Func<SettingsForm>>(sp => () => sp.GetRequiredService<SettingsForm>());

            // Register Forms in DI
            builder.Services.AddTransient<SettingsForm>();

            using var host = builder.Build();

            var appContext = host.Services.GetRequiredService<System.Windows.Forms.ApplicationContext>();

            Application.Run(appContext);

            host.StopAsync().GetAwaiter().GetResult();
            host.Dispose();
        }
    }
}