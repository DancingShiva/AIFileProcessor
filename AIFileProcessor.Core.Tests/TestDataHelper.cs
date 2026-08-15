//-----------------------------------------------------------------------
// <copyright file="TestDataHelper.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests;

using System.Collections.Generic;
using System.Text.Json;
using AIFileProcessor.Response;

/// <summary>
/// Shared factory for realistic Chinese linguistic test data that matches
/// the spec's expected output format: spaced Hanzi, tone-marked pinyin
/// mirroring those spaces, full 6-field breakdown entries, and proper JSON.
/// </summary>
public static class TestDataHelper
{
    // ─── Single-sentence factory ────────────────────────────────────────

    /// <summary>
    /// Creates a realistic single-sentence response: "我 在 超市 买菜 。"
    /// with full breakdown, matching pinyin, and proper corrections.
    /// </summary>
    public static AiApiResponse CreateSupermarketResponse() => new()
    {
        Scratchpad = "wǒ zài chāoshì mǎicài .",
        Corrections = new List<string> { "AMBIGUOUS: 余 → 餘 (line 1)" },
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateSupermarketRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "我 在 超市 买菜 。" — 5 semantic units
    /// (4 words + 1 punctuation), each with a full breakdown entry.
    /// </summary>
    public static ProcessedSentenceRow CreateSupermarketRow() => new()
    {
        Chinese = "我 在 超市 买菜 。",
        Pinyin = "wǒ zài chāoshì mǎicài .",
        English = "I buy groceries at the supermarket.",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("我", "wǒ", "I", "pron", "1"),
            CreateEntry("在", "zài", "at", "prep", "1"),
            CreateEntry("超市", "chāoshì", "supermarket", "n", "3"),
            CreateEntry("买菜", "mǎicài", "buy groceries", "v", "3"),
            CreateEntry("。", ".", ".", "punc", "N/A", " "),
        },
        AudioReordering = new List<AudioReorder>(),
    };

    // ─── Multi-sentence factory ─────────────────────────────────────────

    /// <summary>
    /// Creates a two-sentence response with realistic Chinese data for
    /// testing multi-row flows (chunking, merging, formatting).
    /// </summary>
    public static AiApiResponse CreateTwoSentenceResponse() => new()
    {
        Scratchpad = "wǒ zài chāoshì mǎicài . tā xué xí zhōng wén .",
        Corrections = new List<string>(),
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateSupermarketRow(),
            CreateStudyRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "他 学习 中文 。" — 4 semantic units.
    /// </summary>
    public static ProcessedSentenceRow CreateStudyRow() => new()
    {
        Chinese = "他 学习 中文 。",
        Pinyin = "tā xuéxí zhōngwén .",
        English = "He studies Chinese.",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("他", "tā", "he", "pron", "1"),
            CreateEntry("学习", "xuéxí", "study", "v", "2"),
            CreateEntry("中文", "zhōngwén", "Chinese language", "n", "3"),
            CreateEntry("。", ".", ".", "punc", "N/A", " "),
        },
        AudioReordering = new List<AudioReorder>(),
    };

    // ─── Entry factory ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a full 6-field breakdown entry matching the spec's
    /// Token|Pinyin|Gloss|PS|H|Note format.
    /// </summary>
    public static BreakdownEntry CreateEntry(
        string chinese,
        string pinyin,
        string gloss,
        string partOfSpeech,
        string hskLevel,
        string note = "") => new()
    {
        Chinese = chinese,
        Pinyin = pinyin,
        Gloss = gloss,
        PartOfSpeech = partOfSpeech,
        HskLevel = hskLevel,
        Note = note,
    };

    // ─── JSON factory ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a realistic JSON string matching the spec's OUTPUT_SCHEMA
    /// for a single supermarket sentence. Useful for testing FillFromJsonResponse
    /// and the full parse → verify → format pipeline.
    /// </summary>
    public static string CreateSupermarketJson()
    {
        var obj = new
        {
            SP = "wǒ zài chāoshì mǎicài .",
            Corrections = new[] { "AMBIGUOUS: 余 → 餘 (line 1)" },
            Sentences = new[]
            {
                new
                {
                    C = "我 在 超市 买菜 。",
                    P = "wǒ zài chāoshì mǎicài .",
                    B = "我|wǒ|I|pron|1| ||在|zài|at|prep|1| ||超市|chāoshì|supermarket|n|3| ||买菜|mǎicài|buy groceries|v|3| ||。|.|.|punc|N/A| ",
                    R = "N/A",
                    E = "I buy groceries at the supermarket.",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    /// <summary>
    /// Creates a realistic JSON string for two sentences.
    /// </summary>
    public static string CreateTwoSentenceJson()
    {
        var obj = new
        {
            SP = "wǒ zài chāoshì mǎicài . tā xuéxí zhōngwén .",
            Corrections = Array.Empty<string>(),
            Sentences = new[]
            {
                new
                {
                    C = "我 在 超市 买菜 。",
                    P = "wǒ zài chāoshì mǎicài .",
                    B = "我|wǒ|I|pron|1| ||在|zài|at|prep|1| ||超市|chāoshì|supermarket|n|3| ||买菜|mǎicài|buy groceries|v|3| ||。|.|.|punc|N/A| ",
                    R = "N/A",
                    E = "I buy groceries at the supermarket.",
                },
                new
                {
                    C = "他 学习 中文 。",
                    P = "tā xuéxí zhōngwén .",
                    B = "他|tā|he|pron|1| ||学习|xuéxí|study|v|2| ||中文|zhōngwén|Chinese language|n|3| ||。|.|.|punc|N/A| ",
                    R = "N/A",
                    E = "He studies Chinese.",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }
}