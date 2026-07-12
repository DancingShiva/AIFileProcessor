//-----------------------------------------------------------------------
// <copyright file="WindowsUserContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Configuration
{
    using System;
    using AIFileProcessor.Interfaces;

    /// <summary>
    /// WinForms-specific implementation of <see cref="IUserContext"/> that
    /// resolves the current user from the Windows identity. On a Terminal
    /// Server, each session naturally resolves to a different user.
    /// </summary>
    public class WindowsUserContext : IUserContext
    {
        /// <summary>
        /// Gets the user name portion of the current Windows identity
        /// (e.g., "jdoe" from "DOMAIN\jdoe"). This serves as the per-user
        /// settings key and the folder name under
        /// <c>%APPDATA%\AIFileProcessor\{UserId}\</c>.
        /// </summary>
        public string UserId { get; } = GetUserName();

        private static string GetUserName()
        {
            try
            {
                string? name = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    // Strip the domain prefix to get a clean folder-safe user name
                    int slashIndex = name.LastIndexOf('\\');
                    return slashIndex >= 0 ? name[(slashIndex + 1)..] : name;
                }
            }
            catch
            {
                // Fall through to Environment.UserName
            }

            return Environment.UserName;
        }
    }
}