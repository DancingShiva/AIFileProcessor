# Copilot Instructions for AIFileProcessor

## Project Overview

AIFileProcessor is a .NET 10 solution that sends Chinese text to various LLM APIs (e.g., DeepSeek) for structured linguistic processing and then parses the JSON response.

## The Prompt

The file `Prompt/ChineseProcessingPrompt.txt` is **the system prompt** that this project dispatches to the LLM API. The C# code (primarily in `AIFileProcessor.Core/Services/ChineseTextProcessor.cs`) loads this prompt, sends it alongside chunked Chinese text to the API, and processes the structured JSON response that comes back.

When the user refers to "the prompt," "the system prompt," "the spec," or "the Chinese processing prompt," they are referring to **`Prompt/ChineseProcessingPrompt.txt`** unless otherwise stated. Any changes to prompt behavior should be made in that file and coordinated with the C# code that parses the expected JSON schema.