//-----------------------------------------------------------------------
// <copyright file="HttpUserContext.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Web.Configuration
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using AIFileProcessor.Interfaces;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Web-specific implementation of <see cref="IUserContext"/> that resolves
    /// the user identifier from the HTTP request context. Supports both
    /// authenticated users (via claims) and the API-key-as-identity pattern
    /// where a hash of the supplied API key serves as the user ID.
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
        /// name identifier claim is used. Otherwise, a hash of the API key
        /// from the request header is used as the user identity.
        /// </summary>
        public string UserId => ResolveUserId();

        private static string HashApiKey(string apiKey)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToHexString(bytes)[..16];
        }

        private string ResolveUserId()
        {
            HttpContext? ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return "anonymous";
            }

            // 1. If authenticated via an identity system, use the name claim
            if (ctx.User?.Identity?.IsAuthenticated == true)
            {
                string? name = ctx.User.Identity.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            // 2. API-key-as-identity: derive a stable hash from the supplied API key
            string? apiKey = ctx.Request.Headers.TryGetValue("X-Api-Key", out var keyVal)
                ? keyVal.ToString()
                : null;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = ctx.Request.Headers.TryGetValue("Authorization", out var authVal)
                    ? authVal.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase)
                    : null;
            }

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return HashApiKey(apiKey);
            }

            return "anonymous";
        }
    }
}