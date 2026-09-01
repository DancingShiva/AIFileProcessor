//-----------------------------------------------------------------------
// <copyright file="SettingsForm.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Services;

    /// <summary>
    /// Serves as the configuration settings dialog, allowing users to modify
    /// and persist non-secret preferences for Gemini, OpenAI, and DeepSeek.
    /// API keys are configured via environment variables on the machine and
    /// are not editable from this dialog.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly IEnumerable<IAiService> _aiServices;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private Dictionary<string, ReasoningLevel> _modelReasoningLevels = [];
        private Dictionary<string, int> _modelMaxTokens = [];
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _geminiModelChunking = [];
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _openAiModelChunking = [];
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _deepSeekModelChunking = [];
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _ollamaModelChunking = [];
        private ProcessingSettings _processingSettings = new();

        public SettingsForm(
            IEnumerable<IAiService> aiServices,
            ISettingsProvider settingsProvider,
            IUserContext userContext)
        {
            InitializeComponent();

            _aiServices = aiServices ?? throw new ArgumentNullException(nameof(aiServices));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

            PopulateModelDropdowns(aiServices);

            // Update numeric control maximums when the selected model changes so the UI reflects
            // provider/model hard-limits. Handlers update the corresponding NumericUpDown.Maximum.
            this.ddlGeminiModel.SelectedIndexChanged += (s, e) =>
            {
                UpdateGeminiMaxTokensLimit();
                UpdateGeminiReasoningLevels();
                UpdateGeminiMaxTokensValue();
                UpdateGeminiChunkingValue();
            };
            this.ddlOpenAiModel.SelectedIndexChanged += (s, e) =>
            {
                UpdateOpenAiMaxTokensLimit();
                UpdateOpenAiReasoningLevels();
                UpdateOpenAiMaxTokensValue();
                UpdateOpenAiChunkingValue();
            };
            this.ddlDeepSeekModel.SelectedIndexChanged += (s, e) =>
            {
                UpdateDeepSeekMaxTokensLimit();
                UpdateDeepSeekReasoningLevels();
                UpdateDeepSeekMaxTokensValue();
                UpdateDeepSeekChunkingValue();
            };
            this.ddlOllamaModel.SelectedIndexChanged += (s, e) =>
            {
                UpdateOllamaMaxTokensLimit();
                UpdateOllamaReasoningLevels();
                UpdateOllamaMaxTokensValue();
                UpdateOllamaChunkingValue();
            };
        }

        private static void FillComboBox(ComboBox comboBox, List<AiModelInfo> models)
        {
            comboBox.DisplayMember = nameof(AiModelInfo.ModelName);
            comboBox.ValueMember = nameof(AiModelInfo.ModelId);
            comboBox.Items.AddRange([.. models]);
        }

        private static void SelectModelByModelId(ComboBox comboBox, string? modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return;
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is AiModelInfo model && model.ModelId == modelId)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private static void PopulateReasoningDropdown(ComboBox comboBox, string? modelId, IEnumerable<IAiService> aiServices)
        {
            comboBox.Items.Clear();
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return;
            }

            foreach (var service in aiServices)
            {
                if (service.SupportsModel(modelId))
                {
                    foreach (var level in service.GetSupportedReasoningLevels(modelId))
                    {
                        comboBox.Items.Add(level);
                    }

                    if (comboBox.Items.Count > 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    break;
                }
            }
        }

        private static string? GetModelId(ComboBox comboBox)
        {
            return comboBox.SelectedItem is AiModelInfo model ? model.ModelId : null;
        }

        private static void SaveChunkingForModel(NumericUpDown nudChunkSize, NumericUpDown nudMaxConcurrent, Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> store, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId))
            {
                store[modelId] = ((int)nudChunkSize.Value, (int)nudMaxConcurrent.Value);
            }
        }

        private void SelectReasoningLevelForModel(ComboBox comboBox, string? modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId) || !_modelReasoningLevels.TryGetValue(modelId, out var level))
            {
                return;
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ReasoningLevel item && item == level)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SaveReasoningLevelForModel(ComboBox comboBox, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId) && comboBox.SelectedItem is ReasoningLevel level)
            {
                _modelReasoningLevels[modelId] = level;
            }
        }

        private void UpdateMaxTokensForModel(NumericUpDown nud, CheckBox chkNull, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId) && _modelMaxTokens.TryGetValue(modelId, out var maxTokens))
            {
                int clamped = Math.Min(maxTokens, (int)nud.Maximum);
                nud.Value = clamped;
                chkNull.Checked = false;
            }
            else
            {
                nud.Value = 0;
                chkNull.Checked = true;
            }
        }

        private void SaveMaxTokensForModel(NumericUpDown nud, CheckBox chkNull, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId) && !chkNull.Checked)
            {
                _modelMaxTokens[modelId] = (int)nud.Value;
            }
        }

        private void UpdateGeminiMaxTokensValue()
        {
            UpdateMaxTokensForModel(nudGeminiMaxTokens, chkGeminiMaxTokensNull, GetModelId(ddlGeminiModel));
        }

        private void UpdateOpenAiMaxTokensValue()
        {
            UpdateMaxTokensForModel(nudOpenAiMaxTokens, chkOpenAiMaxTokensNull, GetModelId(ddlOpenAiModel));
        }

        private void UpdateDeepSeekMaxTokensValue()
        {
            UpdateMaxTokensForModel(nudDeepSeekMaxTokens, chkDeepSeekMaxTokensNull, GetModelId(ddlDeepSeekModel));
        }

        private void UpdateOllamaMaxTokensValue()
        {
            UpdateMaxTokensForModel(nudOllamaMaxTokens, chkOllamaMaxTokensNull, GetModelId(ddlOllamaModel));
        }

        private void UpdateChunkingForModel(NumericUpDown nudChunkSize, NumericUpDown nudMaxConcurrent, Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> store, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId) && store.TryGetValue(modelId, out var values))
            {
                nudChunkSize.Value = Math.Clamp(values.ChunkSize, (int)nudChunkSize.Minimum, (int)nudChunkSize.Maximum);
                nudMaxConcurrent.Value = Math.Clamp(values.MaxConcurrentChunks, (int)nudMaxConcurrent.Minimum, (int)nudMaxConcurrent.Maximum);
            }
            else
            {
                nudChunkSize.Value = Math.Min(_processingSettings.ChunkSizeDefault, nudChunkSize.Maximum);
                nudMaxConcurrent.Value = Math.Min(_processingSettings.MaxConcurrentChunksDefault, nudMaxConcurrent.Maximum);
            }
        }

        private void UpdateGeminiChunkingValue()
        {
            UpdateChunkingForModel(nudGeminiChunkSize, nudGeminiMaxConcurrentChunks, _geminiModelChunking, GetModelId(ddlGeminiModel));
        }

        private void UpdateOpenAiChunkingValue()
        {
            UpdateChunkingForModel(nudOpenAiChunkSize, nudOpenAiMaxConcurrentChunks, _openAiModelChunking, GetModelId(ddlOpenAiModel));
        }

        private void UpdateDeepSeekChunkingValue()
        {
            UpdateChunkingForModel(nudDeepSeekChunkSize, nudDeepSeekMaxConcurrentChunks, _deepSeekModelChunking, GetModelId(ddlDeepSeekModel));
        }

        private void UpdateOllamaChunkingValue()
        {
            UpdateChunkingForModel(nudOllamaChunkSize, nudOllamaMaxConcurrentChunks, _ollamaModelChunking, GetModelId(ddlOllamaModel));
        }

        private void PopulateModelDropdowns(IEnumerable<IAiService> aiServices)
        {
            foreach (var service in aiServices)
            {
                var models = service.GetSupportedModels().ToList();
                if (models.Count == 0)
                {
                    continue;
                }

                if (service is GeminiService)
                {
                    FillComboBox(ddlGeminiModel, models);
                }
                else if (service is OpenAiService)
                {
                    FillComboBox(ddlOpenAiModel, models);
                }
                else if (service is DeepSeekService)
                {
                    FillComboBox(ddlDeepSeekModel, models);
                }
                else if (service is OllamaService)
                {
                    FillComboBox(ddlOllamaModel, models);
                }
            }
        }

        private void UpdateGeminiReasoningLevels()
        {
            var modelId = ddlGeminiModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            PopulateReasoningDropdown(ddlGeminiReasoning, modelId, _aiServices);
        }

        private void UpdateOpenAiReasoningLevels()
        {
            var modelId = ddlOpenAiModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            PopulateReasoningDropdown(ddlOpenAiReasoning, modelId, _aiServices);
        }

        private void UpdateDeepSeekReasoningLevels()
        {
            var modelId = ddlDeepSeekModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            PopulateReasoningDropdown(ddlDeepSeekReasoning, modelId, _aiServices);
        }

        private void UpdateOllamaReasoningLevels()
        {
            var modelId = ddlOllamaModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            PopulateReasoningDropdown(ddlOllamaReasoning, modelId, _aiServices);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Fire-and-forget: LoadSettingsAsync handles its own errors via a MessageBox.
            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                // Load global processing settings (defaults + caps) and bound the per-model controls.
                _processingSettings = await _settingsProvider.GetProcessingSettingsAsync().ConfigureAwait(true);
                nudGeminiChunkSize.Maximum = Math.Max(nudGeminiChunkSize.Maximum, _processingSettings.ChunkSizeCap);
                nudGeminiMaxConcurrentChunks.Maximum = Math.Max(nudGeminiMaxConcurrentChunks.Maximum, _processingSettings.MaxConcurrentChunksCap);
                nudOpenAiChunkSize.Maximum = Math.Max(nudOpenAiChunkSize.Maximum, _processingSettings.ChunkSizeCap);
                nudOpenAiMaxConcurrentChunks.Maximum = Math.Max(nudOpenAiMaxConcurrentChunks.Maximum, _processingSettings.MaxConcurrentChunksCap);
                nudDeepSeekChunkSize.Maximum = Math.Max(nudDeepSeekChunkSize.Maximum, _processingSettings.ChunkSizeCap);
                nudDeepSeekMaxConcurrentChunks.Maximum = Math.Max(nudDeepSeekMaxConcurrentChunks.Maximum, _processingSettings.MaxConcurrentChunksCap);
                nudOllamaChunkSize.Maximum = Math.Max(nudOllamaChunkSize.Maximum, _processingSettings.ChunkSizeCap);
                nudOllamaMaxConcurrentChunks.Maximum = Math.Max(nudOllamaMaxConcurrentChunks.Maximum, _processingSettings.MaxConcurrentChunksCap);

                // Refresh the Ollama model catalog from the live server before loading settings,
                // so the dropdown reflects actually-installed local models plus configured cloud models.
                var ollamaService = _aiServices.OfType<OllamaService>().FirstOrDefault();
                if (ollamaService != null)
                {
                    await ollamaService.RefreshModelCatalogAsync().ConfigureAwait(true);
                    ddlOllamaModel.Items.Clear();
                    FillComboBox(ddlOllamaModel, [.. ollamaService.GetSupportedModels()]);
                }

                // Load per-model reasoning levels and MaxTokens from ProcessingOptions (single source of truth)
                var processingOptions = await _settingsProvider.GetProcessingOptionsAsync(_userContext.UserId).ConfigureAwait(true);
                if (processingOptions.ModelReasoningLevels != null)
                {
                    _modelReasoningLevels = processingOptions.ModelReasoningLevels;
                }

                if (processingOptions.ModelMaxTokens != null)
                {
                    _modelMaxTokens = processingOptions.ModelMaxTokens;
                }

                // Load Gemini Settings
                var gemini = await _settingsProvider.GetGeminiSettingsAsync(_userContext.UserId).ConfigureAwait(true);
                SelectModelByModelId(ddlGeminiModel, gemini.ModelName);
                UpdateGeminiReasoningLevels();
                SelectReasoningLevelForModel(ddlGeminiReasoning, GetModelId(ddlGeminiModel));
                nudGeminiTimeout.Value = gemini.TimeoutSeconds;
                txtGeminiProjectId.Text = gemini.ProjectId ?? string.Empty;
                txtGeminiLocation.Text = gemini.Location ?? string.Empty;

                // Ensure UI control maximum matches provider/model hard-limit before assigning value.
                nudGeminiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("Gemini", GetModelId(ddlGeminiModel));
                UpdateMaxTokensForModel(nudGeminiMaxTokens, chkGeminiMaxTokensNull, GetModelId(ddlGeminiModel));
                nudGeminiTemp.Value = (decimal)(gemini.Temperature ?? 0.0);
                chkGeminiTempNull.Checked = !gemini.Temperature.HasValue;
                nudGeminiTopP.Value = (decimal)(gemini.TopP ?? 0.0);
                chkGeminiTopPNull.Checked = !gemini.TopP.HasValue;
                chkGeminiStreaming.Checked = gemini.StreamingEnabled;
                nudGeminiMaxRetries.Value = gemini.MaxRetries;
                nudGeminiBaseDelay.Value = gemini.BaseDelay;
                nudGeminiMaxDelay.Value = gemini.MaxDelay;
                chkGeminiEnableLogging.Checked = gemini.EnableLogging;
                nudGeminiRateLimit.Value = gemini.RateLimitPerMinute ?? 0;
                chkGeminiRateLimitNull.Checked = !gemini.RateLimitPerMinute.HasValue;
                chkGeminiUseManagedIdentity.Checked = gemini.UseManagedIdentity;
                txtGeminiCredentialType.Text = gemini.CredentialType ?? string.Empty;

                // Load per-model chunking for the selected Gemini model
                var geminiModelId = GetModelId(ddlGeminiModel);
                if (!string.IsNullOrWhiteSpace(geminiModelId))
                {
                    var gm = await _settingsProvider.GetGeminiModelSettingsAsync(geminiModelId, _userContext.UserId).ConfigureAwait(true);
                    _geminiModelChunking[geminiModelId] = (gm.ChunkSize ?? _processingSettings.ChunkSizeDefault, gm.MaxConcurrentChunks ?? _processingSettings.MaxConcurrentChunksDefault);
                    UpdateGeminiChunkingValue();
                }

                // Load OpenAI Settings
                var openAi = await _settingsProvider.GetOpenAiSettingsAsync(_userContext.UserId).ConfigureAwait(true);
                SelectModelByModelId(ddlOpenAiModel, openAi.ModelName);
                UpdateOpenAiReasoningLevels();
                SelectReasoningLevelForModel(ddlOpenAiReasoning, GetModelId(ddlOpenAiModel));
                nudOpenAiTimeout.Value = openAi.TimeoutSeconds;
                txtOpenAiOrgId.Text = openAi.OrganizationId ?? string.Empty;
                nudOpenAiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("OpenAi", GetModelId(ddlOpenAiModel));
                UpdateMaxTokensForModel(nudOpenAiMaxTokens, chkOpenAiMaxTokensNull, GetModelId(ddlOpenAiModel));
                nudOpenAiTemp.Value = (decimal)(openAi.Temperature ?? 0.0);
                chkOpenAiTempNull.Checked = !openAi.Temperature.HasValue;

                // Load per-model chunking for the selected OpenAI model
                var openAiModelId = GetModelId(ddlOpenAiModel);
                if (!string.IsNullOrWhiteSpace(openAiModelId))
                {
                    var om = await _settingsProvider.GetOpenAiModelSettingsAsync(openAiModelId, _userContext.UserId).ConfigureAwait(true);
                    _openAiModelChunking[openAiModelId] = (om.ChunkSize ?? _processingSettings.ChunkSizeDefault, om.MaxConcurrentChunks ?? _processingSettings.MaxConcurrentChunksDefault);
                    UpdateOpenAiChunkingValue();
                }

                // Load DeepSeek Settings
                var deepSeek = await _settingsProvider.GetDeepSeekSettingsAsync(_userContext.UserId).ConfigureAwait(true);
                SelectModelByModelId(ddlDeepSeekModel, deepSeek.ModelName);
                UpdateDeepSeekReasoningLevels();
                SelectReasoningLevelForModel(ddlDeepSeekReasoning, GetModelId(ddlDeepSeekModel));
                nudDeepSeekTimeout.Value = deepSeek.TimeoutSeconds;
                nudDeepSeekMaxTokens.Maximum = ModelTokenLimits.GetHardMax("DeepSeek", GetModelId(ddlDeepSeekModel));
                UpdateMaxTokensForModel(nudDeepSeekMaxTokens, chkDeepSeekMaxTokensNull, GetModelId(ddlDeepSeekModel));
                nudDeepSeekTemp.Value = (decimal)(deepSeek.Temperature ?? 0.0);
                chkDeepSeekTempNull.Checked = !deepSeek.Temperature.HasValue;
                chkDeepSeekReasoning.Checked = deepSeek.EnableReasoning;

                // Load per-model chunking for the selected DeepSeek model
                var deepSeekModelId = GetModelId(ddlDeepSeekModel);
                if (!string.IsNullOrWhiteSpace(deepSeekModelId))
                {
                    var dm = await _settingsProvider.GetDeepSeekModelSettingsAsync(deepSeekModelId, _userContext.UserId).ConfigureAwait(true);
                    _deepSeekModelChunking[deepSeekModelId] = (dm.ChunkSize ?? _processingSettings.ChunkSizeDefault, dm.MaxConcurrentChunks ?? _processingSettings.MaxConcurrentChunksDefault);
                    UpdateDeepSeekChunkingValue();
                }

                // Load Ollama Settings
                var ollama = await _settingsProvider.GetOllamaSettingsAsync(_userContext.UserId).ConfigureAwait(true);
                SelectModelByModelId(ddlOllamaModel, ollama.ModelName);
                UpdateOllamaReasoningLevels();
                SelectReasoningLevelForModel(ddlOllamaReasoning, GetModelId(ddlOllamaModel));
                nudOllamaTimeout.Value = ollama.TimeoutSeconds;
                nudOllamaMaxTokens.Maximum = ModelTokenLimits.GetHardMax("Ollama", GetModelId(ddlOllamaModel));
                UpdateMaxTokensForModel(nudOllamaMaxTokens, chkOllamaMaxTokensNull, GetModelId(ddlOllamaModel));
                nudOllamaContext.Value = ollama.ContextWindow ?? 0;
                chkOllamaContextNull.Checked = !ollama.ContextWindow.HasValue;
                nudOllamaTemp.Value = (decimal)(ollama.Temperature ?? 0.0);
                chkOllamaTempNull.Checked = !ollama.Temperature.HasValue;
                nudOllamaTopP.Value = (decimal)(ollama.TopP ?? 0.0);
                chkOllamaTopPNull.Checked = !ollama.TopP.HasValue;
                nudOllamaTopK.Value = ollama.TopK ?? 0;
                chkOllamaTopKNull.Checked = !ollama.TopK.HasValue;
                nudOllamaMinP.Value = (decimal)(ollama.MinP ?? 0.0);
                chkOllamaMinPNull.Checked = !ollama.MinP.HasValue;
                nudOllamaRepeatPenalty.Value = (decimal)(ollama.RepeatPenalty ?? 0.0);
                chkOllamaRepeatPenaltyNull.Checked = !ollama.RepeatPenalty.HasValue;
                nudOllamaRepeatLastN.Value = Math.Clamp(ollama.RepeatLastN ?? 0, (int)nudOllamaRepeatLastN.Minimum, (int)nudOllamaRepeatLastN.Maximum);
                chkOllamaRepeatLastNNull.Checked = !ollama.RepeatLastN.HasValue;
                nudOllamaPresencePenalty.Value = (decimal)(ollama.PresencePenalty ?? 0.0);
                chkOllamaPresencePenaltyNull.Checked = !ollama.PresencePenalty.HasValue;
                nudOllamaFrequencyPenalty.Value = (decimal)(ollama.FrequencyPenalty ?? 0.0);
                chkOllamaFrequencyPenaltyNull.Checked = !ollama.FrequencyPenalty.HasValue;
                nudOllamaSeed.Value = ollama.Seed ?? 0;
                chkOllamaSeedNull.Checked = !ollama.Seed.HasValue;
                txtOllamaStop.Text = ollama.Stop != null ? string.Join(", ", ollama.Stop) : string.Empty;
                nudOllamaTypicalP.Value = (decimal)(ollama.TypicalP ?? 0.0);
                chkOllamaTypicalPNull.Checked = !ollama.TypicalP.HasValue;
                nudOllamaTfsZ.Value = (decimal)(ollama.TfsZ ?? 0.0);
                chkOllamaTfsZNull.Checked = !ollama.TfsZ.HasValue;
                nudOllamaMiroStat.Value = Math.Clamp(ollama.MiroStat ?? 0, (int)nudOllamaMiroStat.Minimum, (int)nudOllamaMiroStat.Maximum);
                chkOllamaMiroStatNull.Checked = !ollama.MiroStat.HasValue;
                nudOllamaMiroStatTau.Value = (decimal)(ollama.MiroStatTau ?? 0.0);
                chkOllamaMiroStatTauNull.Checked = !ollama.MiroStatTau.HasValue;
                nudOllamaMiroStatEta.Value = (decimal)(ollama.MiroStatEta ?? 0.0);
                chkOllamaMiroStatEtaNull.Checked = !ollama.MiroStatEta.HasValue;
                nudOllamaNumKeep.Value = Math.Clamp(ollama.NumKeep ?? 0, (int)nudOllamaNumKeep.Minimum, (int)nudOllamaNumKeep.Maximum);
                chkOllamaNumKeepNull.Checked = !ollama.NumKeep.HasValue;
                nudOllamaNumBatch.Value = Math.Max(nudOllamaNumBatch.Minimum, ollama.NumBatch ?? 0);
                chkOllamaNumBatchNull.Checked = !ollama.NumBatch.HasValue;
                nudOllamaNumThread.Value = ollama.NumThread ?? 0;
                chkOllamaNumThreadNull.Checked = !ollama.NumThread.HasValue;
                nudOllamaNumGpu.Value = ollama.NumGpu ?? 0;
                chkOllamaNumGpuNull.Checked = !ollama.NumGpu.HasValue;
                chkOllamaNumaNull.Checked = !ollama.Numa.HasValue;
                chkOllamaNuma.Checked = ollama.Numa ?? false;
                chkOllamaUseMlockNull.Checked = !ollama.UseMlock.HasValue;
                chkOllamaUseMlock.Checked = ollama.UseMlock ?? false;
                chkOllamaUseMmapNull.Checked = !ollama.UseMmap.HasValue;
                chkOllamaUseMmap.Checked = ollama.UseMmap ?? true;
                chkOllamaLowVramNull.Checked = !ollama.LowVram.HasValue;
                chkOllamaLowVram.Checked = ollama.LowVram ?? false;
                txtOllamaKeepAlive.Text = ollama.KeepAlive ?? string.Empty;

                // Load per-model chunking for the selected Ollama model
                var ollamaModelId = GetModelId(ddlOllamaModel);
                if (!string.IsNullOrWhiteSpace(ollamaModelId))
                {
                    var olm = await _settingsProvider.GetOllamaModelSettingsAsync(ollamaModelId, _userContext.UserId).ConfigureAwait(true);
                    _ollamaModelChunking[ollamaModelId] = (olm.ChunkSize ?? _processingSettings.ChunkSizeDefault, olm.MaxConcurrentChunks ?? _processingSettings.MaxConcurrentChunksDefault);
                    UpdateOllamaChunkingValue();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Save Gemini Settings (ApiKey and Endpoint/ProxyUrl are not included — they come
                // from machine-level environment variables/config per SEC-010)
                var gemini = new GeminiSettings
                {
                    ModelName = ddlGeminiModel.SelectedItem is AiModelInfo geminiModel ? geminiModel.ModelId : null,
                    TimeoutSeconds = (int)nudGeminiTimeout.Value,
                    ProjectId = string.IsNullOrWhiteSpace(txtGeminiProjectId.Text) ? null : txtGeminiProjectId.Text,
                    Location = string.IsNullOrWhiteSpace(txtGeminiLocation.Text) ? null : txtGeminiLocation.Text,
                    Temperature = chkGeminiTempNull.Checked ? null : (double)nudGeminiTemp.Value,
                    TopP = chkGeminiTopPNull.Checked ? null : (double)nudGeminiTopP.Value,
                    StreamingEnabled = chkGeminiStreaming.Checked,
                    MaxRetries = (int)nudGeminiMaxRetries.Value,
                    BaseDelay = (int)nudGeminiBaseDelay.Value,
                    MaxDelay = (int)nudGeminiMaxDelay.Value,
                    EnableLogging = chkGeminiEnableLogging.Checked,
                    RateLimitPerMinute = chkGeminiRateLimitNull.Checked ? null : (int)nudGeminiRateLimit.Value,
                    UseManagedIdentity = chkGeminiUseManagedIdentity.Checked,
                    CredentialType = string.IsNullOrWhiteSpace(txtGeminiCredentialType.Text) ? null : txtGeminiCredentialType.Text
                };
                await _settingsProvider.SaveGeminiSettingsAsync(_userContext.UserId, gemini).ConfigureAwait(true);

                // Save per-model Gemini chunking for the currently selected model
                var geminiModelId = GetModelId(ddlGeminiModel);
                if (!string.IsNullOrWhiteSpace(geminiModelId))
                {
                    SaveChunkingForModel(nudGeminiChunkSize, nudGeminiMaxConcurrentChunks, _geminiModelChunking, geminiModelId);
                    if (_geminiModelChunking.TryGetValue(geminiModelId, out var gchunk))
                    {
                        var gm = new GeminiModelSettings { ChunkSize = gchunk.ChunkSize, MaxConcurrentChunks = gchunk.MaxConcurrentChunks };
                        await _settingsProvider.SaveGeminiModelSettingsAsync(geminiModelId, _userContext.UserId, gm).ConfigureAwait(true);
                    }
                }

                // Save OpenAI Settings (ApiKey and Endpoint are not included — they come
                // from machine-level environment variables/config per SEC-010)
                var openAi = new OpenAiSettings
                {
                    ModelName = ddlOpenAiModel.SelectedItem is AiModelInfo openAiModel ? openAiModel.ModelId : string.Empty,
                    TimeoutSeconds = (int)nudOpenAiTimeout.Value,
                    OrganizationId = string.IsNullOrWhiteSpace(txtOpenAiOrgId.Text) ? null : txtOpenAiOrgId.Text.Trim(),
                    Temperature = chkOpenAiTempNull.Checked ? null : (double)nudOpenAiTemp.Value
                };
                await _settingsProvider.SaveOpenAiSettingsAsync(_userContext.UserId, openAi).ConfigureAwait(true);

                // Save per-model OpenAI chunking for the currently selected model
                var openAiModelId = GetModelId(ddlOpenAiModel);
                if (!string.IsNullOrWhiteSpace(openAiModelId))
                {
                    SaveChunkingForModel(nudOpenAiChunkSize, nudOpenAiMaxConcurrentChunks, _openAiModelChunking, openAiModelId);
                    if (_openAiModelChunking.TryGetValue(openAiModelId, out var ochunk))
                    {
                        var om = new OpenAiModelSettings { ChunkSize = ochunk.ChunkSize, MaxConcurrentChunks = ochunk.MaxConcurrentChunks };
                        await _settingsProvider.SaveOpenAiModelSettingsAsync(openAiModelId, _userContext.UserId, om).ConfigureAwait(true);
                    }
                }

                // Save DeepSeek Settings (ApiKey is not included — it comes from enviro
                var deepSeek = new DeepSeekSettings
                {
                    ModelName = ddlDeepSeekModel.SelectedItem is AiModelInfo deepSeekModel ? deepSeekModel.ModelId : null,
                    TimeoutSeconds = (int)nudDeepSeekTimeout.Value,
                    Temperature = chkDeepSeekTempNull.Checked ? null : (double)nudDeepSeekTemp.Value,
                    EnableReasoning = chkDeepSeekReasoning.Checked
                };
                await _settingsProvider.SaveDeepSeekSettingsAsync(_userContext.UserId, deepSeek).ConfigureAwait(true);

                // Save per-model DeepSeek chunking for the currently selected model
                var deepSeekModelId = GetModelId(ddlDeepSeekModel);
                if (!string.IsNullOrWhiteSpace(deepSeekModelId))
                {
                    SaveChunkingForModel(nudDeepSeekChunkSize, nudDeepSeekMaxConcurrentChunks, _deepSeekModelChunking, deepSeekModelId);
                    if (_deepSeekModelChunking.TryGetValue(deepSeekModelId, out var dchunk))
                    {
                        var dm = new DeepSeekModelSettings { ChunkSize = dchunk.ChunkSize, MaxConcurrentChunks = dchunk.MaxConcurrentChunks };
                        await _settingsProvider.SaveDeepSeekModelSettingsAsync(deepSeekModelId, _userContext.UserId, dm).ConfigureAwait(true);
                    }
                }

                // Save Ollama Settings (ApiKey is not included — it comes from machine configuration/env vars)
                var stopSequences = new List<string>();
                if (!string.IsNullOrWhiteSpace(txtOllamaStop.Text))
                {
                    foreach (var part in txtOllamaStop.Text.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            stopSequences.Add(part.Trim());
                        }
                    }
                }

                var ollama = new OllamaSettings
                {
                    ModelName = ddlOllamaModel.SelectedItem is AiModelInfo ollamaModel ? ollamaModel.ModelId : string.Empty,
                    TimeoutSeconds = (int)nudOllamaTimeout.Value,
                    MaxTokens = chkOllamaMaxTokensNull.Checked ? null : (int)nudOllamaMaxTokens.Value,
                    ContextWindow = chkOllamaContextNull.Checked ? null : (int)nudOllamaContext.Value,
                    Temperature = chkOllamaTempNull.Checked ? null : (double)nudOllamaTemp.Value,
                    TopP = chkOllamaTopPNull.Checked ? null : (double)nudOllamaTopP.Value,
                    TopK = chkOllamaTopKNull.Checked ? null : (int)nudOllamaTopK.Value,
                    MinP = chkOllamaMinPNull.Checked ? null : (double)nudOllamaMinP.Value,
                    RepeatPenalty = chkOllamaRepeatPenaltyNull.Checked ? null : (double)nudOllamaRepeatPenalty.Value,
                    RepeatLastN = chkOllamaRepeatLastNNull.Checked ? null : (int)nudOllamaRepeatLastN.Value,
                    PresencePenalty = chkOllamaPresencePenaltyNull.Checked ? null : (double)nudOllamaPresencePenalty.Value,
                    FrequencyPenalty = chkOllamaFrequencyPenaltyNull.Checked ? null : (double)nudOllamaFrequencyPenalty.Value,
                    Seed = chkOllamaSeedNull.Checked ? null : (int)nudOllamaSeed.Value,
                    Stop = stopSequences,
                    TypicalP = chkOllamaTypicalPNull.Checked ? null : (double)nudOllamaTypicalP.Value,
                    TfsZ = chkOllamaTfsZNull.Checked ? null : (double)nudOllamaTfsZ.Value,
                    MiroStat = chkOllamaMiroStatNull.Checked ? null : (int)nudOllamaMiroStat.Value,
                    MiroStatTau = chkOllamaMiroStatTauNull.Checked ? null : (double)nudOllamaMiroStatTau.Value,
                    MiroStatEta = chkOllamaMiroStatEtaNull.Checked ? null : (double)nudOllamaMiroStatEta.Value,
                    NumKeep = chkOllamaNumKeepNull.Checked ? null : (int)nudOllamaNumKeep.Value,
                    NumBatch = chkOllamaNumBatchNull.Checked ? null : (int)nudOllamaNumBatch.Value,
                    NumThread = chkOllamaNumThreadNull.Checked ? null : (int)nudOllamaNumThread.Value,
                    NumGpu = chkOllamaNumGpuNull.Checked ? null : (int)nudOllamaNumGpu.Value,
                    Numa = chkOllamaNumaNull.Checked ? null : chkOllamaNuma.Checked,
                    UseMlock = chkOllamaUseMlockNull.Checked ? null : chkOllamaUseMlock.Checked,
                    UseMmap = chkOllamaUseMmapNull.Checked ? null : chkOllamaUseMmap.Checked,
                    LowVram = chkOllamaLowVramNull.Checked ? null : chkOllamaLowVram.Checked,
                    KeepAlive = string.IsNullOrWhiteSpace(txtOllamaKeepAlive.Text) ? null : txtOllamaKeepAlive.Text.Trim(),
                };
                await _settingsProvider.SaveOllamaSettingsAsync(_userContext.UserId, ollama).ConfigureAwait(true);

                // Save per-model Ollama chunking for the currently selected model
                var ollamaModelId = GetModelId(ddlOllamaModel);
                if (!string.IsNullOrWhiteSpace(ollamaModelId))
                {
                    SaveChunkingForModel(nudOllamaChunkSize, nudOllamaMaxConcurrentChunks, _ollamaModelChunking, ollamaModelId);
                    if (_ollamaModelChunking.TryGetValue(ollamaModelId, out var olchunk))
                    {
                        var olm = new OllamaModelSettings { ChunkSize = olchunk.ChunkSize, MaxConcurrentChunks = olchunk.MaxConcurrentChunks };
                        await _settingsProvider.SaveOllamaModelSettingsAsync(ollamaModelId, _userContext.UserId, olm).ConfigureAwait(true);
                    }
                }

                // Save per-model reasoning levels and MaxTokens to ProcessingOptions (single source of truth)
                SaveReasoningLevelForModel(ddlGeminiReasoning, GetModelId(ddlGeminiModel));
                SaveReasoningLevelForModel(ddlOpenAiReasoning, GetModelId(ddlOpenAiModel));
                SaveReasoningLevelForModel(ddlDeepSeekReasoning, GetModelId(ddlDeepSeekModel));
                SaveReasoningLevelForModel(ddlOllamaReasoning, GetModelId(ddlOllamaModel));
                SaveMaxTokensForModel(nudGeminiMaxTokens, chkGeminiMaxTokensNull, GetModelId(ddlGeminiModel));
                SaveMaxTokensForModel(nudOpenAiMaxTokens, chkOpenAiMaxTokensNull, GetModelId(ddlOpenAiModel));
                SaveMaxTokensForModel(nudDeepSeekMaxTokens, chkDeepSeekMaxTokensNull, GetModelId(ddlDeepSeekModel));
                SaveMaxTokensForModel(nudOllamaMaxTokens, chkOllamaMaxTokensNull, GetModelId(ddlOllamaModel));

                var options = await _settingsProvider.GetProcessingOptionsAsync(_userContext.UserId).ConfigureAwait(true);
                options.ModelReasoningLevels = _modelReasoningLevels;
                options.ModelMaxTokens = _modelMaxTokens;
                await _settingsProvider.SaveProcessingOptionsAsync(_userContext.UserId, options).ConfigureAwait(true);

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration settings: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ChkGeminiMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiMaxTokens.Enabled = !chkGeminiMaxTokensNull.Checked;
        }

        private void ChkGeminiTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiTemp.Enabled = !chkGeminiTempNull.Checked;
        }

        private void ChkGeminiTopPNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiTopP.Enabled = !chkGeminiTopPNull.Checked;
        }

        private void ChkGeminiRateLimitNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiRateLimit.Enabled = !chkGeminiRateLimitNull.Checked;
        }

        private void ChkOpenAiMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudOpenAiMaxTokens.Enabled = !chkOpenAiMaxTokensNull.Checked;
        }

        private void ChkOpenAiTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudOpenAiTemp.Enabled = !chkOpenAiTempNull.Checked;
        }

        private void ChkDeepSeekMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudDeepSeekMaxTokens.Enabled = !chkDeepSeekMaxTokensNull.Checked;
        }

        private void ChkDeepSeekTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudDeepSeekTemp.Enabled = !chkDeepSeekTempNull.Checked;
        }

        private void ChkOllamaNull_CheckedChanged(object sender, EventArgs e)
        {
            var nullCheckToControl = new (CheckBox NullCheck, Control Control)[]
            {
                (chkOllamaMaxTokensNull, nudOllamaMaxTokens),
                (chkOllamaContextNull, nudOllamaContext),
                (chkOllamaTempNull, nudOllamaTemp),
                (chkOllamaTopPNull, nudOllamaTopP),
                (chkOllamaTopKNull, nudOllamaTopK),
                (chkOllamaMinPNull, nudOllamaMinP),
                (chkOllamaRepeatPenaltyNull, nudOllamaRepeatPenalty),
                (chkOllamaRepeatLastNNull, nudOllamaRepeatLastN),
                (chkOllamaPresencePenaltyNull, nudOllamaPresencePenalty),
                (chkOllamaFrequencyPenaltyNull, nudOllamaFrequencyPenalty),
                (chkOllamaSeedNull, nudOllamaSeed),
                (chkOllamaTypicalPNull, nudOllamaTypicalP),
                (chkOllamaTfsZNull, nudOllamaTfsZ),
                (chkOllamaMiroStatNull, nudOllamaMiroStat),
                (chkOllamaMiroStatTauNull, nudOllamaMiroStatTau),
                (chkOllamaMiroStatEtaNull, nudOllamaMiroStatEta),
                (chkOllamaNumKeepNull, nudOllamaNumKeep),
                (chkOllamaNumBatchNull, nudOllamaNumBatch),
                (chkOllamaNumThreadNull, nudOllamaNumThread),
                (chkOllamaNumGpuNull, nudOllamaNumGpu),
                (chkOllamaNumaNull, chkOllamaNuma),
                (chkOllamaUseMlockNull, chkOllamaUseMlock),
                (chkOllamaUseMmapNull, chkOllamaUseMmap),
                (chkOllamaLowVramNull, chkOllamaLowVram),
            };

            foreach (var pair in nullCheckToControl)
            {
                pair.Control.Enabled = !pair.NullCheck.Checked;
            }
        }

        private async void BtnOllamaRefreshModels_Click(object sender, EventArgs e)
        {
            var ollamaService = _aiServices.OfType<OllamaService>().FirstOrDefault();
            if (ollamaService == null)
            {
                return;
            }

            string? selectedModelId = GetModelId(ddlOllamaModel);
            try
            {
                btnOllamaRefreshModels.Enabled = false;
                await ollamaService.RefreshModelCatalogAsync().ConfigureAwait(true);

                ddlOllamaModel.Items.Clear();
                FillComboBox(ddlOllamaModel, [.. ollamaService.GetSupportedModels()]);
                SelectModelByModelId(ddlOllamaModel, selectedModelId);
                UpdateOllamaReasoningLevels();
                UpdateOllamaMaxTokensLimit();
                UpdateOllamaMaxTokensValue();
                UpdateOllamaChunkingValue();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh the Ollama model list: {ex.Message}", "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnOllamaRefreshModels.Enabled = true;
            }
        }

        private void UpdateGeminiMaxTokensLimit()
        {
            var modelId = ddlGeminiModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudGeminiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("Gemini", modelId);
            if (nudGeminiMaxTokens.Value > nudGeminiMaxTokens.Maximum)
            {
                nudGeminiMaxTokens.Value = nudGeminiMaxTokens.Maximum;
            }
        }

        private void UpdateOpenAiMaxTokensLimit()
        {
            var modelId = ddlOpenAiModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudOpenAiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("OpenAi", modelId);
            if (nudOpenAiMaxTokens.Value > nudOpenAiMaxTokens.Maximum)
            {
                nudOpenAiMaxTokens.Value = nudOpenAiMaxTokens.Maximum;
            }
        }

        private void UpdateDeepSeekMaxTokensLimit()
        {
            var modelId = ddlDeepSeekModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudDeepSeekMaxTokens.Maximum = ModelTokenLimits.GetHardMax("DeepSeek", modelId);
            if (nudDeepSeekMaxTokens.Value > nudDeepSeekMaxTokens.Maximum)
            {
                nudDeepSeekMaxTokens.Value = nudDeepSeekMaxTokens.Maximum;
            }
        }

        private void UpdateOllamaMaxTokensLimit()
        {
            var modelId = ddlOllamaModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudOllamaMaxTokens.Maximum = ModelTokenLimits.GetHardMax("Ollama", modelId);
            if (nudOllamaMaxTokens.Value > nudOllamaMaxTokens.Maximum)
            {
                nudOllamaMaxTokens.Value = nudOllamaMaxTokens.Maximum;
            }
        }
    }
}