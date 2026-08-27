//-----------------------------------------------------------------------
// <copyright file="ProviderStubs.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ProviderStubs;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Response;

/// <summary>
/// Minimal IAiService implementation for testing GetProviderKey.
/// All members throw NotImplementedException — only the type name matters.
/// </summary>
internal interface IAiServiceStub : IAiService
{
    IEnumerable<AiModelInfo> IAiService.GetSupportedModels() =>
        throw new NotImplementedException();

    bool IAiService.SupportsModel(string modelNameOrId) =>
        throw new NotImplementedException();

    IReadOnlyList<ReasoningLevel> IAiService.GetSupportedReasoningLevels(string modelId) =>
        throw new NotImplementedException();

    Task<AiCallResult> IAiService.ProcessTextAsync(
        string modelId, string prompt, string data, CancellationToken ct) =>
        throw new NotImplementedException();

    Task<AiCallResult> IAiService.ProcessTextAsync(
        string modelId, string prompt, string data, IAiServiceSettings settings, CancellationToken ct) =>
        throw new NotImplementedException();
}

internal sealed class GeminiService : IAiServiceStub;

internal sealed class OpenAiService : IAiServiceStub;

internal sealed class DeepSeekService : IAiServiceStub;

internal sealed class OllamaService : IAiServiceStub;

internal sealed class UnknownService : IAiServiceStub;