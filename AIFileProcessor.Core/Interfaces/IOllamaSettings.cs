//-----------------------------------------------------------------------
// <copyright file="IOllamaSettings.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for Ollama configuration settings. Ollama fronts both locally-run
    /// models and models hosted in Ollama Cloud behind the same REST API, so a
    /// single endpoint plus an optional API key covers both scenarios. The API key
    /// is only required when calling Ollama Cloud; local servers ignore it.
    /// </summary>
    public interface IOllamaSettings : IAiServiceSettings
    {
        /// <summary>
        /// Gets the API key for Ollama Cloud. Optional: required only when calling
        /// cloud-hosted models; local Ollama servers typically require no
        /// authentication. Never stored in per-user settings files.
        /// </summary>
        string ApiKey { get; init; }

        /// <summary>
        /// Gets or sets the base endpoint for the Ollama server. Defaults to the
        /// local server at <c>http://localhost:11434</c>; point this at
        /// <c>https://ollama.com</c> (or a proxy) for Ollama Cloud.
        /// </summary>
        string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the name of the model to use for Ollama API calls (e.g.,
        /// <c>llama3.1</c>, <c>qwen2.5:32b</c>). For cloud models this is the cloud
        /// model name (e.g., <c>deepseek-v3.1:671b-cloud</c>).
        /// </summary>
        string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the timeout for Ollama API calls in seconds. Local models
        /// can take substantially longer to generate than hosted APIs, so a longer
        /// timeout is common.
        /// </summary>
        int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate for each Ollama
        /// API call (maps to the <c>num_predict</c> model option). Null leaves the
        /// server default in effect.
        /// </summary>
        int? MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the sampling temperature for Ollama API calls. Controls the
        /// randomness of the generated text; null leaves the model's Modelfile
        /// default in effect.
        /// </summary>
        double? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the nucleus-sampling threshold (maps to the <c>top_p</c>
        /// model option). Null leaves the model's Modelfile default in effect.
        /// </summary>
        double? TopP { get; set; }

        /// <summary>
        /// Gets or sets the number of highest-probability tokens to sample from
        /// (maps to the <c>top_k</c> model option). Null leaves the model's
        /// Modelfile default in effect.
        /// </summary>
        int? TopK { get; set; }

        /// <summary>
        /// Gets or sets the context window size (maps to the <c>num_ctx</c> model
        /// option). Null lets the server use the model's trained context length.
        /// </summary>
        int? ContextWindow { get; set; }

        /// <summary>
        /// Gets or sets the repetition penalty (maps to the <c>repeat_penalty</c>
        /// model option). Null leaves the model's Modelfile default in effect.
        /// </summary>
        double? RepeatPenalty { get; set; }

        /// <summary>
        /// Gets or sets the random seed for reproducible output (maps to the
        /// <c>seed</c> model option). Null uses a random seed per call.
        /// </summary>
        int? Seed { get; set; }

        /// <summary>
        /// Gets or sets stop sequences that halt generation (maps to the
        /// <c>stop</c> model option). Null or empty disables explicit stop
        /// sequences.
        /// </summary>
        IList<string>? Stop { get; set; }

        /// <summary>
        /// Gets or sets the minimum-probability sampling threshold (maps to the
        /// <c>min_p</c> model option). Null leaves the model's Modelfile default
        /// in effect.
        /// </summary>
        double? MinP { get; set; }

        /// <summary>
        /// Gets or sets the typical-P sampling target (maps to the <c>typical_p</c>
        /// model option). Null leaves the model's Modelfile default in effect.
        /// </summary>
        double? TypicalP { get; set; }

        /// <summary>
        /// Gets or sets the tail-free sampling strength (maps to the <c>tfs_z</c>
        /// model option). Null leaves the model's Modelfile default in effect.
        /// </summary>
        double? TfsZ { get; set; }

        /// <summary>
        /// Gets or sets the Mirostat mode (maps to the <c>mirostat</c> model option):
        /// 0 = disabled, 1 = Mirostat 1.0, 2 = Mirostat 2.0. Null leaves the
        /// model's Modelfile default in effect.
        /// </summary>
        int? MiroStat { get; set; }

        /// <summary>
        /// Gets or sets the Mirostat target entropy (maps to the <c>mirostat_tau</c>
        /// model option) when Mirostat is enabled. Null leaves the model's default
        /// in effect.
        /// </summary>
        double? MiroStatTau { get; set; }

        /// <summary>
        /// Gets or sets the Mirostat learning rate (maps to the <c>mirostat_eta</c>
        /// model option) when Mirostat is enabled. Null leaves the model's default
        /// in effect.
        /// </summary>
        double? MiroStatEta { get; set; }

        /// <summary>
        /// Gets or sets the penalty applied to tokens already present in the prompt
        /// (maps to the <c>presence_penalty</c> model option). Null leaves the
        /// model's Modelfile default in effect.
        /// </summary>
        double? PresencePenalty { get; set; }

        /// <summary>
        /// Gets or sets the penalty applied to tokens in proportion to their
        /// existing frequency (maps to the <c>frequency_penalty</c> model option).
        /// Null leaves the model's Modelfile default in effect.
        /// </summary>
        double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Gets or sets how far back the model looks to prevent repetition (maps
        /// to the <c>repeat_last_n</c> model option; 0 disables, -1 = num_ctx).
        /// Null leaves the model's Modelfile default in effect.
        /// </summary>
        int? RepeatLastN { get; set; }

        /// <summary>
        /// Gets or sets the number of transformer layers to offload to the GPU
        /// (maps to the <c>num_gpu</c> model option). Null lets the server decide;
        /// 0 disables GPU offload.
        /// </summary>
        int? NumGpu { get; set; }

        /// <summary>
        /// Gets or sets the prompt-processing batch size (maps to the
        /// <c>num_batch</c> model option). Null leaves the model's Modelfile
        /// default in effect.
        /// </summary>
        int? NumBatch { get; set; }

        /// <summary>
        /// Gets or sets the number of CPU threads used during computation (maps
        /// to the <c>num_thread</c> model option). Null lets the server choose
        /// (recommended: physical core count).
        /// </summary>
        int? NumThread { get; set; }

        /// <summary>
        /// Gets or sets the number of tokens to keep from the initial prompt (maps
        /// to the <c>num_keep</c> model option; -1 = all). Null leaves the model's
        /// Modelfile default in effect.
        /// </summary>
        int? NumKeep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use NUMA optimizations for
        /// multi-socket systems (maps to the <c>numa</c> model option). Null
        /// leaves the model's Modelfile default in effect.
        /// </summary>
        bool? Numa { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to lock model memory to prevent
        /// swapping (maps to the <c>use_mlock</c> model option; requires elevated
        /// privileges on some platforms). Null leaves the model's Modelfile
        /// default in effect.
        /// </summary>
        bool? UseMlock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to memory-map the model file
        /// (maps to the <c>use_mmap</c> model option; disabling loads the entire
        /// model into RAM). Null leaves the model's Modelfile default in effect.
        /// </summary>
        bool? UseMmap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reduce VRAM usage at the
        /// cost of performance (maps to the <c>low_vram</c> model option). Null
        /// leaves the model's Modelfile default in effect.
        /// </summary>
        bool? LowVram { get; set; }

        /// <summary>
        /// Gets or sets how long the model stays loaded in memory after the request
        /// (the Ollama <c>keep_alive</c> parameter, e.g., "5m", "-1" to keep
        /// loaded, "0" to unload immediately). Null uses the server default.
        /// </summary>
        string? KeepAlive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the model's thinking
        /// mode (the Ollama <c>think</c> parameter). Only meaningful for models
        /// that advertise the thinking capability; null leaves the behavior to the
        /// server.
        /// </summary>
        bool? EnableThinking { get; set; }

        /// <summary>
        /// Gets or sets the reasoning effort level for thinking-capable Ollama
        /// models. Null if not configured; mapped onto the Ollama <c>think</c>
        /// parameter for models that support it.
        /// </summary>
        Models.ReasoningLevel? ReasoningLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries after the initial attempt
        /// for transient failures.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the base delay, in seconds, before the first retry;
        /// doubled each subsequent retry.
        /// </summary>
        int BaseDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum delay, in seconds, between retries.
        /// </summary>
        int MaxDelay { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary request headers to include when calling the
        /// provider (e.g., proxy or gateway headers).
        /// </summary>
        IDictionary<string, string?>? RequestHeaders { get; set; }
    }
}
