# Copilot Instructions for AIFileProcessor

## Project Overview

AIFileProcessor is a .NET 10 solution that sends Chinese text to various LLM APIs (e.g., DeepSeek) for structured linguistic processing and then parses the JSON response.

## The Prompt

The file `Prompt/ChineseProcessingPrompt.txt` is **the system prompt** that this project dispatches to the LLM API. The C# code (primarily in `AIFileProcessor.Core/Services/ChineseTextProcessor.cs`) loads this prompt, sends it alongside chunked Chinese text to the API, and processes the structured JSON response that comes back.

Any changes to prompt behavior should be made in that file and coordinated with the C# code that parses the expected JSON schema.

When the user refers to "the prompt," "the system prompt," "the spec," or the "Chinese processing prompt," they are referring to **`Prompt/ChineseProcessingPrompt.txt`** unless otherwise stated.

## C# Member Ordering Standard

All C# type members (classes, records, structs) must be ordered to satisfy StyleCop rules SA1201/SA1202/SA1204. The ordering is **element-type first**, then access modifier, then static/instance:

1. **By element type (global, across all access modifiers):** constructors -> properties -> methods -> fields
2. **Within each element type, by access modifier:** `public` -> `protected` -> `internal` -> `private`
3. **Within each access group, static before instance:** static members first, then instance members
4. **Within each group, alphabetical order:** members are sorted alphabetically by name

For example, a class should be laid out as:

```
public class Example
{
    // 1. ALL constructors — public first, then protected, then internal, then private
    // 2. ALL properties — public, then protected, then internal, then private (alphabetical within each)
    // 3. ALL methods — public, then protected, then internal, then private
    //    Within each access group: static methods first (alphabetical), then instance methods (alphabetical, abstract before virtual before concrete)
    // 4. ALL fields — same sub-ordering as properties
}
```

When adding a new member, insert it at the correct position to maintain this ordering. Do not append to the end of the class by default.

## Always do a full rebuild when building the solution
When performing a build on the project, always do a Clean Solution, then a Rebuild Solution.
