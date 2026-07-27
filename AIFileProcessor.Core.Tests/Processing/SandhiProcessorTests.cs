//-----------------------------------------------------------------------
// <copyright file="SandhiProcessorTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests.Processing;

using System.Collections.Generic;
using AIFileProcessor.Processing;
using AIFileProcessor.Response;
using Xunit;

public class SandhiProcessorTests
{
    private static AiApiResponse CreateResponse(params ProcessedSentenceRow[] rows)
        => new() { ProcessedSentenceRows = new List<ProcessedSentenceRow>(rows) };

    private static ProcessedSentenceRow CreateRow(
        string chinese,
        string pinyin,
        params (string Chinese, string Pinyin, string Pos)[] breakdownEntries)
    {
        var breakdown = new List<BreakdownEntry>();
        foreach (var (ch, py, pos) in breakdownEntries)
        {
            breakdown.Add(new BreakdownEntry
            {
                Chinese = ch,
                Pinyin = py,
                PartOfSpeech = pos,
            });
        }

        return new ProcessedSentenceRow
        {
            Chinese = chinese,
            Pinyin = pinyin,
            Breakdown = breakdown,
        };
    }

    // ─── Null/empty handling ──────────────────────────────────────────────────

    [Fact]
    public void ApplySandhi_NullResponse_DoesNotThrow()
    {
        SandhiProcessor.ApplySandhi(null!);
    }

    [Fact]
    public void ApplySandhi_EmptyRows_DoesNotThrow()
    {
        var response = new AiApiResponse();
        SandhiProcessor.ApplySandhi(response);
    }

    [Fact]
    public void ApplySandhi_RowWithNoBreakdown_DoesNotThrow()
    {
        var response = CreateResponse(new ProcessedSentenceRow
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
        });

