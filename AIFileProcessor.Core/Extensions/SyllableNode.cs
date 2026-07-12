//-----------------------------------------------------------------------
// <copyright file="SyllableNode.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a single syllable in the input stream, along with its associated tone and contextual information for sandhi processing. This record encapsulates all necessary data for applying sandhi rules in a functional programming style, allowing for easy transformation and immutability throughout the processing pipeline.
    /// </summary>
    /// <param name="text">The text of the syllable.</param>
    /// <param name="tone">The tone of the syllable.</param>
    /// <param name="context">The contextual information for sandhi processing.</param>
    public record SyllableNode(string text, Tone tone, SandhiContext context);
}
