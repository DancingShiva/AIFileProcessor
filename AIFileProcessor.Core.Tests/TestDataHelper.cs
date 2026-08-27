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

    // ─── Audio reordering factory ───────────────────────────────────────

    /// <summary>
    /// Creates a realistic single-sentence response with audio reordering:
    /// "我 有 一百 块 。" where "一百" (yī bǎi) is reordered to "bǎi yī"
    /// in spoken order, and the R field carries two reorder segments.
    /// </summary>
    public static AiApiResponse CreateAudioReorderResponse() => new()
    {
        Scratchpad = "wǒ yǒu yī bǎi kuài . bǎi yī",
        Corrections = new List<string>(),
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateAudioReorderRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "我 有 一百 块 。" with populated
    /// AudioReordering containing two reorder segments.
    /// </summary>
    public static ProcessedSentenceRow CreateAudioReorderRow() => new()
    {
        Chinese = "我 有 一百 块 。",
        Pinyin = "wǒ yǒu bǎiyī kuài .",
        English = "I have one hundred yuan.",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("我", "wǒ", "I", "pron", "1"),
            CreateEntry("有", "yǒu", "have", "v", "1"),
            CreateEntry("一百", "bǎiyī", "one hundred", "num", "2"),
            CreateEntry("块", "kuài", "yuan (currency)", "mw", "1"),
            CreateEntry("。", ".", ".", "punc", "N/A", " "),
        },
        AudioReordering = new List<AudioReorder>
        {
            new() { VisualString = "bǎi yī", StartIndex = 0, EndIndex = 1 },
            new() { VisualString = "kuài", StartIndex = 2, EndIndex = 2 },
        },
    };

    // ─── Special punctuation factory ────────────────────────────────────

    /// <summary>
    /// Creates a realistic response with diverse Chinese punctuation:
    /// enumeration comma 、, semicolons ；, quotation marks 「」, and
    /// ellipsis …. Exercises the CleanChinesePunctuation sanitizer and
    /// ChinesePunctuationSanitizingConverter through the full pipeline.
    /// </summary>
    public static AiApiResponse CreateSpecialPunctuationResponse() => new()
    {
        Scratchpad = "tā shuō : 'nǐ hǎo' , zàijiàn ...",
        Corrections = new List<string>(),
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateSpecialPunctuationRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "他 说 ：「 你 好 」， 再见 … 。"
    /// — includes enumeration comma, quotation marks, semicolons,
    /// and ellipsis in the breakdown.
    /// </summary>
    public static ProcessedSentenceRow CreateSpecialPunctuationRow() => new()
    {
        Chinese = "他 说 ：「 你 好 」， 再见 … 。",
        Pinyin = "tā shuō : 'nǐ hǎo' , zàijiàn ... .",
        English = "He said: 'Hello', goodbye... .",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("他", "tā", "he", "pron", "1"),
            CreateEntry("说", "shuō", "say", "v", "2"),
            CreateEntry("：", ":", ":", "punc", "N/A", " "),
            CreateEntry("「", "'", "opening quote", "punc", "N/A", " "),
            CreateEntry("你", "nǐ", "you", "pron", "1"),
            CreateEntry("好", "hǎo", "good", "adj", "1"),
            CreateEntry("」", "'", "closing quote", "punc", "N/A", " "),
            CreateEntry("，", ",", ",", "punc", "N/A", " "),
            CreateEntry("再见", "zàijiàn", "goodbye", "v", "3"),
            CreateEntry("…", "...", "ellipsis", "punc", "N/A", " "),
            CreateEntry("。", ".", ".", "punc", "N/A", " "),
        },
        AudioReordering = new List<AudioReorder>(),
    };

    // ─── Neutral tone particles factory ─────────────────────────────────

    /// <summary>
    /// Creates a realistic response with neutral tone particles (吗, 呢,
    /// 了, 的, 得, 地) in the breakdown entries. The particles carry tone
    /// marks in their pinyin so that sandhi neutralization (RULE D) can
    /// be verified through the full pipeline.
    /// </summary>
    public static AiApiResponse CreateNeutralToneResponse() => new()
    {
        Scratchpad = "nǐ chī le fàn ma ? wǒ de péngyou ne ?",
        Corrections = new List<string>(),
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateNeutralToneRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "你 吃 了 饭 吗 ？ 我 的 朋友 呢 ？"
    /// — includes neutral tone particles 了 (le), 吗 (ma), 的 (de),
    /// 呢 (ne) with tone-marked pinyin that sandhi should strip.
    /// </summary>
    public static ProcessedSentenceRow CreateNeutralToneRow() => new()
    {
        Chinese = "你 吃 了 饭 吗 ？ 我 的 朋友 呢 ？",
        Pinyin = "nǐ chī le fàn ma ? wǒ de péngyou ne ?",
        English = "Have you eaten? What about my friend?",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("你", "nǐ", "you", "pron", "1"),
            CreateEntry("吃", "chī", "eat", "v", "1"),
            CreateEntry("了", "le", "completed action particle", "part", "1"),
            CreateEntry("饭", "fàn", "rice/meal", "n", "1"),
            CreateEntry("吗", "ma", "question particle", "part", "1"),
            CreateEntry("？", "?", "?", "punc", "N/A", " "),
            CreateEntry("我", "wǒ", "I", "pron", "1"),
            CreateEntry("的", "de", "possessive particle", "part", "1"),
            CreateEntry("朋友", "péngyou", "friend", "n", "3"),
            CreateEntry("呢", "ne", "question particle", "part", "1"),
            CreateEntry("？", "?", "?", "punc", "N/A", " "),
        },
        AudioReordering = new List<AudioReorder>(),
    };

    // ─── Three-chunk factory (with diverse corrections) ─────────────────

    /// <summary>
    /// Creates a realistic three-sentence response for testing multi-chunk
    /// (3+ chunk) merge ordering. Includes diverse correction types
    /// (AMBIGUOUS, ERROR, REORDERING) to exercise correction preservation
    /// through the pipeline.
    /// </summary>
    public static AiApiResponse CreateThreeChunkResponse() => new()
    {
        Scratchpad = "wǒ zài chāoshì mǎicài . tā xuéxí zhōngwén . wǒmen qù kàn diànyǐng .",
        Corrections = new List<string>
        {
            "AMBIGUOUS: 余 → 餘 (line 1)",
            "ERROR: OCR mismatch '太' → '大' (line 2)",
            "REORDERING: '一百' spoken as 'bǎi yī' (line 3)",
        },
        ProcessedSentenceRows = new List<ProcessedSentenceRow>
        {
            CreateSupermarketRow(),
            CreateStudyRow(),
            CreateMovieRow(),
        },
    };

    /// <summary>
    /// Creates a realistic row for "我们 去 看 电影 。" — 5 semantic units
    /// (4 words + 1 punctuation).
    /// </summary>
    public static ProcessedSentenceRow CreateMovieRow() => new()
    {
        Chinese = "我们 去 看 电影 。",
        Pinyin = "wǒmen qù kàn diànyǐng .",
        English = "We go to watch a movie.",
        Breakdown = new List<BreakdownEntry>
        {
            CreateEntry("我们", "wǒmen", "we", "pron", "1"),
            CreateEntry("去", "qù", "go", "v", "1"),
            CreateEntry("看", "kàn", "watch/see", "v", "1"),
            CreateEntry("电影", "diànyǐng", "movie", "n", "3"),
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

    // ─── Audio reordering JSON factory ──────────────────────────────────

    /// <summary>
    /// Creates a realistic JSON string with audio reordering (R field
    /// carrying two reorder segments) for testing FillFromJsonResponse
    /// and the full parse → verify → format pipeline.
    /// </summary>
    public static string CreateAudioReorderJson()
    {
        var obj = new
        {
            SP = "wǒ yǒu yī bǎi kuài . bǎi yī",
            Corrections = Array.Empty<string>(),
            Sentences = new[]
            {
                new
                {
                    C = "我 有 一百 块 。",
                    P = "wǒ yǒu bǎiyī kuài .",
                    B = "我|wǒ|I|pron|1| ||有|yǒu|have|v|1| ||一百|bǎiyī|one hundred|num|2| ||块|kuài|yuan (currency)|mw|1| ||。|.|.|punc|N/A| ",
                    R = "bǎi yī|0,1||kuài|2,2",
                    E = "I have one hundred yuan.",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    // ─── Special punctuation JSON factory ───────────────────────────────

    /// <summary>
    /// Creates a realistic JSON string with diverse Chinese punctuation
    /// (enumeration comma 、, quotation marks 「」, ellipsis …) for testing
    /// the CleanChinesePunctuation sanitizer and ChinesePunctuationSanitizingConverter
    /// through the full pipeline.
    /// </summary>
    public static string CreateSpecialPunctuationJson()
    {
        var obj = new
        {
            SP = "tā shuō : 'nǐ hǎo' , zàijiàn ...",
            Corrections = Array.Empty<string>(),
            Sentences = new[]
            {
                new
                {
                    C = "他 说 ：「 你 好 」， 再见 … 。",
                    P = "tā shuō : 'nǐ hǎo' , zàijiàn ... .",
                    B = "他|tā|he|pron|1| ||说|shuō|say|v|2| ||：|:|:|punc|N/A| ||「|'|opening quote|punc|N/A| ||你|nǐ|you|pron|1| ||好|hǎo|good|adj|1| ||」|'|closing quote|punc|N/A| ||，|,|,|punc|N/A| ||再见|zàijiàn|goodbye|v|3| ||…|...|ellipsis|punc|N/A| ||。|.|.|punc|N/A| ",
                    R = "N/A",
                    E = "He said: 'Hello', goodbye... .",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    // ─── Neutral tone particles JSON factory ─────────────────────────────

    /// <summary>
    /// Creates a realistic JSON string with neutral tone particles (了,
    /// 吗, 的, 呢) whose pinyin carries tone marks. Useful for testing
    /// sandhi neutralization (RULE D) through the full pipeline.
    /// </summary>
    public static string CreateNeutralToneJson()
    {
        var obj = new
        {
            SP = "nǐ chī le fàn ma ? wǒ de péngyou ne ?",
            Corrections = Array.Empty<string>(),
            Sentences = new[]
            {
                new
                {
                    C = "你 吃 了 饭 吗 ？ 我 的 朋友 呢 ？",
                    P = "nǐ chī le fàn ma ? wǒ de péngyou ne ?",
                    B = "你|nǐ|you|pron|1| ||吃|chī|eat|v|1| ||了|le|completed action particle|part|1| ||饭|fàn|rice/meal|n|1| ||吗|ma|question particle|part|1| ||？|?|?|punc|N/A| ||我|wǒ|I|pron|1| ||的|de|possessive particle|part|1| ||朋友|péngyou|friend|n|3| ||呢|ne|question particle|part|1| ||？|?|?|punc|N/A| ",
                    R = "N/A",
                    E = "Have you eaten? What about my friend?",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }

    // ─── Three-chunk JSON factory (with diverse corrections) ─────────────

    /// <summary>
    /// Creates a realistic JSON string for three sentences with diverse
    /// correction types (AMBIGUOUS, ERROR, REORDERING). Useful for testing
    /// multi-chunk (3+ chunk) merge ordering and correction preservation
    /// through the full pipeline.
    /// </summary>
    public static string CreateThreeChunkJson()
    {
        var obj = new
        {
            SP = "wǒ zài chāoshì mǎicài . tā xuéxí zhōngwén . wǒmen qù kàn diànyǐng .",
            Corrections = new[]
            {
                "AMBIGUOUS: 余 → 餘 (line 1)",
                "ERROR: OCR mismatch '太' → '大' (line 2)",
                "REORDERING: '一百' spoken as 'bǎi yī' (line 3)",
            },
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
                new
                {
                    C = "我们 去 看 电影 。",
                    P = "wǒmen qù kàn diànyǐng .",
                    B = "我们|wǒmen|we|pron|1| ||去|qù|go|v|1| ||看|kàn|watch/see|v|1| ||电影|diànyǐng|movie|n|3| ||。|.|.|punc|N/A| ",
                    R = "N/A",
                    E = "We go to watch a movie.",
                },
            },
        };

        return JsonSerializer.Serialize(obj);
    }
}