        SandhiProcessor.ApplySandhi(response);
    }

    // ─── Rule A: 3rd + 3rd tone → 2nd + 3rd tone ──────────────────────────────

    [Fact]
    public void ApplySandhi_ThirdThirdTone_FirstSyllableBecomesSecondTone()
    {
        // nǐ (3rd) + hǎo (3rd) → ní hǎo
        var row = new ProcessedSentenceRow
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "你", Pinyin = "nǐ", PartOfSpeech = "Pronoun" },
                new() { Chinese = "好", Pinyin = "hǎo", PartOfSpeech = "Adjective" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("ní", response.ProcessedSentenceRows[0].Breakdown[0].Pinyin);
        Assert.Equal("hǎo", response.ProcessedSentenceRows[0].Breakdown[1].Pinyin);
        Assert.Equal("ní hǎo", response.ProcessedSentenceRows[0].Pinyin);
    }

    // ─── Rule B: bù before 4th tone → bú ──────────────────────────────────────

    [Fact]
    public void ApplySandhi_BuBeforeFourthTone_BecomesSecondTone()
    {
        // bù (4th) + shì (4th) → bú shì
        var response = CreateResponse(CreateRow(
            "不是",
            "bù shì",
            ("不", "bù", "Adverb"),
            ("是", "shì", "Verb")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("bú", row.Breakdown[0].Pinyin);
        Assert.Equal("bú shì", row.Pinyin);
    }

    [Fact]
    public void ApplySandhi_BuBeforeThirdTone_KeepsOriginal()
    {
        // bù (4th) + hǎo (3rd) → bù hǎo (no change)
        var response = CreateResponse(CreateRow(
            "不好",
            "bù hǎo",
            ("不", "bù", "Adverb"),
            ("好", "hǎo", "Adjective")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("bù", row.Breakdown[0].Pinyin);
    }

    // ─── Rule C: yī before 4th tone (non-numeral) → yí ──────────────────────────

    [Fact]
    public void ApplySandhi_YiBeforeFourthToneNonNum_BecomesSecondTone()
    {
        // yī (1st) + gè (4th) → yí gè (non-numeral context)
        var response = CreateResponse(CreateRow(
            "一个",
            "yī gè",
            ("一", "yī", "Numeral"),
            ("个", "gè", "Measure Word")));

        // "Numeral" maps to "num", so yī should NOT shift here
        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        // Since POS is "Numeral" (maps to "num"), yī keeps its tone
        Assert.Equal("yī", row.Breakdown[0].Pinyin);
    }

    [Fact]
    public void ApplySandhi_YiBeforeFourthToneNonNumeral_BecomesSecondTone()
    {
        // yī (1st) + yàng (4th) → yí yàng (non-numeral context, POS="other")
        var response = CreateResponse(CreateRow(
            "一样",
            "yī yàng",
            ("一", "yī", "Adjective"),
            ("样", "yàng", "Noun")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("yí", row.Breakdown[0].Pinyin);
        Assert.Equal("yí yàng", row.Pinyin);
    }

    // ─── Rule D: Target particles get tone stripped ───────────────────────────

    [Fact]
    public void ApplySandhi_TargetParticleMa_StripsTone()
    {
        // mā + ma → mā ma (neutralized)
        var response = CreateResponse(CreateRow(
            "妈吗",
            "mā ma",
            ("妈", "mā", "Noun"),
            ("吗", "ma", "Particle")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        // "吗" is a target particle — tone should be stripped (already neutral, stays "ma")
        Assert.Equal("ma", row.Breakdown[1].Pinyin);
    }

    [Fact]
    public void ApplySandhi_TargetParticleNe_StripsTone()
    {
        // nǐ + ne → nǐ ne
        var response = CreateResponse(CreateRow(
            "你呢",
            "nǐ ne",
            ("你", "nǐ", "Pronoun"),
            ("呢", "ne", "Particle")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("ne", row.Breakdown[1].Pinyin);
    }

    // ─── Sentence-level pinyin consistency ─────────────────────────────────────

    [Fact]
    public void ApplySandhi_SentencePinyin_ReconstructedFromBreakdown()
    {
        var response = CreateResponse(CreateRow(
            "你好",
            "nǐ hǎo",
            ("你", "nǐ", "Pronoun"),
            ("好", "hǎo", "Adjective")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        // After sandhi: ní hǎo — sentence pinyin should match breakdown joined
        Assert.Equal("ní hǎo", row.Pinyin);
        Assert.Equal("ní hǎo", $"{row.Breakdown[0].Pinyin} {row.Breakdown[1].Pinyin}");
    }

    // ─── No change cases ───────────────────────────────────────────────────────

    [Fact]
    public void ApplySandhi_FirstToneSyllables_NoChange()
    {
        var response = CreateResponse(CreateRow(
            "妈妈",
            "mā mā",
            ("妈", "mā", "Noun"),
            ("妈", "mā", "Noun")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("mā", row.Breakdown[0].Pinyin);
        Assert.Equal("mā", row.Breakdown[1].Pinyin);
        Assert.Equal("mā mā", row.Pinyin);
    }

    [Fact]
    public void ApplySandhi_SecondToneSyllables_NoChange()
    {
        var response = CreateResponse(CreateRow(
            "人门",
            "rén mén",
            ("人", "rén", "Noun"),
            ("门", "mén", "Noun")));

        SandhiProcessor.ApplySandhi(response);

        var row = response.ProcessedSentenceRows[0];
        Assert.Equal("rén", row.Breakdown[0].Pinyin);
        Assert.Equal("mén", row.Breakdown[1].Pinyin);
    }

    // ─── Multiple sentences ────────────────────────────────────────────────────

    [Fact]
    public void ApplySandhi_MultipleSentences_EachTransformedIndependently()
    {
        var response = CreateResponse(
            CreateRow("你好", "nǐ hǎo",
                ("你", "nǐ", "Pronoun"),
                ("好", "hǎo", "Adjective")),
            CreateRow("不是", "bù shì",
                ("不", "bù", "Adverb"),
                ("是", "shì", "Verb")));

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("ní hǎo", response.ProcessedSentenceRows[0].Pinyin);
        Assert.Equal("bú shì", response.ProcessedSentenceRows[1].Pinyin);
    }

    // ─── Sandhi notes ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplySandhi_ThirdThirdTone_NoteAddedToChangedEntry()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "你", Pinyin = "nǐ", PartOfSpeech = "Pronoun" },
                new() { Chinese = "好", Pinyin = "hǎo", PartOfSpeech = "Adjective" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("(nǐ → ní before 3rd tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
        Assert.Equal(string.Empty, response.ProcessedSentenceRows[0].Breakdown[1].Note);
    }

    [Fact]
    public void ApplySandhi_BuBeforeFourthTone_NoteAdded()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "不是",
            Pinyin = "bù shì",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "不", Pinyin = "bù", PartOfSpeech = "Adverb" },
                new() { Chinese = "是", Pinyin = "shì", PartOfSpeech = "Verb" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("(bù → bú before 4th tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
    }

    [Fact]
    public void ApplySandhi_YiBeforeFourthToneNonNumeral_NoteAdded()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "一样",
            Pinyin = "yī yàng",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "一", Pinyin = "yī", PartOfSpeech = "Adjective" },
                new() { Chinese = "样", Pinyin = "yàng", PartOfSpeech = "Noun" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("(yī → yí before 4th tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
    }

    [Fact]
    public void ApplySandhi_YiBeforeThirdToneNonNumeral_NoteAdded()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "一起",
            Pinyin = "yī qǐ",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "一", Pinyin = "yī", PartOfSpeech = "Adverb" },
                new() { Chinese = "起", Pinyin = "qǐ", PartOfSpeech = "Verb" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("(yī → yì before 3rd tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
    }

    [Fact]
    public void ApplySandhi_TargetParticleWithTone_NoteAdded()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "好吗",
            Pinyin = "hǎo mǎ",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "好", Pinyin = "hǎo", PartOfSpeech = "Adjective" },
                new() { Chinese = "吗", Pinyin = "mǎ", PartOfSpeech = "Particle" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        // "吗" is a target particle — tone should be stripped and note added
        Assert.Equal("ma", response.ProcessedSentenceRows[0].Breakdown[1].Pinyin);
        Assert.Equal("(mǎ → ma neutral particle)", response.ProcessedSentenceRows[0].Breakdown[1].Note);
    }

    [Fact]
    public void ApplySandhi_NoTransformation_NoNoteAdded()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "妈妈",
            Pinyin = "mā mā",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "妈", Pinyin = "mā", PartOfSpeech = "Noun" },
                new() { Chinese = "妈", Pinyin = "mā", PartOfSpeech = "Noun" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal(string.Empty, response.ProcessedSentenceRows[0].Breakdown[0].Note);
        Assert.Equal(string.Empty, response.ProcessedSentenceRows[0].Breakdown[1].Note);
    }

    [Fact]
    public void ApplySandhi_ExistingNote_SandhiNoteAppendedWithSpace()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "你", Pinyin = "nǐ", PartOfSpeech = "Pronoun", Note = "existing note" },
                new() { Chinese = "好", Pinyin = "hǎo", PartOfSpeech = "Adjective" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("existing note (nǐ → ní before 3rd tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
    }

    [Fact]
    public void ApplySandhi_EmptyNote_SandhiNoteNotPrependedWithSpace()
    {
        var row = new ProcessedSentenceRow
        {
            Chinese = "你好",
            Pinyin = "nǐ hǎo",
            Breakdown = new List<BreakdownEntry>
            {
                new() { Chinese = "你", Pinyin = "nǐ", PartOfSpeech = "Pronoun", Note = "" },
                new() { Chinese = "好", Pinyin = "hǎo", PartOfSpeech = "Adjective" },
            },
        };
        var response = CreateResponse(row);

        SandhiProcessor.ApplySandhi(response);

        Assert.Equal("(nǐ → ní before 3rd tone)", response.ProcessedSentenceRows[0].Breakdown[0].Note);
    }
}