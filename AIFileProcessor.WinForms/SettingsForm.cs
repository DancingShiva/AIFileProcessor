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
        private readonly GeminiSettingsManager _geminiManager;
        private readonly OpenAiSettingsManager _openAiManager;
        private readonly DeepSeekSettingsManager _deepSeekManager;

        public SettingsForm(
            GeminiSettingsManager geminiManager,
            OpenAiSettingsManager openAiManager,
            DeepSeekSettingsManager deepSeekManager,
            IEnumerable<IAiService> aiServices)
        {
            InitializeComponent();

            _geminiManager = geminiManager ?? throw new ArgumentNullException(nameof(geminiManager));
            _openAiManager = openAiManager ?? throw new ArgumentNullException(nameof(openAiManager));
            _deepSeekManager = deepSeekManager ?? throw new ArgumentNullException(nameof(deepSeekManager));

            PopulateModelDropdowns(aiServices);
            // Update numeric control maximums when the selected model changes so the UI reflects
            // provider/model hard-limits. Handlers update the corresponding NumericUpDown.Maximum.
            this.ddlGeminiModel.SelectedIndexChanged += (s, e) => UpdateGeminiMaxTokensLimit();
            this.ddlOpenAiModel.SelectedIndexChanged += (s, e) => UpdateOpenAiMaxTokensLimit();
            this.ddlDeepSeekModel.SelectedIndexChanged += (s, e) => UpdateDeepSeekMaxTokensLimit();
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

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load Gemini Settings
                var gemini = _geminiManager.ReadSettings();
                txtGeminiEndpoint.Text = gemini.Endpoint ?? string.Empty;
                SelectModelByModelId(ddlGeminiModel, gemini.ModelName);
                nudGeminiTimeout.Value = gemini.TimeoutSeconds;
                txtGeminiProjectId.Text = gemini.ProjectId ?? string.Empty;
                txtGeminiLocation.Text = gemini.Location ?? string.Empty;
                // Ensure UI control maximum matches provider/model hard-limit before assigning value.
                nudGeminiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("Gemini", gemini.ModelName);
                var geminiValue = gemini.MaxTokens ?? 0;
                if (geminiValue > (int)nudGeminiMaxTokens.Maximum) geminiValue = (int)nudGeminiMaxTokens.Maximum;
                nudGeminiMaxTokens.Value = geminiValue;
                chkGeminiMaxTokensNull.Checked = !gemini.MaxTokens.HasValue;
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

                // Load OpenAI Settings
                var openAi = _openAiManager.ReadSettings();
                txtOpenAiEndpoint.Text = openAi.Endpoint ?? string.Empty;
                SelectModelByModelId(ddlOpenAiModel, openAi.ModelName);
                nudOpenAiTimeout.Value = openAi.TimeoutSeconds;
                txtOpenAiOrgId.Text = openAi.OrganizationId ?? string.Empty;
                nudOpenAiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("OpenAi", openAi.ModelName);
                var openAiValue = openAi.MaxTokens ?? 0;
                if (openAiValue > (int)nudOpenAiMaxTokens.Maximum) openAiValue = (int)nudOpenAiMaxTokens.Maximum;
                nudOpenAiMaxTokens.Value = openAiValue;
                chkOpenAiMaxTokensNull.Checked = !openAi.MaxTokens.HasValue;
                nudOpenAiTemp.Value = (decimal)(openAi.Temperature ?? 0.0);
                chkOpenAiTempNull.Checked = !openAi.Temperature.HasValue;

                // Load DeepSeek Settings
                var deepSeek = _deepSeekManager.ReadSettings();
                SelectModelByModelId(ddlDeepSeekModel, deepSeek.ModelName);
                txtDeepSeekEndpoint.Text = deepSeek.Endpoint ?? string.Empty;
                nudDeepSeekTimeout.Value = deepSeek.TimeoutSeconds;
                nudDeepSeekMaxTokens.Maximum = ModelTokenLimits.GetHardMax("DeepSeek", deepSeek.ModelName);
                var deepSeekValue = deepSeek.MaxTokens ?? 0;
                if (deepSeekValue > (int)nudDeepSeekMaxTokens.Maximum) deepSeekValue = (int)nudDeepSeekMaxTokens.Maximum;
                nudDeepSeekMaxTokens.Value = deepSeekValue;
                chkDeepSeekMaxTokensNull.Checked = !deepSeek.MaxTokens.HasValue;
                nudDeepSeekTemp.Value = (decimal)(deepSeek.Temperature ?? 0.0);
                chkDeepSeekTempNull.Checked = !deepSeek.Temperature.HasValue;
                chkDeepSeekReasoning.Checked = deepSeek.EnableReasoning;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
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
                    MaxTokens = chkGeminiMaxTokensNull.Checked ? null : (int)nudGeminiMaxTokens.Value,
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
                _geminiManager.SaveSettings(gemini);

                // Save OpenAI Settings (ApiKey is not included — it comes from environment variables)
                var openAi = new OpenAiSettings
                {
                    Endpoint = string.IsNullOrWhiteSpace(txtOpenAiEndpoint.Text) ? string.Empty : txtOpenAiEndpoint.Text,
                    ModelName = ddlOpenAiModel.SelectedItem is AiModelInfo openAiModel ? openAiModel.ModelId : string.Empty,
                    TimeoutSeconds = (int)nudOpenAiTimeout.Value,
                    OrganizationId = string.IsNullOrWhiteSpace(txtOpenAiOrgId.Text) ? null : txtOpenAiOrgId.Text.Trim(),
                    MaxTokens = chkOpenAiMaxTokensNull.Checked ? null : (int)nudOpenAiMaxTokens.Value,
                    Temperature = chkOpenAiTempNull.Checked ? null : (double)nudOpenAiTemp.Value
                };
                _openAiManager.SaveSettings(openAi);

                // Save DeepSeek Settings (ApiKey is not included — it comes from environment variables)
                var deepSeek = new DeepSeekSettings
                {
                    ModelName = ddlDeepSeekModel.SelectedItem is AiModelInfo deepSeekModel ? deepSeekModel.ModelId : null,
                    Endpoint = string.IsNullOrWhiteSpace(txtDeepSeekEndpoint.Text) ? string.Empty : txtDeepSeekEndpoint.Text,
                    TimeoutSeconds = (int)nudDeepSeekTimeout.Value,
                    MaxTokens = chkDeepSeekMaxTokensNull.Checked ? null : (int)nudDeepSeekMaxTokens.Value,
                    Temperature = chkDeepSeekTempNull.Checked ? null : (double)nudDeepSeekTemp.Value,
                    EnableReasoning = chkDeepSeekReasoning.Checked
                };
                _deepSeekManager.SaveSettings(deepSeek);

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
            if (nudGeminiMaxTokens.Value > nudGeminiMaxTokens.Maximum) nudGeminiMaxTokens.Value = nudGeminiMaxTokens.Maximum;
        }

        private void UpdateOpenAiMaxTokensLimit()
        {
            var modelId = ddlOpenAiModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudOpenAiMaxTokens.Maximum = ModelTokenLimits.GetHardMax("OpenAi", modelId);
            if (nudOpenAiMaxTokens.Value > nudOpenAiMaxTokens.Maximum) nudOpenAiMaxTokens.Value = nudOpenAiMaxTokens.Maximum;
        }

        private void UpdateDeepSeekMaxTokensLimit()
        {
            var modelId = ddlDeepSeekModel.SelectedItem is AiModelInfo model ? model.ModelId : null;
            nudDeepSeekMaxTokens.Maximum = ModelTokenLimits.GetHardMax("DeepSeek", modelId);
            if (nudDeepSeekMaxTokens.Value > nudDeepSeekMaxTokens.Maximum) nudDeepSeekMaxTokens.Value = nudDeepSeekMaxTokens.Maximum;
        }
    }
}