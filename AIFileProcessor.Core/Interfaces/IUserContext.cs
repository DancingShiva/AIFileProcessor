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