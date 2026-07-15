//-----------------------------------------------------------------------
// <copyright file="HttpUserContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Configuration
{
    using System;
    using AIFileProcessor.Interfaces;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Web-specific implementation of <see cref="IUserContext"/> that resolves
    /// the user identifier from the HTTP request context. If the user is
    /// authenticated via an identity system (e.g., Entra ID, JWT), the
    /// authenticated user's name identifier claim is used. Otherwise,
    /// "anonymous" is returned.
    /// </summary>
    public class HttpUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUserContext"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor for obtaining the current request.</param>
        public HttpUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Gets the current user identifier. If the user is authenticated via
        /// an identity system (e.g., Entra ID, JWT), the authenticated user's
        /// name identifier claim is used. Otherwise, "anonymous" is returned.
        /// </summary>
        public string UserId => ResolveUserId();

        private string ResolveUserId()
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return "anonymous";
            }

            // If authenticated via an identity system, use the name claim
            if (ctx.User?.Identity?.IsAuthenticated == true)
            {
                string? name = ctx.User.Identity.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            return "anonymous";
        }
    }
}