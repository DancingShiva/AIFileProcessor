//-----------------------------------------------------------------------
// <copyright file="StubHttpHandler.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Services;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Shared <see cref="HttpMessageHandler"/> stub for HTTP-level service tests.
/// Captures the last request's URL, headers, and body, and returns configurable
/// responses based on path or a sequential queue. Supports simulating transient
/// failures (HTTP 429/500) and non-transient errors (HTTP 400).
/// </summary>
public sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _pathResponses = new(StringComparer.OrdinalIgnoreCase);
    private string? _defaultResponse;
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;
    private string _defaultContentType = "application/json";
    private readonly List<(HttpStatusCode StatusCode, string Body)> _queuedResponses = new();
    private int _queueIndex;
    private readonly object _lock = new();

    /// <summary>Gets the last request that was sent through this handler.</summary>
    public HttpRequestMessage? LastRequest { get; private set; }

    /// <summary>Gets the last request's body as a string, or null if no body was sent.</summary>
    public string? LastRequestBody { get; private set; }

    /// <summary>Gets the last request's headers.</summary>
    public HttpRequestHeaders? LastRequestHeaders { get; private set; }

    /// <summary>Gets the total number of requests processed by this handler.</summary>
    public int RequestCount { get; private set; }

    /// <summary>
    /// Sets a fixed response body for a specific URL path.
    /// </summary>
    /// <param name="path">The URL path to match (e.g., "/v1/chat/completions").</param>
    /// <param name="body">The response body to return.</param>
    public void SetResponse(string path, string body)
    {
        _pathResponses[path] = body;
    }

    /// <summary>
    /// Sets the default response body and status code for all unmatched paths.
    /// </summary>
    /// <param name="body">The response body.</param>
    /// <param name="statusCode">The HTTP status code (default: OK).</param>
    /// <param name="contentType">The content type (default: application/json).</param>
    public void SetDefaultResponse(string body, HttpStatusCode statusCode = HttpStatusCode.OK, string? contentType = null)
    {
        _defaultResponse = body;
        _defaultStatusCode = statusCode;
        _defaultContentType = contentType ?? "application/json";
    }

    /// <summary>
    /// Queues a sequence of responses. Each request dequeues the next response.
    /// When the queue is exhausted, the default response is returned.
    /// </summary>
    /// <param name="responses">An ordered list of (status code, body) tuples.</param>
    public void QueueResponses(params (HttpStatusCode StatusCode, string Body)[] responses)
    {
        lock (_lock)
        {
            _queuedResponses.Clear();
            _queuedResponses.AddRange(responses);
            _queueIndex = 0;
        }
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestHeaders = request.Headers;

        LastRequestBody = request.Content == null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        RequestCount++;

        // Check queued responses first
        lock (_lock)
        {
            if (_queueIndex < _queuedResponses.Count)
            {
                var (statusCode, body) = _queuedResponses[_queueIndex++];
                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };
            }
        }

        // Check path-specific responses
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (_pathResponses.TryGetValue(path, out var pathBody))
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(pathBody, Encoding.UTF8, "application/json"),
            };
        }

        // Return default response
        return new HttpResponseMessage(_defaultStatusCode)
        {
            Content = new StringContent(_defaultResponse ?? "{}", Encoding.UTF8, _defaultContentType),
        };
    }
}