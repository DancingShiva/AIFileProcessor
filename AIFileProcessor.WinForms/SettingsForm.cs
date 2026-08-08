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
        private Dictionary<string, ReasoningLevel> _modelReasoningLevels = new();
        private Dictionary<string, int> _modelMaxTokens = new();
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _geminiModelChunking = new();
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _openAiModelChunking = new();
        private Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> _deepSeekModelChunking = new();
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
        }

        private static void FillComboBox(ComboBox comboBox, List<AiModelInfo> models)
        {
            comboBox.DisplayMember = nameof(AiModelInfo.ModelName);
            comboBox.ValueMember = nameof(AiModelInfo.ModelId);
            comboBox.Items.AddRange(models.ToArray());
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

        private void SaveChunkingForModel(NumericUpDown nudChunkSize, NumericUpDown nudMaxConcurrent, Dictionary<string, (int ChunkSize, int MaxConcurrentChunks)> store, string? modelId)
        {
            if (!string.IsNullOrWhiteSpace(modelId))
            {
                store[modelId] = ((int)nudChunkSize.Value, (int)nudMaxConcurrent.Value);
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
                txtGeminiEndpoint.Text = gemini.Endpoint ?? string.Empty;
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
                txtGeminiProxyUrl.Text = gemini.ProxyUrl ?? string.Empty;
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
                txtOpenAiEndpoint.Text = openAi.Endpoint ?? string.Empty;
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
                txtDeepSeekEndpoint.Text = deepSeek.Endpoint ?? string.Empty;
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
                // Save Gemini Settings (ApiKey is not included — it comes from environment variables)
                var gemini = new GeminiSettings
                {
                    Endpoint = string.IsNullOrWhiteSpace(txtGeminiEndpoint.Text) ? null : txtGeminiEndpoint.Text,
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
                    ProxyUrl = string.IsNullOrWhiteSpace(txtGeminiProxyUrl.Text) ? null : txtGeminiProxyUrl.Text,
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

                // Save OpenAI Settings (ApiKey is not included — it comes from environment variables)
                var openAi = new OpenAiSettings
                {
                    Endpoint = string.IsNullOrWhiteSpace(txtOpenAiEndpoint.Text) ? string.Empty : txtOpenAiEndpoint.Text,
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
                    Endpoint = string.IsNullOrWhiteSpace(txtDeepSeekEndpoint.Text) ? string.Empty : txtDeepSeekEndpoint.Text,
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

                // Save per-model reasoning levels and MaxTokens to ProcessingOptions (single source of truth)
                SaveReasoningLevelForModel(ddlGeminiReasoning, GetModelId(ddlGeminiModel));
                SaveReasoningLevelForModel(ddlOpenAiReasoning, GetModelId(ddlOpenAiModel));
                SaveReasoningLevelForModel(ddlDeepSeekReasoning, GetModelId(ddlDeepSeekModel));
                SaveMaxTokensForModel(nudGeminiMaxTokens, chkGeminiMaxTokensNull, GetModelId(ddlGeminiModel));
                SaveMaxTokensForModel(nudOpenAiMaxTokens, chkOpenAiMaxTokensNull, GetModelId(ddlOpenAiModel));
                SaveMaxTokensForModel(nudDeepSeekMaxTokens, chkDeepSeekMaxTokensNull, GetModelId(ddlDeepSeekModel));

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
    }
}