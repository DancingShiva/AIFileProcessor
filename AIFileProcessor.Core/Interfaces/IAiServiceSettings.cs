//-----------------------------------------------------------------------
// <copyright file="IAiServiceSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    /// <summary>
    /// Marker interface implemented by all AI provider settings types
    /// (<see cref="IGeminiSettings"/>, <see cref="IOpenAiSettings"/>,
    /// <see cref="IDeepSeekSettings"/>). Replaces the <c>object</c> parameter
    /// on <see cref="IAiService.ProcessTextAsync(string, string, string, IAiServiceSettings, System.Threading.CancellationToken)"/>
    /// to restore type safety at the interface boundary. Carries no properties —
    /// all property access occurs through the concrete settings type after the
    /// type-safe cast in <see cref="Services.AiServiceBase{TSettings}"/>.
    /// </summary>
    public interface IAiServiceSettings
    {
    }
}