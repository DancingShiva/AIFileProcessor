//-----------------------------------------------------------------------
// <copyright file="PinyinUtility.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Extensions
{
    using System;
    using System.Text;

    public enum Tone
    {
        /// <summary>
        /// The first tone is a high-level tone, often represented with a macron (e.g., ā).
        /// </summary>
        First = 1,

        /// <summary>
        /// The second tone is a rising tone, often represented with an acute accent (e.g., á).
        /// </summary>
        Second = 2,

        /// <summary>
        /// The third tone is a falling-rising tone, often represented with a caron (e.g., ǎ).
        /// </summary>
        Third = 3,

        /// <summary>
        /// The fourth tone is a falling tone, often represented with a grave accent (e.g., à).
        /// </summary>
        Fourth = 4,

        /// <summary>
        /// The neutral tone has no mark and is often light and quick.
        /// </summary>
        Neutral = 5,

        /// <summary>
        /// The unknown tone is used when the tone cannot be determined.
        /// </summary>
        Unknown = 0,
    }

    public static class PinyinUtility
    {
        /// <summary>
        /// Determines the tone of a given Pinyin syllable by inspecting its characters for tone marks.
        /// </summary>
        /// <param name="syllable">The Pinyin syllable to analyze.</param>
        /// <returns>The detected tone of the syllable.</returns>
        public static Tone GetTone(ReadOnlySpan<char> syllable)
        {
            bool hasVowel = false;

            foreach (char c in syllable)
            {
                switch (c)
                {
                    case 'ā' or 'ē' or 'ī' or 'ō' or 'ū' or 'ǖ':
                        return Tone.First;

                    case 'á' or 'é' or 'í' or 'ó' or 'ú' or 'ǘ':
                        return Tone.Second;

                    case 'ǎ' or 'ě' or 'ǐ' or 'ǒ' or 'ǔ' or 'ǚ':
                        return Tone.Third;

                    case 'à' or 'è' or 'ì' or 'ò' or 'ù' or 'ǜ':
                        return Tone.Fourth;

                    case 'a' or 'e' or 'i' or 'o' or 'u' or 'ü' or 'v':
                        hasVowel = true;
                        break;
                }
            }

            return hasVowel ? Tone.Neutral : Tone.Unknown;
        }

        public static string ShiftTone(string syllable, Tone targetTone)
        {
            string baseSyllable = StripTone(syllable);
            if (string.IsNullOrWhiteSpace(baseSyllable))
            {
                return string.Empty;
            }

            int tone = ResolveToneNumber(targetTone);
            if (tone is 0 or 5)
            {
                return baseSyllable; // neutral/no tone mark
            }

            int vowelIndex = FindToneVowelIndex(baseSyllable);
            if (vowelIndex < 0)
            {
                return baseSyllable; // no vowel to mark
            }

            char accented = ApplyTone(baseSyllable[vowelIndex], tone);
            if (accented == baseSyllable[vowelIndex])
            {
                return baseSyllable;
            }

            char[] chars = baseSyllable.ToCharArray();
            chars[vowelIndex] = accented;
            return new string(chars);
        }

        /// <summary>
        /// Removes the tone marks from a Pinyin syllable, leaving only the base letters.
        /// </summary>
        /// <param name="syllable">The Pinyin syllable with tone marks.</param>
        /// <returns>The Pinyin syllable without tone marks.</returns>
        public static string StripTone(string syllable)
        {
            if (string.IsNullOrEmpty(syllable))
            {
                return string.Empty;
            }

            string normalized = syllable.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                // Explicitly strip only the four combining Pinyin tone marks (macron, acute, caron, grave)
                if (c is '\u0304' or '\u0301' or '\u030C' or '\u0300')
                {
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>
        /// Finds the index of the vowel in a Pinyin syllable that should receive the tone mark.
        /// </summary>
        /// <param name="syllable">The Pinyin syllable.</param>
        /// <returns>The index of the vowel to be accented, or -1 if no suitable vowel is found.</returns>
        private static int FindToneVowelIndex(string syllable)
        {
            string lower = syllable.ToLowerInvariant();

            int a = lower.IndexOf('a');
            if (a >= 0) { return a; }

            int o = lower.IndexOf('o');
            if (o >= 0) { return o; }

            int e = lower.IndexOf('e');
            if (e >= 0) { return e; }

            int i = lower.IndexOf('i');
            int u = lower.IndexOf('u');
            if (i >= 0 && u >= 0)
            {
                // If both i and u exist, accent the latter one.
                return i > u ? i : u;
            }

            if (i >= 0) { return i; }
            if (u >= 0) { return u; }

            int umlautU = lower.IndexOf('ü');
            if (umlautU >= 0) { return umlautU; }

            return -1;
        }

        /// <summary>
        /// Shifts the given vowel character to its accented form based on the specified tone number.
        /// </summary>
        /// <param name="vowel">The vowel character to be accented.</param>
        /// <param name="tone">The tone number (1-4) to apply.</param>
        /// <returns>The accented vowel character.</returns>
        private static char ApplyTone(char vowel, int tone)
        {
            return (char.ToLowerInvariant(vowel), tone) switch
            {
                ('a', 1) => char.IsUpper(vowel) ? 'Ā' : 'ā',
                ('a', 2) => char.IsUpper(vowel) ? 'Á' : 'á',
                ('a', 3) => char.IsUpper(vowel) ? 'Ǎ' : 'ǎ',
                ('a', 4) => char.IsUpper(vowel) ? 'À' : 'à',

                ('o', 1) => char.IsUpper(vowel) ? 'Ō' : 'ō',
                ('o', 2) => char.IsUpper(vowel) ? 'Ó' : 'ó',
                ('o', 3) => char.IsUpper(vowel) ? 'Ǒ' : 'ǒ',
                ('o', 4) => char.IsUpper(vowel) ? 'Ò' : 'ò',

                ('e', 1) => char.IsUpper(vowel) ? 'Ē' : 'ē',
                ('e', 2) => char.IsUpper(vowel) ? 'É' : 'é',
                ('e', 3) => char.IsUpper(vowel) ? 'Ě' : 'ě',
                ('e', 4) => char.IsUpper(vowel) ? 'È' : 'è',

                ('i', 1) => char.IsUpper(vowel) ? 'Ī' : 'ī',
                ('i', 2) => char.IsUpper(vowel) ? 'Í' : 'í',
                ('i', 3) => char.IsUpper(vowel) ? 'Ǐ' : 'ǐ',
                ('i', 4) => char.IsUpper(vowel) ? 'Ì' : 'ì',

                ('u', 1) => char.IsUpper(vowel) ? 'Ū' : 'ū',
                ('u', 2) => char.IsUpper(vowel) ? 'Ú' : 'ú',
                ('u', 3) => char.IsUpper(vowel) ? 'Ǔ' : 'ǔ',
                ('u', 4) => char.IsUpper(vowel) ? 'Ù' : 'ù',

                ('ü', 1) => char.IsUpper(vowel) ? 'Ǖ' : 'ǖ',
                ('ü', 2) => char.IsUpper(vowel) ? 'Ǘ' : 'ǘ',
                ('ü', 3) => char.IsUpper(vowel) ? 'Ǚ' : 'ǚ',
                ('ü', 4) => char.IsUpper(vowel) ? 'Ǜ' : 'ǜ',

                _ => vowel,
            };
        }

        /// <summary>
        /// Resolves the tone number from the given Tone enum value, using both numeric values and naming conventions.
        /// </summary>
        /// <param name="targetTone">The Tone enum value to resolve.</param>
        /// <returns>The corresponding tone number (0-5).</returns>
        private static int ResolveToneNumber(Tone targetTone)
        {
            // Prefer numeric enum backing if available.
            int numeric = Convert.ToInt32(targetTone);
            if (numeric is >= 0 and <= 5)
            {
                return numeric;
            }

            // Fallback by enum name conventions.
            string name = targetTone.ToString().ToLowerInvariant();
            if (name.Contains("none") || name.Contains("neutral")) { return 0; }
            if (name.Contains("first") || name.EndsWith("1")) { return 1; }
            if (name.Contains("second") || name.EndsWith("2")) { return 2; }
            if (name.Contains("third") || name.EndsWith("3")) { return 3; }
            if (name.Contains("fourth") || name.EndsWith("4")) { return 4; }

            return 0;
        }
    }
}