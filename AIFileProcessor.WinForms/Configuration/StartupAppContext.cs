//-----------------------------------------------------------------------
// <copyright file="StartupAppContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System;
    using System.Windows.Forms;
    using AIFileProcessor;
    using Microsoft.Extensions.Logging;

    // The orchestrator that manages the application's lifecycle
    public class StartupAppContext : ApplicationContext
    {
        private readonly ILogger<StartupAppContext> _logger;

        public StartupAppContext(
            ILogger<StartupAppContext> logger,
            MainForm mainForm)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ArgumentNullException.ThrowIfNull(mainForm);

            _logger.LogInformation("Application Context initialized. Routing to initial form.");

            // When this specific form closes, terminate the application's message loop.
            // If this was a login screen, you could instead launch the MainForm here
            // and leave the message loop running.
            mainForm.FormClosed += (sender, args) =>
            {
                _logger.LogInformation("Initial form closed. Exiting application.");
                ExitThread();
            };

            mainForm.Show();
        }
    }
}