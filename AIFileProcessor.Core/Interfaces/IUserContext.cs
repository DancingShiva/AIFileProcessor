//-----------------------------------------------------------------------
// <copyright file="IUserContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    /// <summary>
    /// Abstracts how the current user identifier is obtained, allowing the
    /// core library to remain agnostic of the hosting environment (WinForms,
    /// Terminal Server, web).
    /// </summary>
    /// <remarks>
    /// Implementations must account for the deployment scenario:
    /// <list type="bullet">
    /// <item>WindowsUserContext: Desktop or Terminal Server sessions
    /// (multi-user, same machine, cross-process file locking required for settings).</item>
    /// <item>HttpUserContext: Web requests
    /// (multi-user, single shared process, per-request settings resolution).</item>
    /// </list>
    /// </remarks>
    public interface IUserContext
    {
        /// <summary>
        /// Gets the identifier for the current user. On a desktop application
        /// this is the Windows identity name; in a web application this is
        /// the authenticated user's ID.
        /// </summary>
        string UserId { get; }
    }
}