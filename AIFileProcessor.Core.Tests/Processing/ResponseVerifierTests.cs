//-----------------------------------------------------------------------
// <copyright file="ResponseVerifierTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Processing;

using System.Collections.Generic;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using Xunit;

public class ResponseVerifierTests
{
    private static AiApiResponse CreateResponse(params ProcessedSentenceRow[] rows)
        => new() { ProcessedSentenceRows = new List<ProcessedSentenceRow>(rows) };

    private static ProcessedSentenceRow CreateRow(string chinese, params BreakdownEntry[] entries)
        => new() { Chinese = chinese, Breakdown = new List<BreakdownEntry>(entries) };

    private static BreakdownEntry Entry(string chinese, string pos, string hsk = "3", string note = "")
        => new() { Chinese = chinese, PartOfSpeech = pos, HskLevel = hsk, Note = note };

    // ─── Null/empty handling ──────────────────────────────────────────────────

    [Fact]
    public void Verify_NullResponse_ReturnsNoViolations()
    {
        var violations = ResponseVerifier.Verify(null!);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_EmptyRows_ReturnsNoViolations()
    {
        var violations = ResponseVerifier.Verify(new AiApiResponse());

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_RowWithNoBreakdown_ReturnsNoViolations()
    {
        var response = CreateResponse(new ProcessedSentenceRow { Chinese = "我 在 超市 买菜 。" });

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    // ─── Valid input ─────────────────────────────────────────────────────────

    [Fact]
    public void Verify_ValidResponse_ReturnsNoViolations()
    {
        var response = CreateResponse(CreateRow(
            "我 在 超市 买菜 。",
            Entry("我", "pron"),
            Entry("在", "prep"),
            Entry("超市", "n"),
            Entry("买菜", "v"),
            Entry("。", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_AllSpecPosValues_DoNotViolate()
    {
        string[] posValues = { "n", "v", "adj", "pron", "num", "mw", "adv", "prep", "conj", "part", "int", "loc", "t", "pn", "id", "punc" };
        var entries = new List<BreakdownEntry>();
        foreach (string pos in posValues)
        {
            entries.Add(Entry("字", pos, pos == "punc" ? "N/A" : "3", pos == "punc" ? " " : string.Empty));
        }

        var response = CreateResponse(new ProcessedSentenceRow
        {
            Chinese = string.Join(' ', new string[posValues.Length].Select((_, i) => $"字{i}")),
            Breakdown = entries,
        });
        // force segment-count neutral by matching C count to entries
        response.ProcessedSentenceRows[0].Chinese = "字 字 字 字 字 字 字 字 字 字 字 字 字 字 字 字";

        var violations = ResponseVerifier.Verify(response);

        Assert.DoesNotContain(violations, v => v.Contains("PS value not in the spec whitelist"));
    }

    // ─── PS whitelist ────────────────────────────────────────────────────────

    [Fact]
    public void Verify_InvalidPosValue_FlagsViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 跑 。",
            Entry("我", "pron"),
            Entry("跑", "VERB"), // not in whitelist
            Entry("。", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("PS value not in the spec whitelist") && v.Contains("'VERB'") && v.Contains("line 1") && v.Contains("segment 2"));
    }

    [Fact]
    public void Verify_UnknownLongFormPos_FlagsViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 跑",
            Entry("我", "pron"),
            Entry("跑", "Verb")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Contains(violations, v => v.Contains("PS value not in the spec whitelist"));
    }

    // ─── HSK range ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData("0")]
    [InlineData("10")]
    [InlineData("n/a")]
    [InlineData("")]
    [InlineData("3.5")]
    public void Verify_InvalidHskLevel_FlagsViolation(string badHsk)
    {
        var response = CreateResponse(CreateRow(
            "我 跑",
            Entry("我", "pron"),
            Entry("跑", "v", badHsk)));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("H value outside 1-9/N-A") && v.Contains($"'{badHsk}'"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("5")]
    [InlineData("9")]
    [InlineData("N/A")]
    public void Verify_ValidHskLevel_NoViolation(string goodHsk)
    {
        var response = CreateResponse(CreateRow("跑", Entry("跑", "v", goodHsk)));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    // ─── Punctuation 6-element format ─────────────────────────────────────────

    [Fact]
    public void Verify_PuncWithWrongNote_FlagsViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 。",
            Entry("我", "pron"),
            Entry("。", "punc", "N/A", "x")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("marked punc but its Note element is not empty or a single ASCII space"));
    }

    [Fact]
    public void Verify_PuncWithCorrectNote_NoViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 。",
            Entry("我", "pron"),
            Entry("。", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_PuncWithEmptyNote_NoViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 。",
            Entry("我", "pron"),
            Entry("。", "punc", "N/A", string.Empty)));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    // ─── Segment-count alignment ─────────────────────────────────────────────

    [Fact]
    public void Verify_SegmentCountMismatch_FlagsViolation()
    {
        // C tokenizes to 4 units, B has only 3
        var response = CreateResponse(CreateRow(
            "我 在 超市 。",
            Entry("我", "pron"),
            Entry("在", "prep"),
            Entry("超市", "n")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("segment count mismatch") && v.Contains("4") && v.Contains("3"));
    }

    [Fact]
    public void Verify_SegmentCountMatch_NoViolation()
    {
        var response = CreateResponse(CreateRow(
            "我 在 超市 。",
            Entry("我", "pron"),
            Entry("在", "prep"),
            Entry("超市", "n"),
            Entry("。", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    // ─── Fix (2): C-side tokenizer respects §6 bypass + §7 grouping ──────────

    [Fact]
    public void Verify_WordPunctWordGlued_NoFalsePositive()
    {
        // §6 spacing bypass glues 我，你 into one whitespace unit in C, but B has
        // three segments (word, comma, word). Old splitter undercounted C => 1.
        var response = CreateResponse(CreateRow(
            "我，你",
            Entry("我", "pron"),
            Entry("，", "punc", "N/A", " "),
            Entry("你", "pron")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_ContiguousPunctuationGrouped_NoFalsePositive()
    {
        // C: 你好。” glued (period+full-width close-quote); B groups into ONE segment (§7).
        var response = CreateResponse(CreateRow(
            "你好。”",
            Entry("你好", "adj"),
            Entry("。”", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_EllipsisGroupedAsOneUnit_NoFalsePositive()
    {
        // …… is two '…' code points but one Punctuation Set token; run groups to 1.
        var response = CreateResponse(CreateRow(
            "等等……！",
            Entry("等等", "v"),
            Entry("……！", "punc", "N/A", " ")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_GluedMismatchStillDetected()
    {
        // Real mismatch: C has 我，你 (→ 我,，,你 = 3 units) but B dropped the comma.
        var response = CreateResponse(CreateRow(
            "我，你",
            Entry("我", "pron"),
            Entry("你", "pron")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("segment count mismatch") && v.Contains("3") && v.Contains("2"));
    }

    [Fact]
    public void Verify_MultipleRows_LineNumbersAreCorrect()
    {
        var response = CreateResponse(
            CreateRow("我", Entry("我", "pron")),
            CreateRow("你 好", Entry("你", "pron"), Entry("好", "adj", "0")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Single(violations);
        Assert.Contains(violations, v => v.Contains("line 2"));
    }

    [Fact]
    public void Verify_MultipleViolations_AllReported()
    {
        var response = CreateResponse(CreateRow(
            "我 跑 跳",
            Entry("我", "XX"),
            Entry("跑", "v", "0"),
            Entry("跳", "v", "notanumber")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Equal(3, violations.Count);
    }

    [Fact]
    public void Verify_InflatedValuesWithRawPreserved_ReturnsNoViolations()
    {
        // Deserialization inflates wire values (pron→Pronoun, 1→HSK1); the verifier
        // must consult the preserved raw wire values and stay clean for valid input.
        var row = new ProcessedSentenceRow
        {
            Chinese = "我 喜欢 人工智能 。",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "我", PartOfSpeech = "Pronoun", RawPartOfSpeech = "pron", HskLevel = "HSK1", RawHskLevel = "1" },
                new() { Chinese = "喜欢", PartOfSpeech = "Verb", RawPartOfSpeech = "v", HskLevel = "HSK1", RawHskLevel = "1" },
                new() { Chinese = "人工智能", PartOfSpeech = "Noun", RawPartOfSpeech = "n", HskLevel = "HSK4", RawHskLevel = "4" },
                new() { Chinese = "。", PartOfSpeech = "Punctuation", RawPartOfSpeech = "punc", HskLevel = "N/A", RawHskLevel = "N/A", Note = " " },
            },
        };
        var response = CreateResponse(row);

        var violations = ResponseVerifier.Verify(response);

        Assert.Empty(violations);
    }

    [Fact]
    public void Verify_InflatedValuesWithoutRawValue_FlagViolation()
    {
        // If raw values weren't captured, the inflated display values are surfaced so the
        // operator sees what the model actually produced.
        var response = CreateResponse(CreateRow(
            "我 喜欢",
            Entry("我", "Pronoun", "HSK1"),
            Entry("喜欢", "Verb", "HSK1")));

        var violations = ResponseVerifier.Verify(response);

        Assert.Contains(violations, v => v.Contains("PS value not in the spec whitelist") && v.Contains("'Pronoun'"));
        Assert.Contains(violations, v => v.Contains("H value outside 1-9/N-A") && v.Contains("'HSK1'"));
    }
}
